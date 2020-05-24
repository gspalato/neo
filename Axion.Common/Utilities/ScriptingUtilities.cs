using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Axion.Core.Utilities
{
    public enum ScriptStage
    {
        Preprocessing,
        Compilation,
        Execution,
        Postprocessing
    }

    public sealed class ScriptingUtility
    {
        public static readonly IReadOnlyList<string> Imports = new ReadOnlyCollection<string>(new List<string>()
        {
            "Axion.Core",
            "Axion.Core.Commands",
            "Axion.Core.Commands.ArgumentParsers",
            "Axion.Core.Commands.Modules",
            "Axion.Core.Commands.TypeParsers",
            "Axion.Core.Database",
            "Axion.Core.Database.Entities",
            "Axion.Core.Extensions",
            "Axion.Core.Services",
            "Axion.Core.Structures",
            "Axion.Core.Structures.Attributes",
            "Axion.Core.Structures.Database",
            "Axion.Core.Structures.Interactivity",
            "Axion.Core.Structures.Miscellaneous",
            "Axion.Core.Utilities",
            "Discord",
            "Discord.Net",
            "Discord.Rest",
            "Discord.WebSocket",
            "Microsoft.Extensions.DependencyInjection",
            "Newtonsoft.Json",
            "System",
            "System.Collections.Generic",
            "System.Linq",
            "System.Text",
            "System.Threading.Tasks",
        });

        public static async Task<ScriptingResult> EvaluateScriptAsync<T>(string code, T properties)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return ScriptingResult.FromError(
                   new ArgumentException("Code parameter cannot be empty, null or whitespace", nameof(code)),
                   ScriptStage.Preprocessing);
            }

            var options = ScriptOptions.Default
                .WithReferences(AppDomain.CurrentDomain.GetAssemblies()
                    .Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)))
                .WithImports(Imports);

            var script = CSharpScript.Create(code, options, typeof(T));

            var compilationTimer = Stopwatch.StartNew();
            var compilationDiagnostics = script.Compile();
            compilationTimer.Stop();

            if (compilationDiagnostics.Length > 0
                && compilationDiagnostics.Any(a => a.Severity == DiagnosticSeverity.Error))
            {
                return ScriptingResult.FromError(compilationDiagnostics, ScriptStage.Compilation,
                   compilationTime: compilationTimer.ElapsedMilliseconds);
            }

            var executionTimer = new Stopwatch();

            try
            {
                executionTimer.Start();
                var executionResult = await script.RunAsync(properties).ConfigureAwait(false);
                executionTimer.Stop();
                var returnValue = executionResult.ReturnValue;

                GC.Collect();

                return ScriptingResult.FromSuccess(returnValue, compilationTimer.ElapsedMilliseconds,
                    executionTimer.ElapsedMilliseconds);
            }
            catch (Exception exception)
            {
                return ScriptingResult.FromError(exception, ScriptStage.Execution, compilationTimer.ElapsedMilliseconds,
                    executionTimer.ElapsedMilliseconds);
            }
        }
    }

    public sealed class ScriptingResult
    {
        private ScriptingResult(bool success = true, object returnValue = null,
            IEnumerable<Diagnostic> compilationDiagnostics = null, Exception exception = null,
            long compilationTime = -1, long executionTime = -1, ScriptStage failedStage = default)
        {
            ReturnValue = returnValue;
            IsSuccess = success;
            CompilationDiagnostics = compilationDiagnostics?.ToList();
            Exception = exception;
            CompilationTime = compilationTime;
            ExecutionTime = executionTime;
            FailedStage = failedStage;
        }

        public object ReturnValue { get; }
        public bool IsSuccess { get; }
        public List<Diagnostic> CompilationDiagnostics { get; }
        public Exception Exception { get; }
        public ScriptStage FailedStage { get; }

        public long CompilationTime { get; }
        public long ExecutionTime { get; }

        public static ScriptingResult FromSuccess(object returnValue, long compilationTime = -1,
            long executionTime = -1)
        {
            return new ScriptingResult(true, returnValue, executionTime: executionTime,
                compilationTime: compilationTime);
        }

        public static ScriptingResult FromError(IEnumerable<Diagnostic> diagnostics, ScriptStage failedStage,
            Exception exception = null, long compilationTime = -1, long executionTime = -1)
        {
            return new ScriptingResult(false, compilationDiagnostics: diagnostics, exception: exception,
                compilationTime: compilationTime, executionTime: executionTime, failedStage: failedStage);
        }

        public static ScriptingResult FromError(Exception exception, ScriptStage failedStage, long compilationTime = -1,
            long executionTime = -1)
        {
            return new ScriptingResult(false, exception: exception, compilationTime: compilationTime,
                executionTime: executionTime, failedStage: failedStage);
        }
    }
}