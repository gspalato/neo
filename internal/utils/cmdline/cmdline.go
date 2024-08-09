package cmdline

import (
	"fmt"
	"log/slog"
	"os"

	"github.com/bwmarrin/discordgo"
	"golang.org/x/exp/slices"
	"unreal.sh/neo/internal/utils/static"
)

func HandleCommandLineArguments(session *discordgo.Session) (end bool, e error) {
	args := os.Args[1:]

	shouldEnd := false

	if slices.Contains(args, static.CmdArgUnregisterSlashCommands) {
		err := session.Open()
		if err != nil {
			return true, err
		}

		guildSpecificCmds, err := session.ApplicationCommands(session.State.User.ID, os.Getenv("MISFITS_GUILD_ID"))
		if err != nil {
			return true, err
		}

		cmds, err := session.ApplicationCommands(session.State.User.ID, "")
		if err != nil {
			return true, err
		}

		for _, cmd := range append(guildSpecificCmds, cmds...) {
			slog.Info(fmt.Sprintf("Unregistering command '%s'...", cmd.Name))
			err = session.ApplicationCommandDelete(session.State.User.ID, cmd.GuildID, cmd.ID)
			if err != nil {
				return true, err
			}
		}

		slog.Info("Unregistered all slash commands.")

		shouldEnd = true
	}

	return shouldEnd, nil
}
