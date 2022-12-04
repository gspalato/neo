using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using System.Reflection;
using Oculus.Kernel.Services;
using System.Windows.Input;
using Lavalink4NET;
using Microsoft.Extensions.Logging;

namespace Oculus.Kernel
{
    public interface IApp : IHostedService { }

    public class App : IApp
    {
        private readonly IConfiguration _configuration;
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactionService;

        private readonly CommandHandlerService _commandHandlerService;
        private readonly IAudioService _audioService;
        private readonly ILoggingService _logger;

        public App(IConfiguration configuration, DiscordSocketClient client,
            InteractionService interactionService, CommandHandlerService commandHandlerService,
            IAudioService audioService, ILoggingService logger)
        {
            _configuration = configuration;
            _client = client;
            _interactionService = interactionService;

            _commandHandlerService = commandHandlerService;
            _audioService = audioService;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;

            await _client.LoginAsync(TokenType.Bot, _configuration.GetValue<string>("TOKEN"));
            await _client.StartAsync();

            await _commandHandlerService.InitializeAsync();

            await Task.Delay(-1, cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _audioService.Dispose();

            await _client.LogoutAsync();
            await _client.StopAsync();
        }

        private async Task ReadyAsync()
        {
            ulong mainGuildId = _configuration.GetValue<ulong>("MAIN_GUILD");

            _logger.Info($"In debug mode, adding commands to {mainGuildId}...", className: "App");
            await _interactionService.RegisterCommandsToGuildAsync(mainGuildId);

            await _audioService.InitializeAsync();
        }
        private Task LogAsync(LogMessage msg)
        {
            switch (msg.Severity)
            {
                default:
                case LogSeverity.Info:
                    _logger.Info(msg.Message, msg.Exception, msg.Source);
                    break;

                case LogSeverity.Warning:
                    _logger.Warn(msg.Message, msg.Exception, msg.Source);
                    break;

                case LogSeverity.Error:
                    _logger.Error(msg.Message, msg.Exception, msg.Source);
                    break;

                case LogSeverity.Critical:
                    _logger.Critical(msg.Message, msg.Exception, msg.Source);
                    break;

                case LogSeverity.Debug:
                    _logger.Debug(msg.Message, msg.Exception, msg.Source);
                    break;

                case LogSeverity.Verbose:
                    _logger.Verbose(msg.Message, msg.Exception, msg.Source);
                    break;
            };

            return Task.CompletedTask;
        }
    }
}