using Axion.Core.Structures.Attributes;
using Qmmands;
using System.Text;
using System.Threading.Tasks;

namespace Axion.Core.Commands.Modules.Miscellaneous
{
    [Category(Category.Miscellaneous)]
    [Group("fw", "fullwidth")]
    public class Fullwidth : AxionModule
    {
        [Command]
        public async Task ExecuteAsync([Name("Text")] [Remainder] string text)
        {
            var output = new StringBuilder();

            foreach (var c in text)
                if (0x0020 < c && c < 0x007F)
                    output.Append((char)(0xFF00 + (c - 0x0020)));
                else if (c == 0x0020)
                    output.Append((char)0x3000);
                else
                    output.Append(c);

            await Context.ReplyAsync(output.ToString());
        }
    }
}
