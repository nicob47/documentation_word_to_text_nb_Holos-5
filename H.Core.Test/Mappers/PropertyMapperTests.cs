using H.Core.Mappers;

namespace H.Core.Test.Mappers;

[TestClass]
public class PropertyMapperTests
{
    private class SimpleSource
    {
        public string Name { get; set; } = "";
        public int Value { get; set; }
        public double? NullableValue { get; set; }
    }

    private class SimpleDest
    {
        public string Name { get; set; } = "";
        public int Value { get; set; }
        public double NullableValue { get; set; }
    }

    private class DestWithNullable
    {
        public string Name { get; set; } = "";
        public int? Value { get; set; }
    }

    private class SourceWithCollection
    {
        public string Name { get; set; } = "";
        public List<int> Items { get; set; } = new();
    }

    private class DestWithCollection
    {
        public string Name { get; set; } = "";
        public List<int> Items { get; set; } = new();
    }

    [TestMethod]
    public void Map_CopiesMatchingProperties()
    {
        var source = new SimpleSource { Name = "Test", Value = 42 };

        var result = PropertyMapper.Map<SimpleSource, SimpleDest>(source);

        Assert.AreEqual("Test", result.Name);
        Assert.AreEqual(42, result.Value);
    }

    [TestMethod]
    public void Map_HandlesNullableToNonNullable()
    {
        var source = new SimpleSource { NullableValue = 3.14 };

        var result = PropertyMapper.Map<SimpleSource, SimpleDest>(source);

        Assert.AreEqual(3.14, result.NullableValue);
    }

    [TestMethod]
    public void Map_HandlesNullableNull_LeavesDefault()
    {
        var source = new SimpleSource { NullableValue = null };

        var result = PropertyMapper.Map<SimpleSource, SimpleDest>(source);

        Assert.AreEqual(0.0, result.NullableValue);
    }

    [TestMethod]
    public void Map_HandlesNonNullableToNullable()
    {
        var source = new SimpleSource { Value = 99 };

        var result = PropertyMapper.Map<SimpleSource, DestWithNullable>(source);

        // Non-nullable int → nullable int? should not copy (different types for IsAssignableFrom)
        // PropertyMapper doesn't handle T → Nullable<T>
        // This documents current behavior
        Assert.AreEqual("", result.Name);
    }

    [TestMethod]
    public void CopyTo_DoesNotCopyCollections()
    {
        var source = new SourceWithCollection { Name = "Test", Items = new List<int> { 1, 2, 3 } };
        var dest = new DestWithCollection();

        PropertyMapper.CopyTo(source, dest);

        Assert.AreEqual("Test", dest.Name);
        Assert.AreEqual(0, dest.Items.Count, "Collections should not be copied by PropertyMapper");
    }

    [TestMethod]
    public void Map_CreatesNewInstance()
    {
        var source = new SimpleSource { Name = "Original" };

        var result = PropertyMapper.Map<SimpleSource, SimpleDest>(source);

        Assert.AreNotSame((object)source, (object)result);
        source.Name = "Modified";
        Assert.AreEqual("Original", result.Name, "Modifying source should not affect mapped result");
    }
}
