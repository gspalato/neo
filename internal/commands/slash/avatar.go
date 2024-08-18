package slash

import (
	"os"

	"github.com/bwmarrin/discordgo"
	"github.com/zekrotja/ken"
	embedutils "unreal.sh/neo/internal/utils/embedutils"
)

type AvatarCommand struct{}

var (
	_ ken.Command            = (*AvatarCommand)(nil)
	_ ken.SlashCommand       = (*AvatarCommand)(nil)
	_ ken.GuildScopedCommand = (*AvatarCommand)(nil)
)

func (c *AvatarCommand) Name() string {
	return "avatar"
}

func (c *AvatarCommand) Description() string {
	return "Get the user's avatar."
}

func (c *AvatarCommand) Version() string {
	return "1.0.0"
}

func (c *AvatarCommand) Type() discordgo.ApplicationCommandType {
	return discordgo.ChatApplicationCommand
}

func (c *AvatarCommand) Options() []*discordgo.ApplicationCommandOption {
	return []*discordgo.ApplicationCommandOption{
		{
			Type:        discordgo.ApplicationCommandOptionUser,
			Name:        "user",
			Description: "The user to get the avatar from.",
			Required:    false,
		},
	}
}

func (c *AvatarCommand) Guild() string {
	return os.Getenv("MISFITS_GUILD_ID")
}

func (c *AvatarCommand) Run(ctx ken.Context) (err error) {
	if err = ctx.Defer(); err != nil {
		return err
	}

	var user *discordgo.User
	u, ok := ctx.Options().GetByNameOptional("user")
	if !ok {
		user = ctx.User()
	} else {
		user = u.UserValue(ctx)
	}

	embed := embedutils.CreateBasicEmbed("")
	embed.Author = &discordgo.MessageEmbedAuthor{
		Name:    user.Username,
		IconURL: user.AvatarURL("1024"),
	}
	embed.Image = &discordgo.MessageEmbedImage{
		URL: user.AvatarURL("1024"),
	}

	return ctx.FollowUpEmbed(embed).Send().Error
}
