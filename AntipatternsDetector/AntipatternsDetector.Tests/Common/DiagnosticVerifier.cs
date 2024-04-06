using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntipatternsDetector.Tests.Helpers;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace AntipatternsDetector.Tests.Common;

public abstract class DiagnosticVerifier
{
    protected abstract DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer();

    public DiagnosticAnalyzer Analyzer => GetCSharpDiagnosticAnalyzer();

    private static bool _isSetuped = false;

    protected DiagnosticVerifier()
    {
        if (_isSetuped)
        {
            return;
        }

        lock ("MSBuildLocator.RegisterDefaults")
        {
            if (_isSetuped)
            {
                return;
            }

            MSBuildLocator.RegisterDefaults();
            _isSetuped = true;
        }
    }

    protected Task VerifyDiagnosticsAsync(string source, params DiagnosticResult[] expected)
    {
        return VerifyDiagnosticsAsync(new ProjectOptions { Sources = new[] { source } },
            this.GetCSharpDiagnosticAnalyzer(), expected);
    }

    private static async Task VerifyDiagnosticsAsync(ProjectOptions projectOptions, DiagnosticAnalyzer analyzer,
        params DiagnosticResult[] expected)
    {
        var diagnostics = await DiagnosticVerifierHelper.GetSortedDiagnosticsAsync(projectOptions, analyzer);
        VerifyDiagnosticResults(diagnostics, analyzer, expected);
    }

    private static void VerifyDiagnosticResults(IEnumerable<Diagnostic> actualResults, DiagnosticAnalyzer analyzer,
        params DiagnosticResult[] expectedResults)
    {
        int expectedCount = expectedResults.Count();
        int actualCount = actualResults.Count();

        if (expectedCount != actualCount)
        {
            string diagnosticsOutput =
                actualResults.Any() ? FormatDiagnostics(analyzer, actualResults.ToArray()) : "    NONE.";

            Assert.True(false,
                string.Format(
                    "Mismatch between number of diagnostics returned, expected \"{0}\" actual \"{1}\"\r\n\r\nDiagnostics:\r\n{2}\r\n",
                    expectedCount, actualCount, diagnosticsOutput));
        }

        for (int i = 0; i < expectedResults.Length; i++)
        {
            var actual = actualResults.ElementAt(i);
            var expected = expectedResults[i];

            if (expected.Line == -1 && expected.Column == -1)
            {
                if (actual.Location != Location.None)
                {
                    Assert.True(false,
                        string.Format("Expected:\nA project diagnostic with No location\nActual:\n{0}",
                            FormatDiagnostics(analyzer, actual)));
                }
            }
            else
            {
                VerifyDiagnosticLocation(analyzer, actual, actual.Location, expected.Locations.First());
                var additionalLocations = actual.AdditionalLocations.ToArray();

                if (additionalLocations.Length != expected.Locations.Length - 1)
                {
                    Assert.True(false,
                        string.Format("Expected {0} additional locations but got {1} for Diagnostic:\r\n    {2}\r\n",
                            expected.Locations.Length - 1, additionalLocations.Length,
                            FormatDiagnostics(analyzer, actual)));
                }

                for (int j = 0; j < additionalLocations.Length; ++j)
                {
                    VerifyDiagnosticLocation(analyzer, actual, additionalLocations[j], expected.Locations[j + 1]);
                }
            }

            if (actual.Id != expected.Id)
            {
                Assert.True(false,
                    string.Format("Expected diagnostic id to be \"{0}\" was \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
                        expected.Id, actual.Id, FormatDiagnostics(analyzer, actual)));
            }

            if (actual.Severity != expected.Severity)
            {
                Assert.True(false,
                    string.Format(
                        "Expected diagnostic severity to be \"{0}\" was \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
                        expected.Severity, actual.Severity, FormatDiagnostics(analyzer, actual)));
            }

            if (actual.GetMessage() != expected.Message)
            {
                Assert.True(false,
                    string.Format(
                        "Expected diagnostic message to be \"{0}\" was \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
                        expected.Message, actual.GetMessage(), FormatDiagnostics(analyzer, actual)));
            }
        }
    }

    private static string FormatDiagnostics(DiagnosticAnalyzer analyzer, params Diagnostic[] diagnostics)
    {
        var builder = new StringBuilder();
        for (int i = 0; i < diagnostics.Length; ++i)
        {
            builder.AppendLine("// " + diagnostics[i]);

            var analyzerType = analyzer.GetType();
            var rules = analyzer.SupportedDiagnostics;

            foreach (var rule in rules)
            {
                if (rule != null && rule.Id == diagnostics[i].Id)
                {
                    var location = diagnostics[i].Location;
                    if (location == Location.None)
                    {
                        builder.AppendFormat("GetGlobalResult({0}.{1})", analyzerType.Name, rule.Id);
                    }
                    else
                    {
                        Assert.True(location.IsInSource,
                            $"Test base does not currently handle diagnostics in metadata locations. Diagnostic in metadata: {diagnostics[i]}\r\n");

                        string resultMethodName = "GetCSharpResultAt";
                        var linePosition = diagnostics[i].Location.GetLineSpan().StartLinePosition;

                        builder.Append(
                            $"{resultMethodName}({linePosition.Line + 1}, {linePosition.Character + 1}, {analyzerType.Name}.{rule.Id})");
                    }

                    if (i != diagnostics.Length - 1)
                    {
                        builder.Append(',');
                    }

                    builder.AppendLine();
                    break;
                }
            }
        }

        return builder.ToString();
    }

    private static void VerifyDiagnosticLocation(DiagnosticAnalyzer analyzer, Diagnostic diagnostic, Location actual,
        DiagnosticResultLocation expected)
    {
        var actualSpan = actual.GetLineSpan();

        Assert.True(
            actualSpan.Path == expected.Path || (actualSpan.Path != null && actualSpan.Path.Contains("Test0.") &&
                                                 expected.Path.Contains("Test.")),
            string.Format(
                "Expected diagnostic to be in file \"{0}\" was actually in file \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
                expected.Path, actualSpan.Path, FormatDiagnostics(analyzer, diagnostic)));

        var actualLinePosition = actualSpan.StartLinePosition;

        // Only check line position if there is an actual line in the real diagnostic
        if (actualLinePosition.Line > 0)
        {
            if (actualLinePosition.Line + 1 != expected.Line)
            {
                Assert.True(false,
                    string.Format(
                        "Expected diagnostic to be on line \"{0}\" was actually on line \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
                        expected.Line, actualLinePosition.Line + 1, FormatDiagnostics(analyzer, diagnostic)));
            }
        }

        // Only check column position if there is an actual column position in the real diagnostic
        if (actualLinePosition.Character > 0)
        {
            if (actualLinePosition.Character + 1 != expected.Column)
            {
                Assert.True(false,
                    string.Format(
                        "Expected diagnostic to start at column \"{0}\" was actually at column \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
                        expected.Column, actualLinePosition.Character + 1, FormatDiagnostics(analyzer, diagnostic)));
            }
        }
    }
}