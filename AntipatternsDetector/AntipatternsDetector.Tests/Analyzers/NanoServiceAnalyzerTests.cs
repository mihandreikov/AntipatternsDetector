using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AntipatternsDetector.Analyzers;
using AntipatternsDetector.Tests.Common;
using FluentAssertions;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;
using Xunit;

namespace AntipatternsDetector.Tests.Analyzers
{
    public class NanoServiceAnalyzerTests : DiagnosticVerifier
    {
        
        [Fact]
        public async Task ServiceWithOneEndpoint_ShouldAddDiagnostic()
        {
            MSBuildLocator.RegisterDefaults();
            
            using (var workspace = MSBuildWorkspace.Create())
            {
                var projectPath = Path.Combine("NanoService", "NanoService", "NanoService.csproj");
                
                var project = await workspace.OpenProjectAsync(projectPath);
                var compilation = await project.GetCompilationAsync();
                var compilationWithAnalyzersOptions = new CompilationWithAnalyzersOptions(
                    options: default,
                    onAnalyzerException: default,
                    concurrentAnalysis: false,
                    logAnalyzerExecutionTime: false,
                    reportSuppressedDiagnostics: false);

                var diagnosticAnalyzers = new []{ Analyzer }.ToImmutableArray();
                var compilationWithAnalyzers = new CompilationWithAnalyzers(compilation, diagnosticAnalyzers, compilationWithAnalyzersOptions);

                var diagnostics = await compilationWithAnalyzers.GetAnalysisResultAsync(CancellationToken.None);

                var allDiagnostics = diagnostics.GetAllDiagnostics();
                allDiagnostics.Should().HaveCount(1);
            }
        }
        
        

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new NanoServiceAnalyzer();
        }
    }
}