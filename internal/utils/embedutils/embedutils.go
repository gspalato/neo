package embed

import (
	"github.com/bwmarrin/discordgo"
	"unreal.sh/neo/internal/utils/static"
)

func CreateSuccessEmbed(description string) *discordgo.MessageEmbed {
	return &discordgo.MessageEmbed{
		Title:       "✅ **Success**",
		Color:       0x00FF00,
		Description: description,
	}
}

func CreatePromptEmbed(description string) *discordgo.MessageEmbed {
	return &discordgo.MessageEmbed{
		Title:       "⁉ **Are you sure?**",
		Color:       static.ColorEmbedGray,
		Description: description,
	}
}

func CreateErrorEmbed(description string) *discordgo.MessageEmbed {
	return &discordgo.MessageEmbed{
		Title:       "⚠ **Error**",
		Color:       static.ColorEmbedOrange,
		Description: description,
	}
}

func CreateBasicEmbed(description string) *discordgo.MessageEmbed {
	return &discordgo.MessageEmbed{
		Color:       static.ColorEmbedGray,
		Description: description,
	}
}
