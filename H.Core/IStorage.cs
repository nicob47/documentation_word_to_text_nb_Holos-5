using H.Core.Models;

namespace H.Core;

/// <summary>
/// Low-level on-disk persistence contract. Defines the load / save lifecycle and exposes
/// the in-memory <see cref="Models.ApplicationData"/> aggregate that everything else
/// reads from. <see cref="Storage"/> is the production implementation; tests substitute a
/// mock that bypasses the file system.
///
/// <para><b>Two-tier abstraction:</b></para>
/// Most callers go through <see cref="Services.StorageService.IStorageService"/> instead of
/// this interface — that higher-level service wraps active-farm tracking, farm-add semantics,
/// and DI-friendly access. <see cref="IStorage"/> is purely the file-system / serialization
/// boundary.
/// </summary>
public interface IStorage
{
    #region Properties

    /// <summary>
    /// The root aggregate the rest of the application reads. Populated by <see cref="Load"/>
    /// at startup; mutated freely throughout the session; written back by <see cref="Save"/> /
    /// <see cref="SaveAsync"/> on demand or at shutdown.
    /// </summary>
    public ApplicationData ApplicationData { get; set; }

    /// <summary>
    /// In-flight save task — callers (notably <c>App.OnExit</c>) await this before tearing
    /// down so we don't kill a save mid-write. Defaults to <see cref="Task.CompletedTask"/>
    /// when no save is running.
    /// </summary>
    public Task SaveTask { get; set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Tries to load the user's .json data file. If the data file cannot be loaded, the method checks if there
    /// are any backups available. If yes tries to load the most recent backup, otherwise it sets <see cref="ApplicationData"/> to a new instance
    /// of <see cref="Models.ApplicationData"/>
    /// </summary>
    void Load();
    void Save();
    Task SaveAsync();

    #endregion
}