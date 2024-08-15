package music

import (
	"context"
	"fmt"
	"log/slog"
	"sync"

	"github.com/bwmarrin/discordgo"
	"github.com/disgoorg/disgolink/v3/disgolink"
	"github.com/disgoorg/disgolink/v3/lavalink"
	"unreal.sh/neo/internal/utils/colorutils"
	"unreal.sh/neo/internal/utils/static"
	stringutils "unreal.sh/neo/internal/utils/stringutils"
)

type MusicSession struct {
	sync.Mutex
	session *discordgo.Session
	player  *disgolink.Player

	TextChannelID string
	GuildID       string

	Queue        []*lavalink.Track  // A queue of tracks to play next.
	Data         []TrackRequestData // A list of data for each track in the queue.
	CurrentTrack *lavalink.Track
}

func NewMusicSession(session *discordgo.Session, player *disgolink.Player, guildID string, textChannelID string) *MusicSession {
	return &MusicSession{
		session: session,
		player:  player,

		GuildID:       guildID,
		TextChannelID: textChannelID,

		Queue:        make([]*lavalink.Track, 0),
		CurrentTrack: nil,
	}
}

// Play plays a track.
func (s *MusicSession) Play(track *lavalink.Track, data TrackRequestData) error {
	err := (*s.player).Update(context.TODO(), lavalink.WithTrack(*track), lavalink.WithTrackUserData(data))
	if err != nil {
		return err
	}

	return nil
}

// Enqueue adds a track to the queue.
func (s *MusicSession) Enqueue(track *lavalink.Track, data TrackRequestData) {
	s.Queue = append(s.Queue, track)
	s.Data = append(s.Data, data)
}

func (s *MusicSession) PlayOrEnqueue(track *lavalink.Track, data TrackRequestData) (enqueued bool, err error) {
	if s.IsPlaying() {
		s.Enqueue(track, data)
		return true, nil
	}

	return false, s.Play(track, data)
}

func (s *MusicSession) Dequeue() (*lavalink.Track, TrackRequestData) {
	if len(s.Queue) == 0 {
		return nil, TrackRequestData{}
	}

	track := s.Queue[0]
	s.Queue = s.Queue[1:]

	data := s.Data[0]
	s.Data = s.Data[1:]

	return track, data
}

// IsPlaying returns whether the session is currently playing a track.
// Note that this does not check if the player is paused.
func (s *MusicSession) IsPlaying() bool {
	return s.CurrentTrack != nil
}

// Pauses the current track.
func (s *MusicSession) Pause() error {
	player := (*s.player)

	return player.Update(context.TODO(), lavalink.WithPaused(true))
}

// Returns whether the current track is paused.
func (s *MusicSession) Paused() bool {
	player := (*s.player)

	return player.Paused()
}

// Resumes the current track.
func (s *MusicSession) Resume() error {
	player := (*s.player)

	return player.Update(context.TODO(), lavalink.WithPaused(false))
}

func (s *MusicSession) Skip() error {
	player := (*s.player)

	next, data := s.Dequeue()
	if next == nil {
		return s.Stop()
	} else {
		player.Update(context.TODO(), lavalink.WithTrack(*next), lavalink.WithTrackUserData(data))
	}

	return nil
}

func (s *MusicSession) Stop() error {
	player := (*s.player)

	player.Update(context.TODO(), lavalink.WithNullTrack())

	return nil
}

func (s *MusicSession) Position() lavalink.Duration {
	player := (*s.player)

	return player.State().Position
}

func (s *MusicSession) Remaining() lavalink.Duration {
	player := (*s.player)

	return player.Track().Info.Length - player.State().Position
}

func (s *MusicSession) Volume(volume int) error {
	player := (*s.player)

	return player.Update(context.TODO(), lavalink.WithVolume(volume))
}

func (s *MusicSession) HandleTrackStart(track *lavalink.Track) error {
	s.CurrentTrack = track

	textChannel, err := s.session.Channel(s.TextChannelID)
	if err != nil {
		return err
	}

	// Get the prominent color of the track's artwork.
	color := static.ColorEmbedGray
	c, err := colorutils.GetDominantColorFromImageURL(*track.Info.ArtworkURL)
	if err == nil {
		color = c
	}

	var data TrackRequestData
	slog.Debug(fmt.Sprintf("Unmarshalling user data: %s", track.UserData.String()))
	track.UserData.Unmarshal(&data)

	author, err := s.session.User(data.AuthorID)
	if err != nil {
		return err
	}

	embed := &discordgo.MessageEmbed{
		Title: "🎶  **Now Playing**",
		Color: color,
		Description: fmt.Sprintf("**[%s](%s)**\nby %s\n",
			stringutils.Truncate(track.Info.Title, 30), *track.Info.URI, track.Info.Author),
		Thumbnail: &discordgo.MessageEmbedThumbnail{
			URL: *track.Info.ArtworkURL,
		},
		Footer: &discordgo.MessageEmbedFooter{
			IconURL: author.AvatarURL("512"),
			Text:    fmt.Sprintf("Requested by %s", author.Username),
		},
	}

	_, err = s.session.ChannelMessageSendEmbed(textChannel.ID, embed)
	if err != nil {
		return err
	}

	return nil
}

func (s *MusicSession) HandleTrackPause(track *lavalink.Track) error {
	return nil
}

func (s *MusicSession) HandleTrackEnd(track *lavalink.Track) error {
	s.CurrentTrack = nil

	if len(s.Queue) == 0 {
		return nil
	}

	next, data := s.Dequeue()

	s.Play(next, data)

	return nil
}
