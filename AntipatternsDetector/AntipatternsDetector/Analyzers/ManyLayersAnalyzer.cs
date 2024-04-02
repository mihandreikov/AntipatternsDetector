using System;
using System.Collections.Immutable;
using System.Linq;
using Cian.CodeAnalysis.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AntipatternsDetector.Analyzers;


[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ManyLayersAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "AP_6";

    private const string Title = "Method has many layers.";
    private const string MessageFormat = "Method has many layers.";
    private const string Description = "Method has many layers.";
    private const string Category = "Complexity";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
        Category, DiagnosticSeverity.Warning, true, Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
    
    private static readonly KnownType ControllerType = new KnownType("Microsoft.AspNetCore.Mvc.ControllerBase");
    
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        
        
    }

    public int CalculateMethodDepth(MethodDeclarationSyntax method, SemanticModel semanticModel)
    {
        int maxDepth = 0;
        var methodSymbol = ModelExtensions.GetDeclaredSymbol(semanticModel, method);

        if (methodSymbol != null)
        {
            foreach (var reference in methodSymbol.DeclaringSyntaxReferences)
            {
                var syntaxTree = reference.SyntaxTree;
                var root = syntaxTree.GetRoot();

                // Find all method invocations within the syntax tree
                var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

                // Calculate the depth of each invocation of the method
                foreach (var invocation in invocations)
                {
                    var invokedMethodSymbol = ModelExtensions.GetSymbolInfo(semanticModel, invocation).Symbol as IMethodSymbol;

                    if (invokedMethodSymbol != null && methodSymbol.Equals(invokedMethodSymbol.OriginalDefinition))
                    {
                        // Recursively calculate the depth of the invoking method
                        int depth = 1 + CalculateMethodDepth(invokedMethodSymbol.DeclaringSyntaxReferences.First().GetSyntax() as MethodDeclarationSyntax, semanticModel);
                        maxDepth = Math.Max(maxDepth, depth);
                    }
                }
            }
        }

        return maxDepth;
    }
}