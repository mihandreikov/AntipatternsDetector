using Microsoft.CodeAnalysis;

namespace Cian.CodeAnalysis.Helpers
{
    /// <summary>
    ///     Custom created type
    /// </summary>
    public class KnownType
    {
        private readonly bool isSpecialType;
        private readonly SpecialType specialType;

        internal KnownType(string typeName)
            : this(SpecialType.None, typeName)
        {
        }

        internal KnownType(SpecialType specialType, string typeName)
        {
            TypeName = typeName;
            this.specialType = specialType;
            this.isSpecialType = specialType != SpecialType.None;
        }

        public string TypeName { get; }

        internal bool Matches(string type)
        {
            return !this.isSpecialType && TypeName == type;
        }

        internal bool Matches(SpecialType type)
        {
            return this.isSpecialType && this.specialType == type;
        }
    }
}