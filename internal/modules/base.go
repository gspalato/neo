package modules

import (
	"github.com/bwmarrin/discordgo"
	"github.com/zekrotja/ken"

	"unreal.sh/neo/internal/commands/slash"
	mod "unreal.sh/neo/internal/services/modules"
)

var (
	_ mod.Module = (*BaseModule)(nil)
)

type BaseModule struct{}

func (m *BaseModule) ID() string {
	return "base"
}

func (m *BaseModule) Name() string {
	return "🚀 Base"
}

func (m *BaseModule) Description() string {
	return "A module that provides basic commands!"
}

func (m *BaseModule) Version() string {
	return "1.0.0"
}

func (m *BaseModule) IsGlobal() bool {
	return false
}

func (m *BaseModule) Commands() *[]ken.Command {
	return &[]ken.Command{
		new(slash.AvatarCommand),
		new(slash.ModuleCommand),
		new(slash.PingCommand),
		new(slash.WhoIsCommand),
	}
}

func (m *BaseModule) Middlewares() []ken.Middleware {
	return []ken.Middleware{}
}

func (m *BaseModule) EventHandlers() []discordgo.EventHandler {
	return []discordgo.EventHandler{}
}
