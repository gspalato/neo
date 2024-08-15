package slash

import (
	"fmt"
	"log/slog"
	"os"

	"github.com/bwmarrin/discordgo"
	"github.com/zekrotja/ken"
	"unreal.sh/neo/internal/services/music"

	embedutils "unreal.sh/neo/internal/utils/embedutils"
)

type VolumeCommand struct{}

var (
	_ ken.SlashCommand       = (*VolumeCommand)(nil)
	_ ken.GuildScopedCommand = (*VolumeCommand)(nil)
)

func (c *VolumeCommand) Name() string {
	return "volume"
}

func (c *VolumeCommand) Description() string {
	return "Sets the volume."
}

func (c *VolumeCommand) Version() string {
	return "1.0.0"
}

func (c *VolumeCommand) Type() discordgo.ApplicationCommandType {
	return discordgo.ChatApplicationCommand
}

func (c *VolumeCommand) Options() []*discordgo.ApplicationCommandOption {
	return []*discordgo.ApplicationCommandOption{
		{
			Type:        discordgo.ApplicationCommandOptionInteger,
			Name:        "volume",
			Description: "The volume to set.",
			Required:    true,
		},
	}
}

func (c *VolumeCommand) Guild() string {
	return os.Getenv("MISFITS_GUILD_ID")
}

func (c *VolumeCommand) Run(ctx ken.Context) (err error) {
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

	volume := ctx.Options().GetByName("volume").IntValue()
	musicSession.Volume(int(volume))

	embed := embedutils.CreateBasicEmbed("🔊 **Set volume to `" + fmt.Sprint(volume) + "%`.**")
	err = ctx.RespondEmbed(embed)
	if err != nil {
		slog.Error("Failed to respond to command.")
		slog.Error(err.Error())
		return err
	}

	return nil
}
