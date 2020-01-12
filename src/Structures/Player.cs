using System;
using System.Collections.Generic;
using System.Linq;
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
		Task Play(DiscordChannel channel, DiscordChannel textChannel);
	}

	public class Player : IPlayer
	{
		public readonly MusicService musicService;

		public readonly ulong guildId;
		public readonly List<LavalinkTrack> queue = new List<LavalinkTrack>();
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
			this.queue.Add(track);
		}

		public async Task Play(DiscordChannel channel, DiscordChannel textChannel)
		{
			if (this.connection != null && !channel.Equals(this.connection.Channel))
			{
				Console.WriteLine("Player already in use!");
				return;
			}

			this.connection = await this.nodeConnection.ConnectAsync(channel);

			if (this.queue.Count == 0)
			{
				Console.WriteLine("Empty queue.");

				this.musicService.RemovePlayer(channel.Guild);
				this.connection.Stop();

				this.isPlaying = false;
				this.textChannel = null;

				return;
			}

			LavalinkTrack next = this.isLooping ? this.current : this.queue[0];
			this.queue.RemoveAt(0);

			this.connection.Play(next);
			this.textChannel = textChannel;
			this.current = next;
			this.isPlaying = true;

			await this.HandleTrackStart(next);

			this.connection.PlaybackFinished -= HandleTrackFinish;
			this.connection.PlaybackFinished += HandleTrackFinish;
		}

		private Nullable<LavalinkTrack> GetNextFromQueue(bool delete = false)
		{
			if (this.queue.Count == 0)
				return null;

			LavalinkTrack next = this.queue.ElementAt(0);
			if (delete)
				this.queue.RemoveAt(0);

			return next;
		}

		private async Task HandleTrackStart(LavalinkTrack track)
		{
			DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
				.WithTitle("🎶 Now Playing")
				.WithDescription($"[{track.Title}]({track.Uri})")
				.WithColor(new DiscordColor(0xAA0099));

			await this.textChannel.SendMessageAsync(embed: embed.Build());
		}

		private async Task HandleTrackFinish(TrackFinishEventArgs e)
		{
			if (this.GetNextFromQueue().Equals(null))
				return;

			await this.Play(e.Player.Channel, this.textChannel);
		}
	}
}