using H.Core.Models;

namespace H.Core.Services.StorageService;

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