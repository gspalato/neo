using Qmmands;
using Oculus.Core.Structures.Attributes;
using System.Threading.Tasks;

namespace Oculus.Core.Commands.Modules.Administration
{
    [Category(Category.Admin)]
    [Description("Refreshes the bot's cache")]
    [Group("refresh", "update")]
    public class Refresh
    {
        [Command]
        [RequireTrustedUser]
        public async Task ExecuteAsync() { }
    }
}
