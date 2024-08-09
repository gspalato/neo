package slash

import (
	"context"
	"fmt"
	"log/slog"
	"os"

	"github.com/bwmarrin/discordgo"
	"github.com/disgoorg/disgolink/v3/disgolink"
	"github.com/disgoorg/disgolink/v3/lavalink"
	"github.com/zekrotja/ken"

	"unreal.sh/neo/internal/services/music"
	"unreal.sh/neo/internal/utils/static"
	stringutils "unreal.sh/neo/internal/utils/stringutils"
)

type PlayCommand struct{}

var (
	_ ken.SlashCommand       = (*PingCommand)(nil)
	_ ken.GuildScopedCommand = (*PingCommand)(nil)
)

func (c *PlayCommand) Name() string {
	return "play"
}

func (c *PlayCommand) Description() string {
	return "Plays a song."
}

func (c *PlayCommand) Version() string {
	return "1.0.0"
}

func (c *PlayCommand) Type() discordgo.ApplicationCommandType {
	return discordgo.ChatApplicationCommand
}

func (c *PlayCommand) Options() []*discordgo.ApplicationCommandOption {
	return []*discordgo.ApplicationCommandOption{
		{
			Type:        discordgo.ApplicationCommandOptionString,
			Name:        "query",
			Description: "The song's name or link.",
			Required:    true,
		},
	}
}

func (c *PlayCommand) Guild() string {
	return os.Getenv("MISFITS_GUILD_ID")
}

func (c *PlayCommand) Run(ctx ken.Context) (err error) {
	session := ctx.GetSession()

	if err = ctx.Defer(); err != nil {
		return err
	}

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

	textChannel, err := ctx.Channel()
	if err != nil {
		slog.Error("Failed to fetch text channel.")
		return err
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
	}

	err = session.ChannelVoiceJoinManual(guild.ID, *voiceChannelID, false, false)
	if err != nil {
		slog.Error("Failed to join voice channel.")
		return err
	}

	query := ctx.Options().GetByName("query").StringValue()

	if !stringutils.IsUrl(query) {
		slog.Info(fmt.Sprintf("Searching for \"%s\" on Deezer.", query))
		query = fmt.Sprintf("dzsearch:%s", query)
	} else {
		slog.Info("Query is an URL. Loading directly.")
	}

	musicSession := musicService.MusicSession(guild.ID, textChannel.ID)

	musicService.LavalinkClient.BestNode().LoadTracksHandler(context.TODO(), query, disgolink.NewResultHandler(
		// Loaded a single track
		func(track lavalink.Track) {
			slog.Info(fmt.Sprintf("Found track %s.", track.Info.Title))
			musicSession.PlayOrEnqueue(&track)
		},

		// Loaded a playlist
		func(playlist lavalink.Playlist) {
			slog.Info(fmt.Sprintf("Found a playlist %s with %d tracks.", playlist.Info.Name, len(playlist.Tracks)))
			for _, track := range playlist.Tracks {
				musicSession.PlayOrEnqueue(&track)
			}
		},

		// Loaded a search result
		func(tracks []lavalink.Track) {
			if len(tracks) == 0 {
				ctx.FollowUpMessage("No results found.")
				return
			}

			slog.Info(fmt.Sprintf("Found %d tracks.", len(tracks)))

			musicSession.SupressNextEventMessage()
			enqueued, err := musicSession.PlayOrEnqueue(&tracks[0])
			if err != nil {
				slog.Error(fmt.Sprintf("Failed to play or enqueue track: %s", err.Error()))
				ctx.FollowUpMessage("Failed to play or enqueue track.")
				return
			}

			if enqueued {
				slog.Info(fmt.Sprintf("Enqueued %s.", tracks[0].Info.Title))

				description := fmt.Sprintf("▶ **Enqueued** [%s](%s)",
					stringutils.Truncate(tracks[0].Info.Title, 30), *tracks[0].Info.URI)

				embed := &discordgo.MessageEmbed{
					Color:       static.ColorEmbedGray,
					Description: description,
				}

				if m := ctx.FollowUpEmbed(embed).Send(); m.Error != nil {
					slog.Error(fmt.Sprintf("Failed to send enqueued message: %s", m.Error.Error()))
					return
				}

				return
			} else {
				embed := GetPlayingNotificationEmbed(&tracks[0])
				if m := ctx.FollowUpEmbed(embed).Send(); m.Error != nil {
					slog.Error(fmt.Sprintf("Failed to send playing notification: %s", m.Error.Error()))
					return
				}
			}

			/*
				slog.Info(fmt.Sprintf("Found %d tracks.", len(tracks)))

				var description string
				for i, track := range tracks {
					slog.Info(fmt.Sprintf("Track %d: %s", i+1, track.Info.Title))
					description += fmt.Sprintf("**%d.** [%s](%s)\n",
						i+1, stringutils.EllipticalTruncate(track.Info.Title, 30), *track.Info.URI)
				}

				embed := &discordgo.MessageEmbed{
					Title:       "Search Results",
					Description: description,
					Color:       static.ColorEmbedGray,
				}

				b := ctx.FollowUpEmbed(embed)
				b.AddComponents(func(cb *ken.ComponentBuilder) {
					cb.AddActionsRow(func(b ken.ComponentAssembler) {
						for i, track := range tracks {
							b.Add(discordgo.Button{
								Label:    string(i + 1),
								Style:    discordgo.PrimaryButton,
								CustomID: fmt.Sprintf("play:%s", *track.Info.URI),
							}, func(ctx ken.ComponentContext) bool {
								musicSession.PlayOrEnqueue(&track)
								return true
							}, true)
						}
					}).Condition(func(cctx ken.ComponentContext) bool {
						return cctx.User().ID == ctx.User().ID
					})
				})

				msg := b.Send()
				if msg == nil {
					slog.Error("Failed to send search results.")
					return
				} else if msg.Error != nil {
					slog.Error("Failed to send search results: %s", msg.Error.Error())
					return
				}

				slog.Info("Sent search results.")
			*/
		},

		// Nothing matching the query found
		func() {
			slog.Info("No results found.")
			ctx.SetEphemeral(true)
			ctx.FollowUpMessage("No results found.").Send()
		},

		// Something went wrong
		func(err error) {
			slog.Error("Something went wrong while loading the track.")
			ctx.SetEphemeral(true)
			ctx.FollowUpMessage("Something went wrong while loading the track.").Send()
		},
	))

	return nil
}

func GetPlayingNotificationEmbed(track *lavalink.Track) *discordgo.MessageEmbed {
	return &discordgo.MessageEmbed{
		Title:       "🎶 **Now Playing**",
		Color:       static.ColorEmbedGray,
		Description: fmt.Sprintf("[%s](%s)", stringutils.Truncate(track.Info.Title, 30), *track.Info.URI),
	}
}
