using Discord;
using Qmmands;
using Spade.Core.Services;
using Spade.Core.Structures.Attributes;
using System.Threading.Tasks;

namespace Spade.Core.Commands.Modules.Administration
{
	[Category(Category.Admin)]
	[Description("Enables use of internal bot commands.")]
	[Group("sudo", "#")]
	public sealed class Sudo : SpadeModule
	{
		public ICommandHandlingService CommandHandlingService { get; set; }

		[Command]
		[RequireTrustedUser]
		public async Task SudoAsync([Remainder] string command)
		{
			var context = new SpadeContext(Context.Message, Context.Me, Context.ServiceProvider,
				isSudo: true, overrideUser: Context.User, overridePermissionsUser: Context.User);
			var result = await CommandService.ExecuteAsync(command, context);

			if (result.IsSuccessful)
				await Context.Message.AddReactionAsync(new Emoji("\u2705"));
			else
				await Context.Message.AddReactionAsync(new Emoji("\u274c"));

			_ = CommandHandlingService.HandleCommandResult(result, Context.Message);
		}
	}
}
