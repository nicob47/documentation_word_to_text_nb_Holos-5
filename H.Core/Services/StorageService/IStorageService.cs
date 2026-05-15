using H.Core.Models;

namespace H.Core.Services.StorageService;

/// <summary>
/// Higher-level wrapper over <see cref="IStorage"/> that the GUI and most services consume.
/// Adds the concept of an "active farm" (the one the user is currently viewing / editing)
/// plus add-farm helpers, so callers don't have to walk <see cref="ApplicationData.Farms"/>
/// themselves.
///
/// <para><b>Why this exists separately from <see cref="IStorage"/>:</b></para>
/// <see cref="IStorage"/> is the file-system / serialization boundary — testing it requires
/// real file I/O or a heavyweight mock. <see cref="IStorageService"/> is the in-memory
/// abstraction: tests can swap the whole service for a Moq with a couple of <c>Setup</c>
/// calls, which is what the existing test suite does.
/// </para>
/// </summary>
public interface IStorageService
{
    #region Properties
    
    public IStorage Storage { get; set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Returns the active farm being used in the application.
    /// </summary>
    /// <returns>A <see cref="Farm"/> that the user is currently working with</returns>
    Farm GetActiveFarm();

    /// <summary>
    /// Returns all farms created by the user.
    /// </summary>
    /// <returns>A collection of all the farms in the system</returns>
    List<Farm> GetAllFarms();

    /// <summary>
    /// Sets the active farm
    /// </summary>
    /// <param name="farm">The <see cref="Farm"/> to be used as the active farm</param>
    /// <returns><see langword="true"/> if setting the active farm; <see langword="false"/> otherwise</returns>
    bool SetActiveFarm(Farm? farm);

    /// <summary>
    /// Add a new farm to storage
    /// </summary>
    /// <param name="farm">The new <see cref="Farm"/> that will be added to storage</param>
    void AddFarm(Farm? farm);

    #endregion
}