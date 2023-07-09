using Lavalink4NET;

namespace Neo.Core.Services
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

        public new Task InitializeAsync() => InitializeAsync(0);

        private async Task InitializeAsync(int currentAttempt)
        {
            try
            {
                await base.InitializeAsync();
            }
            catch (Exception ex)
            {
                _logger.Warn($"Failed to initialize Lavalink node. Retrying in 2 seconds. Attempt {currentAttempt + 1} of 10.", null, nameof(MusicService));
                await Task.Delay(2000);

                if (currentAttempt < 10)
                    await InitializeAsync(currentAttempt + 1);
                else
                    _logger.Error(ex.Message, ex, nameof(MusicService));

                return;
            }

            _logger.Info("Lavalink node initialized.", null, nameof(MusicService));
            IsInitialized = true;
        }
    }
}