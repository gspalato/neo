package slash

import (
	"fmt"
	"log/slog"
	"math"
	"os"

	"github.com/bwmarrin/discordgo"
	"github.com/zekrotja/ken"
	"unreal.sh/neo/internal/services/music"
	"unreal.sh/neo/internal/utils/datetime"
	"unreal.sh/neo/internal/utils/static"
	stringutils "unreal.sh/neo/internal/utils/stringutils"
)

type NowPlayingCommand struct{}

var (
	_ ken.SlashCommand       = (*PingCommand)(nil)
	_ ken.GuildScopedCommand = (*PingCommand)(nil)
)

func (c *NowPlayingCommand) Name() string {
	return "nowplaying"
}

func (c *NowPlayingCommand) Description() string {
	return "Tells the current song and it's position."
}

func (c *NowPlayingCommand) Version() string {
	return "1.0.0"
}

func (c *NowPlayingCommand) Type() discordgo.ApplicationCommandType {
	return discordgo.ChatApplicationCommand
}

func (c *NowPlayingCommand) Options() []*discordgo.ApplicationCommandOption {
	return []*discordgo.ApplicationCommandOption{}
}

func (c *NowPlayingCommand) Guild() string {
	return os.Getenv("MISFITS_GUILD_ID")
}

func (c *NowPlayingCommand) Run(ctx ken.Context) (err error) {
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

	track := musicSession.CurrentTrack

	remaining := datetime.Pretty(datetime.ToDuration(musicSession.Remaining()))

	var description string
	description += fmt.Sprintf("**[%s](%s)**\n", stringutils.Truncate(track.Info.Title, 30), *track.Info.URI)
	description += fmt.Sprintf("`%s` remaining.\n\n", remaining)

	max := 5
	for i, t := range musicSession.Queue {
		if i >= max {
			description += fmt.Sprintf("And %d more...", len(musicSession.Queue)-max)
			break
		}

		description += fmt.Sprintf("**%d.** [%s](%s)\n", i+1, stringutils.Truncate(t.Info.Title, 30), *t.Info.URI)
	}

	var progressBar string

	duration := float64(musicSession.CurrentTrack.Info.Length.Seconds())
	elapsed := float64(musicSession.Position().Seconds())
	position := int(math.Floor((elapsed * 20.0) / duration))

	for i := 0; i < 20; i++ {
		if i == position {
			progressBar += "🔵"
		} else {
			progressBar += "▬"
		}
	}

	embed := &discordgo.MessageEmbed{
		Title:       "🎶 **Now Playing**",
		Description: description,
		Color:       static.ColorEmbedGray,
		Footer: &discordgo.MessageEmbedFooter{
			Text: fmt.Sprintf("%s \u200B %s / %s", progressBar,
				datetime.Pretty(datetime.ToDuration(musicSession.Position())),
				datetime.Pretty(datetime.ToDuration(musicSession.CurrentTrack.Info.Length))),
		},
	}

	err = ctx.RespondEmbed(embed)
	if err != nil {
		slog.Error("Failed to respond to command.", slog.String("error", err.Error()))
	}

	return nil
}
