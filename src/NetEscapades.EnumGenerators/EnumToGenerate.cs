using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace NetEscapades.EnumGenerators;

public readonly record struct EnumToGenerate
{
    public readonly string Name;
    public readonly string FullyQualifiedName;
    public readonly string Namespace;
    public readonly bool IsPublic;
    public readonly bool HasFlags;
    public readonly string UnderlyingType;

    /// <summary>
    /// Key is the enum name.
    /// </summary>
    public readonly EquatableArray<(string Key, EnumValueOption Value)> Names;

    public readonly bool IsDisplayAttributeUsed;

    public EnumToGenerate(
        string name,
        string ns,
        string fullyQualifiedName,
        string underlyingType,
        bool isPublic,
        List<(string, EnumValueOption)> names,
        bool hasFlags,
        bool isDisplayAttributeUsed)
    {
        Name = name;
        Namespace = ns;
        UnderlyingType = underlyingType;
        Names = new EquatableArray<(string, EnumValueOption)>(names.ToArray());
        HasFlags = hasFlags;
        IsPublic = isPublic;
        FullyQualifiedName = fullyQualifiedName;
        IsDisplayAttributeUsed = isDisplayAttributeUsed;
    }
}

#if INTERCEPTORS
#pragma warning disable RSEXPERIMENTAL002 // / Experimental interceptable location API
public record CandidateInvocation(InterceptableLocation Location, string EnumName);
#pragma warning restore RSEXPERIMENTAL002

public record MethodToIntercept(
    EquatableArray<CandidateInvocation> Invocations,
    string ExtensionTypeName,
    string FullyQualifiedName,
    string EnumNamespace)
{
    public MethodToIntercept(EquatableArray<CandidateInvocation> invocations, EnumToGenerate enumToGenerate)
    : this(invocations, enumToGenerate.Name, enumToGenerate.FullyQualifiedName, enumToGenerate.Namespace)
    {
    }
};
#endif