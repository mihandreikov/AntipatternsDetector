using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AntipatternsDetector.Analyzers;


[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ClassLinesCountAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "AP_6";

    private const string Title = "Too much lines in class.";
    private const string MessageFormat = "Too much lines in class. {0} lines found, {1} allowed.";
    private const string Description = "Too much lines in class.";
    private const string Category = "Complexity";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
        Category, DiagnosticSeverity.Warning, true, Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
    
    private const int MaxClassLinesThreshold = 600;
    
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var linesCount = context.Node.DescendantTrivia()
            .Count(token => token.RawKind == (int)SyntaxKind.EndOfLineTrivia);
        
        if (linesCount > MaxClassLinesThreshold)
        {
            var diagnostic = Diagnostic.Create(Rule, context.Node.GetLocation(), linesCount, MaxClassLinesThreshold);
            context.ReportDiagnostic(diagnostic);
        }
    }
}