using Axion.Core.Structures.Attributes;
using Qmmands;
using System.Threading.Tasks;

namespace Axion.Core.Commands.Modules.Miscellaneous
{
	[Category(Category.Miscellaneous)]
	[Group("echo", "say")]
	public class Echo : AxionModule
	{
		[Command]
		[RequireOwner]
		public async Task ExecuteAsync([Remainder] string text)
		{
			await Context.ReplyAsync(text);
		}
	}
}
