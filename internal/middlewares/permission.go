package middlewares

import (
	"fmt"
	"strings"

	"github.com/bwmarrin/discordgo"
	"github.com/zekrotja/ken"

	sliceutils "unreal.sh/neo/internal/utils/sliceutils"
)

var (
	_ ken.MiddlewareBefore = (*PermissionsMiddleware)(nil)
)

// Represents an interface for commands that require all of the specified roles.
type RequiresRolesCommand interface {
	RequiresRoles() []string // Returns a list of role IDs that are required to execute the command.
}

// Represents an interface for commands that require at least one of the specified roles.
type RequiresAnyRolesCommand interface {
	RequiresAnyRoles() []string // Returns a list of role IDs where at least one is required to execute the command.
}

// Represents an interface for commands that require a specific permission to execute.
type RequiresPermissionCommand interface {
	RequiresPermission() int64 // Returns the permission integer that is required to execute the command.
}

type PermissionsMiddleware struct{}

func (c *PermissionsMiddleware) Before(ctx *ken.Ctx) (next bool, err error) {
	if cmd, ok := ctx.Command.(RequiresRolesCommand); ok {
		return c.handleRolesRequirement(ctx, cmd)
	} else if cmd, ok := ctx.Command.(RequiresAnyRolesCommand); ok {
		return c.handleAnyRolesRequirement(ctx, cmd)
	} else if cmd, ok := ctx.Command.(RequiresPermissionCommand); ok {
		return c.handlePermissionRequirement(ctx, cmd)
	}

	return true, nil
}

func (c *PermissionsMiddleware) handleRolesRequirement(ctx *ken.Ctx, cmd RequiresRolesCommand) (next bool, err error) {
	// Check if the user has the required roles.
	next = sliceutils.IsSubset(cmd.RequiresRoles(), ctx.GetEvent().Member.Roles)

	if !next {
		// Get the lacking roles
		lackingRoles := sliceutils.Difference(cmd.RequiresRoles(), ctx.GetEvent().Member.Roles)

		var description string
		if len(lackingRoles) == 1 {
			description = fmt.Sprintf("You must have the role <@&%s> to perform this command!", lackingRoles[0])
		} else {
			roleMentions := sliceutils.Map(lackingRoles, func(roleID string) string {
				return fmt.Sprintf("<@&%s>", roleID)
			})

			description = fmt.Sprintf(
				"You must have the following roles to perform this command: %s",
				strings.Join(roleMentions, ", "))
		}

		ctx.SetEphemeral(true)
		err = ctx.RespondEmbed(&discordgo.MessageEmbed{
			Title:       "🛑 Insufficient Permissions",
			Color:       0xCC5500,
			Description: description,
		})
	}

	return next, err
}

func (c *PermissionsMiddleware) handleAnyRolesRequirement(ctx *ken.Ctx, cmd RequiresAnyRolesCommand) (next bool, err error) {
	// Check if the user has at least one of the required roles.
	next = sliceutils.IsAnySubset(cmd.RequiresAnyRoles(), ctx.GetEvent().Member.Roles)

	if !next {
		// Get the possible roles
		roleMentions := sliceutils.Map(cmd.RequiresAnyRoles(), func(roleID string) string {
			return fmt.Sprintf("<@&%s>", roleID)
		})

		description := fmt.Sprintf(
			"You must have at least one of the following roles to perform this command: %s",
			strings.Join(roleMentions, ", "))

		ctx.SetEphemeral(true)
		err = ctx.RespondEmbed(&discordgo.MessageEmbed{
			Title:       "🛑 Insufficient Permissions",
			Color:       0xCC5500,
			Description: description,
		})
	}

	return next, err
}

func (c *PermissionsMiddleware) handlePermissionRequirement(ctx *ken.Ctx, cmd RequiresPermissionCommand) (next bool, err error) {
	perms, err := ctx.GetSession().UserChannelPermissions(ctx.GetEvent().Member.User.ID, ctx.GetEvent().ChannelID)
	if err != nil {
		return false, err
	}

	requiredPermission := cmd.RequiresPermission()

	// Check if the user has the required permission using bitwise AND.
	next = perms&requiredPermission == requiredPermission
	if !next {
		ctx.SetEphemeral(true)
		err = ctx.RespondEmbed(&discordgo.MessageEmbed{
			Title:       "🛑 Insufficient Permissions",
			Color:       0xCC5500,
			Description: "You don't have the required permissions to perform this command!",
		})
	}

	return next, nil
}
