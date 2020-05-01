using Axion.Commands;
using Discord;
using Qmmands;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Axion.Structures.TypeParsers
{
    public class MessageParser : BaseTypeParser<IMessage>
    {
        private readonly Regex _messagePathRegex =
            new Regex(@"^\/channels\/(?<guild>(?:\d+|@me))\/(?<channel>\d+)\/(?<message>\d+)\/?$",
                RegexOptions.ECMAScript | RegexOptions.Compiled);

        public override async ValueTask<TypeParserResult<IMessage>> ParseAsync(Parameter parameter, string value,
            AxionContext context, IServiceProvider provider)
        {
            if (string.IsNullOrWhiteSpace(value))
                return TypeParserResult<IMessage>.Unsuccessful("Couldn't parse message.");

            var msguri = value.StartsWith("<") && value.EndsWith(">") ? value.Substring(1, value.Length - 2) : value;
            ulong mid;
            if (Uri.TryCreate(msguri, UriKind.Absolute, out var uri))
            {
                if (uri.Host != "discordapp.com" && !uri.Host.EndsWith(".discordapp.com"))
                    return TypeParserResult<IMessage>.Unsuccessful("Couldn't parse message.");

                var uripath = _messagePathRegex.Match(uri.AbsolutePath);
                if (!uripath.Success
                    || !ulong.TryParse(uripath.Groups["channel"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var cid)
                    || !ulong.TryParse(uripath.Groups["message"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out mid))
                    return TypeParserResult<IMessage>.Unsuccessful("Couldn't parse message.");

                var chn = context.Client.GetChannel(cid);
                if (chn == null)
                    return TypeParserResult<IMessage>.Unsuccessful("Couldn't parse message.");

                var msg = await ((ITextChannel)chn).GetMessageAsync(mid).ConfigureAwait(false);
                return msg != null
                    ? TypeParserResult<IMessage>.Successful(msg)
                    : TypeParserResult<IMessage>.Unsuccessful("Couldn't parse message.");
            }

            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out mid))
            {
                var result = await context.Channel.GetMessageAsync(mid).ConfigureAwait(false);
                return result != null
                    ? TypeParserResult<IMessage>.Successful(result)
                    : TypeParserResult<IMessage>.Unsuccessful("Couldn't parse message.");
            }

            return TypeParserResult<IMessage>.Unsuccessful("Couldn't parse message.");
        }
    }
}