package slash

import (
	"log/slog"
	"os"

	"github.com/bwmarrin/discordgo"
	"github.com/zekrotja/ken"
	"unreal.sh/neo/internal/services/music"
	"unreal.sh/neo/internal/utils/static"
)

type StopCommand struct{}

var (
	_ ken.Command            = (*StopCommand)(nil)
	_ ken.SlashCommand       = (*StopCommand)(nil)
	_ ken.GuildScopedCommand = (*StopCommand)(nil)
)

func (c *StopCommand) Name() string {
	return "stop"
}

func (c *StopCommand) Description() string {
	return "Stops the current song."
}

func (c *StopCommand) Version() string {
	return "1.0.0"
}

func (c *StopCommand) Type() discordgo.ApplicationCommandType {
	return discordgo.ChatApplicationCommand
}

func (c *StopCommand) Options() []*discordgo.ApplicationCommandOption {
	return []*discordgo.ApplicationCommandOption{}
}

func (c *StopCommand) Guild() string {
	return os.Getenv("MISFITS_GUILD_ID")
}

func (c *StopCommand) Run(ctx ken.Context) (err error) {
	if err = ctx.Defer(); err != nil {
		return nil
	}

	session := ctx.GetSession()

	musicService := ctx.Get("MusicService").(*music.MusicService)
	if musicService == nil {
		slog.Error("Music service not found in context.")
		return nil
	}

	guild, err := ctx.Guild()
	if err != nil {
		slog.Error("Failed to fetch guild.")
		return err
	}

	var isBotInVoiceChannel bool
	var botVoiceState *discordgo.VoiceState
	for _, state := range guild.VoiceStates {
		if state.UserID == session.State.User.ID {
			isBotInVoiceChannel = true
			botVoiceState = state
		}
	}

	if !isBotInVoiceChannel {
		ctx.RespondMessage("I'm not connected to a voice channel.")
		return nil
	}

	var voiceChannelID *string
	for _, state := range guild.VoiceStates {
		if state.UserID == ctx.User().ID {
			voiceChannelID = &state.ChannelID
		}
	}

	if voiceChannelID == nil {
		ctx.RespondMessage("You need to be in a voice channel to use this command.")
		return nil
	} else if *voiceChannelID != botVoiceState.ChannelID {
		ctx.RespondMessage("You have to be in the same voice channel as me to use this command.")
		return nil
	}

	musicSession := musicService.GetMusicSession(guild.ID)
	if musicSession == nil {
		slog.Error("Music session not found. Something's really wrong here.")
		ctx.RespondMessage("I'm not playing anything right now.")
		return nil
	}

	err = musicSession.Stop()
	if err != nil {
		slog.Error("Failed to stop track.", slog.String("error", err.Error()))
		return err
	}

	embed := &discordgo.MessageEmbed{
		Color:       static.ColorEmbedGray,
		Description: "⏹ **Stopped**",
	}

	err = ctx.RespondEmbed(embed)
	if err != nil {
		slog.Error("Failed to respond to command.", slog.String("error", err.Error()))
	}

	return nil
}
