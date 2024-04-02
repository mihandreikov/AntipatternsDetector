using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AntipatternsDetector.Analyzers;


[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HighComplexityAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "AP_3";

    private const string Title = "Class has high complexity.";
    private const string MessageFormat = "Class has high complexity. Too much dependencies injected. {0} dependencies found, {1} allowed.";
    private const string Description = "Class has high complexity.";
    private const string Category = "Complexity";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
        Category, DiagnosticSeverity.Warning, true, Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
    
    private const int MaxInjectionCount = 10;
    
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var classDeclarationSyntax = context.Node as ClassDeclarationSyntax;
        if (classDeclarationSyntax is null)
            return;
        
        var constructors = classDeclarationSyntax.Members.OfType<ConstructorDeclarationSyntax>();
        foreach (var constructor in constructors)
        {
            var parameterCount = constructor.ParameterList.Parameters.Count;
            if (parameterCount > MaxInjectionCount)
            {
                var diagnostic = Diagnostic.Create(Rule, constructor.GetLocation(), parameterCount, MaxInjectionCount);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}