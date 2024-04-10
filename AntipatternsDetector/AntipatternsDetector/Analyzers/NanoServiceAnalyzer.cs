using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using AntipatternsDetector.Helpers;
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
        
        private static readonly KnownType ControllerType = new KnownType("Microsoft.AspNetCore.Mvc.ControllerBase");
        private static readonly KnownType RouteTemplateProviderInterface = new KnownType("Microsoft.AspNetCore.Mvc.Routing.IRouteTemplateProvider");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
        private static int TotalApiCount = 0;
        private static bool Analyzed = false;

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSymbolAction(AnalyzeNode, SymbolKind.NamedType);
            context.RegisterSemanticModelAction(OnSemanticModelFinished);
        }

        private void OnSemanticModelFinished(SemanticModelAnalysisContext context)
        {
            lock ("Analyzed")
            {
                if (Analyzed)
                {
                    return;
                }

                if (TotalApiCount <= 1)
                {
                    var diagnostic = Diagnostic.Create(Rule, Location.None);
                    context.ReportDiagnostic(diagnostic);
                }

                Analyzed = true;
            }
        }


        private void AnalyzeNode(SymbolAnalysisContext context)
        {
            if (context.IsGeneratedCode)
                return;
            
            var classSymbol = (INamedTypeSymbol) context.Symbol;
            if (classSymbol.DerivesFrom(ControllerType) == false)
            {
                return;
            }
            
            var methodCount = classSymbol.GetMembers().Count(x => x.GetAttributes().Any(y => y.AttributeClass?.Implements(RouteTemplateProviderInterface) == true));

            TotalApiCount += methodCount;
        }
    }
}