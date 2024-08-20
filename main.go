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

	"unreal.sh/neo/internal/commands/slash"
	"unreal.sh/neo/internal/database"
	"unreal.sh/neo/internal/middlewares"
	mods "unreal.sh/neo/internal/modules"
	"unreal.sh/neo/internal/services"
	"unreal.sh/neo/internal/services/modules"
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

	// Create database
	db, err := database.NewDatabase()
	utils.MUST(err)

	// Setup commands and services
	dependencyProvider := services.NewServiceProvider()

	k, err := ken.New(session, ken.Options{
		CommandStore:       store.NewDefault(),
		DependencyProvider: dependencyProvider,
	})
	utils.MUST(err)

	err = k.RegisterCommands(
		new(slash.AvatarCommand),
		new(slash.BanCommand),
		new(slash.KickCommand),
		new(slash.ModuleCommand),
		new(slash.NowPlayingCommand),
		new(slash.PauseCommand),
		new(slash.PingCommand),
		new(slash.PlayCommand),
		new(slash.PurgeCommand),
		new(slash.ResumeCommand),
		new(slash.SkipCommand),
		new(slash.SoftbanCommand),
		new(slash.StopCommand),
		new(slash.QueueCommand),
		new(slash.VolumeCommand),
		new(slash.WhoIsCommand),
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

	dependencyProvider.Register("Database", db)

	// Create guild settings for guilds that do not have one yet.
	guilds := session.State.Guilds
	_ = db.AssureGuildSettings(guilds...)

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

	// Start module system.
	moduleManager := modules.NewModuleManager(session, db)
	moduleManager.RegisterModules(
		new(mods.BaseModule),
		new(mods.ModerationModule),
		new(mods.MusicModule),
	)

	moduleManager.EnableModule("base", "")
	moduleManager.Initialize()
	moduleManager.RegisterEventHandlers()

	dependencyProvider.Register("ModuleManager", moduleManager)

	slog.Info(fmt.Sprintf("Started bot as %s.", session.State.User.String()))

	close := make(chan os.Signal, 1)
	signal.Notify(close, syscall.SIGINT, syscall.SIGTERM, os.Interrupt, os.Kill)
	<-close
}
