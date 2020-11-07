using Discord;
using Qmmands;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Spade.Core.Commands.TypeParsers
{
	public class GuildUserParser : BaseTypeParser<IGuildUser>
	{
		public static readonly GuildUserParser Instance = new GuildUserParser();

		private GuildUserParser() { }

		private readonly Regex m_UserRegex = new Regex(@"^<@\!?(\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

		public override async ValueTask<TypeParserResult<IGuildUser>> ParseAsync(Parameter parameter, string value,
			SpadeContext context, IServiceProvider provider)
		{
			if (context.Guild is null)
				return TypeParserResult<IGuildUser>.Unsuccessful("You're not in a guild.");

			if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var uid))
			{
				var result = await context.Guild.GetUserAsync(uid);
				return result is not null
					? TypeParserResult<IGuildUser>.Successful(result)
					: TypeParserResult<IGuildUser>.Unsuccessful("Couldn't parse user."); ;
			}

			var m = m_UserRegex.Match(value);
			if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uid))
			{
				var result = await context.Guild.GetUserAsync(uid);
				return result is not null
					? TypeParserResult<IGuildUser>.Successful(result)
					: TypeParserResult<IGuildUser>.Unsuccessful("Couldn't parse user.");
			}

			value = value.ToLowerInvariant();

			var di = value.IndexOf('#');
			var un = di is not -1 ? value.Substring(0, di) : value;
			var dv = di is not -1 ? value.Substring(di + 1) : null;

			var mbr = (await context.Guild.GetUsersAsync()).FirstOrDefault(u =>
				u.Username.ToLowerInvariant() == un
				&& (dv is not null && u.Discriminator == dv || dv is null)
				|| u.Nickname?.ToLowerInvariant() == value);

			return mbr is not null
				? TypeParserResult<IGuildUser>.Successful(mbr)
				: TypeParserResult<IGuildUser>.Unsuccessful("Couldn't parse user.");
		}
	}
}