using Discord;
using Discord.Interactions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Oculus.Common.Utilities;
using Oculus.Core.Structures;
using System.Reflection;

namespace Oculus.Core.Commands.Modules
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
                    var t = result.ReturnValue.GetType();
                    var ti = t.GetTypeInfo();
                    var embed = Utilities.CreateDefaultEmbed("Evaluate").WithFields(
                        new[] {
                            new EmbedFieldBuilder()
                                .WithName("Return Type")
                                .WithValue(t.ToString()),
                        }
                    );

                    if (ti.IsPrimitive || ti.IsEnum || t == typeof(DateTime) || t == typeof(DateTimeOffset) || t == typeof(TimeSpan))
                        embed.AddField("Value", Utilities.ObjectToString(result.ReturnValue), false);
                    else
                    {
                        var rv = result.ReturnValue;
                        var psr = ti.GetProperties();
                        var ps = psr.Take(25);
                        foreach (var xps in ps)
                            embed.AddField(string.Concat(xps.Name, " (", xps.PropertyType.ToString(), ")"), Utilities.ObjectToString(xps.GetValue(rv)), true);

                        if (psr.Length > 25)
                            embed.Description = string.Concat(embed.Description, "\n\n**Warning**: Property count exceeds 25. Not all properties are displayed.");
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


        public async Task<ScriptState<object>> EvaluateAsync(IInteractionContext ctx, string code)
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
                "Oculus",
                "Oculus.Common.Data",
                "Oculus.Common.Entities",
                "Oculus.Common.Utilities.Extensions",
                "Oculus.Common.Utilities",
                "Oculus.Core",
                "Oculus.Core.Commands",
                "Oculus.Core.Commands.Modules",
                "Oculus.Core.Repositories",
                "Oculus.Core.Services",
                "Oculus.Core.Structures",
            };

            var cs = code;

            await RespondAsync("Evaluating...", ephemeral: true);

            var globals = new OculusVariables(ctx, _serviceProvider);

            var scriptOptions = ScriptOptions.Default;
            scriptOptions = scriptOptions.WithImports(imports);
            scriptOptions = scriptOptions.WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

            var script = CSharpScript.Create(cs, scriptOptions, typeof(OculusVariables));
            script.Compile();
            var result = await script.RunAsync(globals);

            return result;
        }
    }
}
