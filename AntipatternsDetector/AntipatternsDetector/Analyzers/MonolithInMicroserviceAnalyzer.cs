using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AntipatternsDetector.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MonolithInMicroserviceAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "AP_2";

    private const string Title = "Solution too big. Consider splitting it into many microservices.";
    private const string MessageFormat = "Solution too big. Consider splitting it into many microservices.";
    private const string Description = "Solution too big. Consider splitting it into many microservices.";
    private const string Category = "Size";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
        Category, DiagnosticSeverity.Warning, true, Description);
    
    private const int MaxTotalLinesThreshold = 10_000;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
    
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterCompilationAction(AnalyzeCompilation);
    }

    private void AnalyzeCompilation(CompilationAnalysisContext context)
    {
        var totalLineCount = context.Compilation.SyntaxTrees.Sum(tree =>
            tree.GetRoot().DescendantTrivia().Count(token =>
                token.RawKind == (int) SyntaxKind.EndOfLineTrivia));

        if (totalLineCount > MaxTotalLinesThreshold)
        {
            var diagnostic = Diagnostic.Create(Rule, Location.None, totalLineCount, MaxTotalLinesThreshold);
            context.ReportDiagnostic(diagnostic);
        }
    }
}