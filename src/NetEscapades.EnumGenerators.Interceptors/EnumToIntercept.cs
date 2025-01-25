using Microsoft.CodeAnalysis.CSharp;

namespace NetEscapades.EnumGenerators.Interceptors;

public readonly record struct EnumToIntercept(string Name, string FullyQualifiedName, string Namespace);

#pragma warning disable RSEXPERIMENTAL002 // / Experimental interceptable location API
public record CandidateInvocation(InterceptableLocation Location, string EnumName, InterceptorTarget Target);
#pragma warning restore RSEXPERIMENTAL002

public record MethodToIntercept(
    EquatableArray<CandidateInvocation> Invocations,
    string ExtensionTypeName,
    string FullyQualifiedName,
    string EnumNamespace)
{
    public MethodToIntercept(EquatableArray<CandidateInvocation> invocations, EnumToIntercept enumToIntercept)
    : this(invocations, enumToIntercept.Name, enumToIntercept.FullyQualifiedName, enumToIntercept.Namespace)
    {
    }
}

public enum InterceptorTarget
{
    ToString,
    HasFlag,
}