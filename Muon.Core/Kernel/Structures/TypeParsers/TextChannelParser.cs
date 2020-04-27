using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Muon.Commands;
using Qmmands;

namespace Muon.Kernel.Structures.TypeParsers
{
	public class TextChannelParser : BaseTypeParser<ITextChannel>
	{
		private readonly Regex _channelRegex = new Regex(@"^<#(\d+)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

		public override async ValueTask<TypeParserResult<ITextChannel>> ParseAsync(Parameter parameter, string value,
			MuonContext context, IServiceProvider provider)
		{
			static TypeParserResult<ITextChannel> CheckType(SocketChannel channel)
			{
				if (channel is ITextChannel textChannel)
					return TypeParserResult<ITextChannel>.Successful(textChannel);
				
				return TypeParserResult<ITextChannel>.Unsuccessful("Couldn't parse text channel");
			}
			
			if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var cid))
				return CheckType(context.Client.GetChannel(cid));

			var m = _channelRegex.Match(value);
			if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out cid))
				return CheckType(context.Client.GetChannel(cid));

			value = value.ToLowerInvariant();

			var chns =
				from channel in await context.Guild.GetTextChannelsAsync()
				where channel.Name.ToLowerInvariant() == value
				select channel;

			var chn = chns.FirstOrDefault();

			return chn != null
				? TypeParserResult<ITextChannel>.Successful(chn)
				: TypeParserResult<ITextChannel>.Unsuccessful("Couldn't parse text channel");
		}
	}
}