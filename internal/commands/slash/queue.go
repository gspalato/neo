package slash

import (
	"fmt"
	"log/slog"
	"os"

	"github.com/bwmarrin/discordgo"
	"github.com/zekrotja/ken"
	"unreal.sh/neo/internal/services/music"
	"unreal.sh/neo/pkg/widgets"

	embedutils "unreal.sh/neo/internal/utils/embedutils"
	sliceutils "unreal.sh/neo/internal/utils/sliceutils"
	stringutils "unreal.sh/neo/internal/utils/stringutils"
)

type QueueCommand struct{}

var (
	_ ken.SlashCommand       = (*QueueCommand)(nil)
	_ ken.GuildScopedCommand = (*QueueCommand)(nil)
)

func (c *QueueCommand) Name() string {
	return "queue"
}

func (c *QueueCommand) Description() string {
	return "Shows the track queue."
}

func (c *QueueCommand) Version() string {
	return "1.0.0"
}

func (c *QueueCommand) Type() discordgo.ApplicationCommandType {
	return discordgo.ChatApplicationCommand
}

func (c *QueueCommand) Options() []*discordgo.ApplicationCommandOption {
	return []*discordgo.ApplicationCommandOption{}
}

func (c *QueueCommand) Guild() string {
	return os.Getenv("MISFITS_GUILD_ID")
}

func (c *QueueCommand) Run(ctx ken.Context) (err error) {
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

	queue := musicSession.Queue
	list := make([]string, len(queue))

	for i, track := range queue {
		list[i] = fmt.Sprintf("**%d.** [%s](%s)", i+1, stringutils.Truncate(track.Info.Title, 30), *track.Info.URI)
	}

	split := sliceutils.Chunk(list, 5)
	pages := make([]*discordgo.MessageEmbed, 0)

	if len(queue) == 0 {
		embed := embedutils.CreateBasicEmbed("No tracks in queue.")
		embed.Title = "🎼  **Queue**"
		pages = append(pages, embed)
	} else {
		for i, chunk := range split {
			var description string

			for _, item := range chunk {
				description += item + "\n"
			}

			embed := embedutils.CreateBasicEmbed(description)
			embed.Title = fmt.Sprintf("🎼  **Queue** (Page %d/%d)", i+1, len(split))

			pages = append(pages, embed)
		}
	}

	paginator := widgets.NewPaginator(&ctx)
	paginator.Add(pages...)

	err = paginator.Spawn()
	if err != nil {
		slog.Error("Failed to spawn paginator.")
		slog.Error(err.Error())
		return err
	}

	return nil
}
