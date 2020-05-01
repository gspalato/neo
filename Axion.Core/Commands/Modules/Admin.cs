using Axion.Structures.Attributes;
using Axion.Structures.Miscellaneous;
using Axion.Utilities;
using Discord;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Axion.Commands.Modules
{
	[Category("Administration")]
	[Description("Elite-only commands.")]
	public sealed class Admin : AxionModule
	{
		[Command("eval", "$", "roslyn")]
		[Description("Evaluates C# code.")]
		[RequireOwner]
		public async Task EvalAsync([Remainder] string code)
		{
			var cs1 = code.IndexOf("```csharp", StringComparison.Ordinal) + 9;
			cs1 = code.IndexOf('\n', cs1) + 1;
			var cs2 = code.LastIndexOf("```", StringComparison.Ordinal);

			if (cs1 == -1 || cs2 == -1)
				throw new ArgumentException("You need to wrap the code into a code block.");

			var cs = code.Substring(cs1, cs2 - cs1);

			var evalMessage = await Context.ReplyAsync(new EmbedBuilder()
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
						"Axion",
						"Axion",
						"Axion.Structures",
						"Axion.Utilities",
						"Axion.Services")
					.WithReferences(AppDomain.CurrentDomain.GetAssemblies()
					.Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

				var script = CSharpScript.Create(cs, options, typeof(RoslynVariables));
				script.Compile();

				var result = await script.RunAsync(globals).ConfigureAwait(false);

				if (result?.ReturnValue != null && !string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
				{
					await evalMessage.ModifyAsync(props =>
					{
						props.Embed = new EmbedBuilder()
							.WithSuccess()
							.WithDescription($"```json\n{result.ReturnValue.ToString().Escape('`')}\n```")
							.Build();
					});
				}
				else
				{
					await evalMessage.ModifyAsync(props =>
					{
						props.Embed = new EmbedBuilder()
							.WithSuccess()
							.WithDescription("No result was returned.")
							.Build();
					});
				}
			}
			catch (Exception ex)
			{
				var error = string.Concat("**", ex.GetType().ToString(), "**: ", ex.Message);
				await evalMessage.ModifyAsync(props =>
				{
					props.Embed = new EmbedBuilder()
						.WithError()
						.WithDescription(error)
						.Build();
				});
			}
		}
	}
}