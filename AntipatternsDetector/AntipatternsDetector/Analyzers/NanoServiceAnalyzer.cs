using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AntipatternsDetector.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NanoServiceAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AP_1";

        private const string Title = "Only one API found in the service.";
        private const string MessageFormat = "Only one API found in the service.";
        private const string Description = "Only one API found in the service. Service looks like a nano-service. Consider merging it with another service.";
        private const string Category = "Size";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, true, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
            context.RegisterCompilationAction(OnCompilationFinished);
        }
        
        private static int TotalApiCount = 0;

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax) context.Node;

            var hasApiControllerAttribute = classDeclaration.AttributeLists
                .SelectMany(list => list.Attributes)
                .Any(attribute => attribute.Name.ToString() == "ApiController");

            if (hasApiControllerAttribute)
            {
                // Count the number of methods in the class
                var methodCount = classDeclaration.Members
                    .OfType<MethodDeclarationSyntax>()
                    .Count(x => x.Modifiers.Any(SyntaxKind.PublicKeyword));

                // Increment the total API count
                TotalApiCount += methodCount;
            }
        }

        private void OnCompilationFinished(CompilationAnalysisContext context)
        {
            if (TotalApiCount <= 1)
            {
                var diagnostic = Diagnostic.Create(Rule, Location.None);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}