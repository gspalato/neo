using System;
using System.Collections.Generic;
using System.Reflection;
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

		public LavalinkTrack current;
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
			this.queue.Add(track);
		}

		public async Task Play(DiscordChannel channel)
		{
			if (this.connection != null && channel.Id != this.connection.Channel.Id)
			{
				Console.WriteLine("Player already in use!");
				return;
			}

			this.connection = await this.nodeConnection.ConnectAsync(channel);

			LavalinkTrack next = this.isLooping ? this.current : this.queue[0];
			if (next.Equals(null))
			{
				this.musicService.RemovePlayer(channel.Guild);
				this.connection.Stop();
				this.isPlaying = false;
			}

			this.queue.RemoveAt(0);

			this.connection.Play(next);
			this.current = next;
			this.isPlaying = true;
		}
	}
}