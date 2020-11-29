using Qmmands;
using Oculus.Core.Commands;
using System;
using System.Threading.Tasks;

namespace Oculus.Core.Structures.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RequireReferenceAttribute : BaseCheckAttribute
    {
        public override async ValueTask<CheckResult> CheckAsync(OculusContext context, IServiceProvider provider)
        {
            if (context.Message.ReferencedMessage is null)
                return CheckResult.Unsuccessful("This command requires a reply to a message.");

            return CheckResult.Successful;
        }
    }
}
