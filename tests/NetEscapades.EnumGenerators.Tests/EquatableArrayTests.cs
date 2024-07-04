using FluentAssertions;
using Xunit;

namespace NetEscapades.EnumGenerators.Tests;
public class EquatableArrayTests {
    [Fact]
    public void ObjectEqualsWorksAsExpected() {
        var instances = new EquatableArray<string>(["A"]);
        object comparand = new EquatableArray<string>(["A"]);

        instances.Equals(comparand).Should().BeTrue();
    }
}
