using Axion.Core.Structures.Attributes;
using Axion.Core.Structures.Miscellaneous;
using Axion.Core.Utilities;
using Discord;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Qmmands;
using System;
using System.Linq;
using System.Text.RegularExpressions;
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
			var match = Regex.Match(code, @"(?<=```(csharp\n)?)(.*)(?=```)");
			if (!match.Success)
				throw new ArgumentException("You need to wrap the code into a code block.");

			var cs = match.Value;

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
						"Axion.Core.Structures",
						"Axion.Core.Utilities",
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