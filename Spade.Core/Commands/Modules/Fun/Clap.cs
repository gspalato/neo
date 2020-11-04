using Spade.Core.Structures.Attributes;
using Qmmands;
using System.Threading.Tasks;

namespace Spade.Core.Commands.Modules.Fun
{
	[Category(Category.Fun)]
	[Group("clap")]
	public class Clap : SpadeModule
	{
		[Command]
		public async Task ExecuteAsync([Name("Text")] [Remainder] string text)
		{
			await Context.ReplyAsync(text.Replace(" ", " 👏 "));
		}
	}
}