package discord

import (
	"strconv"
	"time"

	"github.com/bwmarrin/discordgo"
)

// GetDiscordSnowflakeCreationTime returns the time.Time
// of creation of the passed snowflake string.
//
// Returns an error when the passed snowflake string could
// not be parsed to an integer.
func GetDiscordSnowflakeCreationTime(snowflake string) (time.Time, error) {
	sfI, err := strconv.ParseInt(snowflake, 10, 64)
	if err != nil {
		return time.Time{}, err
	}
	timestamp := (sfI >> 22) + 1420070400000
	return time.Unix(timestamp/1000, timestamp), nil
}

func GetCustomStatus(presence *discordgo.Presence) *discordgo.Activity {
	if presence == nil {
		return nil
	}

	for _, activity := range presence.Activities {
		if activity.Type == discordgo.ActivityTypeCustom {
			return activity
		}
	}

	return nil
}
