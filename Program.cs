using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;


namespace Arpa
{
	internal sealed class Program
	{
		static void Main(string[] args) =>
			new App().MainAsync(args).GetAwaiter().GetResult();
	}
}
