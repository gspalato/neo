package database

import (
	"errors"
	"log/slog"
	"os"

	"github.com/bwmarrin/discordgo"
	"github.com/supabase-community/supabase-go"
)

const (
	GuildSettingsTable = "guild_settings"
)

type Database struct {
	client *supabase.Client
}

func NewDatabase() (*Database, error) {
	db := &Database{}

	apiUrl, ok := os.LookupEnv("DATABASE_URL")
	if !ok {
		return db, errors.New("DATABASE_URL is not set")
	}

	apiKey, ok := os.LookupEnv("DATABASE_KEY")
	if !ok {
		return db, errors.New("DATABASE_KEY is not set")
	}

	client, err := supabase.NewClient(apiUrl, apiKey, nil)
	if err != nil {
		return db, err
	}

	db.client = client

	return db, nil
}

func (d *Database) Client() *supabase.Client {
	return d.client
}

func (d *Database) GetGuildSettings(guildID string) (settings GuildSettings, exists bool, err error) {
	res := make([]GuildSettings, 1)

	f := d.client.From(GuildSettingsTable).Select("*", "exact", false).Eq("guild_id", guildID).Limit(1, "")
	count, err := f.ExecuteTo(&res)
	if err != nil {
		return settings, false, err
	}

	if count == 0 {
		return settings, false, nil
	}

	return res[0], true, nil
}

func (d *Database) CreateGuildSettings(settings GuildSettings) (GuildSettings, error) {
	res := make([]GuildSettings, 1)

	q := d.client.From(GuildSettingsTable).Insert(settings, false, "", "", "exact")
	count, err := q.ExecuteTo(&res)

	if err != nil {
		return settings, err
	}

	if count == 0 {
		return settings, errors.New("failed to insert guild settings")
	}

	return res[0], nil
}

func (d *Database) GetOrCreateGuildSettings(guildID string) (s GuildSettings, created bool, err error) {
	if guildID == "" {
		return GuildSettings{}, false, errors.New("guildID is empty")
	}

	settings, exists, err := d.GetGuildSettings(guildID)
	if err != nil {
		return settings, false, err
	}

	if exists {
		return settings, false, nil
	}

	settings = GuildSettings{
		GuildID:        guildID,
		EnabledModules: []string{},
	}

	settings, err = d.CreateGuildSettings(settings)

	return settings, true, err
}

func (d *Database) UpdateGuildSettings(settings GuildSettings) (GuildSettings, error) {
	res := make([]GuildSettings, 1)

	q := d.client.From(GuildSettingsTable).Update(settings, "", "exact").Eq("guild_id", settings.GuildID)
	count, err := q.ExecuteTo(&res)

	if err != nil {
		return settings, err
	}

	if count == 0 {
		return settings, errors.New("failed to update guild settings")
	}

	return res[0], nil
}

func (d *Database) AssureGuildSettings(guilds ...*discordgo.Guild) []error {
	errs := make([]error, 0)

	for _, guild := range guilds {
		go func(guildID string) {
			_, created, err := d.GetOrCreateGuildSettings(guildID)
			if err != nil {
				slog.Error("Failed to create guild settings.",
					slog.String("guild_id", guildID), slog.String("error", err.Error()))

				errs = append(errs, err)
			}

			if created {
				slog.Info("Created guild settings.", slog.String("guild_id", guildID))
			}
		}(guild.ID)
	}

	return errs
}

func (d *Database) EnableModule(guildID string, moduleID string) (GuildSettings, error) {
	settings, _, err := d.GetOrCreateGuildSettings(guildID)
	if err != nil {
		return settings, err
	}

	settings.EnabledModules = append(settings.EnabledModules, moduleID)
	settings, err = d.UpdateGuildSettings(settings)
	if err != nil {
		return settings, err
	}

	return settings, nil
}

func (d *Database) DisableModule(guildID string, moduleID string) (GuildSettings, error) {
	settings, _, err := d.GetOrCreateGuildSettings(guildID)
	if err != nil {
		return settings, err
	}

	for i, id := range settings.EnabledModules {
		if id == moduleID {
			settings.EnabledModules = append(settings.EnabledModules[:i], settings.EnabledModules[i+1:]...)
			break
		}
	}

	settings, err = d.UpdateGuildSettings(settings)
	if err != nil {
		return settings, err
	}

	return settings, nil
}
