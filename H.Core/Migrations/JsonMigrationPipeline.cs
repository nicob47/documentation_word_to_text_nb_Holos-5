using System.Diagnostics;
using Newtonsoft.Json.Linq;
using NLog;

namespace H.Core.Migrations;

/// <summary>
/// Schema-migration pipeline for Holos JSON files. Runs a chain of
/// <see cref="IJsonMigration"/> instances in order to upgrade raw JSON from an older schema
/// version up to <see cref="CurrentVersion"/> <i>before</i> Newtonsoft.Json deserialization
/// touches the data. Operating on the JObject before deserialization lets us rename / move /
/// add fields without needing the destination type to still accept the legacy shape.
///
/// <para><b>Two file shapes:</b></para>
/// <list type="bullet">
///   <item>
///     <b><see cref="MigrateApplicationData"/></b> — the primary <c>json-data.json</c> file,
///     which serializes the whole <c>ApplicationData</c> aggregate (farms + settings +
///     defaults). Has a top-level <c>Version</c> field, parsed via <see cref="ParseVersion"/>.
///   </item>
///   <item>
///     <b><see cref="MigrateFarmExport"/></b> — the v4 export shape: a bare JSON array of
///     farms with <i>no</i> version field. Assumed to be <see cref="DefaultVersion"/> (4.0)
///     and run through every migration unconditionally. This is the path that v4 .json
///     imports hit.
///   </item>
/// </list>
///
/// <para><b>Adding a new migration:</b></para>
/// Implement <see cref="IJsonMigration"/> (with appropriate <c>FromVersion</c> /
/// <c>ToVersion</c>) and append to the <see cref="Migrations"/> list. The pipeline picks it
/// up automatically — migrations are applied in <c>FromVersion</c> order so later steps see
/// the output of earlier steps.
/// </summary>
public static class JsonMigrationPipeline
{
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The current schema version. All newly saved files will have this version.
    /// </summary>
    public static readonly Version CurrentVersion = new("5.0");

    /// <summary>
    /// The version assumed for files that have no Version field (i.e. v4 files).
    /// </summary>
    private static readonly Version DefaultVersion = new("4.0");

    /// <summary>
    /// Registered migrations, ordered by <see cref="IJsonMigration.FromVersion"/>.
    /// Add new migrations here as needed.
    /// </summary>
    private static readonly List<IJsonMigration> Migrations = new()
    {
        new V4ToV5Migration(),
    };

    /// <summary>
    /// Migrate an ApplicationData JObject from its detected version to <see cref="CurrentVersion"/>.
    /// The Version field is updated in the JObject after migration.
    /// </summary>
    public static void MigrateApplicationData(JObject root)
    {
        var version = ParseVersion(root.Value<string>("Version"));

        foreach (var migration in Migrations.OrderBy(m => m.FromVersion))
        {
            if (version >= migration.FromVersion && version < migration.ToVersion)
            {
                _log.Info($"Running migration {migration.GetType().Name} ({migration.FromVersion} â†’ {migration.ToVersion})");
                migration.MigrateApplicationData(root);
                version = migration.ToVersion;
            }
        }

        root["Version"] = CurrentVersion.ToString();
    }

    /// <summary>
    /// Migrate a List&lt;Farm&gt; JArray (the bare export file format used by v4).
    /// </summary>
    public static void MigrateFarmExport(JArray farms)
    {
        // Export files in v4 format are bare arrays with no version field.
        // We assume the oldest supported version and run all applicable migrations.
        var version = DefaultVersion;

        foreach (var migration in Migrations.OrderBy(m => m.FromVersion))
        {
            if (version >= migration.FromVersion && version < migration.ToVersion)
            {
                _log.Info($"Running export migration {migration.GetType().Name} ({migration.FromVersion} â†’ {migration.ToVersion})");
                migration.MigrateFarmExport(farms);
                version = migration.ToVersion;
            }
        }
    }

    /// <summary>
    /// Parse a version string, falling back to <see cref="DefaultVersion"/> if null, empty, or invalid.
    /// </summary>
    private static Version ParseVersion(string? versionString)
    {
        if (string.IsNullOrWhiteSpace(versionString))
        {
            return DefaultVersion;
        }

        return Version.TryParse(versionString, out var parsed) ? parsed : DefaultVersion;
    }
}
