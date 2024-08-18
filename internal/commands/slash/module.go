package slash

import (
	"errors"
	"fmt"
	"os"

	"github.com/bwmarrin/discordgo"
	"github.com/zekrotja/ken"

	"unreal.sh/neo/internal/middlewares"
	"unreal.sh/neo/internal/services/modules"
	embedutils "unreal.sh/neo/internal/utils/embedutils"
	sliceutils "unreal.sh/neo/internal/utils/sliceutils"
)

type ModuleCommand struct{}

var (
	_ ken.Command            = (*ModuleCommand)(nil)
	_ ken.SlashCommand       = (*ModuleCommand)(nil)
	_ ken.GuildScopedCommand = (*ModuleCommand)(nil)

	_ middlewares.RequiresPermissionCommand = (*ModuleCommand)(nil)
)

func (c *ModuleCommand) Name() string {
	return "module"
}

func (c *ModuleCommand) Description() string {
	return "Allows you to enable or disable certain bot modules."
}

func (c *ModuleCommand) Version() string {
	return "1.0.0"
}

func (c *ModuleCommand) RequiresPermission() int64 {
	return discordgo.PermissionAdministrator
}

func (c *ModuleCommand) Type() discordgo.ApplicationCommandType {
	return discordgo.ChatApplicationCommand
}

func (c *ModuleCommand) Options() []*discordgo.ApplicationCommandOption {
	return []*discordgo.ApplicationCommandOption{
		{
			Type:        discordgo.ApplicationCommandOptionSubCommand,
			Name:        "enable",
			Description: "Enables a module.",
			Options: []*discordgo.ApplicationCommandOption{
				{
					Type:        discordgo.ApplicationCommandOptionString,
					Name:        "module",
					Description: "The module to enable.",
					Required:    true,
				},
			},
		},
		{
			Type:        discordgo.ApplicationCommandOptionSubCommand,
			Name:        "disable",
			Description: "Disables a module.",
			Options: []*discordgo.ApplicationCommandOption{
				{
					Type:        discordgo.ApplicationCommandOptionString,
					Name:        "module",
					Description: "The module to disable.",
					Required:    true,
				},
			},
		},
		{
			Type:        discordgo.ApplicationCommandOptionSubCommand,
			Name:        "get",
			Description: "Gets a list of modules.",
		},
	}
}

func (c *ModuleCommand) Guild() string {
	return os.Getenv("MISFITS_GUILD_ID")
}

func (c *ModuleCommand) Run(ctx ken.Context) (err error) {
	err = ctx.HandleSubCommands(
		ken.SubCommandHandler{"enable", c.enable},
		ken.SubCommandHandler{"disable", c.disable},
		ken.SubCommandHandler{"get", c.get},
	)

	return
}

func (c *ModuleCommand) enable(ctx ken.SubCommandContext) error {
	if err := ctx.Defer(); err != nil {
		return err
	}

	manager := ctx.Get("ModuleManager").(*modules.ModuleManager)
	if manager == nil {
		return errors.New("failed to get ModuleManager")
	}

	moduleName := ctx.Options().GetByName("module").StringValue()
	_, exists := manager.GetModule(moduleName)
	if !exists {
		ctx.FollowUpMessage("Module doesn't exist.")
	}

	err := manager.EnableModule(moduleName, ctx.GetEvent().GuildID)
	if err != nil {
		return err
	}

	err = manager.ReloadGuildCommands(ctx.GetEvent().GuildID)
	if err != nil {
		return err
	}

	ctx.FollowUpMessage(fmt.Sprintf("Enabled module `%s`.", moduleName)).Send()

	return nil
}

func (c *ModuleCommand) disable(ctx ken.SubCommandContext) error {
	if err := ctx.Defer(); err != nil {
		return err
	}

	manager := ctx.Get("ModuleManager").(*modules.ModuleManager)
	if manager == nil {
		return errors.New("failed to get ModuleManager")
	}

	moduleName := ctx.Options().GetByName("module").StringValue()
	_, exists := manager.GetModule(moduleName)
	if !exists {
		ctx.FollowUpMessage("Module doesn't exist.")
	}

	err := manager.DisableModule(moduleName, ctx.GetEvent().GuildID)
	if err != nil {
		return err
	}

	err = manager.ReloadGuildCommands(ctx.GetEvent().GuildID)
	if err != nil {
		return err
	}

	ctx.FollowUpMessage(fmt.Sprintf("Disabled module `%s`.", moduleName)).Send()

	return nil
}

func (c *ModuleCommand) get(ctx ken.SubCommandContext) (err error) {
	if err := ctx.Defer(); err != nil {
		return err
	}

	manager := ctx.Get("ModuleManager").(*modules.ModuleManager)
	if manager == nil {
		return errors.New("failed to get ModuleManager")
	}

	allModules := manager.Modules()

	allModuleIDs := sliceutils.Map(allModules, func(module modules.Module) string {
		return module.ID()
	})

	enabledModuleIDs := sliceutils.Map(manager.GetEnabledModules(ctx.GetEvent().GuildID), func(module modules.Module) string {
		return module.ID()
	})

	disabledModuleIDs := sliceutils.Difference(allModuleIDs, enabledModuleIDs)

	var enabled string
	for _, id := range enabledModuleIDs {
		module, exists := manager.GetModule(id)
		if !exists {
			continue
		}

		enabled += fmt.Sprintf("• **%s** (`%s`)\n", module.Name(), id)
	}

	var disabled string
	for _, id := range disabledModuleIDs {
		module, exists := manager.GetModule(id)
		if !exists {
			continue
		}

		disabled += fmt.Sprintf("• **%s** (`%s`)\n", module.Name(), id)
	}

	embed := embedutils.CreateBasicEmbed("")
	embed.Title = "📦  **Modules**"
	embed.Fields = []*discordgo.MessageEmbedField{
		{
			Name:   "Enabled",
			Value:  enabled,
			Inline: false,
		},
	}

	if len(disabledModuleIDs) > 0 {
		embed.Fields = append(embed.Fields, &discordgo.MessageEmbedField{
			Name:   "More modules",
			Value:  disabled,
			Inline: false,
		})
	}

	ctx.FollowUpEmbed(embed).Send()

	return nil
}
