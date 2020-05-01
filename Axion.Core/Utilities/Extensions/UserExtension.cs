using System.Linq;
using Discord;

namespace Axion.Core.Utilities
{
	public static class UserExtension
	{
		public static string GetStatus(this IUser user)
		{
			var activity = user?.Activity;
			var type = activity.Type;

			if (activity is null)
				return "";

			switch (type)
			{
				default:
					return "";

				case ActivityType.CustomStatus:
					{
						var game = activity as CustomStatusGame;
						var emote = game.Emote is null ? "" : $"{game.Emote} ";

						return $"> {emote}{(activity as CustomStatusGame).State}";
					}

				case ActivityType.Playing:
					return $"**Playing** {activity.Name}";

				case ActivityType.Watching:
					return $"**Watching** {activity.Name}";

				case ActivityType.Listening:
					return $"**Listening to** {activity.Name}";

				case ActivityType.Streaming:
					return $"**Streaming** {activity.Name}";
			}
		}

		public static Color GetHighestColor(this IGuildUser member, Color fallback)
		{
			var roles =
				from role in member.Guild.Roles
				orderby role.Position descending
				select role;

			IRole highestRoleWithColor = roles.ToList().Find((role) => role.Color.RawValue > 0);
			return highestRoleWithColor?.Color ?? fallback;
		}
	}
}
