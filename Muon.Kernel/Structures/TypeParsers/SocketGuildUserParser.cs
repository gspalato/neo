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
    public class SocketGuildUserParser : BaseTypeParser<SocketGuildUser>
    {
        public override ValueTask<TypeParserResult<SocketGuildUser>> ParseAsync(Parameter parameter, string value,
            MuonContext context, IServiceProvider provider = null)
        {
            if (MentionUtils.TryParseUser(value, out var id))
                if (context.Guild.GetUser(id) is SocketGuildUser user)
                    return TypeParserResult<SocketGuildUser>.Successful(user);
            //: TypeParserResult<SocketGuildUser>.Unsuccessful("Couldn't parse text channel");

            if (ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out id))
                if (context.Guild.GetUser(id) is SocketGuildUser user)
                    return TypeParserResult<SocketGuildUser>.Successful(user);
            //: TypeParserResult<SocketGuildUser>.Unsuccessful("Couldn't parse text channel");

            return context.Guild.Users.FirstOrDefault(x => x.Username.ToLower().StartsWith(value.ToLower())) is SocketGuildUser userCheck
                ? TypeParserResult<SocketGuildUser>.Successful(userCheck)
                : TypeParserResult<SocketGuildUser>.Unsuccessful("Couldn't parse user.");
        }
    }
}