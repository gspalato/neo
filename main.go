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
	"github.com/zekrotja/ken"
	"github.com/zekrotja/ken/store"

	slashCommands "unreal.sh/neo/internal/commands/slash"
	"unreal.sh/neo/internal/services"
	"unreal.sh/neo/internal/services/music"
	"unreal.sh/neo/internal/utils"
	"unreal.sh/neo/internal/utils/cmdline"
)

func main() {
	err := godotenv.Load()
	utils.MUST(err)

	token, exists := os.LookupEnv("DISCORD_TOKEN")
	if !exists {
		panic("No token provided.")
	}

	session, err := discordgo.New("Bot " + token)
	utils.MUST(err)

	session.Identify.Intents = discordgo.IntentsAll

	end, err := cmdline.HandleCommandLineArguments(session)
	utils.MUST(err)
	if end {
		return
	}

	dependencyProvider := services.NewServiceProvider()

	k, err := ken.New(session, ken.Options{
		CommandStore:       store.NewDefault(),
		DependencyProvider: dependencyProvider,
	})
	utils.MUST(err)

	err = k.RegisterCommands(
		new(slashCommands.PingCommand),
		new(slashCommands.PlayCommand),
		new(slashCommands.SkipCommand),
		new(slashCommands.WhoIsCommand),
	)
	utils.MUST(err)
	defer k.Unregister()

	err = session.Open()
	utils.MUST(err)
	defer session.Close()

	// Wait for the bot to be ready to create MusicService, which depends on the session.
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
