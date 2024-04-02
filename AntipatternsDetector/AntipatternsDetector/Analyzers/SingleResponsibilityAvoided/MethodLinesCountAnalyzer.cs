using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AntipatternsDetector.Analyzers;


[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MethodLinesCountAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "AP_5";

    private const string Title = "Too much lines in method.";
    private const string MessageFormat = "Too much lines in method. {0} lines found, {1} allowed.";
    private const string Description = "Too much lines in method.";
    private const string Category = "Complexity";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
        Category, DiagnosticSeverity.Warning, true, Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
    
    private const int MaxMethodLinesThreshold = 100;
    
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        if (context.IsGeneratedCode)
            return;

        var linesCount = context.Node.DescendantTrivia()
            .Count(token => token.RawKind == (int)SyntaxKind.EndOfLineTrivia);
        
        if (linesCount > MaxMethodLinesThreshold)
        {
            var diagnostic = Diagnostic.Create(Rule, context.Node.GetLocation(), linesCount, MaxMethodLinesThreshold);
            context.ReportDiagnostic(diagnostic);
        }
    }
}