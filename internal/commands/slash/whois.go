package slash

import (
	"fmt"
	"log/slog"
	"os"
	"slices"
	"strings"

	"golang.org/x/text/cases"
	"golang.org/x/text/language"

	"github.com/bwmarrin/discordgo"
	"github.com/zekrotja/ken"

	discordutil "unreal.sh/neo/internal/utils/discordutils"
	"unreal.sh/neo/internal/utils/static"
	stringutil "unreal.sh/neo/internal/utils/stringutils"
)

type WhoIsCommand struct {
	ken.EphemeralCommand
}

var (
	_ ken.Command            = (*WhoIsCommand)(nil)
	_ ken.SlashCommand       = (*WhoIsCommand)(nil)
	_ ken.GuildScopedCommand = (*WhoIsCommand)(nil)
)

func (c *WhoIsCommand) Name() string {
	return "whois"
}

func (c *WhoIsCommand) Description() string {
	return "Get information about a user."
}

func (c *WhoIsCommand) Version() string {
	return "1.0.0"
}

func (c *WhoIsCommand) Type() discordgo.ApplicationCommandType {
	return discordgo.ChatApplicationCommand
}

func (c *WhoIsCommand) Options() []*discordgo.ApplicationCommandOption {
	return []*discordgo.ApplicationCommandOption{
		{
			Type:        discordgo.ApplicationCommandOptionUser,
			Name:        "user",
			Description: "The user to be displayed.",
			Required:    true,
		},
	}
}

func (c *WhoIsCommand) Guild() string {
	return os.Getenv("MISFITS_GUILD_ID")
}

func (c *WhoIsCommand) Run(ctx ken.Context) error {
	if err := ctx.Defer(); err != nil {
		return nil
	}

	var user *discordgo.User

	if resolved := ctx.GetEvent().ApplicationCommandData().Resolved; resolved != nil {
		for _, user = range ctx.GetEvent().ApplicationCommandData().Resolved.Users {
			break
		}
	}

	if user == nil {
		user = ctx.Options().GetByName("user").UserValue(ctx)
	}

	guild, err := ctx.GetSession().Guild(ctx.GetEvent().GuildID)
	if err != nil {
		return nil
	}

	member, err := ctx.GetSession().GuildMember(ctx.GetEvent().GuildID, user.ID)
	if err != nil {
		return nil
	}

	membRoleIDs := make(map[string]struct{})
	for _, rID := range member.Roles {
		membRoleIDs[rID] = struct{}{}
	}

	roles := make([]string, len(member.Roles))
	for i, rID := range member.Roles {
		roles[i] = "<@&" + rID + ">"
	}
	slices.Reverse(roles)

	maxPos := len(guild.Roles)
	roleColor := static.ColorEmbedGray
	for _, guildRole := range guild.Roles {
		if _, ok := membRoleIDs[guildRole.ID]; ok && guildRole.Position > maxPos && guildRole.Color != 0 {
			maxPos = guildRole.Position
			roleColor = guildRole.Color
		}
	}

	//createdTime, err := discordutil.GetDiscordSnowflakeCreationTime(member.User.ID)
	//if err != nil {
	//	return err
	//}

	var nameField string
	if member.Nick != "" {
		nameField += fmt.Sprintf("%s (%s)", member.Nick, member.User.Username)
	} else {
		nameField = member.User.Username
	}

	discriminatorField := "None"
	if member.User.Discriminator != "0" {
		discriminatorField = fmt.Sprintf("#%s", member.User.Discriminator)
	}

	botField := "No"
	if member.User.Bot {
		botField = "Yes"
	}

	presence, err := ctx.GetSession().State.Presence(member.GuildID, member.User.ID)
	if err != nil {
		return err
	}

	// If bot doesn't have the presence, just set it to offline.
	if presence == nil {
		slog.Info(fmt.Sprintf("Presence not found for user %s", member.User.ID))
		presence = &discordgo.Presence{
			Status: discordgo.StatusOffline,
		}
	}

	customStatus := ""
	customStatusActivity := discordutil.GetCustomStatus(presence)
	if customStatusActivity != nil {
		customStatus = fmt.Sprintf("> %s", customStatusActivity.State)
	}

	avatarUrl := member.User.AvatarURL("256")

	embed := &discordgo.MessageEmbed{
		Color: roleColor,
		Title: member.DisplayName(),
		//Author: &discordgo.MessageEmbedAuthor{
		//	IconURL: avatarUrl,
		//	Name:    member.DisplayName(),
		//},
		Thumbnail: &discordgo.MessageEmbedThumbnail{
			URL:    avatarUrl,
			Height: 256,
			Width:  256,
		},
		Description: customStatus,
		Fields: []*discordgo.MessageEmbedField{
			{
				Name:   "Name",
				Value:  nameField,
				Inline: true,
			},
			{
				Name:   "Discriminator",
				Value:  discriminatorField,
				Inline: true,
			},
			{
				Name:   "ID",
				Value:  member.User.ID,
				Inline: true,
			},
			{
				Name:   "Bot?",
				Value:  botField,
				Inline: true,
			},
			{
				Name:   "Status",
				Value:  cases.Title(language.English, cases.NoLower).String(string(presence.Status)),
				Inline: true,
			},
			{
				Name:   "Joined at",
				Value:  member.JoinedAt.Format("01/02/2006 15:04:05"),
				Inline: true,
			},
			{
				Name:  "Roles",
				Value: stringutil.EnsureNotEmpty(strings.Join(roles, " "), "None"),
			},
		},
	}

	ctx.FollowUpEmbed(embed).Send()

	return nil
}
