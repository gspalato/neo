package slash

import (
	"fmt"
	"os"

	"github.com/bwmarrin/discordgo"
	"github.com/zekrotja/ken"
	"unreal.sh/neo/internal/middlewares"

	embedutils "unreal.sh/neo/internal/utils/embedutils"
)

type KickCommand struct{}

var (
	_ ken.SlashCommand       = (*KickCommand)(nil)
	_ ken.GuildScopedCommand = (*KickCommand)(nil)

	_ middlewares.RequiresPermissionCommand = (*KickCommand)(nil)
)

func (c *KickCommand) Name() string {
	return "kick"
}

func (c *KickCommand) Description() string {
	return "Kicks a member from the server."
}

func (c *KickCommand) Version() string {
	return "1.0.0"
}

func (c *KickCommand) RequiresPermission() int64 {
	return discordgo.PermissionKickMembers
}

func (c *KickCommand) Type() discordgo.ApplicationCommandType {
	return discordgo.ChatApplicationCommand
}

func (c *KickCommand) Options() []*discordgo.ApplicationCommandOption {
	return []*discordgo.ApplicationCommandOption{
		{
			Type:        discordgo.ApplicationCommandOptionUser,
			Name:        "user",
			Description: "The victim.",
			Required:    true,
		},
		{
			Type:        discordgo.ApplicationCommandOptionString,
			Name:        "reason",
			Description: "The reason for the kick.",
			Required:    false,
		},
	}
}

func (c *KickCommand) Guild() string {
	return os.Getenv("MISFITS_GUILD_ID")
}

func (c *KickCommand) Run(ctx ken.Context) (err error) {
	if err = ctx.Defer(); err != nil {
		return
	}

	session := ctx.GetSession()

	guild, err := ctx.Guild()

	user := ctx.Options().GetByName("user").UserValue(ctx)

	reason := "No reason provided."
	reasonArg, ok := ctx.Options().GetByNameOptional("reason")
	if ok {
		reason = reasonArg.StringValue()
	}

	// Create prompt
	embed := embedutils.CreatePromptEmbed("Do you really want to kick them?")
	embed.Fields = []*discordgo.MessageEmbedField{
		{
			Name:   "User",
			Value:  user.Mention(),
			Inline: true,
		},
		{
			Name:   "Reason",
			Value:  fmt.Sprintf("```%s```", reason),
			Inline: true,
		},
	}

	prompt := ctx.FollowUpEmbed(embed)

	prompt.AddComponents(func(cb *ken.ComponentBuilder) {
		cb.Add(discordgo.Button{
			Label:    "Yes",
			Style:    discordgo.DangerButton,
			CustomID: "kick_confirm",
		}, func(ctx ken.ComponentContext) bool {
			err = session.GuildMemberDeleteWithReason(guild.ID, user.ID, reason)
			if err != nil {
				embed = embedutils.CreateErrorEmbed("Failed to kick user.")
				ctx.FollowUpEmbed(embed).Send()
				return false
			}

			embed = embedutils.CreateSuccessEmbed(
				fmt.Sprintf("Kicked %s for `%s`", user.String(), reason),
			)

			ctx.FollowUpEmbed(embed).Send()

			return true
		}, true)

		cb.Add(discordgo.Button{
			Label:    "No",
			Style:    discordgo.SecondaryButton,
			CustomID: "kick_cancel",
		}, func(ctx ken.ComponentContext) bool {
			ctx.FollowUpMessage("They got away, this time.").Send()

			return true
		}, true)
	})

	msg := prompt.Send()
	if msg.Error != nil {
		return msg.Error
	}

	return nil
}
