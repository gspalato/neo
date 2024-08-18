package slash

import (
	"fmt"
	"os"
	"time"

	"github.com/bwmarrin/discordgo"
	"github.com/zekrotja/ken"
	"unreal.sh/neo/pkg/startup"
)

type PingCommand struct{}

var (
	_ ken.Command            = (*PingCommand)(nil)
	_ ken.SlashCommand       = (*PingCommand)(nil)
	_ ken.GuildScopedCommand = (*PingCommand)(nil)
)

func (c *PingCommand) Name() string {
	return "ping"
}

func (c *PingCommand) Description() string {
	return "Pong! Returns the bot's latency."
}

func (c *PingCommand) Version() string {
	return "1.0.0"
}

func (c *PingCommand) Type() discordgo.ApplicationCommandType {
	return discordgo.ChatApplicationCommand
}

func (c *PingCommand) Options() []*discordgo.ApplicationCommandOption {
	return []*discordgo.ApplicationCommandOption{}
}

func (c *PingCommand) Guild() string {
	return os.Getenv("MISFITS_GUILD_ID")
}

func (c *PingCommand) Run(ctx ken.Context) (err error) {
	botLatencyStart := time.Now()
	if err = ctx.Defer(); err != nil {
		return
	}
	botLatencyTime := time.Since(botLatencyStart).String()

	var fields []*discordgo.MessageEmbedField
	fields = append(fields, &discordgo.MessageEmbedField{
		Name:   "API Latency",
		Value:  fmt.Sprintf("```%.0dms```", ctx.GetSession().HeartbeatLatency().Milliseconds()),
		Inline: true,
	})

	fields = append(fields, &discordgo.MessageEmbedField{
		Name:   "Bot Latency",
		Value:  fmt.Sprintf("```%s```", botLatencyTime),
		Inline: true,
	})

	fields = append(fields, &discordgo.MessageEmbedField{
		Name:   "Uptime",
		Value:  fmt.Sprintf("```%s```", startup.Took().String()),
		Inline: true,
	})

	embed := &discordgo.MessageEmbed{
		Title:       "🏓 Pong!",
		Description: "\u200B",
		Fields:      fields,
		Color:       0x2B2D31,
	}

	ctx.FollowUpEmbed(embed).Send()

	return nil
}
