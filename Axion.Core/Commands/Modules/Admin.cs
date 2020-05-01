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
		public async Task EvalAsync([Remainder] string text)
		{
			var match = Regex.Match(text, @"(?<=```(csharp\n)?|\n?)(.*)(?=```)");
			if (!match.Success)
				throw new ArgumentException("You need to wrap the code into a code block.");

			var code = match.Value;

			var evalMessage = await SendDefaultEmbedAsync("Evaluating...");

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
						"Axion.Core.Structures",
						"Axion.Core.Utilities",
						"Axion.Core.Services")
					.WithReferences(AppDomain.CurrentDomain.GetAssemblies()
					.Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

				var script = CSharpScript.Create(code, options, typeof(RoslynVariables));
				script.Compile();

				var result = await script.RunAsync(globals).ConfigureAwait(false);

				if (result?.ReturnValue != null && !string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
				{
					await evalMessage.ModifyAsync(props =>
					{
						props.Embed = CreateDefaultEmbed($"```json\n{result.ReturnValue.ToString().Escape('`', true)}\n```").Build();
					});
				}
				else
				{
					await evalMessage.ModifyAsync(props =>
					{
						props.Embed = CreateDefaultEmbed("No result was returned.").Build();
					});
				}
			}
			catch (Exception ex)
			{
				var error = string.Concat("**", ex.GetType().ToString(), "**: ", ex.Message);
				await evalMessage.ModifyAsync(props =>
				{
					props.Embed = CreateErrorEmbed(error).Build();
				});
			}
		}
	}
}