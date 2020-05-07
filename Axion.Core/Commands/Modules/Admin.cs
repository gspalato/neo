using Axion.Core.Extensions;
using Axion.Core.Structures.Attributes;
using Axion.Core.Structures.Miscellaneous;
using Axion.Core.Utilities;
using Discord;
using Qmmands;
using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Axion.Core.Commands.Modules
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
			var match = Regex.Match(text, @"(?<=^```[a-z]*\n)[\s\S]*?(?=\n?```$)");
			if (!match.Success)
				throw new ArgumentException("You need to wrap the code into a code block.");

			var code = match.Value;
			Console.WriteLine(code);
			var evalMessage = await SendDefaultEmbedAsync("Evaluating...");

			try
			{
				var globals = new RoslynVariables
				{
					Client = Context.Client,
					Message = Context.Message,
					Context = Context
				};

                var result = await ScriptingUtility.EvaluateScriptAsync(code, globals);
				if (!result.IsSuccess)
                {
                    var message = result.CompilationDiagnostics.First(a => a.Severity == DiagnosticSeverity.Error)
                        .GetMessage();

					await SendErrorAsync($"Evaluation failed at **{result.FailedStage}** step.\n{Format.Code(message, "")}");
					return;
				}

				var value = result.ReturnValue;
				var timeTook = TimeSpan.FromMilliseconds(result.ExecutionTime).ToHumanDuration();

				if (value is null)
				{
					await evalMessage.DeleteAsync();
					await Context.Message.AddReactionAsync(new Emoji("✅"));
				}
				else
                {
                    var res = value switch
					{
						string str => str,
						IEnumerable enumerable => string.Join("\n", enumerable.Cast<object>().Select(x => $"{x}")),
						_ => EvaluationUtility.SerializeObject(value)
					};

					await evalMessage.ModifyAsync(props =>
					{
						props.Embed = CreateDefaultEmbed(Format.Code(code.EscapeCodeblock(), "csharp"))
							.AddField($"Result: {result.GetType().Name}", Format.Code(res.EscapeCodeblock(), "js"))
							.WithFooter(new EmbedFooterBuilder()
								.WithText($"Took {timeTook} | React with ❌ to delete."))
							.Build();
					});

					var reactionAwaiter = evalMessage.AwaitReaction(Context.Client, (r) =>
						r.UserId == Context.Message.Author.Id && r.Emote.Name == "❌");
					var reaction = await reactionAwaiter;

					if (!reactionAwaiter.IsCompleted || reactionAwaiter.IsCanceled)
						return;

					if (reaction.Emote.Name == "❌")
						await evalMessage.DeleteAsync();
				}
			}
			catch (Exception ex)
			{
				var error = string.Concat("**", ex.GetType().ToString(), "**: ", ex.Message, "\n", Format.Code(ex.StackTrace, ""));
				await evalMessage.ModifyAsync(props =>
					props.Embed = CreateErrorEmbed($"{error}").Build());
			}
		}
	}
}