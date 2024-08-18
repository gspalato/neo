package modules

import (
	"github.com/bwmarrin/discordgo"
	"github.com/zekrotja/ken"

	"unreal.sh/neo/internal/commands/slash"
	mod "unreal.sh/neo/internal/services/modules"
)

var (
	_ mod.Module = (*MusicModule)(nil)
)

type MusicModule struct{}

func (m *MusicModule) ID() string {
	return "music"
}

func (m *MusicModule) Name() string {
	return "🎧 Music"
}

func (m *MusicModule) Description() string {
	return "A module that provides music commands and functionality!"
}

func (m *MusicModule) Version() string {
	return "1.0.0"
}

func (m *MusicModule) IsGlobal() bool {
	return false
}

func (m *MusicModule) Commands() *[]ken.Command {
	return &[]ken.Command{
		new(slash.NowPlayingCommand),
		new(slash.PauseCommand),
		new(slash.PlayCommand),
		new(slash.QueueCommand),
		new(slash.ResumeCommand),
		new(slash.SkipCommand),
		new(slash.StopCommand),
		new(slash.VolumeCommand),
	}
}

func (m *MusicModule) Middlewares() []ken.Middleware {
	return []ken.Middleware{}
}

func (m *MusicModule) EventHandlers() []discordgo.EventHandler {
	return []discordgo.EventHandler{}
}
