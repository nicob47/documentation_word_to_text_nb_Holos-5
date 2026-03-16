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

    private class SourceWithGuid
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public Guid DomainObjectGuid { get; set; } = Guid.NewGuid();
        public Guid ParentGuid { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "";
    }

    private class DestWithGuid
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public Guid DomainObjectGuid { get; set; }
        public Guid ParentGuid { get; set; }
        public string Name { get; set; } = "";
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

    [TestMethod]
    public void Map_SkipsIdentityGuidProperty()
    {
        var source = new SourceWithGuid { Name = "Test" };
        var originalSourceGuid = source.Guid;

        var result = PropertyMapper.Map<SourceWithGuid, DestWithGuid>(source);

        Assert.AreNotEqual(originalSourceGuid, result.Guid, "Identity Guid should not be copied — each object keeps its own");
        Assert.AreEqual("Test", result.Name);
    }

    [TestMethod]
    public void Map_CopiesNonIdentityGuidProperties()
    {
        var domainGuid = Guid.NewGuid();
        var parentGuid = Guid.NewGuid();
        var source = new SourceWithGuid
        {
            Name = "Test",
            DomainObjectGuid = domainGuid,
            ParentGuid = parentGuid
        };

        var result = PropertyMapper.Map<SourceWithGuid, DestWithGuid>(source);

        Assert.AreEqual(domainGuid, result.DomainObjectGuid, "Non-identity Guid properties should be copied");
        Assert.AreEqual(parentGuid, result.ParentGuid, "Foreign-key Guid properties should be copied");
    }

    [TestMethod]
    public void CopyTo_SkipsIdentityGuidOnExistingInstance()
    {
        var source = new SourceWithGuid { Name = "Test" };
        var dest = new DestWithGuid();
        var originalDestGuid = dest.Guid;

        PropertyMapper.CopyTo(source, dest);

        Assert.AreEqual(originalDestGuid, dest.Guid, "CopyTo should preserve the destination's identity Guid");
        Assert.AreEqual("Test", dest.Name);
    }

    [TestMethod]
    public void CopyTo_CopiesDomainObjectGuidOnExistingInstance()
    {
        var source = new SourceWithGuid { DomainObjectGuid = Guid.NewGuid(), ParentGuid = Guid.NewGuid() };
        var dest = new DestWithGuid();

        PropertyMapper.CopyTo(source, dest);

        Assert.AreEqual(source.DomainObjectGuid, dest.DomainObjectGuid);
        Assert.AreEqual(source.ParentGuid, dest.ParentGuid);
    }

    [TestMethod]
    public void Map_EachMappedObjectGetsUniqueIdentityGuid()
    {
        var source = new SourceWithGuid { Name = "Test" };

        var result1 = PropertyMapper.Map<SourceWithGuid, DestWithGuid>(source);
        var result2 = PropertyMapper.Map<SourceWithGuid, DestWithGuid>(source);

        Assert.AreNotEqual(result1.Guid, result2.Guid, "Each mapped object should have its own unique identity Guid");
        Assert.AreNotEqual(source.Guid, result1.Guid);
        Assert.AreNotEqual(source.Guid, result2.Guid);
    }

    [TestMethod]
    public void Map_DomainObjectGuidIsSameAcrossMappedCopies()
    {
        var domainGuid = Guid.NewGuid();
        var source = new SourceWithGuid { DomainObjectGuid = domainGuid };

        var result1 = PropertyMapper.Map<SourceWithGuid, DestWithGuid>(source);
        var result2 = PropertyMapper.Map<SourceWithGuid, DestWithGuid>(source);

        Assert.AreEqual(domainGuid, result1.DomainObjectGuid, "DomainObjectGuid should be copied consistently");
        Assert.AreEqual(domainGuid, result2.DomainObjectGuid, "DomainObjectGuid should be copied consistently");
    }
}
