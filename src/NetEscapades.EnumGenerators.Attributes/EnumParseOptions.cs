using System;

namespace NetEscapades.EnumGenerators
{
    /// <summary>
    /// Defines the options use when parsing enums using members provided by NetEscapades.EnumGenerator.
    /// </summary>
    public readonly struct EnumParseOptions
    {
        private const StringComparison DefaultComparisonType = StringComparison.Ordinal;
    
        private readonly StringComparison? _comparisonType;
        private readonly bool _blockNumberParsing;

        /// <summary>
        /// Create an instance of <see cref="global::NetEscapades.EnumGenerators.EnumParseOptions"/>
        /// </summary>
        /// <param name="comparisonType">Sets the <see cref="global::System.StringComparison"/> to use during parsing.</param>
        /// <param name="allowMatchingMetadataAttribute">Sets whether the value of the selected metadata value attribute
        /// values applied to an enum should be used as the parse value for an enum.</param>
        /// <param name="enableNumberParsing">Sets a value defining whether numbers should be parsed as a fallback when
        /// other parsing fails.</param>
        public EnumParseOptions(
            StringComparison comparisonType = DefaultComparisonType,
            bool allowMatchingMetadataAttribute = false,
            bool enableNumberParsing = true)
        {
            _comparisonType = comparisonType;
            AllowMatchingMetadataAttribute = allowMatchingMetadataAttribute;
            _blockNumberParsing = !enableNumberParsing;
        }

        /// <summary>
        /// Gets or sets the <see cref="global::System.StringComparison"/> to use during parsing.
        /// </summary>
        /// <remarks>
        /// By default, it's set to <see cref="global::System.StringComparison.Ordinal"/>, and a case-sensitive
        /// comparison will be used.
        /// </remarks>
        public StringComparison ComparisonType => _comparisonType ?? DefaultComparisonType;

        /// <summary>
        /// Gets or sets whether the value of the selected metadata value attribute
        /// values applied to an enum should be used as the parse value for an enum.
        /// </summary>
        /// <remarks>
        /// By default, it's set to <see langword="false"/>, so the value of any metadata attributes on the
        /// enum values are ignored. Set to <see langword="true"/> to enable parsing using the applicable
        /// <see cref="global::NetEscapades.EnumGenerators.MetadataSource"/> for each enum member.
        /// </remarks>
        public bool AllowMatchingMetadataAttribute { get; }

        /// <summary>
        /// Gets or sets a value defining whether numbers should be parsed as a fallback when
        /// other parsing fails. 
        /// </summary>
        /// <remarks>
        /// By default, it's set to <see langword="true"/>, and numbers will be parsed as well as names.
        /// </remarks>
        public bool EnableNumberParsing => !_blockNumberParsing;
    }
}