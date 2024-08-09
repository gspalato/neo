package snipe

import "github.com/bwmarrin/discordgo"

type SnipeService struct {
	session *discordgo.Session

	snipes map[string]*discordgo.Message // channelID -> *discordgo.Message
}

func NewSnipeService(session *discordgo.Session) *SnipeService {
	return &SnipeService{
		session: session,

		snipes: make(map[string]*discordgo.Message),
	}
}

func (s *SnipeService) GetLatestMessageFromChannel(channelID string) (msg *discordgo.Message, exists bool) {
	msg, exists = s.snipes[channelID]
	return msg, exists
}

func (s *SnipeService) HookEvents() {
	s.session.AddHandler(s.HandleMessageDelete)
}

func (s *SnipeService) HandleMessageDelete(_ *discordgo.Session, m *discordgo.MessageDelete) {
	msg := m.BeforeDelete
	s.snipes[msg.ChannelID] = msg
}
