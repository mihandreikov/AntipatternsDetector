using System.Linq;
using Microsoft.CodeAnalysis;

namespace AntipatternsDetector.Helpers;

public static class TypeHelper
{
    public static bool DerivesFrom(this ITypeSymbol typeSymbol, KnownType type)
    {
        var currentType = typeSymbol;
        while (currentType != null)
        {
            if (currentType.Is(type))
            {
                return true;
            }

            currentType = currentType.BaseType?.ConstructedFrom;
        }

        return false;
    }
    
    public static bool Implements(this ITypeSymbol typeSymbol, ITypeSymbol type)
    {
        return typeSymbol != null &&
               typeSymbol.AllInterfaces.Any(symbol => symbol.ConstructedFrom.Equals(type));
    }
    
    public static bool Implements(this ITypeSymbol typeSymbol, KnownType type)
    {
        return typeSymbol != null &&
               typeSymbol.AllInterfaces.Any(symbol => symbol.ConstructedFrom.Is(type));
    }
    
    public static bool Is(this ITypeSymbol typeSymbol, KnownType type)
    {
        return typeSymbol != null && IsMatch(typeSymbol, type);
    }
    
    private static bool IsMatch(ITypeSymbol typeSymbol, KnownType type)
    {
        return type.Matches(typeSymbol.SpecialType) ||
               type.Matches(typeSymbol.OriginalDefinition.SpecialType) ||
               type.Matches(typeSymbol.ToDisplayString()) ||
               type.Matches(typeSymbol.OriginalDefinition.ToDisplayString());
    }
}