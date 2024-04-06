﻿using System.Threading.Tasks;
using AntipatternsDetector.Analyzers;
using AntipatternsDetector.Tests.Common;
using AntipatternsDetector.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace AntipatternsDetector.Tests.Analyzers.SingleResponsibilityAvoidedTests;

public class MethodLinesCountAnalyzerTests : DiagnosticVerifier
{
    [Fact]
    public async Task LongMethod_ShouldAddDiagnostic()
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
            void Asd()
            {
                var a = new int[1000];
                a[0] = 0;
                a[1] = 1
                a[2] = 2
                a[3] = 3
                a[4] = 4
                a[5] = 5
                a[6] = 6
                a[7] = 7
                a[8] = 8
                a[9] = 9
                a[10] = 10
                a[11] = 11
                a[12] = 12
                a[13] = 13
                a[14] = 14
                a[15] = 15
                a[16] = 16
                a[17] = 17
                a[18] = 18
                a[19] = 19
                a[20] = 20
                a[21] = 21
                a[22] = 22
                a[23] = 23
                a[24] = 24
                a[25] = 25
                a[26] = 26
                a[27] = 27
                a[28] = 28
                a[29] = 29
                a[30] = 30
                a[31] = 31
                a[32] = 32
                a[33] = 33
                a[34] = 34
                a[35] = 35
                a[36] = 36
                a[37] = 37
                a[38] = 38
                a[39] = 39
                a[40] = 40
                a[41] = 41
                a[42] = 42
                a[43] = 43
                a[44] = 44
                a[45] = 45
                a[46] = 46
                a[47] = 47
                a[48] = 48
                a[49] = 49
                a[50] = 50
                a[51] = 51
                a[52] = 52
                a[53] = 53
                a[54] = 54
                a[55] = 55
                a[56] = 56
                a[57] = 57
                a[58] = 58
                a[59] = 59
                a[60] = 60
                a[61] = 61
                a[62] = 62
                a[63] = 63
                a[64] = 64
                a[65] = 65
                a[66] = 66
                a[67] = 67
                a[68] = 68
                a[69] = 69
                a[70] = 70
                a[71] = 71
                a[72] = 72
                a[73] = 73
                a[74] = 74
                a[75] = 75
                a[76] = 76
                a[77] = 77
                a[78] = 78
                a[79] = 79
                a[80] = 80
                a[81] = 81
                a[82] = 82
                a[83] = 83
                a[84] = 84
                a[85] = 85
                a[86] = 86
                a[87] = 87
                a[88] = 88
                a[89] = 89
                a[90] = 90
                a[91] = 91
                a[92] = 92
                a[93] = 93
                a[94] = 94
                a[95] = 95
                a[96] = 96
                a[97] = 97
                a[98] = 98
                a[99] = 99
            }
        }
    }";
        
        var expected = new DiagnosticResult
        {
            Id = MethodLinesCountAnalyzer.DiagnosticId,
            Message = "Too much lines in method. 105 lines found, 100 allowed.",
            Severity = DiagnosticSeverity.Warning,
            Locations =
                new[] {
                    new DiagnosticResultLocation("Test0.cs", 11, 13)
                }
        };

        await VerifyDiagnosticsAsync(test, expected);
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
        return new MethodLinesCountAnalyzer();
    }
}