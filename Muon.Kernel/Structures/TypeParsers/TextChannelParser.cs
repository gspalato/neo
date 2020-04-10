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
    public class TextChannelParser : BaseTypeParser<SocketTextChannel>
    {
        public override ValueTask<TypeParserResult<SocketTextChannel>> ParseAsync(Parameter parameter, string value,
            MuonContext context, IServiceProvider provider = null)
        {
            if (MentionUtils.TryParseChannel(value, out var id))
                if (context.Guild.GetChannel(id) is SocketTextChannel textChannel)
                    return TypeParserResult<SocketTextChannel>.Successful(textChannel);

            if (ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out id))
                if (context.Guild.GetTextChannel(id) is SocketTextChannel textChannel)
                    return TypeParserResult<SocketTextChannel>.Successful(textChannel);

            return context.Guild.TextChannels.FirstOrDefault(x => x.Name == value) is SocketTextChannel textChannelCheck
                ? TypeParserResult<SocketTextChannel>.Successful(textChannelCheck)
                : TypeParserResult<SocketTextChannel>.Unsuccessful("Couldn't parse text channel");
        }
    }
}