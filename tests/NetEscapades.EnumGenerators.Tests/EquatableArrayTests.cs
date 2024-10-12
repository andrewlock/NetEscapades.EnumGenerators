using FluentAssertions;
using Xunit;

#if INTERCEPTORS
namespace NetEscapades.EnumGenerators.Tests;
#else
namespace NetEscapades.EnumGenerators.Tests.Roslyn4_04;
#endif

public class EquatableArrayTests
{
    [Fact]
    public void PrimitiveComparison()
    {
        int[] val1 = [1, 2, 3, 4, 5];
        int[] val2 = [1, 2, 3, 4, 5];

        var arr1 = new EquatableArray<int>(val1);
        var arr2 = new EquatableArray<int>(val2);

        arr1.Equals(arr2).Should().BeTrue();
    }

    [Fact]
    public void RecordComparison()
    {
        Record[] val1 = [new(1), new(2), new(3), new(4), new(5)];
        Record[] val2 = [new(1), new(2), new(3), new(4), new(5)];

        var arr1 = new EquatableArray<Record>(val1);
        var arr2 = new EquatableArray<Record>(val2);

        arr1.Equals(arr2).Should().BeTrue();
    }

    [Fact]
    public void NestedEquatableArrayComparison()
    {
        EquatableArray<int>[] val1 = [new([1]), new([2]), new([3]), new([4]), new([5])];
        EquatableArray<int>[] val2 = [new([1]), new([2]), new([3]), new([4]), new([5])];

        var arr1 = new EquatableArray<EquatableArray<int>>(val1);
        var arr2 = new EquatableArray<EquatableArray<int>>(val2);

        arr1.Equals(arr2).Should().BeTrue();
    }

    [Fact]
    public void BoxedNestedEquatableArrayComparison()
    {
        EquatableArray<int>[] val1 = [new([1]), new([2]), new([3]), new([4]), new([5])];
        EquatableArray<int>[] val2 = [new([1]), new([2]), new([3]), new([4]), new([5])];

        object arr1 = new EquatableArray<EquatableArray<int>>(val1);
        var arr2 = new EquatableArray<EquatableArray<int>>(val2);

        arr1.Equals(arr2).Should().BeTrue();
        arr2.Equals(arr1).Should().BeTrue();
    }

    public record Record
    {
        public Record(int value)
        {
            Value = value;
        }

        public int Value { get; }
    }
}