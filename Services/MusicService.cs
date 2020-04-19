using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Muon.Kernel.Utilities;
using Victoria;
using Victoria.EventArgs;

namespace Muon.Services
{
	public interface IMusicService
	{ }

	public sealed class MusicService : IMusicService
	{
		private LavaNode _lavaNode { get; }
		private ILoggingService _logger { get; }

		public MusicService(DiscordSocketClient client,
			LavaNode lavaNode, ILoggingService logger)
		{
			client.Ready += OnReady;
			_lavaNode = lavaNode;
			_logger = logger;

			_lavaNode.OnLog += OnLog;
			_lavaNode.OnPlayerUpdated += OnPlayerUpdated;
			_lavaNode.OnStatsReceived += OnStatsReceived;
			_lavaNode.OnTrackEnded += OnTrackEnded;
			_lavaNode.OnTrackException += OnTrackException;
			_lavaNode.OnTrackStuck += OnTrackStuck;
			_lavaNode.OnWebSocketClosed += OnWebSocketClosed;
		}

		private Task OnLog(LogMessage log)
		{
			_logger.Log(log.Severity, log.Message, log.Exception, "vict");

			return Task.CompletedTask;
		}

		private Task OnPlayerUpdated(PlayerUpdateEventArgs arg)
		{
			// _logger.LogInformation($"Player update received for {arg.Player.VoiceChannel.Name}.");
			return Task.CompletedTask;
		}

		private Task OnStatsReceived(StatsEventArgs arg)
		{
			// _logger.LogInformation($"Lavalink Uptime {arg.Uptime}.");
			return Task.CompletedTask;
		}

		private async Task OnTrackEnded(TrackEndedEventArgs args)
		{
			if (!args.Reason.ShouldPlayNext())
				return;

			var player = args.Player;
			if (!player.Queue.TryDequeue(out var queueable))
				return;

			if (!(queueable is LavaTrack track))
			{
				_logger.Warn("Next item in queue is not a track.");

				EmbedBuilder errorEmbed = new EmbedBuilder()
					.WithTitle("Error")
					.WithWarning()
					.WithDescription($"An exception ocurred.\n`MU0001`");

				await args.Player.TextChannel.SendMessageAsync(embed: errorEmbed.Build())
					.ConfigureAwait(false);

				return;
			}

			await args.Player.PlayAsync(track)
				.ConfigureAwait(false);

			EmbedBuilder embed = new EmbedBuilder()
				.WithTitle(":notes: Now Playing")
				.WithDefaultColor()
				.WithDescription($"**[{track.Title.TruncateAndEscape()}]({track.Url})**");

			await args.Player.TextChannel.SendMessageAsync(embed: embed.Build())
				.ConfigureAwait(false);
		}

		private Task OnTrackException(TrackExceptionEventArgs arg)
		{
			_logger.Critical($"Track exception received for {arg.Track.Title}.");
			return Task.CompletedTask;
		}

		private Task OnTrackStuck(TrackStuckEventArgs arg)
		{
			_logger.Error($"Track stuck received for {arg.Track.Title}.");
			return Task.CompletedTask;
		}

		private Task OnWebSocketClosed(WebSocketClosedEventArgs arg)
		{
			_logger.Critical($"Discord WebSocket connection closed: {arg.Reason}");
			return Task.CompletedTask;
		}

		private async Task OnReady()
		{
			await _lavaNode.ConnectAsync().ConfigureAwait(false);
		}
	}
}