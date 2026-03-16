using Newtonsoft.Json.Linq;

namespace H.Core.Migrations;

/// <summary>
/// Migrates farm JSON from v4.0 to v5.0 schema.
///
/// Currently a placeholder — no patches are applied yet. When new properties are added
/// to domain models (e.g. NumberOfFields on RotationComponent), the patch logic goes here
/// to set sensible defaults for v4 files that lack those properties.
/// </summary>
public class V4ToV5Migration : IJsonMigration
{
    public Version FromVersion => new("4.0");
    public Version ToVersion => new("5.0");

    public void MigrateApplicationData(JObject root)
    {
        if (root["Farms"] is JArray farms)
        {
            MigrateFarms(farms);
        }
    }

    public void MigrateFarmExport(JArray farms)
    {
        MigrateFarms(farms);
    }

    private void MigrateFarms(JArray farms)
    {
        foreach (var farm in farms)
        {
            MigrateComponents(farm);
        }
    }

    private void MigrateComponents(JToken farm)
    {
        if (farm["Components"] is not JArray components)
        {
            return;
        }

        foreach (var component in components)
        {
            var typeName = component.Value<string>("$type");
            if (typeName == null)
            {
                continue;
            }

            // Future: patch specific component types here.
            // Example (when NumberOfFields is added to RotationComponent):
            //
            // if (typeName.Contains("RotationComponent"))
            // {
            //     if (component["NumberOfFields"] == null)
            //     {
            //         component["NumberOfFields"] = 1;
            //     }
            // }
        }
    }
}
