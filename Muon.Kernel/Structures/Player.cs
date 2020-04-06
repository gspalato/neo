using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;

using Muon.Services;
using Muon.Kernel.Utilities;

namespace Muon.Kernel.Structures
{
	public interface IPlayer
	{
		void Push(LavalinkTrack track);
		Task Play(DiscordChannel channel, DiscordChannel textChannel);
	}

	public class Player : IPlayer
	{
		private readonly MusicService _musicService;

		public readonly ulong guildId;
		public readonly ConcurrentQueue<LavalinkTrack> queue = new ConcurrentQueue<LavalinkTrack>();
		public readonly LavalinkNodeConnection nodeConnection;

		public LavalinkGuildConnection connection;

		public LavalinkTrack current;
		public DiscordChannel textChannel;
		public bool isLooping = false;
		public bool isPlaying = false;

		public Player(MusicService musicService, LavalinkNodeConnection node, DiscordGuild guild)
		{
			guildId = guild.Id;
			nodeConnection = node;
			_musicService = musicService;
		}

		public void Push(LavalinkTrack track)
		{
			this.queue.Enqueue(track);
		}

		public async Task Play(DiscordChannel channel, DiscordChannel textChannel)
		{
			if (this.connection != null && !channel.Equals(this.connection.Channel))
			{
				Console.WriteLine("Player already in use!");
				return;
			}

			this.connection = await this.ConnectAsync(channel);

			LavalinkTrack next;
			if (this.isLooping)
				next = current;
			else
			{
				if (this.queue.TryDequeue(out LavalinkTrack track))
					next = track;
				else
				{
					this.Stop();
					return;
				}
			}

			await this.connection.PlayAsync(next);
			this.textChannel = textChannel;
			this.current = next;
			this.isPlaying = true;

			await this.HandleTrackStart(next);
		}

		public void Skip()
		{
			this.connection.StopAsync();
		}

		public void Stop()
		{
			_musicService.RemovePlayer(this.connection.Guild);
			connection.StopAsync();
			connection.DisconnectAsync();

			isPlaying = false;
			textChannel = null;
			connection = null;
		}

		private async Task<LavalinkGuildConnection> ConnectAsync(DiscordChannel channel)
		{
			if (connection != null)
				return connection;
			else
			{
				connection = await nodeConnection.ConnectAsync(channel);
				connection.PlaybackFinished -= HandleTrackFinish;
				connection.PlaybackFinished += HandleTrackFinish;

				return connection;
			}
		}

		private async Task HandleTrackStart(LavalinkTrack track)
		{
			DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
					.WithTitle("🎶 Now Playing")
					.WithDescription($"**[{track.Title.Escape()}]({track.Uri})**")
					.WithColor(new DiscordColor(0x2F3136))
					.WithTimestamp(new DateTimeOffset(DateTime.Now));

			await this.textChannel.SendMessageAsync(embed: embed.Build());
		}

		private async Task HandleTrackFinish(TrackFinishEventArgs e)
		{
			if (connection == null || !connection.IsConnected)
				return;

			await this.Play(e.Player.Channel, this.textChannel);
		}
	}
}