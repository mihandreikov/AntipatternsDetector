using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;

namespace AntipatternsDetector.Tests.Common;

public abstract class ProjectsHelper
{
    // This semaphore is used to prevent multiple projects from being opened and built at the same time
    private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

    public static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsFromProjectAsync(DiagnosticAnalyzer analyzer,
        string projectName)
    {
        await Semaphore.WaitAsync();
        try
        {
            using (var workspace = MSBuildWorkspace.Create())
            {
                var projectPath = Path.Combine(projectName, projectName, $"{projectName}.csproj");

                var project = await workspace.OpenProjectAsync(projectPath);
                var compilation = await project.GetCompilationAsync();
                var compilationWithAnalyzersOptions = new CompilationWithAnalyzersOptions(
                    options: default,
                    onAnalyzerException: default,
                    concurrentAnalysis: false,
                    logAnalyzerExecutionTime: false,
                    reportSuppressedDiagnostics: false);

                var diagnosticAnalyzers = new[] { analyzer }.ToImmutableArray();
                var compilationWithAnalyzers = new CompilationWithAnalyzers(compilation, diagnosticAnalyzers,
                    compilationWithAnalyzersOptions);

                var diagnostics = await compilationWithAnalyzers.GetAnalysisResultAsync(CancellationToken.None);
                return diagnostics.GetAllDiagnostics();
            }
        }
        finally
        {
            Semaphore.Release();
        }
    }
}