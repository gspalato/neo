using Spade.Core.Structures.Attributes;
using Qmmands;
using System.Threading.Tasks;

namespace Spade.Core.Commands.Modules.Miscellaneous
{
	[Category(Category.Miscellaneous)]
	[Group("echo", "say")]
	public class Echo : SpadeModule
	{
		[Command]
		//[RequireOwner]
		public async Task ExecuteAsync([Remainder] string text)
		{
			await Context.ReplyAsync(text);
		}
	}
}
