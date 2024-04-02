using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AntipatternsDetector.Analyzers;


[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PrepositionsAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "AP_4";

    private const string Title = "'And' or 'or' contains in class.";
    private const string MessageFormat = "'And' or 'or' contains in class.";
    private const string Description = "'And' or 'or' contains in class.";
    private const string Category = "Complexity";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
        Category, DiagnosticSeverity.Warning, true, Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
    
    private const int MaxInjectionCount = 10;
    
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InterfaceDeclaration);

    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var name = GetName(context.Node);
        if (name.Contains("And") || name.Contains("Or"))
        {
            var diagnostic = Diagnostic.Create(Rule, context.Node.GetLocation(), name);
            context.ReportDiagnostic(diagnostic);
        }
    }
    
    private static string GetName(SyntaxNode node)
    {
        return node switch
        {
            MethodDeclarationSyntax methodSyntax => methodSyntax.Identifier.ValueText,
            InterfaceDeclarationSyntax interfaceSyntax => interfaceSyntax.Identifier.ValueText,
            ClassDeclarationSyntax classSyntax => classSyntax.Identifier.ValueText,
            _ => ""
        };
    }
}