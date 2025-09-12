namespace NetEscapades.EnumGenerators
{
    /// <summary>
    /// Defines where to obtain metadata for serializing and deserializing the enum
    /// </summary>
    public enum MetadataSource
    {
        /// <summary>
        /// Don't use attributes applied to enum members as a source of metadata for
        /// <c>ToStringFast()</c> and <c>TryParse()</c>. The name of the enum member
        /// will always be used for serialization.
        /// </summary>
        None,
    
        /// <summary>
        /// Use values provided in <c>System.ComponentModel.DataAnnotations.DisplayAttribute</c> for
        /// determining the value to use for <c>ToStringFast()</c> and <c>TryParse()</c>.
        /// The value of the attribute will be used if available, otherwise the
        /// name of the enum member will be used for serialization.
        /// </summary>
        DisplayAttribute,
    
        /// <summary>
        /// Use values provided in <see cref="System.ComponentModel.DescriptionAttribute"/> for
        /// determining the value to use for <c>ToStringFast()</c> and <c>TryParse()</c>.
        /// The value of the attribute will be used if available, otherwise the
        /// name of the enum member will be used for serialization.
        /// </summary>
        DescriptionAttribute,
    
        /// <summary>
        /// Use values provided in <see cref="System.Runtime.Serialization.EnumMemberAttribute"/> for
        /// determining the value to use for <c>ToStringFast()</c> and <c>TryParse()</c>.
        /// The value of the attribute will be used if available, otherwise the
        /// name of the enum member will be used for serialization.
        /// </summary>
        EnumMemberAttribute,
    }
}