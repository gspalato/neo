using Axion.Core.Extensions;
using Axion.Core.Structures.Attributes;
using Axion.Core.Structures.Miscellaneous;
using Axion.Core.Utilities;
using Discord;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using Qmmands;
using System;
using System.Collections;
using System.Diagnostics;
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
						_ => SerializeObject(result)
					};

					await evalMessage.ModifyAsync(props =>
					{
						props.Embed = CreateDefaultEmbed(Format.Code(code.EscapeCodeblock(), "csharp"))
							.AddField($"Result: {result.GetType().Name}", Format.Code(res.EscapeCodeblock(), "js"))
							.WithFooter(new EmbedFooterBuilder()
								.WithText($"Took {sw.Elapsed.ToHumanDuration()} | React with ❌ to delete."))
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
					props.Embed = CreateErrorEmbed($"{error}\n{ex.StackTrace}").Build());
			}
		}

		private string SerializeObject(object obj, bool serializeInner = true)
		{
			var type = obj.GetType();

			if (type.IsPrimitive)
				return obj.ToString();
			else if (obj is string)
				return $"\"{obj.ToString().Replace("\n", @"\n")}\"";
			else if (obj is decimal)
				return obj.ToString();

			static string ReplaceIndex(string s)
			{
				var indexed = Regex.Match(s, @"(?<=([a-zA-Z]+)`)([0-9]+)");
				if (indexed.Success)
					return s.ReplaceAt(indexed.Index - 1, indexed.Length + 1, $"[{indexed.Value}]");
				else
					return s;
			}

			if (!serializeInner)
				return $"[{ReplaceIndex(type.Name)}]";

			var props = type.GetProperties();

			var builder = new StringBuilder();
			builder.AppendLine($"{type.Name} {{");

			int total = 0;
			foreach (var prop in props)
			{
				if (total >= 10)
				{
					builder.AppendLine("\t...");
					break;
				}

				var value = prop.GetValue(obj);
				string serialized;
				if (value != null)
					serialized = SerializeObject(value, false);
				else
					serialized = null;

				string typeName = ReplaceIndex(prop.PropertyType.Name);

				builder.Append($"\t<{typeName}> {prop.Name}");
				builder.Append($": {serialized ?? "null"}\n");

				total++;
			}

			builder.Append("}");

			return builder.ToString();
		}
	}
}