package slash

import (
	"fmt"
	"os"

	"github.com/bwmarrin/discordgo"
	"github.com/zekrotja/ken"
	"unreal.sh/neo/internal/middlewares"
	embedutils "unreal.sh/neo/internal/utils/embedutils"
)

type SoftbanCommand struct{}

var (
	_ ken.SlashCommand       = (*SoftbanCommand)(nil)
	_ ken.GuildScopedCommand = (*SoftbanCommand)(nil)

	_ middlewares.RequiresPermissionCommand = (*BanCommand)(nil)
)

func (c *SoftbanCommand) Name() string {
	return "softban"
}

func (c *SoftbanCommand) Description() string {
	return "Bans and unbans a member from the server."
}

func (c *SoftbanCommand) Version() string {
	return "1.0.0"
}

func (c *SoftbanCommand) RequiresPermission() int64 {
	return discordgo.PermissionBanMembers
}

func (c *SoftbanCommand) Type() discordgo.ApplicationCommandType {
	return discordgo.ChatApplicationCommand
}

func (c *SoftbanCommand) Options() []*discordgo.ApplicationCommandOption {
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
	}
}

func (c *SoftbanCommand) Guild() string {
	return os.Getenv("MISFITS_GUILD_ID")
}

func (c *SoftbanCommand) Run(ctx ken.Context) (err error) {
	if err = ctx.Defer(); err != nil {
		return
	}

	session := ctx.GetSession()
	ctx.SetEphemeral(true)

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
	embed := embedutils.CreatePromptEmbed("Do you really want to softban them?")
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
		cb.Add(
			discordgo.Button{
				Label:    "Yes",
				Style:    discordgo.DangerButton,
				CustomID: "softban_confirm",
			},
			func(ctx ken.ComponentContext) bool {
				err = session.GuildBanCreateWithReason(guild.ID, user.ID, reason, days)
				if err != nil {
					embed = embedutils.CreateErrorEmbed("Failed to softban user.")
					ctx.FollowUpEmbed(embed).Send()
					return false
				}

				err = session.GuildBanDelete(guild.ID, user.ID)
				if err != nil {
					embed = embedutils.CreateErrorEmbed("Failed to softban user.")
					ctx.FollowUpEmbed(embed).Send()
					return false
				}

				embed = embedutils.CreateSuccessEmbed(
					fmt.Sprintf("Softbanned %s for `%s`", user.String(), reason),
				)

				ctx.FollowUpEmbed(embed).Send()

				return true
			},
			true,
		).Condition(func(cctx ken.ComponentContext) bool {
			return cctx.User().ID == ctx.User().ID
		})

		cb.Add(
			discordgo.Button{
				Label:    "No",
				Style:    discordgo.SecondaryButton,
				CustomID: "softban_cancel",
			},
			func(ctx ken.ComponentContext) bool {
				ctx.FollowUpMessage("They got away, this time.").Send()

				return true
			},
			true,
		).Condition(func(cctx ken.ComponentContext) bool {
			return cctx.User().ID == ctx.User().ID
		})
	})

	msg := prompt.Send()
	if msg.Error != nil {
		return msg.Error
	}

	return nil
}
