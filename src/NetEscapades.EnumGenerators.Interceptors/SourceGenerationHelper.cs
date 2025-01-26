using System.Text;

namespace NetEscapades.EnumGenerators.Interceptors;

public static class SourceGenerationHelper
{
    private const string Header = @"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the NetEscapades.EnumGenerators.Interceptors source generator
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable";

    public const string Attribute =
        $$"""
          {{Header}}

          #if NETESCAPADES_ENUMGENERATORS_EMBED_ATTRIBUTES
          namespace NetEscapades.EnumGenerators
          {
              /// <summary>
              /// Add to an assembly to indicate that usages of the enum should
              /// be automatically intercepted to use the extension methods
              /// generated by EnumExtensionsAttribute in this project.
              /// Note that the extension methods must be accessible from this project,
              /// otherwise you will receive compilation errors
              /// </summary>
              [global::System.AttributeUsage(global::System.AttributeTargets.Assembly, AllowMultiple = true)]
              [global::System.Diagnostics.Conditional("NETESCAPADES_ENUMGENERATORS_USAGES")]
          #if NET5_0_OR_GREATER
              [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Generated by the NetEscapades.EnumGenerators.Interceptors source generator.")]
          #else
              [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
          #endif
              public class InterceptableAttribute<T> : global::System.Attribute
                  where T: global::System.Enum
              {
                  /// <summary>
                  /// The namespace generated for the extension class. If not provided,
                  /// and the referenced enum is in a different project, the namespace
                  /// of the extension methods are assumed to be the same as the enum.
                  /// </summary>
                  public string? ExtensionClassNamespace { get; set; }
                  
                  /// <summary>
                  /// The name used for the extension class. If not provided,
                  /// and the referenced enum is in a different project, the enum name
                  /// with an <c>Extensions</c> suffix will be assumed. For example for
                  /// an Enum called StatusCodes, the assumed name will be StatusCodesExtensions.
                  /// </summary>
                  public string? ExtensionClassName { get; set; }
              }
          }
          #endif
          """;

    public static (string Content, string Filename) GenerateInterceptorsClass(MethodToIntercept toIntercept)
    {
        var sb = new StringBuilder(
            $$"""
            {{Header}}
            namespace System.Runtime.CompilerServices
            {
                // this type is needed by the compiler to implement interceptors - it doesn't need to
                // come from the runtime itself, though
            
                [global::System.Diagnostics.Conditional("DEBUG")] // not needed post-build, so: evaporate
                [global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true)]
                sealed file class InterceptsLocationAttribute : global::System.Attribute
                {
                    public InterceptsLocationAttribute(int version, string data)
                    {
                        _ = version;
                        _ = data;
                    }
                }
            }

            #pragma warning disable CS0612 // Ignore usages of obsolete members or enums
            #pragma warning disable CS0618 // Ignore usages of obsolete members or enums
            namespace NetEscapades.EnumGenerators
            {
                static file class EnumInterceptors
                {

            """);

        bool toStringIntercepted = false;
        foreach (var location in toIntercept.Invocations)
        {
            if(location!.Target == InterceptorTarget.ToString)
            {
                toStringIntercepted = true;
                sb.AppendLine(GetInterceptorAttr(location));                
            }
        }

        if(toStringIntercepted)
        {
            sb.AppendLine(
                $$"""
                          public static string {{toIntercept.ExtensionTypeName}}ToString(this global::System.Enum value)
                              => global::{{toIntercept.EnumNamespace}}{{(string.IsNullOrEmpty(toIntercept.EnumNamespace) ? "" : ".")}}{{toIntercept.ExtensionTypeName}}.ToStringFast((global::{{toIntercept.FullyQualifiedName}})value);

                  """);
        }
        
        bool hasFlagIntercepted = false;
        foreach (var location in toIntercept.Invocations)
        {
            if(location!.Target == InterceptorTarget.HasFlag)
            {
                hasFlagIntercepted = true;
                sb.AppendLine(GetInterceptorAttr(location));                
            }
        }

        if(hasFlagIntercepted)
        {
            sb.AppendLine(
                $$"""
                          public static bool {{toIntercept.ExtensionTypeName}}HasFlag(this global::System.Enum value, global::System.Enum flag)
                              => global::{{toIntercept.EnumNamespace}}{{(string.IsNullOrEmpty(toIntercept.EnumNamespace) ? "" : ".")}}{{toIntercept.ExtensionTypeName}}.HasFlagFast((global::{{toIntercept.FullyQualifiedName}})value, (global::{{toIntercept.FullyQualifiedName}})flag);

                  """);
        }
        
        sb.AppendLine(
            $$"""
                  }
              }
              #pragma warning restore CS0612 // Ignore usages of obsolete members or enums
              #pragma warning restore CS0618 // Ignore usages of obsolete members or enums
              """);
        var content = sb.ToString();
        sb.Clear();
        
        var filename = sb
            .Append(toIntercept.FullyQualifiedName)
            .Append("_Interceptors.g.cs")
            .Replace('<', '_')
            .Replace('>', '_')
            .Replace(',', '.')
            .Replace(' ', '_')
            .ToString();
        return (content, filename);

        static string GetInterceptorAttr(CandidateInvocation location)
        {
            return $"""        [global::System.Runtime.CompilerServices.InterceptsLocation({location.Location.Version}, "{location.Location.Data}")] // {location.Location.GetDisplayLocation()}""";
        }
    }
}