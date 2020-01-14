using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

using Arpa;
using Arpa.Structures;

namespace Arpa.Services
{
	public class MusicService
	{
		public readonly DiscordClient client;

		public LavalinkExtension lavalink;
		public LavalinkNodeConnection nodeConnection;

		private readonly Dictionary<ulong, IPlayer> players = new Dictionary<ulong, IPlayer>();

		public MusicService(DiscordClient client)
		{
			this.client = client;
		}

		public async Task Initialize(LavalinkExtension lavalink)
		{
			this.lavalink = lavalink;

			try
			{
				Console.WriteLine("Trying to connect to Lavalink...");
				this.nodeConnection = await this.lavalink.ConnectAsync(new LavalinkConfiguration
				{
					Password = "bluisthebestbotever"
				});
			}
			catch
			{
				Console.WriteLine("Failed to connect. Trying again in 10 seconds.");

				await Task.Delay(10000);

				await this.Initialize(lavalink);
			}
		}

		public IPlayer GetPlayer(DiscordGuild guild)
		{
			if (this.players.TryGetValue(guild.Id, out IPlayer player))
				return player;
			else
			{
				this.players.Add(guild.Id, new Player(this, this.nodeConnection, guild));
				return this.GetPlayer(guild);
			}
		}

		public bool RemovePlayer(DiscordGuild guild)
		{
			return this.players.Remove(guild.Id);

		}

		public async Task<LavalinkLoadResult> Resolve(string s)
		{
			return await this.nodeConnection.GetTracksAsync(new Uri(s));
		}
	}
}