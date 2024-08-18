package database

import "encoding/json"

type GuildSettings struct {
	GuildID        string   `json:"guild_id"`
	EnabledModules []string `json:"enabled_modules"`
}

func (g *GuildSettings) String() string {
	b, _ := json.Marshal(g)
	return string(b)
}
