using H.Core.Migrations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace H.Core.Test.Migrations;

[TestClass]
public class V4ToV5MigrationTests
{
    private V4ToV5Migration _migration = null!;

    [TestInitialize]
    public void Setup()
    {
        _migration = new V4ToV5Migration();
    }

    [TestMethod]
    public void FromVersion_ReturnsV4()
    {
        Assert.AreEqual(new Version("4.0"), _migration.FromVersion);
    }

    [TestMethod]
    public void ToVersion_ReturnsV5()
    {
        Assert.AreEqual(new Version("5.0"), _migration.ToVersion);
    }

    [TestMethod]
    public void MigrateApplicationData_WithFarms_NoErrors()
    {
        var root = new JObject
        {
            ["Farms"] = new JArray
            {
                new JObject
                {
                    ["Name"] = "Test Farm",
                    ["Components"] = new JArray
                    {
                        new JObject
                        {
                            ["$type"] = "H.Core.Models.LandManagement.Rotation.RotationComponent, H.Core",
                            ["ShiftLeft"] = true
                        }
                    }
                }
            }
        };

        // Should not throw
        _migration.MigrateApplicationData(root);

        // Farm data should still be intact
        var farms = root["Farms"] as JArray;
        Assert.IsNotNull(farms);
        Assert.AreEqual(1, farms!.Count);
    }

    [TestMethod]
    public void MigrateApplicationData_NoFarmsProperty_NoErrors()
    {
        var root = new JObject
        {
            ["GlobalSettings"] = new JObject()
        };

        // Should not throw even when Farms is missing
        _migration.MigrateApplicationData(root);
    }

    [TestMethod]
    public void MigrateFarmExport_WithFarms_NoErrors()
    {
        var farms = new JArray
        {
            new JObject
            {
                ["Name"] = "Farm 1",
                ["Components"] = new JArray
                {
                    new JObject
                    {
                        ["$type"] = "H.Core.Models.Animals.Beef.CowCalfComponent, H.Core",
                        ["Name"] = "Beef Herd"
                    }
                }
            },
            new JObject
            {
                ["Name"] = "Farm 2",
                ["Components"] = new JArray()
            }
        };

        // Should not throw
        _migration.MigrateFarmExport(farms);

        Assert.AreEqual(2, farms.Count);
    }

    [TestMethod]
    public void MigrateFarmExport_EmptyArray_NoErrors()
    {
        var farms = new JArray();

        // Should not throw
        _migration.MigrateFarmExport(farms);

        Assert.AreEqual(0, farms.Count);
    }
}
