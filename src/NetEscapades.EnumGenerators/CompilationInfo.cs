using Microsoft.CodeAnalysis;

namespace NetEscapades.EnumGenerators;

internal readonly struct CompilationInfo : IEquatable<CompilationInfo>
{
    public INamedTypeSymbol? EnumAttribute { get; }
	public INamedTypeSymbol? DisplayAttribute { get; }
	public INamedTypeSymbol? HasFlagsAttribute { get; }

	public CompilationInfo(INamedTypeSymbol? enumAttribute, INamedTypeSymbol? displayAttribute, INamedTypeSymbol? hasFlagsAttribute)
	{
		EnumAttribute = enumAttribute;
		DisplayAttribute = displayAttribute;
		HasFlagsAttribute = hasFlagsAttribute;
	}

	public bool Equals(CompilationInfo other)
	{
		return SymbolEqualityComparer.Default.Equals(EnumAttribute, other.EnumAttribute)
			&& SymbolEqualityComparer.Default.Equals(DisplayAttribute, other.DisplayAttribute)
			&& SymbolEqualityComparer.Default.Equals(HasFlagsAttribute, other.HasFlagsAttribute);
    }

	public override int GetHashCode()
	{
		return SymbolEqualityComparer.Default.GetHashCode(EnumAttribute) *
			SymbolEqualityComparer.Default.GetHashCode(DisplayAttribute) *
			SymbolEqualityComparer.Default.GetHashCode(HasFlagsAttribute);
	}
}