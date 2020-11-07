using Discord;
using Qmmands;
using Spade.Core.Services;
using Spade.Core.Structures.Attributes;
using System.Threading.Tasks;

namespace Spade.Core.Commands.Modules.Administration
{
    [Category(Category.Admin)]
    [Group("sudo")]
    public sealed class Sudo : SpadeModule
    {
        public ICommandHandlingService CommandHandlingService { get; set; }

        [Command]
        [RequireTrustedUser]
        public async Task SudoAsync(IGuildUser user, [Remainder] string command)
        {
            var context = new SpadeContext(Context.Message, Context.Me, Context.ServiceProvider, user, Context.User);
            var result = await CommandService.ExecuteAsync(command, context);

            await Context.Message.AddReactionAsync(new Emoji("\u2705"));

            _ = CommandHandlingService.HandleCommandResult(result, Context.Message);
        }
    }
}
