using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Muon;
using Muon.Kernel.Structures;

namespace Muon.Commands
{
	public partial class Admin : BaseCommandModule
	{
		[Command("eval")]
		[Aliases("$", "roslyn")]
		[Description("Evaluates C# code.")]
		[Hidden]
		[RequireOwner]
		public async Task EvalAsync(CommandContext ctx, [RemainingText] string code)
		{
			DiscordMessage msg = ctx.Message;

			var cs1 = code.IndexOf("```") + 3;
			cs1 = code.IndexOf('\n', cs1) + 1;
			var cs2 = code.LastIndexOf("```");

			if (cs1 == -1 || cs2 == -1)
				throw new ArgumentException("You need to wrap the code into a code block.");

			var cs = code.Substring(cs1, cs2 - cs1);

			msg = await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
				.WithColor(new DiscordColor(0xFF007F))
				.WithDescription("Evaluating...")
				.Build()).ConfigureAwait(false);

			try
			{
				var globals = new RoslynVariables
				{
					Client = ctx.Client,
					Message = ctx.Message,
					Context = ctx
				};

				var options = ScriptOptions.Default
					.WithImports(
						"System",
						"System.Collections.Generic",
						"System.Linq",
						"System.Text",
						"System.Threading.Tasks",
						"Microsoft.Extensions.DependencyInjection",
						"DSharpPlus",
						"DSharpPlus.CommandsNext",
						"DSharpPlus.Entities",
						"DSharpPlus.Interactivity",
						"Muon",
						"Muon.Kernel",
						"Muon.Kernel.Structures",
						"Muon.Kernel.Utilities",
						"Muon.Services"
					)
					.WithReferences(AppDomain.CurrentDomain.GetAssemblies()
					.Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

				var script = CSharpScript.Create(cs, options, typeof(RoslynVariables));
				script.Compile();

				var result = await script.RunAsync(globals).ConfigureAwait(false);

				if (result != null && result.ReturnValue != null && !string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
					await msg.ModifyAsync(embed: new DiscordEmbedBuilder
					{
						Title = "Evaluation Result",
						Description = result.ReturnValue.ToString(),
						Color = new DiscordColor("#007FFF")
					}.Build()).ConfigureAwait(false);
				else
					await msg.ModifyAsync(embed: new DiscordEmbedBuilder
					{
						Title = "Evaluation Successful",
						Description = "No result was returned.",
						Color = new DiscordColor("#007FFF")
					}.Build()).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await msg.ModifyAsync(embed: new DiscordEmbedBuilder
				{
					Title = "Evaluation Failure",
					Description = string.Concat("**", ex.GetType().ToString(), "**: ", ex.Message),
					Color = new DiscordColor("#FF0000")
				}.Build()).ConfigureAwait(false);
			}
		}
	}
}