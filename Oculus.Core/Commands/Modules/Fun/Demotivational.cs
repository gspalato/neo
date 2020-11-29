using Qmmands;
using Oculus.Core.Structures.Attributes;
using System;
using System.Threading.Tasks;

namespace Oculus.Core.Commands.Modules.Fun
{
    [Category(Category.Fun)]
    [Group("demotivational", "demotivationalposter", "demotivational-poster")]
    public class Demotivational : OculusModule
    {
        [Command]
        public async Task ExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}