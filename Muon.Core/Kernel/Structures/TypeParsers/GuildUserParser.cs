using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Muon.Commands;
using Qmmands;

namespace Muon.Kernel.Structures.TypeParsers
{
	public class GuildUserParser : BaseTypeParser<IGuildUser>
	{
        private readonly Regex _userRegex = new Regex(@"^<@\!?(\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

        public override async ValueTask<TypeParserResult<IGuildUser>> ParseAsync(Parameter parameter, string value,
			MuonContext context, IServiceProvider provider)
		{
            if (context.Guild == null)
                return TypeParserResult<IGuildUser>.Unsuccessful("You're not in a guild.");

            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var uid))
            {
                var result = await context.Guild.GetUserAsync(uid);
                var ret = result != null
                    ? TypeParserResult<IGuildUser>.Successful(result)
                    : TypeParserResult<IGuildUser>.Unsuccessful("Couldn't parse user.");
                return ret;
            }

            var m = _userRegex.Match(value);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uid))
            {
                var result = await context.Guild.GetUserAsync(uid);
                var ret = result != null
                    ? TypeParserResult<IGuildUser>.Successful(result)
                    : TypeParserResult<IGuildUser>.Unsuccessful("Couldn't parse user.");
                return ret;
            }

            value = value.ToLowerInvariant();

            var di = value.IndexOf('#');
            var un = di != -1 ? value.Substring(0, di) : value;
            var dv = di != -1 ? value.Substring(di + 1) : null;

            var us =
                from user in await context.Guild.GetUsersAsync()
                where user.Username.ToLowerInvariant() == un 
                      && (dv != null && user.Discriminator == dv || dv == null)
                      || user.Nickname?.ToLowerInvariant() == value
                select user;

            var mbr = us.FirstOrDefault();
            return mbr != null
                ? TypeParserResult<IGuildUser>.Successful(mbr)
                : TypeParserResult<IGuildUser>.Unsuccessful("Couldn't parse user.");
        }
	}
}