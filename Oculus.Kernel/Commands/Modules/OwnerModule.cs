using Discord;
using Discord.Interactions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Oculus.Common.Utilities;
using Oculus.Kernel.Structures;
using System.Reflection;

namespace Oculus.Kernel.Commands.Modules
{
    [RequireOwner]
    public class OwnerModule : InteractionModuleBase
    {
        public OwnerModule() { }

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
                "Discord",
                "Discord.Interactions",
                "Oculus",
                "Oculus.Kernel",
                "Oculus.Kernel.Commands",
                "Oculus.Kernel.Commands.Modules",
                "Oculus.Kernel.Repositories",
                "Oculus.Kernel.Services",
                "Oculus.Kernel.Structures",
                "Oculus.Common.Data",
                "Oculus.Common.Entities",
                "Oculus.Kernel.Repositories",
                "Oculus.Common.Utilities.Extensions",
                "Oculus.Common.Utilities",
            };

            /*
            var cs1 = code.IndexOf("```") + 3;
            cs1 = code.IndexOf('\n', cs1) + 1;
            var cs2 = code.LastIndexOf("```");

            if (cs1 == -1 || cs2 == -1)
                throw new ArgumentException("You need to wrap the code into a code block.");
            */

            var cs = code;

            await RespondAsync("Evaluating...", ephemeral: true);

            var globals = new OculusVariables(ctx);

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
