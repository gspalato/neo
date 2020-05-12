using Axion.Core.Extensions;
using Axion.Core.Structures.Attributes;
using Axion.Core.Structures.Miscellaneous;
using Axion.Core.Utilities;
using Discord;
using Microsoft.CodeAnalysis;
using Qmmands;
using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Axion.Core.Commands.Modules.Administration
{
	[Category(Category.Admin)]
	[Group("eval", "$", "roslyn")]
	public sealed class Evaluate : AxionModule
	{
		[Command]
		[RequireChannelBotPermissions(ChannelPermission.ManageMessages)]
		[RequireOwner]
		public async Task ExecuteAsync([Remainder] string text)
		{
			var match = Regex.Match(text, @"(?<=^```[a-z]*\n)[\s\S]*?(?=\n?```$)");
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
							.AddField($"Result: {value.GetType().Name}", Format.Code(res.EscapeCodeblock(), "js"))
							.WithFooter(new EmbedFooterBuilder()
								.WithText($"took {timeTook} · react with ❌ to delete."))
							.Build();
					});

					var reactionAwaiter = evalMessage.AwaitReaction(Context.Client,
						r => r.UserId == Context.Message.Author.Id && r.Emote.Name == "❌");
					var reaction = await reactionAwaiter;

					if (!reactionAwaiter.IsCompleted || reactionAwaiter.IsCanceled)
						return;

					if (reaction.Emote.Name == "❌")
						await evalMessage.DeleteAsync();
				}
			}
			catch (Exception ex)
			{
				var error = $"**{ex.GetType()}**: {ex.Message}\n{Format.Code(ex.StackTrace, "")}";
				await evalMessage.ModifyAsync(props =>
					props.Embed = CreateErrorEmbed(error).Build());
			}
		}
	}
}