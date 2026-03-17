using Newtonsoft.Json.Linq;

namespace H.Core.Migrations;

/// <summary>
/// Defines a single version-to-version migration that patches raw JSON before deserialization.
/// Each implementation handles one version bump (e.g. v4.0 → v5.0).
/// </summary>
public interface IJsonMigration
{
    /// <summary>
    /// The version this migrator upgrades from.
    /// </summary>
    Version FromVersion { get; }

    /// <summary>
    /// The version this migrator upgrades to.
    /// </summary>
    Version ToVersion { get; }

    /// <summary>
    /// Patch a JObject representing an ApplicationData document (the main save file format).
    /// </summary>
    void MigrateApplicationData(JObject root);

    /// <summary>
    /// Patch a JArray representing a List&lt;Farm&gt; export document (the farm export format).
    /// </summary>
    void MigrateFarmExport(JArray farms);
}
