using Axion.Core.Extensions;
using Axion.Core.Structures.Attributes;
using Axion.Core.Structures.Miscellaneous;
using Axion.Core.Utilities;
using Discord;
using Newtonsoft.Json;
using Qmmands;
using System;
using System.Collections;
using System.Linq;
using System.Text;
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


				var result = await ScriptingUtility.EvaluateScriptAsync(code, globals);
				if (!result.IsSuccess)
				{
					await SendErrorAsync($"Evaluation failed at **{result.FailedStage}** step.");
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
					var jsonSettings = new JsonSerializerSettings()
					{
						Formatting = Formatting.Indented,
						MaxDepth = 2,
						NullValueHandling = NullValueHandling.Include,
						PreserveReferencesHandling = PreserveReferencesHandling.None,
						ReferenceLoopHandling = ReferenceLoopHandling.Ignore
					};

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
				var error = string.Concat("**", ex.GetType().ToString(), "**: ", ex.Message);
				await evalMessage.ModifyAsync(props =>
					props.Embed = CreateErrorEmbed($"{error}").Build());
			}
		}
	}
}