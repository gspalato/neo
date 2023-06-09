using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Oculus.Common.Entities;
using Oculus.Core.Repositories;
using Oculus.Core.Services;

namespace Oculus.Core
{
    public interface IApp : IHostedService { }

    public class App : IApp
    {
        private readonly IConfiguration _configuration;
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactionService;

        private readonly CommandHandlerService _commandHandlerService;
        private readonly InteractiveService _interactiveService;
        private readonly IMusicService _musicService;
        private readonly ILoggingService _logger;

        private readonly IGuildSettingsRepository _guildSettingsRepository;

        public App(IConfiguration configuration, DiscordSocketClient client,
            InteractionService interactionService, CommandHandlerService commandHandlerService,
            InteractiveService interactiveService, IMusicService musicService,
            ILoggingService logger, IGuildSettingsRepository guildSettingsRepository)
        {
            _configuration = configuration;
            _client = client;
            _interactionService = interactionService;

            _commandHandlerService = commandHandlerService;
            _interactiveService = interactiveService;
            _musicService = musicService;
            _logger = logger;

            _guildSettingsRepository = guildSettingsRepository;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;

            _interactiveService.Log += LogAsync;

            await _client.LoginAsync(TokenType.Bot, _configuration.GetValue<string>("Discord:Token"));
            await _client.StartAsync();

            await _commandHandlerService.InitializeAsync();

            await Task.Delay(-1, cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _musicService.Dispose();

            await _client.LogoutAsync();
            await _client.StopAsync();
        }

        private async Task ReadyAsync()
        {
            ulong mainGuildId = _configuration.GetValue<ulong>("Discord:MainGuildId");

            _logger.Info($"In debug mode, adding commands to {mainGuildId}...", className: "App");
            await _interactionService.RegisterCommandsToGuildAsync(mainGuildId);

            await _musicService.InitializeAsync();

            foreach (var guild in _client.Guilds)
            {
                GuildSettings? settings = await _guildSettingsRepository.GetGuildSettingsAsync(guild.Id);
                settings ??= await _guildSettingsRepository.CreateGuildSettingsAsync(guild.Id);

                if (settings is null)
                    _logger.Error($"Failed to create guild settings for guild {guild.Id}.");
            }
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