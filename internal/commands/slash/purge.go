package slash

import (
	"fmt"
	"os"

	"github.com/bwmarrin/discordgo"
	"github.com/zekrotja/ken"

	"unreal.sh/neo/internal/middlewares"
	sliceutils "unreal.sh/neo/internal/utils/sliceutils"
	"unreal.sh/neo/internal/utils/static"
)

type PurgeCommand struct{}

var (
	_ ken.SlashCommand       = (*PurgeCommand)(nil)
	_ ken.GuildScopedCommand = (*PurgeCommand)(nil)

	_ middlewares.RequiresPermissionCommand = (*PurgeCommand)(nil)
)

func (c *PurgeCommand) Name() string {
	return "purge"
}

func (c *PurgeCommand) Description() string {
	return "Purge a certain amount of messages."
}

func (c *PurgeCommand) Version() string {
	return "1.0.0"
}

func (c *PurgeCommand) RequiresPermission() int64 {
	return discordgo.PermissionManageMessages
}

func (c *PurgeCommand) Type() discordgo.ApplicationCommandType {
	return discordgo.ChatApplicationCommand
}

func (c *PurgeCommand) Options() []*discordgo.ApplicationCommandOption {
	return []*discordgo.ApplicationCommandOption{
		{
			Type:        discordgo.ApplicationCommandOptionInteger,
			Name:        "amount",
			Description: "The amount of messages to purge.",
			Required:    true,
		},
		{
			Type:        discordgo.ApplicationCommandOptionChannel,
			Name:        "channel",
			Description: "The channel to purge messages from. If not specified, the current channel will be used.",
			Required:    false,
		},
		{
			Type:        discordgo.ApplicationCommandOptionUser,
			Name:        "user",
			Description: "The user to purge messages from. If not specified, all messages will be purged.",
			Required:    false,
		},
	}
}

func (c *PurgeCommand) Guild() string {
	return os.Getenv("MISFITS_GUILD_ID")
}

func (c *PurgeCommand) Run(ctx ken.Context) (err error) {
	if err = ctx.Defer(); err != nil {
		return
	}

	channel, err := ctx.Channel()
	if err != nil {
		return err
	}

	channelArg, hasChannelArg := ctx.Options().GetByNameOptional("channel")
	if hasChannelArg {
		channel = channelArg.ChannelValue(ctx)
	}

	userArg, hasUserArg := ctx.Options().GetByNameOptional("user")

	amount := ctx.Options().GetByName("amount").IntValue()
	msgs, err := ctx.GetSession().ChannelMessages(channel.ID, int(amount), "", "", "")
	if err != nil {
		return err
	}

	msgIDs := sliceutils.Map(msgs, func(msg *discordgo.Message) string {
		return msg.ID
	})

	// If a user is specified, filter out messages from other users.
	if hasUserArg {
		user := userArg.UserValue(ctx)
		msgIDs = sliceutils.Filter(msgIDs, func(msgID string) bool {
			msg, err := ctx.GetSession().ChannelMessage(channel.ID, msgID)
			if err != nil {
				return false
			}

			return msg.Author.ID == user.ID
		})
	}

	err = ctx.GetSession().ChannelMessagesBulkDelete(channel.ID, msgIDs)
	if err != nil {
		ctx.RespondEmbed(&discordgo.MessageEmbed{
			Title: "⚠ Error",
			Color: static.ColorEmbedOrange,
			Description: `Failed to purge messages.
				Make sure the messages are not older than 14 days and that the bot has the required permissions.`,
		})
		return nil
	}

	ctx.SetEphemeral(true)
	err = ctx.RespondEmbed(&discordgo.MessageEmbed{
		Title:       "✅ Success",
		Color:       0x00FF00,
		Description: fmt.Sprintf("Successfully purged messages **%d** messages.", len(msgIDs)),
	})

	if err != nil {
		return err
	}

	return nil
}
