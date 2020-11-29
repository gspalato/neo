using Discord;
using Qmmands;
using Oculus.Core.Services;
using Oculus.Core.Structures.Attributes;
using System.Threading.Tasks;

namespace Oculus.Core.Commands.Modules.Administration
{
	[Category(Category.Admin)]
	[Description("Run command as someone else.")]
	[Group("sudo", "#")]
	public sealed class Sudo : OculusModule
	{
		public ICommandHandlingService CommandHandlingService { get; set; }

		[Command]
		[RequireTrustedUser]
		public async Task SudoAsync(IGuildUser user, [Remainder] string command)
		{
			var context = new OculusContext(Context.Message, Context.Me, Context.ServiceProvider);

			context.SetReply(false);
			context.OverrideUser(user);
			context.OverridePermissionsUser(Context.User);

			var result = await CommandService.ExecuteAsync(command, context);

			await Context.Message.AddReactionAsync(new Emoji("\u2705"));

			_ = CommandHandlingService.HandleCommandResult(result, Context.Message);
		}
	}
}
