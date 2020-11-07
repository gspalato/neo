using Discord;
using Qmmands;
using Spade.Core.Services;
using Spade.Core.Structures.Attributes;
using Spade.Database.Repositories;
using System.Threading.Tasks;

namespace Spade.Core.Commands.Modules.Administration
{
    [Category(Category.Admin)]
    [Group("trust")]
    public sealed class Trust : SpadeModule
    {
        public ITrustedUserRepository TrustedUserRepository { get; set; }

        [Command]
        [RequireOwner]
        public async Task TrustAsync(IGuildUser user)
            => await TrustAsync(user.Id);

        [Command]
        [RequireOwner]
        public async Task TrustAsync(ulong userId)
        {
            await TrustedUserRepository.AddTrustedUserAsync(userId);

            await Context.ReactAsync("✅");
        }

        [Command("remove")]
        public async Task UntrustAsync(IGuildUser user)
            => await UntrustAsync(user.Id);

        [Command("remove")]
        public async Task UntrustAsync(ulong userId)
        {
            await TrustedUserRepository.RemoveTrustedUserAsync(userId);

            await Context.ReactAsync("✅");
        }
    }
}
