// This module system overwrites the guild's commands, ignoring the first time they're registered by Ken.
// This isn't a good practice and can lead to unexpected behavior and bugs.
// The module system _should_ be refactored to work with Ken's command registration system.
// But will I do it? Nah, too much trouble, and this works for now.

package modules

import (
	"fmt"
	"log/slog"

	"github.com/bwmarrin/discordgo"
	"github.com/zekrotja/ken"
	"unreal.sh/neo/internal/database"
)

//go:generate go run ../../../tools/cmd/module_events/main.go

// Module represents a module of the bot.
// A module is a collection of commands, middlewares and event handlers.
//
// Guilds can enable or disable modules to restrict the usage of commands and features.
type Module interface {
	// ID of the module.
	ID() string

	// Name of the module.
	Name() string

	// Description of the module.
	Description() string

	// Version of the module.
	Version() string

	// If the module is global, it will be enabled for all guilds.
	IsGlobal() bool

	// Commands of the module.
	Commands() *[]ken.Command

	// Middlewares of the module.
	Middlewares() []ken.Middleware

	// EventHandlers of the module.
	// These should be exactly like the example given by discordgo.
	EventHandlers() []interface{}
}

// ModuleManager manages the modules of the bot.
type ModuleManager struct {
	session *discordgo.Session
	db      *database.Database

	// Modules maps module IDs to modules.
	modules map[string]Module

	// GuildModules maps guild IDs to module IDs.
	GuildModules map[string][]string

	// GlobalModules maps module IDs to global state.
	GlobalModules map[string]bool
}

// NewModuleManager creates a new instance of ModuleManager.
func NewModuleManager(session *discordgo.Session, db *database.Database) *ModuleManager {
	return &ModuleManager{
		session: session,
		db:      db,

		modules: make(map[string]Module),

		GuildModules:  make(map[string][]string),
		GlobalModules: make(map[string]bool),
	}
}

func (m *ModuleManager) Initialize() {
	// Load each guild's enabled modules from database.
	for _, guild := range m.session.State.Guilds {
		settings, _, err := m.db.GetOrCreateGuildSettings(guild.ID)
		if err != nil {
			slog.Error("Failed to get guild settings.", slog.String("guild_id", guild.ID), slog.String("error", err.Error()))
			continue
		}

		slog.Info("Loaded guild settings.", slog.String("guild_id", guild.ID), slog.String("json", settings.String()))

		m.GuildModules[guild.ID] = settings.EnabledModules
		go m.ReloadGuildCommands(guild.ID)
	}

	// Load commands from global modules.
	m.ReloadGlobalCommands()
}

func (m *ModuleManager) GetAllKenCommands() []ken.Command {
	cmds := make([]ken.Command, 0)

	for _, module := range m.modules {
		cmds = append(cmds, *module.Commands()...)
	}

	return cmds
}

// RegisterModule registers a module.
func (m *ModuleManager) RegisterModules(modules ...Module) {
	for _, module := range modules {
		m.modules[module.ID()] = module

		if module.IsGlobal() {
			m.GlobalModules[module.ID()] = true
		}
	}
}

// GetModule returns a module by its ID.
func (m *ModuleManager) GetModule(id string) (Module, bool) {
	module, exists := m.modules[id]
	return module, exists
}

func (m *ModuleManager) Modules() []Module {
	modules := make([]Module, 0)
	for _, module := range m.modules {
		modules = append(modules, module)
	}
	return modules
}

// EnableModule enables a module for a guild. This can be ran as a goroutine.
// An empty guildID will enable the module globally.
// To apply changes to the command list, call ReloadGuildCommands or ReloadGlobalCommands after this.
func (m *ModuleManager) EnableModule(moduleID string, guildID string) error {
	if guildID == "" {
		m.GlobalModules[moduleID] = true
		slog.Info("Enabling module globally.", slog.String("module_id", moduleID))
		return nil
	}

	slog.Info("Enabling module for guild.", slog.String("module_id", moduleID), slog.String("guild_id", guildID))

	s, err := m.db.EnableModule(guildID, moduleID)
	if err != nil {
		return err
	}

	m.GuildModules[guildID] = s.EnabledModules

	return nil
}

// EnableModules enables multiple modules for a guild. This can be ran as a goroutine.
// An empty guildID will enable the modules globally.
// To apply changes to the command list, call ReloadGuildCommands or ReloadGlobalCommands after this.
func (m *ModuleManager) EnableModules(guildID string, moduleIDs ...string) []error {
	errs := make([]error, 0)
	for _, moduleID := range moduleIDs {
		err := m.EnableModule(moduleID, guildID)
		if err != nil {
			errs = append(errs, err)
		}
	}

	return errs
}

// DisableModule disables a module for a guild.
// An empty guildID will disable the module globally.
// To apply changes to the command list, call ReloadGuildCommands or ReloadGlobalCommands after this.
func (m *ModuleManager) DisableModule(moduleID string, guildID string) error {
	if guildID == "" {
		delete(m.GlobalModules, moduleID)
		return nil
	}

	s, err := m.db.DisableModule(guildID, moduleID)
	if err != nil {
		return err
	}

	m.GuildModules[guildID] = s.EnabledModules

	return nil
}

func (m *ModuleManager) ReloadGlobalCommands() error {
	cmds := make([]*discordgo.ApplicationCommand, 0)

	for _, module := range m.GetGlobalModules() {
		for _, command := range *module.Commands() {
			cmds = append(cmds, toApplicationCommand(command))
		}
	}

	_, err := m.session.ApplicationCommandBulkOverwrite(m.session.State.User.ID, "", cmds)
	if err != nil {
		slog.Error("Failed to bulk overwrite global commands.", slog.String("error", err.Error()))
		return err
	}

	return nil
}

// ReloadGuildCommands loads all commands of enabled modules for a guild.
func (m *ModuleManager) ReloadGuildCommands(guildID string) error {
	cmds := make([]*discordgo.ApplicationCommand, 0)

	modules := m.GetEnabledModules(guildID)

	for _, module := range modules {
		for _, command := range *module.Commands() {
			cmds = append(cmds, toApplicationCommand(command))
		}
	}

	slog.Info("Reloading guild commands.", slog.String("guild_id", guildID), slog.Int("count", len(cmds)))

	_, err := m.session.ApplicationCommandBulkOverwrite(m.session.State.User.ID, guildID, cmds)
	if err != nil {
		slog.Error("Failed to bulk overwrite guild commands.",
			slog.String("guild_id", guildID), slog.String("error", err.Error()))
		return err
	}

	return nil
}

// IsModuleEnabled checks if a module is enabled for a guild.
func (m *ModuleManager) IsModuleEnabled(guildID string, moduleID string) bool {
	if guildID == "" {
		return m.GlobalModules[moduleID]
	}

	modules, ok := m.GuildModules[guildID]
	if !ok {
		return false
	}

	for _, id := range modules {
		if id == moduleID {
			return true
		}
	}

	return false
}

// GetEnabledModules returns a list of enabled modules for a guild.
func (m *ModuleManager) GetEnabledModules(guildID string) []Module {
	modules := make([]Module, 0)

	for _, moduleID := range m.GuildModules[guildID] {
		modules = append(modules, m.modules[moduleID])
	}

	return modules
}

// GetGlobalModules returns a list of enabled global modules.
func (m *ModuleManager) GetGlobalModules() []Module {
	modules := make([]Module, 0)

	for moduleID, enabled := range m.GlobalModules {
		if enabled {
			modules = append(modules, m.modules[moduleID])
		}
	}

	return modules
}

func toApplicationCommand(c ken.Command) *discordgo.ApplicationCommand {
	switch cm := c.(type) {
	case ken.UserCommand:
		return &discordgo.ApplicationCommand{
			Name: cm.Name(),
			Type: discordgo.UserApplicationCommand,
		}
	case ken.MessageCommand:
		return &discordgo.ApplicationCommand{
			Name: cm.Name(),
			Type: discordgo.MessageApplicationCommand,
		}
	case ken.SlashCommand:
		return &discordgo.ApplicationCommand{
			Name:        cm.Name(),
			Type:        discordgo.ChatApplicationCommand,
			Description: cm.Description(),
			Version:     cm.Version(),
			Options:     cm.Options(),
		}
	default:
		panic(fmt.Sprintf("Command type not implemented for command: %s", cm.Name()))
	}
}
