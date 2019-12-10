using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.WebSocket;


namespace Arpa
{
	internal sealed class Program
	{
		static void Main(string[] args) =>
			new App().MainAsync(args).GetAwaiter().GetResult();
	}
}
