using Discord;
using Qmmands;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Axion.Commands.TypeParsers
{
    public class UserParser : BaseTypeParser<IUser>
    {
        public static readonly UserParser Instance = new UserParser();

        private UserParser() { }

        private readonly Regex _userRegex = new Regex(@"^<@\!?(\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

        public override ValueTask<TypeParserResult<IUser>> ParseAsync(Parameter parameter, string value,
            AxionContext context, IServiceProvider provider)
        {
            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var uid))
            {
                var result = context.Client.GetUser(uid);
                var ret = result != null
                    ? TypeParserResult<IUser>.Successful(result)
                    : TypeParserResult<IUser>.Unsuccessful("Couldn't parse user.");
                return ret;
            }

            var m = _userRegex.Match(value);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uid))
            {
                var result = context.Client.GetUser(uid);
                var ret = result != null
                    ? TypeParserResult<IUser>.Successful(result)
                    : TypeParserResult<IUser>.Unsuccessful("Couldn't parse user.");
                return ret;
            }

            value = value.ToLowerInvariant();

            var sep = value.IndexOf('#');
            var username = sep != -1 ? value.Substring(0, sep) : value;
            var discrim = sep != -1 ? value.Substring(sep + 1) : null;

            var us = context.Client.Guilds
                .SelectMany(guild => guild.Users)
                .Where(user =>
                    user.Username.ToLowerInvariant() == username
                    && ((discrim != null && user.Discriminator == discrim) || discrim == null));

            var usr = us.FirstOrDefault();
            return usr != null
                ? TypeParserResult<IUser>.Successful(usr)
                : TypeParserResult<IUser>.Unsuccessful("Couldn't parse users.");
        }
    }
}