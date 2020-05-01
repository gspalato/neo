using Axion.Core.Utilities;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;

namespace Axion.Services
{
	public interface IMusicService
	{ }

	public sealed class MusicService : IMusicService
	{
		private LavaNode LavaNode { get; }
		private ILoggingService Logger { get; }

		public MusicService(DiscordSocketClient client,
			LavaNode lavaNode, ILoggingService logger)
		{
			client.Ready += OnReady;
			LavaNode = lavaNode;
			Logger = logger;

			LavaNode.OnLog += OnLog;
			LavaNode.OnPlayerUpdated += OnPlayerUpdated;
			LavaNode.OnStatsReceived += OnStatsReceived;
			LavaNode.OnTrackEnded += OnTrackEnded;
			LavaNode.OnTrackException += OnTrackException;
			LavaNode.OnTrackStuck += OnTrackStuck;
			LavaNode.OnWebSocketClosed += OnWebSocketClosed;
		}

		private Task OnLog(LogMessage log)
		{
			Logger.Log(log.Severity, log.Message, log.Exception, "vict");

			return Task.CompletedTask;
		}

		private Task OnPlayerUpdated(PlayerUpdateEventArgs arg)
		{
			// Logger.LogInformation($"Player update received for {arg.Player.VoiceChannel.Name}.");
			return Task.CompletedTask;
		}

		private async Task OnReady()
		{
			await LavaNode.ConnectAsync().ConfigureAwait(false);
		}

		private Task OnStatsReceived(StatsEventArgs arg)
		{
			// Logger.LogInformation($"Lavalink Uptime {arg.Uptime}.");
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
				Logger.Warn("Next item in queue is not a track.");

				var errorEmbed = new EmbedBuilder()
					.WithTitle("Error")
					.WithWarning()
					.WithDescription($"An exception occurred.\n`MU0001`");

				await args.Player.TextChannel.SendMessageAsync(embed: errorEmbed.Build())
					.ConfigureAwait(false);

				return;
			}

			await args.Player.PlayAsync(track)
				.ConfigureAwait(false);

			var embed = new EmbedBuilder()
				.WithTitle(":notes: Now Playing")
				.WithDefaultColor()
				.WithDescription($"**[{track.Title.TruncateAndEscape()}]({track.Url})**");

			await args.Player.TextChannel.SendMessageAsync(embed: embed.Build())
				.ConfigureAwait(false);
		}

		private Task OnTrackException(TrackExceptionEventArgs arg)
		{
			Logger.Critical($"Track exception received for {arg.Track.Title}.");
			return Task.CompletedTask;
		}

		private Task OnTrackStuck(TrackStuckEventArgs arg)
		{
			Logger.Error($"Track stuck received for {arg.Track.Title}.");
			return Task.CompletedTask;
		}

		private Task OnWebSocketClosed(WebSocketClosedEventArgs arg)
		{
			Logger.Critical($"Discord WebSocket connection closed: {arg.Reason}");
			return Task.CompletedTask;
		}
	}
}