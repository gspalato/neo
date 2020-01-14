using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;

using Arpa.Services;

namespace Arpa.Structures
{
	public interface IPlayer
	{
		void Push(LavalinkTrack track);
		Task Play(DiscordChannel channel, DiscordChannel textChannel);
	}

	public class Player : IPlayer
	{
		public readonly MusicService musicService;

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
			this.guildId = guild.Id;
			this.nodeConnection = node;
			this.musicService = musicService;
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

			this.connection = this.connection ?? await this.nodeConnection.ConnectAsync(channel);

			LavalinkTrack next;
			if (this.isLooping)
				next = current;
			else
			{
				if (this.queue.TryDequeue(out LavalinkTrack track))
				{
					next = track;
				}
				else
				{
					Console.WriteLine("Empty queue.");

					this.musicService.RemovePlayer(channel.Guild);
					this.connection.Stop();

					this.isPlaying = false;
					this.textChannel = null;

					return;
				}
			}

			this.connection.Play(next);
			this.textChannel = textChannel;
			this.current = next;
			this.isPlaying = true;

			this.connection.PlaybackFinished -= HandleTrackFinish;
			this.connection.PlaybackFinished += HandleTrackFinish;
		}

		private async Task HandleTrackFinish(TrackFinishEventArgs e)
		{
			await this.Play(e.Player.Channel, this.textChannel);
		}
	}
}