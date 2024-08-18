package main

import (
	"context"
	"fmt"
	"log/slog"
	"os"
	"os/signal"
	"syscall"

	"github.com/bwmarrin/discordgo"
	"github.com/disgoorg/disgolink/v3/disgolink"
	"github.com/joho/godotenv"
	"github.com/lmittmann/tint"
	"github.com/zekrotja/ken"
	"github.com/zekrotja/ken/store"

	slashCommands "unreal.sh/neo/internal/commands/slash"
	"unreal.sh/neo/internal/middlewares"
	"unreal.sh/neo/internal/services"
	"unreal.sh/neo/internal/services/music"
	"unreal.sh/neo/internal/utils"
	"unreal.sh/neo/internal/utils/cmdline"
)

func main() {
	// Setup logger
	slog.SetDefault(slog.New(
		tint.NewHandler(os.Stderr, &tint.Options{
			Level:      slog.LevelInfo,
			TimeFormat: "01/02/2006 15:04:05",
		}),
	))

	// Load environment variables
	err := godotenv.Load()
	utils.MUST(err)

	// Get token from environment
	token, exists := os.LookupEnv("DISCORD_TOKEN")
	if !exists {
		panic("No token provided.")
	}

	// Create Discord bot session
	session, err := discordgo.New("Bot " + token)
	utils.MUST(err)

	session.Identify.Intents = discordgo.IntentsAll

	// Handle command line arguments
	end, err := cmdline.HandleCommandLineArguments(session)
	utils.MUST(err)
	if end {
		return
	}

	// Setup commands and services
	dependencyProvider := services.NewServiceProvider()

	k, err := ken.New(session, ken.Options{
		CommandStore:       store.NewDefault(),
		DependencyProvider: dependencyProvider,
	})
	utils.MUST(err)

	err = k.RegisterCommands(
		new(slashCommands.AvatarCommand),
		new(slashCommands.BanCommand),
		new(slashCommands.KickCommand),
		new(slashCommands.NowPlayingCommand),
		new(slashCommands.PauseCommand),
		new(slashCommands.PingCommand),
		new(slashCommands.PlayCommand),
		new(slashCommands.PurgeCommand),
		new(slashCommands.ResumeCommand),
		new(slashCommands.SkipCommand),
		new(slashCommands.SoftbanCommand),
		new(slashCommands.StopCommand),
		new(slashCommands.QueueCommand),
		new(slashCommands.VolumeCommand),
		new(slashCommands.WhoIsCommand),
	)
	utils.MUST(err)

	err = k.RegisterMiddlewares(
		new(middlewares.PermissionsMiddleware),
		new(middlewares.VoiceChannelMiddleware),
	)
	utils.MUST(err)

	defer k.Unregister()

	err = session.Open()
	utils.MUST(err)
	defer session.Close()

	// Open session before creating MusicService, which depends on it.
	musicService, err := music.NewMusicService(session)
	utils.MUST(err)
	dependencyProvider.Register("MusicService", musicService)

	musicService.AddNode(context.Background(), disgolink.NodeConfig{
		Name:     "main",
		Address:  os.Getenv("LAVALINK_ADDRESS"),
		Password: os.Getenv("LAVALINK_PASSWORD"),
		Secure:   false,
	})

	musicService.HookEvents()

	slog.Info(fmt.Sprintf("Started bot as %s.", session.State.User.String()))

	close := make(chan os.Signal, 1)
	signal.Notify(close, syscall.SIGINT, syscall.SIGTERM, os.Interrupt, os.Kill)
	<-close
}
