using Oculus.Core.Structures.Attributes;
using Qmmands;
using System.Threading.Tasks;

namespace Oculus.Core.Commands.Modules.Miscellaneous
{
	[Category(Category.Miscellaneous)]
	[Group("echo", "say")]
	public class Echo : OculusModule
	{
		[Command]
		//[RequireOwner]
		public async Task ExecuteAsync([Remainder] string text)
		{
			await Context.ReplyAsync(text);
		}
	}
}
