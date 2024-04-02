using Microsoft.CodeAnalysis.Diagnostics;

namespace AntipatternsDetector.Tests.Common;

public abstract class DiagnosticVerifier
{
    /// <summary>
    /// Get the CSharp analyzer being tested - to be implemented in non-abstract class
    /// </summary>
    protected abstract DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer();

    public DiagnosticAnalyzer Analyzer => GetCSharpDiagnosticAnalyzer();
}