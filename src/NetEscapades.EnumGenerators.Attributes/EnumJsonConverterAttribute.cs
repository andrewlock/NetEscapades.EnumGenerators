namespace NetEscapades.EnumGenerators
{
    /// <summary>
    /// Add to enums to indicate that a JsonConverter for the enum should be generated.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Enum)]
    [System.Diagnostics.Conditional("NETESCAPADES_ENUMGENERATORS_USAGES")]
    public sealed class EnumJsonConverterAttribute : System.Attribute
    {
        /// <summary>
        /// The converter that should be generated.
        /// </summary>
        public System.Type ConverterType { get; }

        /// <summary>
        /// Indicates if the string representation is case sensitive when deserializing it as an enum.
        /// </summary>
        public bool CaseSensitive { get; set; }

        /// <summary>
        /// Indicates if the value of <see cref="PropertyName"/> should be camel cased.
        /// </summary>
        public bool CamelCase { get; set; }

        /// <summary>
        /// If <see langword="true" />, considers the value of metadata attributes, otherwise ignores them.
        /// </summary>
        public bool AllowMatchingMetadataAttribute { get; set; }

        /// <summary>
        /// If set, this value will be used in messages when there are problems with validation and/or serialization/deserialization occurs.
        /// </summary>
        public string? PropertyName { get; set; }

        /// <summary>
        /// Creates an instance of <see cref="EnumJsonConverterAttribute"/>.
        /// </summary>
        /// <param name="converterType">The converter to generate.</param>
        public EnumJsonConverterAttribute(System.Type converterType) => ConverterType = converterType;
    }
}