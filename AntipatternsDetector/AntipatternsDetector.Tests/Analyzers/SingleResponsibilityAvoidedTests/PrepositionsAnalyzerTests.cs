using System.Threading.Tasks;
using AntipatternsDetector.Analyzers;
using AntipatternsDetector.Tests.Common;
using AntipatternsDetector.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace AntipatternsDetector.Tests.Analyzers.SingleResponsibilityAvoidedTests;

public class PrepositionsAnalyzerTests : DiagnosticVerifier
{
    [Theory]
    [InlineData("Or")]
    [InlineData("And")]
    public async Task MethodWithPrepositions_ShouldAddDiagnostic(string preposition)
    {
        var test = @"
    using Microsoft.AspNetCore.Mvc;

    namespace ConsoleApplication1
    {
        public class GeoController : ControllerBase
        {
            /// <summary>
            /// Method Test
            /// </summary>
            [Route(""v1/get-geo-tree/"")]
            void GetGeo" + preposition + @"OrGeoTree()
            {
                var a = new int[1000];
                a[0] = 0;
            }
        }
    }";
        
        var expected = new DiagnosticResult
        {
            Id = PrepositionsAnalyzer.DiagnosticId,
            Message = "'And' or 'or' contains in class.",
            Severity = DiagnosticSeverity.Warning,
            Locations =
                new[] {
                    new DiagnosticResultLocation("Test0.cs", 11, 13)
                }
        };

        await VerifyDiagnosticsAsync(test, expected);
    }
    
    [Theory]
    [InlineData("GetGeoTree")]
    [InlineData("GetOrdersAsync")]
    [InlineData("GetAnderson")]
    public async Task MethodWithoutPreposition_ShouldSkip(string methodName)
    {
        var test = @"
    using Microsoft.AspNetCore.Mvc;

    namespace ConsoleApplication1
    {
        public class GeoController : ControllerBase
        {
            /// <summary>
            /// Method Test
            /// </summary>
            [Route(""v1/get-geo-tree/"")]
            void " + methodName + @"()
            {
                var a = new int[1000];
                a[0] = 0;
            }
        }
    }";

        await VerifyDiagnosticsAsync(test);
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
        return new PrepositionsAnalyzer();
    }
}