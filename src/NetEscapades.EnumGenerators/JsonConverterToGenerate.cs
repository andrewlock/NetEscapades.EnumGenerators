namespace NetEscapades.EnumGenerators
{
    public readonly record struct JsonConverterToGenerate
    {
        public readonly string ExtensionName;
        public readonly string FullyQualifiedName;
        public readonly string ExtensionNamespace;
        public readonly string ConverterType;
        public readonly string? ConverterNamespace;
        public readonly bool IsPublic;
        public readonly bool CaseSensitive;
        public readonly bool CamelCase;
        public readonly bool AllowMatchingMetadataAttribute;
        public readonly string? PropertyName;

        public JsonConverterToGenerate(string extensionName,
            string fullyQualifiedName,
            string extensionNamespace,
            string converterType,
            string? converterNamespace,
            bool isPublic,
            bool caseSensitive,
            bool camelCase,
            bool allowMatchingMetadataAttribute,
            string? propertyName)
        {
            ExtensionName = extensionName;
            FullyQualifiedName = fullyQualifiedName;
            ExtensionNamespace = extensionNamespace;
            ConverterType = converterType;
            ConverterNamespace = !string.IsNullOrEmpty(converterNamespace) ? converterNamespace : extensionNamespace;
            IsPublic = isPublic;
            CaseSensitive = caseSensitive;
            CamelCase = camelCase;
            AllowMatchingMetadataAttribute = allowMatchingMetadataAttribute;
            PropertyName = propertyName;
        }
    }
}