namespace NetEscapades.EnumGenerators
{
    /// <summary>
    /// Add to enums to indicate that extension methods should be generated for the type
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Enum)]
    [System.Diagnostics.Conditional("NETESCAPADES_ENUMGENERATORS_USAGES")]
    public class EnumExtensionsAttribute : System.Attribute
    {
        /// <summary>
        /// The namespace to generate the extension class.
        /// If not provided, the namespace of the enum will be used
        /// </summary>
        public string? ExtensionClassNamespace { get; set; }

        /// <summary>
        /// The name to use for the extension class.
        /// If not provided, the enum name with ""Extensions"" will be used.
        /// For example for an Enum called StatusCodes, the default name
        /// will be StatusCodesExtensions
        /// </summary>
        public string? ExtensionClassName { get; set; }

        /// <summary>
        /// By default, when used with NetEscapades.EnumGenerators.Interceptors
        /// any interceptable usages of the enum will be replaced by usages of
        /// the extension method in this project. To disable interception of
        /// the enum in this project when used with the interceptable package,
        /// set <see cref="IsInterceptable"/> to <see langword="false"/>.
        /// </summary>
        public bool IsInterceptable { get; set; } = true;
    }
    
    /// <summary>
    /// Add to enums to indicate that extension methods should be generated for the type
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
    [System.Diagnostics.Conditional("NETESCAPADES_ENUMGENERATORS_USAGES")]
    public class EnumExtensionsAttribute<T> : System.Attribute
        where T: System.Enum
    {
        /// <summary>
        /// The namespace to generate the extension class.
        /// If not provided, the namespace of the enum will be used.
        /// </summary>
        public string? ExtensionClassNamespace { get; set; }

        /// <summary>
        /// The name to use for the extension class.
        /// If not provided, the enum name with an <c>Extensions</c> suffix will be used.
        /// For example for an Enum called StatusCodes, the default name
        /// will be StatusCodesExtensions.
        /// </summary>
        public string? ExtensionClassName { get; set; }

        /// <summary>
        /// By default, when used with NetEscapades.EnumGenerators.Interceptors
        /// any interceptable usages of the enum will be replaced by usages of
        /// the extension method in this project. To disable interception of
        /// the enum in this project when used with the interceptable package,
        /// set <see cref="IsInterceptable"/> to <see langword="false"/>.
        /// </summary>
        public bool IsInterceptable { get; set; } = true;
    }
}
