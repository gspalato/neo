using Oculus.Core.Structures.Attributes;
using Qmmands;
using System.Threading.Tasks;

namespace Oculus.Core.Commands.Modules.Fun
{
	[Category(Category.Fun)]
	[Group("clap")]
	public class Clap : OculusModule
	{
		[Command]
		public async Task ExecuteAsync([Name("Text")] [Remainder] string text)
		{
			await Context.ReplyAsync(text.Replace(" ", " 👏 "));
		}
	}
}