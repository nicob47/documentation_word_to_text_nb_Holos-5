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

        Assert.AreEqual(1, (farms[0]["Components"] as JArray)!.Count);
        Assert.AreEqual(0, (farms[1]["Components"] as JArray)!.Count);
    }

    [TestMethod]
    public void MigrateFarmExport_EmptyArray_NoErrors()
    {
        var farms = new JArray();

        // Should not throw
        _migration.MigrateFarmExport(farms);

        Assert.AreEqual(0, farms.Count);
    }

    [TestMethod]
    public void MigrateApplicationData_WithNullComponents_NoErrors()
    {
        var root = new JObject
        {
            ["Farms"] = new JArray
            {
                new JObject
                {
                    ["Name"] = "Test Farm",
                    ["Components"] = null
                }
            }
        };

        // Should not throw
        _migration.MigrateApplicationData(root);

        var farms = root["Farms"] as JArray;
        Assert.IsNotNull(farms);
        Assert.AreEqual(1, farms.Count);
    }

    [TestMethod]
    public void MigrateApplicationData_WithMultipleFarmsAndComponents_PreservesStructure()
    {
        var root = new JObject
        {
            ["Farms"] = new JArray
            {
                new JObject
                {
                    ["Name"] = "Farm 1",
                    ["Components"] = new JArray
                    {
                        new JObject
                        {
                            ["$type"] = "H.Core.Models.LandManagement.Rotation.RotationComponent, H.Core",
                            ["Name"] = "Rotation 1"
                        },
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
                    ["Components"] = new JArray
                    {
                        new JObject
                        {
                            ["$type"] = "H.Core.Models.Animals.Swine.SwineComponent, H.Core"
                        }
                    }
                }
            }
        };

        _migration.MigrateApplicationData(root);

        var farms = root["Farms"] as JArray;
        Assert.IsNotNull(farms);
        Assert.AreEqual(2, farms.Count);
        Assert.AreEqual(2, (farms[0]["Components"] as JArray)!.Count);
        Assert.AreEqual(1, (farms[1]["Components"] as JArray)!.Count);
    }

    [TestMethod]
    public void MigrateFarmExport_WithVariousComponentTypes_PreserveAll()
    {
        var farms = new JArray
        {
            new JObject
            {
                ["Name"] = "Mixed Farm",
                ["Components"] = new JArray
                {
                    new JObject { ["$type"] = "H.Core.Models.LandManagement.Rotation.RotationComponent, H.Core" },
                    new JObject { ["$type"] = "H.Core.Models.Animals.Beef.CowCalfComponent, H.Core" },
                    new JObject { ["$type"] = "H.Core.Models.Animals.Poultry.LayerComponent, H.Core" },
                }
            }
        };

        _migration.MigrateFarmExport(farms);

        var components = (farms[0]["Components"] as JArray)!;
        Assert.AreEqual(3, components.Count);
    }

    [TestMethod]
    public void MigrateApplicationData_WithEmptyFarmsArray_NoErrors()
    {
        var root = new JObject
        {
            ["Farms"] = new JArray()
        };

        _migration.MigrateApplicationData(root);

        var farms = root["Farms"] as JArray;
        Assert.IsNotNull(farms);
        Assert.AreEqual(0, farms.Count);
    }
}
