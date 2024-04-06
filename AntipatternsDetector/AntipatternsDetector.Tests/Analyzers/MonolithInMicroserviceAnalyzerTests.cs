using System.Threading.Tasks;
using AntipatternsDetector.Analyzers;
using AntipatternsDetector.Tests.Common;
using FluentAssertions;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace AntipatternsDetector.Tests.Analyzers;

[CollectionDefinition("MonolithInMicroserviceAnalyzerTests")]
public class MonolithInMicroserviceAnalyzerTests : DiagnosticVerifier
{
    

    [Fact]
    public async Task LargeSolution_ShouldAddDiagnostic()
    {
        var diagnostics = await ProjectsHelper.GetDiagnosticsFromProjectAsync(Analyzer, "MonolithInMicroservice");
        diagnostics.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task SmallSolution_ShouldSkip()
    {
        var diagnostics = await ProjectsHelper.GetDiagnosticsFromProjectAsync(Analyzer, "NanoService");
        diagnostics.Should().HaveCount(0);
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
        return new MonolithInMicroserviceAnalyzer();
    }
}