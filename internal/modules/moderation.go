package modules

import (
	"log/slog"

	"github.com/bwmarrin/discordgo"
	"github.com/zekrotja/ken"

	"unreal.sh/neo/internal/commands/slash"
	mod "unreal.sh/neo/internal/services/modules"
)

var (
	_ mod.Module = (*ModerationModule)(nil)
)

type ModerationModule struct{}

func (m *ModerationModule) ID() string {
	return "moderation"
}

func (m *ModerationModule) Name() string {
	return "\\🛡️ Moderation"
}

func (m *ModerationModule) Description() string {
	return "A module that provides music commands and functionality!"
}

func (m *ModerationModule) Version() string {
	return "1.0.0"
}

func (m *ModerationModule) IsGlobal() bool {
	return false
}

func (m *ModerationModule) Commands() *[]ken.Command {
	return &[]ken.Command{
		new(slash.BanCommand),
		new(slash.KickCommand),
		new(slash.PurgeCommand),
		new(slash.SoftbanCommand),
	}
}

func (m *ModerationModule) Middlewares() []ken.Middleware {
	return []ken.Middleware{}
}

func (m *ModerationModule) EventHandlers() []interface{} {
	return []interface{}{
		func(session *discordgo.Session, handler *discordgo.MessageCreate) {
			slog.Info("Message received! TEST SUCCEEDED!")
		},
	}
}
