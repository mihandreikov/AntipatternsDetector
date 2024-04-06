using System.Collections.Immutable;
using AntipatternsDetector.Analyzers;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;

namespace ConsoleApplication
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            MSBuildLocator.RegisterDefaults();

            
            using (var workspace = MSBuildWorkspace.Create())
            {
                var project = await workspace.OpenProjectAsync(@"C:\src\thesis\Blogifier\src\Blogifier\Blogifier.csproj");
                var compilation = await project.GetCompilationAsync();
                var compilationWithAnalyzersOptions = new CompilationWithAnalyzersOptions(
                    options: default,
                    onAnalyzerException: default,
                    concurrentAnalysis: false,
                    logAnalyzerExecutionTime: false,
                    reportSuppressedDiagnostics: false);

                var analyzers = AllAnalyzers.ToList();

                var compilationWithAnalyzers = new CompilationWithAnalyzers(compilation, analyzers.ToImmutableArray(),
                    compilationWithAnalyzersOptions);

                var diagnostics = await compilationWithAnalyzers.GetAnalysisResultAsync(CancellationToken.None);

                foreach (var diagnostic in diagnostics.GetAllDiagnostics())
                {
                    Console.ForegroundColor = GetColor(diagnostic.Severity);
                    Console.WriteLine("{1} {0}: {2}", diagnostic.Id, diagnostic.Severity, diagnostic.GetMessage());
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("{0}", diagnostic.Location);
                    Console.WriteLine();
                }
            }

            Console.ReadKey();
        }

        private static ConsoleColor GetColor(DiagnosticSeverity severity)
        {
            switch (severity)
            {
                case DiagnosticSeverity.Info:
                    return ConsoleColor.Cyan;
                case DiagnosticSeverity.Warning:
                    return ConsoleColor.DarkYellow;
                case DiagnosticSeverity.Error:
                    return ConsoleColor.Red;
                default:
                    return DefaultColor;
            }
        }

        private static readonly ConsoleColor DefaultColor = Console.ForegroundColor;
        
        private static readonly IReadOnlyList<DiagnosticAnalyzer> AllAnalyzers =
            typeof(NanoServiceAnalyzer).Assembly.GetTypes()
                .Where(type => typeof(DiagnosticAnalyzer).IsAssignableFrom(type) && type.IsAbstract == false)
                .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
                .ToArray();
    }
}