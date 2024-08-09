package slash

import (
	"log/slog"
	"os"

	"github.com/bwmarrin/discordgo"
	"github.com/zekrotja/ken"
	"unreal.sh/neo/internal/services/music"
	embedutils "unreal.sh/neo/internal/utils/embedutils"
)

type ResumeCommand struct{}

var (
	_ ken.SlashCommand       = (*PingCommand)(nil)
	_ ken.GuildScopedCommand = (*PingCommand)(nil)
)

func (c *ResumeCommand) Name() string {
	return "resume"
}

func (c *ResumeCommand) Description() string {
	return "Resumes the current song."
}

func (c *ResumeCommand) Version() string {
	return "1.0.0"
}

func (c *ResumeCommand) Type() discordgo.ApplicationCommandType {
	return discordgo.ChatApplicationCommand
}

func (c *ResumeCommand) Options() []*discordgo.ApplicationCommandOption {
	return []*discordgo.ApplicationCommandOption{}
}

func (c *ResumeCommand) Guild() string {
	return os.Getenv("MISFITS_GUILD_ID")
}

func (c *ResumeCommand) Run(ctx ken.Context) (err error) {
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

	err = musicSession.Resume()
	if err != nil {
		slog.Error("Failed to resume track.", err)
		return err
	}

	embed := embedutils.CreateBasicEmbed("⏸ **Resumed**")
	err = ctx.RespondEmbed(embed)
	if err != nil {
		slog.Error("Failed to respond to command.", err)
	}

	return nil
}
