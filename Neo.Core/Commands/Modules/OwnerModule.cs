using Discord;
using Discord.Interactions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Neo.Common.Utilities;
using Neo.Core.Structures;
using System.Reflection;

namespace Neo.Core.Commands.Modules
{
    [RequireOwner]
    public class OwnerModule : InteractionModuleBase
    {
        private readonly IServiceProvider _serviceProvider;

        public OwnerModule(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [SlashCommand("eval", "Evaluates C# code.")]
        public async Task EvaluateAsync(string code)
        {
            try
            {
                var result = await EvaluateAsync(Context, code);

                if (result != null && result.ReturnValue != null)
                {
                    var returnValue = result.ReturnValue;
                    var type = returnValue.GetType();
                    var typeInfo = type.GetTypeInfo();
                    var embed = Utilities.CreateDefaultEmbed("Evaluate").WithFields(
                        new[] {
                            new EmbedFieldBuilder()
                                .WithName("Return Type")
                                .WithValue(type.ToString()),
                        }
                    );

                    if (typeInfo.IsPrimitive || typeInfo.IsEnum || type == typeof(DateTime)
                        || type == typeof(DateTimeOffset) || type == typeof(TimeSpan))
                    {
                        Console.WriteLine("Primitive");
                        embed.WithDescription(Format.Code(Utilities.ObjectToString(result.ReturnValue), "diff"));
                    }
                    else
                    {
                        var allPropertyInfos = typeInfo.GetProperties();
                        var propertyInfos = allPropertyInfos.Take(25);
                        foreach (var info in propertyInfos)
                            embed.AddField(
                                string.Concat(info.Name, " (", info.PropertyType.ToString(), ")"),
                                Utilities.ObjectToString(info.GetValue(returnValue)!),
                                true
                            );

                        if (allPropertyInfos.Length > 25)
                            embed.Description = string.Concat(
                                embed.Description,
                                "\n\n**Warning**: Property count exceeds 25. Not all properties are displayed."
                            );
                    }

                    await FollowupAsync(embed: embed.Build());
                }
                else
                {
                    var responseEmbed = Utilities.CreateDefaultEmbed("Evaluate", "No result was returned.");
                    await FollowupAsync(embed: responseEmbed.Build());
                }
            }
            catch (Exception ex)
            {
                var errorEmbed = Utilities.CreateDefaultEmbed("Evaluate", string.Concat("**", ex.GetType().ToString(), "**: ", ex.Message), Color.Red);
                await FollowupAsync(embed: errorEmbed.Build());
            }
        }


        private async Task<ScriptState<object>> EvaluateAsync(IInteractionContext ctx, string code)
        {
            string[] imports = {
                "System",
                "System.Collections.Generic",
                "System.Linq",
                "System.Text",
                "System.Threading.Tasks",
                "Microsoft.Extensions.DependencyInjection",
                "Discord",
                "Discord.Interactions",
                "Neo",
                "Neo.Common.Data",
                "Neo.Common.Entities",
                "Neo.Common.Utilities.Extensions",
                "Neo.Common.Utilities",
                "Neo.Core",
                "Neo.Core.Commands",
                "Neo.Core.Commands.Modules",
                "Neo.Core.Repositories",
                "Neo.Core.Services",
                "Neo.Core.Structures",
            };

            await RespondAsync("Evaluating...", ephemeral: true);

            var globals = new NeoVariables(ctx, _serviceProvider);

            var scriptOptions = ScriptOptions.Default;
            scriptOptions = scriptOptions.WithImports(imports);
            scriptOptions = scriptOptions.WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

            var script = CSharpScript.Create(code, scriptOptions, typeof(NeoVariables));
            script.Compile();
            var result = await script.RunAsync(globals);

            return result;
        }
    }
}
