package middlewares

import (
	"github.com/zekrotja/ken"
)

var (
	_ ken.MiddlewareBefore = (*VoiceChannelMiddleware)(nil)
)

// Represents an interface for commands that requires the user to be in a voice channel.
type RequiresVoiceChannelCommand interface {
	RequiresVoiceChannel() bool
}

type VoiceChannelMiddleware struct{}

func (c *VoiceChannelMiddleware) Before(ctx *ken.Ctx) (next bool, err error) {
	cmd, ok := ctx.Command.(RequiresVoiceChannelCommand)
	if !ok {
		return true, nil
	}

	if !cmd.RequiresVoiceChannel() {
		return true, nil
	}

	guild, err := ctx.GetSession().Guild(ctx.GetEvent().GuildID)
	if err != nil {
		return false, err
	}

	var voiceChannelID *string
	for _, state := range guild.VoiceStates {
		if state.UserID == ctx.User().ID {
			voiceChannelID = &state.ChannelID
		}
	}

	if voiceChannelID == nil {
		ctx.RespondMessage("You need to be in a voice channel to use this command.")
		return false, nil
	}

	voiceChannel, err := ctx.GetSession().Channel(*voiceChannelID)
	if err != nil {
		return false, err
	}

	ctx.Set("VoiceChannel", voiceChannel)

	return true, nil
}
