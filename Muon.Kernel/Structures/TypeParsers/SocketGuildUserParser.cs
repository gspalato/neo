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
    public class SocketGuildUserParser : BaseTypeParser<IGuildUser>
    {
        public override ValueTask<TypeParserResult<IGuildUser>> ParseAsync(Parameter parameter, string value,
            MuonContext context, IServiceProvider provider = null)
        {
            SocketGuild guild = context.Guild as SocketGuild;

            if (MentionUtils.TryParseUser(value, out var id))
                if (guild.GetUser(id) is SocketGuildUser user)
                    return TypeParserResult<IGuildUser>.Successful(user);

            if (ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out id))
                if (guild.GetUser(id) is SocketGuildUser user)
                    return TypeParserResult<IGuildUser>.Successful(user);

            return guild.Users.FirstOrDefault(x => x.Username.ToLower().StartsWith(value.ToLower())) is SocketGuildUser userCheck
                ? TypeParserResult<IGuildUser>.Successful(userCheck)
                : TypeParserResult<IGuildUser>.Unsuccessful("Couldn't parse user.");
        }
    }
}