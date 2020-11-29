using Discord;
using Qmmands;
using Oculus.Core.Services;
using Oculus.Core.Structures.Attributes;
using System.Threading.Tasks;

namespace Oculus.Core.Commands.Modules.Administration
{
	[Category(Category.Admin)]
	[Description("Execute a command without mentioning the user.")]
	[Group("silent", "hide")]
	public sealed class Silent : OculusModule
	{
		public ICommandHandlingService CommandHandlingService { get; set; }

		[Command]
		[RequireTrustedUser]
		[RequireChannelBotPermissions(ChannelPermission.ManageMessages)]
		public async Task HideAsync([Remainder] string command)
		{
			var context = new OculusContext(Context.Message, Context.Me, Context.ServiceProvider);

			context.SetReply(false);

			var result = await CommandService.ExecuteAsync(command, context);

			_ = CommandHandlingService.HandleCommandResult(result, Context.Message);
		}
	}
}
