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
	public class TextChannelParser : BaseTypeParser<ITextChannel>
	{
		private readonly Regex _channelRegex = new Regex(@"^<#(\d+)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

		public override async ValueTask<TypeParserResult<ITextChannel>> ParseAsync(Parameter parameter, string value,
			MuonContext context, IServiceProvider provider)
		{
			if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var cid))
			{
				var result = context.Client.GetChannel(cid);
				if (!(result is ITextChannel textChannel))
					return TypeParserResult<ITextChannel>.Unsuccessful("Couldn't parse text channel");

				return TypeParserResult<ITextChannel>.Successful(textChannel);
			}

			var m = _channelRegex.Match(value);
			if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out cid))
			{
				var result = context.Client.GetChannel(cid);
				if (!(result is ITextChannel textChannel))
					return TypeParserResult<ITextChannel>.Unsuccessful("Couldn't parse text channel");

				return TypeParserResult<ITextChannel>.Successful(textChannel);
			}

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