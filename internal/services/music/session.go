package music

import (
	"context"
	"fmt"

	"github.com/bwmarrin/discordgo"
	"github.com/disgoorg/disgolink/v3/disgolink"
	"github.com/disgoorg/disgolink/v3/lavalink"
	"unreal.sh/neo/internal/utils/static"
	stringutils "unreal.sh/neo/internal/utils/strings"
)

type MusicSession struct {
	session *discordgo.Session
	player  *disgolink.Player

	TextChannelID string
	GuildID       string
	Queue         []*lavalink.Track
	CurrentTrack  *lavalink.Track
}

func NewMusicSession(session *discordgo.Session, player *disgolink.Player, guildID string, textChannelID string) *MusicSession {
	return &MusicSession{
		session:       session,
		player:        player,
		GuildID:       guildID,
		TextChannelID: textChannelID,
		Queue:         make([]*lavalink.Track, 0),
	}
}

// Play plays a track.
func (s *MusicSession) Play(track *lavalink.Track) error {
	err := (*s.player).Update(context.TODO(), lavalink.WithTrack(*track))
	if err != nil {
		return err
	}

	return nil
}

// Enqueue adds a track to the queue.
func (s *MusicSession) Enqueue(track *lavalink.Track) {
	s.Queue = append(s.Queue, track)
}

func (s *MusicSession) PlayOrEnqueue(track *lavalink.Track) (enqueued bool, err error) {
	if s.IsPlaying() {
		s.Enqueue(track)
		return true, nil
	}

	return false, s.Play(track)
}

func (s *MusicSession) Dequeue() *lavalink.Track {
	if len(s.Queue) == 0 {
		return nil
	}

	track := s.Queue[0]
	s.Queue = s.Queue[1:]

	return track
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

	next := s.Dequeue()
	if next == nil {
		return s.Stop()
	} else {
		player.Update(context.TODO(), lavalink.WithTrack(*next))
	}

	return nil
}

func (s *MusicSession) Stop() error {
	player := (*s.player)

	player.Update(context.TODO(), lavalink.WithNullTrack())

	return nil
}

func (s *MusicSession) HandleTrackStart(track *lavalink.Track) error {
	s.CurrentTrack = track

	textChannel, err := s.session.Channel(s.TextChannelID)
	if err != nil {
		return err
	}

	embed := &discordgo.MessageEmbed{
		Title:       "🎶 **Now Playing**",
		Color:       static.ColorEmbedGray,
		Description: fmt.Sprintf("[%s](%s)", stringutils.Truncate(track.Info.Title, 30), *track.Info.URI),
	}

	_, err = s.session.ChannelMessageSendEmbed(textChannel.ID, embed)
	if err != nil {
		return err
	}

	return nil
}

func (s *MusicSession) HandleTrackPause(track *lavalink.Track) error {
	textChannel, err := s.session.Channel(s.TextChannelID)
	if err != nil {
		return err
	}

	embed := &discordgo.MessageEmbed{
		Color:       static.ColorEmbedGray,
		Description: fmt.Sprintf("**Paused** [%s](%s).", track.Info.Title, *track.Info.URI),
	}

	_, err = s.session.ChannelMessageSendEmbed(textChannel.ID, embed)
	if err != nil {
		return err
	}

	return nil
}

func (s *MusicSession) HandleTrackEnd(track *lavalink.Track) error {
	s.CurrentTrack = nil

	if len(s.Queue) == 0 {
		return nil
	}

	nextTrack := s.Queue[0]
	s.Queue = s.Queue[1:]

	s.Play(nextTrack)

	return nil
}
