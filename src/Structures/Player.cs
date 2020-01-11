using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;

using Arpa;
using Arpa.Services;

namespace Arpa.Structures
{
	public interface IPlayer
	{
		void Push(LavalinkTrack track);
		Task Play(DiscordChannel channel);
	}

	public class Player : IPlayer
	{
		public readonly MusicService musicService;

		public readonly ulong guildId;
		public readonly List<LavalinkTrack> queue = new List<LavalinkTrack>();
		public readonly LavalinkNodeConnection nodeConnection;

		public LavalinkGuildConnection connection;

		public Player(MusicService musicService, LavalinkNodeConnection node, DiscordGuild guild)
		{
			this.guildId = guild.Id;
			this.nodeConnection = node;
			this.musicService = musicService;
		}

		public void Push(LavalinkTrack track)
		{
			this.queue.Add(track);
		}

		public async Task Play(DiscordChannel channel)
		{
			if (this.connection != null)
			{
				Console.WriteLine("Already in use.");
			}

			this.connection = await this.nodeConnection.ConnectAsync(channel);

			LavalinkTrack track = this.queue[0];
			if (track.Equals(null))
			{
				this.musicService.RemovePlayer(channel.Guild);
				this.connection.Stop();
			}

			this.queue.RemoveAt(0);

			this.connection.Play(track);

			this.connection.PlaybackFinished -= this.HandleTrackEnd;
			this.connection.PlaybackFinished += this.HandleTrackEnd;
		}

		private async Task HandleTrackEnd(TrackFinishEventArgs e)
		{
			await this.Play(e.Player.Channel);
		}
	}
}