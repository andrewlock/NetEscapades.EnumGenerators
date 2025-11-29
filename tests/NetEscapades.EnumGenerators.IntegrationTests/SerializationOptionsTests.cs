using System;
using FluentAssertions;
using Xunit;

namespace NetEscapades.EnumGenerators.IntegrationTests;

public class SerializationOptionsTests
{
    [Fact]
    public void Default()
    {
        SerializationOptions options = default;
        options.UseMetadataAttributes.Should().BeFalse();
        options.Transform.Should().Be(SerializationTransform.None);
    }

    [Fact]
    public void DefaultConstructor()
    {
        var options = new SerializationOptions();
        options.UseMetadataAttributes.Should().BeFalse();
        options.Transform.Should().Be(SerializationTransform.None);
    }

    [Fact]
    public void Transform()
    {
        var options = new SerializationOptions(transform: SerializationTransform.UpperInvariant);
        options.UseMetadataAttributes.Should().BeFalse();
        options.Transform.Should().Be(SerializationTransform.UpperInvariant);
    }

    [Fact]
    public void Metadata()
    {
        var options = new SerializationOptions(true);
        options.UseMetadataAttributes.Should().BeTrue();
        options.Transform.Should().Be(SerializationTransform.None);
    }

    [Fact]
    public void TransformAndMetadata()
    {
        var options = new SerializationOptions(true, transform: SerializationTransform.UpperInvariant);
        options.UseMetadataAttributes.Should().BeTrue();
        options.Transform.Should().Be(SerializationTransform.UpperInvariant);
    }
}