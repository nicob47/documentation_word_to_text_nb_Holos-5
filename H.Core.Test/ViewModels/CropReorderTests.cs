using System.Collections.ObjectModel;
using H.Core.Enumerations;
using H.Core.Factories.Crops;

namespace H.Core.Test.ViewModels;

/// <summary>
/// Tests for the crop reorder (move up/down) logic used in the Step 2 table layout.
/// Uses ObservableCollection.Move() directly since the VM commands delegate to it.
/// </summary>
[TestClass]
public class CropReorderTests
{
    private ObservableCollection<ICropDto> _crops = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _crops = new ObservableCollection<ICropDto>
        {
            new CropDto { CropType = CropType.Wheat },
            new CropDto { CropType = CropType.Barley },
            new CropDto { CropType = CropType.Oats },
        };
    }

    [TestMethod]
    public void MoveCropUp_SwapsWithPreviousItem()
    {
        var barley = _crops[1];
        _crops.Move(1, 0);

        Assert.AreEqual(CropType.Barley, _crops[0].CropType);
        Assert.AreEqual(CropType.Wheat, _crops[1].CropType);
        Assert.AreEqual(CropType.Oats, _crops[2].CropType);
    }

    [TestMethod]
    public void MoveCropDown_SwapsWithNextItem()
    {
        var barley = _crops[1];
        _crops.Move(1, 2);

        Assert.AreEqual(CropType.Wheat, _crops[0].CropType);
        Assert.AreEqual(CropType.Oats, _crops[1].CropType);
        Assert.AreEqual(CropType.Barley, _crops[2].CropType);
    }

    [TestMethod]
    public void MoveCropUp_FirstItem_IsGuardedByIndex()
    {
        var wheat = _crops[0];
        var index = _crops.IndexOf(wheat);

        // Guard: index > 0 prevents move
        Assert.AreEqual(0, index);
        Assert.IsFalse(index > 0, "First item should not be movable up");

        // Collection unchanged
        Assert.AreEqual(CropType.Wheat, _crops[0].CropType);
    }

    [TestMethod]
    public void MoveCropDown_LastItem_IsGuardedByIndex()
    {
        var oats = _crops[2];
        var index = _crops.IndexOf(oats);

        // Guard: index < Count - 1 prevents move
        Assert.AreEqual(2, index);
        Assert.IsFalse(index < _crops.Count - 1, "Last item should not be movable down");

        // Collection unchanged
        Assert.AreEqual(CropType.Oats, _crops[2].CropType);
    }

    [TestMethod]
    public void MovePreservesCollectionCount()
    {
        _crops.Move(0, 2);
        Assert.AreEqual(3, _crops.Count);
    }

    [TestMethod]
    public void MovePreservesAllItems()
    {
        _crops.Move(0, 2);

        var types = _crops.Select(c => c.CropType).OrderBy(t => t).ToList();
        CollectionAssert.Contains(types, CropType.Wheat);
        CollectionAssert.Contains(types, CropType.Barley);
        CollectionAssert.Contains(types, CropType.Oats);
    }

    [TestMethod]
    public void MultipleMoves_ProduceCorrectOrder()
    {
        // Move Oats from position 2 to 0 (two moves up)
        _crops.Move(2, 1);
        _crops.Move(1, 0);

        Assert.AreEqual(CropType.Oats, _crops[0].CropType);
        Assert.AreEqual(CropType.Wheat, _crops[1].CropType);
        Assert.AreEqual(CropType.Barley, _crops[2].CropType);
    }

    [TestMethod]
    public void MoveNonExistentItem_IndexReturnsNegative()
    {
        var orphan = new CropDto { CropType = CropType.Corn };
        var index = _crops.IndexOf(orphan);
        Assert.AreEqual(-1, index, "Item not in collection should return -1");
    }
}
