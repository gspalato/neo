package slash

import (
	"log/slog"
	"os"

	"github.com/bwmarrin/discordgo"
	"github.com/zekrotja/ken"
	"unreal.sh/neo/internal/services/music"
)

type SkipCommand struct{}

var (
	_ ken.SlashCommand       = (*PingCommand)(nil)
	_ ken.GuildScopedCommand = (*PingCommand)(nil)
)

func (c *SkipCommand) Name() string {
	return "skip"
}

func (c *SkipCommand) Description() string {
	return "Skips the current song."
}

func (c *SkipCommand) Version() string {
	return "1.0.0"
}

func (c *SkipCommand) Type() discordgo.ApplicationCommandType {
	return discordgo.ChatApplicationCommand
}

func (c *SkipCommand) Options() []*discordgo.ApplicationCommandOption {
	return []*discordgo.ApplicationCommandOption{}
}

func (c *SkipCommand) Guild() string {
	return os.Getenv("MISFITS_GUILD_ID")
}

func (c *SkipCommand) Run(ctx ken.Context) (err error) {
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
		if state.UserID == ctx.GetSession().State.User.ID {
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
		var hasOtherPeople bool
		for _, state := range guild.VoiceStates {
			if state.ChannelID == botVoiceState.ChannelID && state.UserID != session.State.User.ID {
				hasOtherPeople = true
				break
			}
		}

		if hasOtherPeople {
			ctx.RespondMessage("You have to be in the same voice channel as me to use this command.")
		} else {
			// Move bot to user's voice channel
			err = session.ChannelVoiceJoinManual(guild.ID, *voiceChannelID, false, false)
			if err != nil {
				slog.Error("Failed to join voice channel.")
				return err
			}
		}
	}

	musicSession := musicService.GetMusicSession(guild.ID)
	if musicSession == nil {
		slog.Error("Music session not found. Something's really wrong here.")
		ctx.RespondMessage("I'm not playing anything right now.")
		return nil
	}

	musicSession.Skip()

	return nil
}
