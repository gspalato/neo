using Discord;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Muon.Kernel.Structures;
using Muon.Kernel.Structures.Attributes;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Muon.Commands
{
	[Category("Administration")]
	[Description("Elite-only commands.")]
	public sealed class Admin : MuonModule
	{

		[Command("eval", "$", "roslyn")]
		[Description("Evaluates C# code.")]
		[RequireOwner]
		public async Task EvalAsync([Remainder] string code)
		{
			IUserMessage msg = Context.Message;

			var cs1 = code.IndexOf("```") + 3;
			cs1 = code.IndexOf('\n', cs1) + 1;
			var cs2 = code.LastIndexOf("```");

			if (cs1 == -1 || cs2 == -1)
				throw new ArgumentException("You need to wrap the code into a code block.");

			var cs = code.Substring(cs1, cs2 - cs1);

			var evalMessage = await Context.ReplyAsync(embed: new EmbedBuilder()
				.WithColor(new Color(0xFF007F))
				.WithDescription("Evaluating...")
				.Build()).ConfigureAwait(false);

			try
			{
				var globals = new RoslynVariables
				{
					Client = Context.Client,
					Message = Context.Message,
					Context = Context
				};

				var options = ScriptOptions.Default
					.WithImports(
						"System",
						"System.Collections.Generic",
						"System.Linq",
						"System.Text",
						"System.Threading.Tasks",
						"Microsoft.Extensions.DependencyInjection",
						"Discord",
						"Discord.Net",
						"Discord.Rest",
						"Discord.WebSocket",
						"Muon",
						"Muon.Kernel",
						"Muon.Kernel.Structures",
						"Muon.Kernel.Utilities",
						"Muon.Services")
					.WithReferences(AppDomain.CurrentDomain.GetAssemblies()
					.Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

				var script = CSharpScript.Create(cs, options, typeof(RoslynVariables));
				script.Compile();

				var result = await script.RunAsync(globals).ConfigureAwait(false);

				if (result != null && result.ReturnValue != null && !string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
					await SendOkAsync(result.ReturnValue.ToString());
				else
					await SendOkAsync("No result was returned.");
			}
			catch (Exception ex)
			{
				await SendErrorAsync(
					string.Concat("**", ex.GetType().ToString(), "**: ", ex.Message));
			}
		}
	}
}