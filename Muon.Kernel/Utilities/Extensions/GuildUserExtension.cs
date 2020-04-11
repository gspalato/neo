using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;

namespace Muon.Kernel.Utilities
{
	public static class GuildUserExtension
	{
		public static string GetStatus(this SocketGuildUser member)
		{
			var activity = member.Activity;
			var type = activity?.Type;

			if (activity is null || type is null)
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

		public static Color GetHighestColor(this SocketGuildUser member, Color fallback)
		{
			List<SocketRole> roles = member.Roles.OrderByDescending((role) => role.Position).ToList();

			SocketRole highestRoleWithColor = roles.Find((role) =>
			{
				var RGB = role.Color.R + role.Color.G + role.Color.B;
				return RGB > 0;
			});
			return highestRoleWithColor?.Color ?? fallback;
		}
	}
}
