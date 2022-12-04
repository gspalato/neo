using Discord;
using Discord.WebSocket;
using System.Text;

namespace Oculus.Common.Utilities.Extensions
{
    public static class GuildUserExtension
    {
        public static string GetStatus(this SocketGuildUser member)
        {
            var text = new StringBuilder();

            var customStatus = member.Activities.First((act) => act.Type is ActivityType.CustomStatus) as CustomStatusGame;

            if (customStatus is not null)
            {
                text.AppendLine($"{customStatus.Emote} {customStatus.State}");
                text.AppendLine();
            }

            var activity = member.Activities.First((act) => act.Type is not ActivityType.CustomStatus);

            if (activity is null)
                return text.ToString();

            var verb = activity.Type switch
            {
                ActivityType.Playing => "**Playing** ",
                ActivityType.Listening => "**Listening** ",
                ActivityType.Watching => "**Watching** ",
                ActivityType.Streaming => "**Streaming** ",
                ActivityType.Competing => "**Competing in** ",
                _ => ""
            };

            text.AppendLine($"{verb} {activity.Name}");

            return text.ToString();
        }

        public static Color GetHighestColor(this SocketGuildUser member, Color fallback)
        {
            List<SocketRole> roles = member.Roles.OrderBy((role) => role.Position).ToList();

            return roles.First().Color;
        }
    }
}