using Discord;
using Discord.WebSocket;
using System.Text;

namespace Oculus.Common.Utilities.Extensions
{
    public static class UserExtension
    {
        public static string GetStatus(this SocketUser user)
        {
            var text = new StringBuilder();

            if (!user.Activities.Any())
                return "";

            CustomStatusGame customStatus;
            try
            {
                customStatus = (CustomStatusGame)user.Activities.First((act) => act.Type is ActivityType.CustomStatus);
            }
            catch (Exception ex)
            {
                customStatus = null;
            }


            if (customStatus is not null)
            {
                text.AppendLine($"> {customStatus.Emote} {customStatus.State}");
                text.AppendLine();
            }

            IActivity activity;
            try
            {
                activity = user.Activities.First((act) => act.Type is not ActivityType.CustomStatus);
            }
            catch (Exception ex)
            {
                activity = null;
            }

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
    }
}