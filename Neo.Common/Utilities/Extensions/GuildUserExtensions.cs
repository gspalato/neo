using Discord;
using Discord.WebSocket;
using System.Text;

namespace Neo.Common.Utilities.Extensions
{
    public static class GuildUserExtension
    {
        public static string GetStatus(this SocketGuildUser user)
        {
            var text = new StringBuilder();

            if (!user.Activities.Any())
                return "";

            CustomStatusGame? customStatus = (CustomStatusGame)user.Activities.FirstOrDefault((act) => act.Type is ActivityType.CustomStatus)!;

            if (customStatus is not null)
            {
                text.AppendLine($"> {customStatus.Emote} {customStatus.State}");
                text.AppendLine();
            }

            IActivity? activity = user.Activities.FirstOrDefault((act) => act.Type is not ActivityType.CustomStatus);

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
            if (!member.Roles.Any())
                return fallback;

            var roleList = from role in member.Roles
                           orderby role.Position descending
                           select role;

            var colors = from role in roleList
                         where role.Color.RawValue > 0
                         select role.Color;

            if (!colors.Any())
                return fallback;

            return colors.First();
        }
    }
}