using System.Collections.Immutable;
using System.Text.RegularExpressions;
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
    private const string MessageFormat = "'And' or 'or' contains in method/class '{0}'.";
    private const string Description = "'And' or 'or' contains in class.";
    private const string Category = "Complexity";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
        Category, DiagnosticSeverity.Warning, true, Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
    
    private Regex regex = new Regex(@".*[a-z0-9]((Or)|(And))[A-Z0-9].*", RegexOptions.Compiled);
    
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
        if (regex.IsMatch(name))
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