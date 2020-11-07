using Spade.Common.Extensions;
using Spade.Common.Structures;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;
using System;

namespace Spade.Core.Services
{
	public interface IMusicService
	{ }

	public sealed class MusicService : ServiceBase, IMusicService
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

		private async Task OnReady()
		{
			await LavaNode.ConnectAsync().ConfigureAwait(false);
		}

		private async Task OnTrackEnded(TrackEndedEventArgs args)
		{
			Console.WriteLine("reached track end event");
			if (!args.Reason.ShouldPlayNext())
				return;

			var player = args.Player;
			if (!player.Queue.TryDequeue(out var queueable))
				return;

			if (queueable is not LavaTrack track)
			{
				Logger.Warn("Next item in queue is not a track.");

				var errorEmbed = new EmbedBuilder()
					.WithTitle("Error")
					.WithWarning()
					.WithDescription($"An exception occurred.\n`SPD001`");

				await args.Player.TextChannel.SendMessageAsync(embed: errorEmbed.Build())
					.ConfigureAwait(false);

				return;
			}

			await args.Player.PlayAsync(track)
				.ConfigureAwait(false);

			var embed = new EmbedBuilder()
				.WithTitle(":notes: Now Playing")
				.WithDefaultColor()
				.WithDescription($"**[{track.Title.TruncateAndSanitize()}]({track.Url})**");

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