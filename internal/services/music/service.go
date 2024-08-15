package music

import (
	"context"
	"log/slog"

	"github.com/bwmarrin/discordgo"
	"github.com/disgoorg/disgolink/v3/disgolink"
	"github.com/disgoorg/disgolink/v3/lavalink"
	"github.com/disgoorg/snowflake/v2"
)

type MusicService struct {
	session *discordgo.Session

	LavalinkClient disgolink.Client
	MusicSessions  map[string]*MusicSession // GuildID -> MusicSession
}

func NewMusicService(session *discordgo.Session) (*MusicService, error) {
	userID, err := snowflake.Parse(session.State.User.ID)
	if err != nil {
		return &MusicService{}, err
	}

	service := &MusicService{
		session: session,

		MusicSessions: make(map[string]*MusicSession),
	}

	client := disgolink.New(userID,
		disgolink.WithListenerFunc(service.onPlayerUpdate),
		disgolink.WithListenerFunc(service.onPlayerPause),
		disgolink.WithListenerFunc(service.onPlayerResume),
		disgolink.WithListenerFunc(service.onTrackStart),
		disgolink.WithListenerFunc(service.onTrackEnd),
		disgolink.WithListenerFunc(service.onTrackException),
		disgolink.WithListenerFunc(service.onTrackStuck),
		disgolink.WithListenerFunc(service.onWebSocketClosed),
	)

	service.LavalinkClient = client

	return service, nil
}

func (s *MusicService) HookEvents() {
	s.session.AddHandler(s.onVoiceStateUpdate)
	s.session.AddHandler(s.onVoiceServerUpdate)
}

func (s *MusicService) AddNode(ctx context.Context, config disgolink.NodeConfig) (disgolink.Node, error) {
	return s.LavalinkClient.AddNode(ctx, config)
}

func (s *MusicService) GetMusicSession(guildID string) *MusicSession {
	session, exists := s.MusicSessions[guildID]
	if !exists {
		return nil
	}

	return session
}

// MusicSession returns the music session for the passed guild ID.
// If no session exists, a new one will be created.
func (s *MusicService) MusicSession(guildID string, textChannelID string) *MusicSession {
	session, exists := s.MusicSessions[guildID]

	guildSnowflake, err := snowflake.Parse(guildID)
	if err != nil {
		return nil
	}

	if !exists {
		player := s.LavalinkClient.Player(guildSnowflake)
		session = NewMusicSession(s.session, &player, guildID, textChannelID)
		s.MusicSessions[guildID] = session
	}

	return session
}

// Events

// onVoiceStateUpdate is called when a voice state update event is received.
// It needs to be hooked to the bot's events via MusicService.HookEvents().
func (s *MusicService) onVoiceStateUpdate(session *discordgo.Session, event *discordgo.VoiceStateUpdate) {
	// Filter all non-bot voice state updates out
	if event.VoiceState.UserID != session.State.Application.ID {
		return
	}

	guildID, err := snowflake.Parse(event.VoiceState.GuildID)
	if err != nil {
		return
	}

	channelID, err := snowflake.Parse(event.VoiceState.ChannelID)
	if err != nil {
		return
	}

	s.LavalinkClient.OnVoiceStateUpdate(
		context.TODO(),
		guildID,
		&channelID,
		event.VoiceState.SessionID,
	)
}

// onVoiceServerUpdate is called when a voice server update event is received.
// It needs to be hooked to the bot's events via MusicService.HookEvents().
func (s *MusicService) onVoiceServerUpdate(session *discordgo.Session, event *discordgo.VoiceServerUpdate) {
	guildID, err := snowflake.Parse(event.GuildID)
	if err != nil {
		return
	}

	s.LavalinkClient.OnVoiceServerUpdate(context.TODO(), guildID, event.Token, event.Endpoint)
}

func (s *MusicService) onPlayerUpdate(player disgolink.Player, event lavalink.PlayerUpdateMessage) {
	// do something with the event

}

func (s *MusicService) onPlayerPause(player disgolink.Player, event lavalink.PlayerPauseEvent) {
	// do something with the event
}

func (s *MusicService) onPlayerResume(player disgolink.Player, event lavalink.PlayerResumeEvent) {
	// do something with the event
}

func (s *MusicService) onTrackStart(player disgolink.Player, event lavalink.TrackStartEvent) {
	musicSession := s.MusicSession(event.GuildID().String(), "")

	err := musicSession.HandleTrackStart(&event.Track)
	if err != nil {
		slog.Error(err.Error())
	}
}

func (s *MusicService) onTrackEnd(player disgolink.Player, event lavalink.TrackEndEvent) {
	musicSession := s.MusicSession(event.GuildID().String(), "")

	err := musicSession.HandleTrackEnd(&event.Track)
	if err != nil {
		slog.Error(err.Error())
	}
}

func (s *MusicService) onTrackException(player disgolink.Player, event lavalink.TrackExceptionEvent) {
	// do something with the event
}

func (s *MusicService) onTrackStuck(player disgolink.Player, event lavalink.TrackStuckEvent) {
	// do something with the event
}

func (s *MusicService) onWebSocketClosed(player disgolink.Player, event lavalink.WebSocketClosedEvent) {
	// do something with the event
}
