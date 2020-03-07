using System;

using DSharpPlus;

using Muon.Core;

namespace Muon
{
	internal sealed class Program
	{
		static void Main(string[] args) =>
			new App().MainAsync(args).GetAwaiter().GetResult();
	}
}
