using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Victoria;
using Victoria.Node.EventArgs;
using Victoria.Node;
using Victoria.Player;
using System.Text.Json;
using Lavalink4NET;
using Lavalink4NET.Events;

namespace Oculus.Kernel.Services
{
    public sealed class MusicService
    {
        private readonly IAudioService _audioService;

        public MusicService(IAudioService audioService)
        {
            _audioService = audioService;
        }

        public async Task StartAsync()
        {
            await _audioService.InitializeAsync();
        }
    }
}
