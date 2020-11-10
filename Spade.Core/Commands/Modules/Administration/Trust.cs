using Discord;
using Qmmands;
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

		[Command("allow", "add")]
		[RequireOwner]
		public async Task TrustAsync(IGuildUser user)
			=> await TrustAsync(user.Id);

		[Command("allow", "add")]
		[RequireOwner]
		public async Task TrustAsync(ulong userId)
		{
			await TrustedUserRepository.AddTrustedUserAsync(userId);

			await Context.ReactAsync("✅");
		}

		[Command("disallow", "remove")]
		public async Task UntrustAsync(IGuildUser user)
			=> await UntrustAsync(user.Id);

		[Command("disallow", "remove")]
		public async Task UntrustAsync(ulong userId)
		{
			await TrustedUserRepository.RemoveTrustedUserAsync(userId);

			await Context.ReactAsync("✅");
		}
	}
}
