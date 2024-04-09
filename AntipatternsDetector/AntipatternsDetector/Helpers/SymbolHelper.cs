using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace AntipatternsDetector.Helpers;

public static class SymbolHelper
{
    internal static SyntaxNode? GetSyntax(this ISymbol symbol, CancellationToken cancellationToken = default)
    {
        return symbol
            ?.DeclaringSyntaxReferences.FirstOrDefault()
            ?.GetSyntax(cancellationToken);
    }
}