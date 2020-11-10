using Qmmands;
using Spade.Core.Commands;
using System;
using System.Threading.Tasks;

namespace Spade.Core.Structures.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireSudoAttribute : BaseCheckAttribute
    {
        public override async ValueTask<CheckResult> CheckAsync(SpadeContext context, IServiceProvider provider)
        {
            await Task.CompletedTask;

            if (context.IsSudo)
                return CheckResult.Successful;

            return CheckResult.Unsuccessful("This command can only be ran via sudo.");
        }
    }
}