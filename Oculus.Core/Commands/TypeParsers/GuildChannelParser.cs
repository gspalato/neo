using Discord;
using Discord.WebSocket;
using Qmmands;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Oculus.Core.Commands.TypeParsers
{
	public class GuildChannelParser : BaseTypeParser<IGuildChannel>
	{
		public static readonly GuildChannelParser Instance = new GuildChannelParser();

		private GuildChannelParser() { }

		private readonly Regex _channelRegex = new Regex(@"^<#(\d+)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

		public override async ValueTask<TypeParserResult<IGuildChannel>> ParseAsync(Parameter parameter, string value,
			OculusContext context, IServiceProvider provider)
		{
			static TypeParserResult<IGuildChannel> CheckType(SocketChannel channel)
			{
				if (channel is IGuildChannel guildChannel)
					return TypeParserResult<IGuildChannel>.Successful(guildChannel);

				return TypeParserResult<IGuildChannel>.Unsuccessful("Couldn't parse channel");
			}

			if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var cid))
				return CheckType(context.Client.GetChannel(cid));

			var m = _channelRegex.Match(value);
			if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out cid))
				return CheckType(context.Client.GetChannel(cid));

			value = value.ToLowerInvariant();

			var chn = (await context.Guild.GetChannelsAsync())
				.First(c => c.Name.ToLowerInvariant() == value);

			return chn is not null
				? TypeParserResult<IGuildChannel>.Successful(chn)
				: TypeParserResult<IGuildChannel>.Unsuccessful("Couldn't parse guild channel");
		}
	}
}