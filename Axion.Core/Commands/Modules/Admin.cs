using Axion.Core.Structures.Attributes;
using Axion.Core.Structures.Miscellaneous;
using Axion.Core.Utilities;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Discord;
using System.Diagnostics;
using System.Collections;

namespace Axion.Commands.Modules
{
	[Category("Administration")]
	[Description("Elite-only commands.")]
	public sealed class Admin : AxionModule
	{
		[Command("eval", "$", "roslyn")]
		[Description("Evaluates C# code.")]
		[RequireChannelBotPermissions(ChannelPermission.ManageMessages)]
		[RequireOwner]
		public async Task EvalAsync([Remainder] string text)
		{
			var match = Regex.Match(text, @"(?<=```(csharp\n)?(\n)?)(.*)(?=\n?```)");
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

				var sw = Stopwatch.StartNew();
				var result = await CSharpScript.EvaluateAsync(code, options, globals, typeof(RoslynVariables)).ConfigureAwait(false);
				sw.Stop();

				if (result is null)
				{
					await evalMessage.DeleteAsync();
					await Context.Message.AddReactionAsync(new Emoji("✅"));
				}
				else
				{
					var jsonSettings = new JsonSerializerSettings()
					{
						Formatting = Formatting.Indented,
						MaxDepth = 2,
						NullValueHandling = NullValueHandling.Include,
						PreserveReferencesHandling = PreserveReferencesHandling.None,
						ReferenceLoopHandling = ReferenceLoopHandling.Ignore
					};

					var res = result switch
					{
						string str => str,
						IEnumerable enumerable => string.Join("\n", enumerable.Cast<object>().Select(x => $"{x}")),
						_ => JsonConvert.SerializeObject(result, jsonSettings)
					};

					await evalMessage.ModifyAsync(props =>
					{
						props.Embed = CreateDefaultEmbed(Format.Code(res, "json"))
							.AddField("Took", $"{sw.Elapsed.ToHumanDuration()}", true)
							.AddField("Returned", $"{result.GetType()}", true)
							.Build();
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