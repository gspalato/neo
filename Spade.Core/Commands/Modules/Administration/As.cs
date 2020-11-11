using Discord;
using Qmmands;
using Spade.Core.Services;
using Spade.Core.Structures.Attributes;
using System;
using System.Threading.Tasks;

namespace Spade.Core.Commands.Modules.Administration
{
	[Category(Category.Admin)]
	[Group("as")]
	public sealed class As : SpadeModule
	{
		public ICommandHandlingService CommandHandlingService { get; set; }

		[Command]
		[RequireSudo]
		public async Task ExecuteAsAsync(IGuildUser user, [Remainder] string command)
		{
			Console.WriteLine("reached as cmd lol");

			var context = new SpadeContext(Context.Message, Context.Me, Context.ServiceProvider,
				overrideUser: user, overridePermissionsUser: Context.User);
			var result = await CommandService.ExecuteAsync(command, context);

			await Context.Message.AddReactionAsync(new Emoji("\u2705"));

			_ = CommandHandlingService.HandleCommandResult(result, Context.Message);
		}
	}
}
