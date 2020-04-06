using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

using Muon.Kernel.Structures;

namespace Muon.Services
{
	public interface IMusicService
	{
		public LavalinkExtension _lavalink { get; set; }
		public LavalinkNodeConnection _nodeConnection { get; set; }

		public Task Initialize(LavalinkExtension lavalink);
		public IPlayer GetPlayer(DiscordGuild guild);
		public bool RemovePlayer(DiscordGuild guild);
		public Task<LavalinkLoadResult> Resolve(string s);
	}

	public class MusicService : IMusicService
	{
		private readonly IConfiguration _configuration;

		public LavalinkExtension _lavalink { get; set; }
		public LavalinkNodeConnection _nodeConnection { get; set; }

		private readonly Dictionary<ulong, IPlayer> players = new Dictionary<ulong, IPlayer>();


		public MusicService(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task Initialize(LavalinkExtension lavalink)
		{
			_lavalink = lavalink;

			try
			{
				Console.WriteLine("Trying to connect to Lavalink...");
				_nodeConnection = await _lavalink.ConnectAsync(new LavalinkConfiguration
				{
					Password = _configuration.GetValue<string>("LAVALINK")
				});
			}
			catch
			{
				Console.WriteLine("Failed to connect. Trying again in 10 seconds.");

				await Task.Delay(10000);
				await this.Initialize(_lavalink);
				return;
			}
		}

		public IPlayer GetPlayer(DiscordGuild guild)
		{
			if (this.players.TryGetValue(guild.Id, out IPlayer player))
				return player;
			else
			{
				this.players.Add(guild.Id, new Player(this, _nodeConnection, guild));
				return this.GetPlayer(guild);
			}
		}

		public bool RemovePlayer(DiscordGuild guild) =>
			this.players.Remove(guild.Id);

		public async Task<LavalinkLoadResult> Resolve(string s)
		{
			if (this.IsValidLink(s))
				return await _nodeConnection.Rest.GetTracksAsync(new Uri(s));
			else
				return await _nodeConnection.Rest.GetTracksAsync(s);
		}

		private bool IsValidLink(string item)
		{
			var accepted = new string[]
			{
				"youtube.com",
				"youtu.be",
			};

			try
			{
				HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(item);
				request.Method = "HEAD";

				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
				{
					foreach (string domain in accepted)
					{
						string uri = response.ResponseUri.ToString();

						bool httpTest = uri.Substring(0, domain.Length + 6) == "http://" + domain;
						bool httpsTest = uri.Substring(0, domain.Length + 7) == "https://" + domain;

						if (httpsTest || httpTest)
							return true;
						else
							continue;
					}

					return false;
				}
			}
			catch
			{
				return false;
			}
		}
	}
}