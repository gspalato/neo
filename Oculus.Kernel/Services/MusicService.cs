using Lavalink4NET;

namespace Oculus.Kernel.Services
{
    public interface IMusicService : IAudioService
    {
        public bool IsInitialized { get; }
    }

    public sealed class MusicService : LavalinkNode, IMusicService
    {
        public bool IsInitialized { get; private set; } = false;
        private readonly ILoggingService _logger;

        public MusicService(LavalinkNodeOptions options, IDiscordClientWrapper client,
            ILoggingService logger) : base(options, client)
        {
            _logger = logger;
        }

        public new async Task InitializeAsync()
        {
            try
            {
                await base.InitializeAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return;
            }

            IsInitialized = true;
        }
    }
}
