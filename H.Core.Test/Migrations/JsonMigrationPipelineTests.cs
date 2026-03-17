using H.Core.Migrations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace H.Core.Test.Migrations;

[TestClass]
public class JsonMigrationPipelineTests
{
    [TestMethod]
    public void MigrateApplicationData_NoVersionField_TreatsAsV4AndSetsToCurrentVersion()
    {
        var root = new JObject
        {
            ["Farms"] = new JArray()
        };

        JsonMigrationPipeline.MigrateApplicationData(root);

        Assert.AreEqual(JsonMigrationPipeline.CurrentVersion.ToString(), root.Value<string>("Version"));
    }

    [TestMethod]
    public void MigrateApplicationData_AlreadyCurrentVersion_NoChanges()
    {
        var root = new JObject
        {
            ["Version"] = JsonMigrationPipeline.CurrentVersion.ToString(),
            ["Farms"] = new JArray()
        };

        var farmsBefore = root["Farms"]!.ToString();

        JsonMigrationPipeline.MigrateApplicationData(root);

        Assert.AreEqual(JsonMigrationPipeline.CurrentVersion.ToString(), root.Value<string>("Version"));
        Assert.AreEqual(farmsBefore, root["Farms"]!.ToString());
    }

    [TestMethod]
    public void MigrateApplicationData_SetsVersionStringAfterMigration()
    {
        var root = new JObject
        {
            ["Farms"] = new JArray()
        };

        JsonMigrationPipeline.MigrateApplicationData(root);

        var versionToken = root["Version"];
        Assert.IsNotNull(versionToken);
        Assert.AreEqual(JTokenType.String, versionToken!.Type);
        Assert.AreEqual("5.0", versionToken.Value<string>());
    }

    [TestMethod]
    public void MigrateFarmExport_BareArray_RunsMigrators()
    {
        var farms = new JArray
        {
            new JObject
            {
                ["Name"] = "Test Farm",
                ["Components"] = new JArray()
            }
        };

        // Should not throw
        JsonMigrationPipeline.MigrateFarmExport(farms);

        // Farm should still be intact
        Assert.AreEqual(1, farms.Count);
        Assert.AreEqual("Test Farm", farms[0].Value<string>("Name"));
    }

    [TestMethod]
    public void MigrateFarmExport_EmptyArray_NoErrors()
    {
        var farms = new JArray();

        // Should not throw
        JsonMigrationPipeline.MigrateFarmExport(farms);

        Assert.AreEqual(0, farms.Count);
    }
}
