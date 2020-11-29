using Qmmands;
using Oculus.Core.Structures.Attributes;
using Oculus.Database.Repositories;
using System.Threading.Tasks;

namespace Oculus.Core.Commands.Modules.Moderation
{
    [Category(Category.Moderation)]
    [Group("settings")]
    public class Settings : OculusModule
    {
        public IGuildSettingsRepository GuildSettingsRepository { get; set; }

        [Command("prefix")]
        public async Task ExecuteAsync([Remainder] string prefix)
        {
            await GuildSettingsRepository.GetOrCreateForGuildAsync(Context.Guild.Id);

            _ = await GuildSettingsRepository.UpdatePrefixAsync(Context.Guild.Id, prefix);
            _ = SendDefaultEmbedAsync($"Updated prefix to \"{prefix}\"");
        }
    }
}
