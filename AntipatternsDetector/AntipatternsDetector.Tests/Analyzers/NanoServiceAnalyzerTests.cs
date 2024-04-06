using System.Threading.Tasks;
using AntipatternsDetector.Analyzers;
using AntipatternsDetector.Tests.Common;
using FluentAssertions;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace AntipatternsDetector.Tests.Analyzers
{
    public class NanoServiceAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public async Task ServiceWithOneEndpoint_ShouldAddDiagnostic()
        {
            var diagnostics = await ProjectsHelper.GetDiagnosticsFromProjectAsync(Analyzer, "NanoService");
            diagnostics.Should().HaveCount(1);
        }
        

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new NanoServiceAnalyzer();
        }
    }
}