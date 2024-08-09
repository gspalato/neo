package slash

import (
	"fmt"
	"os"

	"github.com/bwmarrin/discordgo"
	"github.com/zekrotja/ken"
	"unreal.sh/neo/internal/middlewares"
	embedutils "unreal.sh/neo/internal/utils/embedutils"
)

type BanCommand struct{}

var (
	_ ken.SlashCommand       = (*BanCommand)(nil)
	_ ken.GuildScopedCommand = (*BanCommand)(nil)

	_ middlewares.RequiresPermissionCommand = (*BanCommand)(nil)
)

func (c *BanCommand) Name() string {
	return "ban"
}

func (c *BanCommand) Description() string {
	return "Bans a member from the server."
}

func (c *BanCommand) Version() string {
	return "1.0.0"
}

func (c *BanCommand) RequiresPermission() int64 {
	return discordgo.PermissionBanMembers
}

func (c *BanCommand) Type() discordgo.ApplicationCommandType {
	return discordgo.ChatApplicationCommand
}

func (c *BanCommand) Options() []*discordgo.ApplicationCommandOption {
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
			Description: "The reason for the ban.",
			Required:    false,
		},
		{
			Type:        discordgo.ApplicationCommandOptionInteger,
			Name:        "days",
			Description: "The amount of days of messages to delete.",
			Required:    false,
		},
	}
}

func (c *BanCommand) Guild() string {
	return os.Getenv("MISFITS_GUILD_ID")
}

func (c *BanCommand) Run(ctx ken.Context) (err error) {
	if err = ctx.Defer(); err != nil {
		return
	}

	session := ctx.GetSession()

	guild, err := ctx.Guild()

	user := ctx.Options().GetByName("user").UserValue(ctx)

	reason := "No reason provided."
	reasonArg, hasReasonArg := ctx.Options().GetByNameOptional("reason")
	if hasReasonArg {
		reason = reasonArg.StringValue()
	}

	days := 7
	daysArg, hasDaysArg := ctx.Options().GetByNameOptional("days")
	if hasDaysArg {
		days = int(daysArg.IntValue())
	}

	// Create prompt
	embed := embedutils.CreatePromptEmbed("Do you really want to ban them?")
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
			CustomID: "ban_confirm",
		}, func(ctx ken.ComponentContext) bool {
			err = session.GuildBanCreateWithReason(guild.ID, user.ID, reason, days)
			if err != nil {
				embed = embedutils.CreateErrorEmbed("Failed to ban user.")
				ctx.FollowUpEmbed(embed).Send()
				return false
			}

			embed = embedutils.CreateSuccessEmbed(
				fmt.Sprintf("Banned %s for `%s`", user.String(), reason),
			)

			ctx.FollowUpEmbed(embed).Send()

			return true
		}, true)

		cb.Add(discordgo.Button{
			Label:    "No",
			Style:    discordgo.SecondaryButton,
			CustomID: "ban_cancel",
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
