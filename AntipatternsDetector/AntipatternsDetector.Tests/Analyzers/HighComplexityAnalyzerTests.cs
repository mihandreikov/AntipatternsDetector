using System.Threading.Tasks;
using AntipatternsDetector.Analyzers;
using AntipatternsDetector.Tests.Common;
using AntipatternsDetector.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace AntipatternsDetector.Tests.Analyzers;

public class HighComplexityAnalyzerTests : DiagnosticVerifier
{
    [Fact]
    public async Task ControllerHasAFewDependencies_ShouldSkip()
    {
        var test = @"
    using Microsoft.AspNetCore.Mvc;

    namespace ConsoleApplication1
    {
        public class GeoController : ControllerBase
        {
            public GeoController(IService service, IAnotherService anotherService, IAnotherService anotherService1, IAnotherService anotherService2, IAnotherService anotherService3, IAnotherService anotherService4, IAnotherService anotherService5, IAnotherService anotherService6, IAnotherService anotherService7)
            {
                _service = service;
                _anotherService = anotherService;
                _anotherService1 = anotherService1;
                _anotherService2 = anotherService2;
                _anotherService3 = anotherService3;
                _anotherService4 = anotherService4;
                _anotherService5 = anotherService5;
                _anotherService6 = anotherService6;
                _anotherService7 = anotherService7;
            }

            /// <summary>
            /// Method Test
            /// </summary>
            [Route(""v1/get-geo-tree/"")]
            void Asd()
            {
            }
        }
    }";

        await VerifyDiagnosticsAsync(test);
    }
    
    [Fact]
    public async Task ControllerHasALotOfDependencies_ShouldAddDiagnostic()
    {
        var test = @"
    using Microsoft.AspNetCore.Mvc;

    namespace ConsoleApplication1
    {
        public class GeoController : ControllerBase
        {
            public GeoController(IService service, IAnotherService anotherService, IAnotherService anotherService1, IAnotherService anotherService2, IAnotherService anotherService3, IAnotherService anotherService4, IAnotherService anotherService5, IAnotherService anotherService6, IAnotherService anotherService7, IAnotherService anotherService8, IAnotherService anotherService9, IAnotherService anotherService10, IAnotherService anotherService11)
            {
                _service = service;
                _anotherService = anotherService;
                _anotherService1 = anotherService1;
                _anotherService2 = anotherService2;
                _anotherService3 = anotherService3;
                _anotherService4 = anotherService4;
                _anotherService5 = anotherService5;
                _anotherService6 = anotherService6;
                _anotherService7 = anotherService7;
                _anotherService8 = anotherService8;
                _anotherService9 = anotherService9;
                _anotherService10 = anotherService10;
                _anotherService11 = anotherService11;
            }

            /// <summary>
            /// Method Test
            /// </summary>
            [Route(""v1/get-geo-tree/"")]
            void Asd()
            {
            }
        }
    }";
        
        var expected = new DiagnosticResult
        {
            Id = HighComplexityAnalyzer.DiagnosticId,
            Message = "Class has high complexity. Too much dependencies injected. 13 dependencies found, 10 allowed.",
            Severity = DiagnosticSeverity.Warning,
            Locations =
                new[] {
                    new DiagnosticResultLocation("Test0.cs", 8, 13)
                }
        };

        await VerifyDiagnosticsAsync(test, expected);
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
        return new HighComplexityAnalyzer();
    }
}