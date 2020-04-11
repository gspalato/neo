using Discord;
using Discord.WebSocket;
using Muon.Commands;
using Qmmands;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Muon.Kernel.Structures
{
	public class TextChannelParser : BaseTypeParser<ITextChannel>
	{
		public override ValueTask<TypeParserResult<ITextChannel>> ParseAsync(Parameter parameter, string value,
			MuonContext context, IServiceProvider provider = null)
		{
			SocketGuild guild = context.Guild as SocketGuild;

			if (MentionUtils.TryParseChannel(value, out var id))
				if (guild.GetChannel(id) is ITextChannel textChannel)
					return TypeParserResult<ITextChannel>.Successful(textChannel);

			if (ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out id))
				if (guild.GetTextChannel(id) is ITextChannel textChannel)
					return TypeParserResult<ITextChannel>.Successful(textChannel);

			return guild.TextChannels.FirstOrDefault(x => x.Name == value) is SocketTextChannel textChannelCheck
				? TypeParserResult<ITextChannel>.Successful(textChannelCheck)
				: TypeParserResult<ITextChannel>.Unsuccessful("Couldn't parse text channel");
		}
	}
}