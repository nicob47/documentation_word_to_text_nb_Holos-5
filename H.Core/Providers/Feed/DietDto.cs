using H.Core.Enumerations;

namespace H.Core.Providers.Feed;

public class DietDto : IDietDto
{
    #region Constructors

    public DietDto()
    {
        this.Ingredients = new List<IFeedIngredient>();
    } 

    #endregion

    public bool IsDefaultDiet { get; set; }
    public string Name { get; set; } = string.Empty;
    public DietType DietType { get; set; }
    public AnimalType AnimalType { get; set; }
    public double MethaneConversionFactor { get; set; }
    public double DietaryNetEnergyConcentration { get; set; }
    public IReadOnlyCollection<IFeedIngredient> Ingredients { get; set; }
    public double Forage { get; set; }
    public string Comments { get; set; } = string.Empty;
    public double DailyDryMatterFeedIntakeOfFeed { get; set; }
    public double CrudeProtein { get; set; }
    public double TotalDigestibleNutrient { get; set; }
    public double Ash { get; set; }
    public bool IsCustomPlaceholderDiet { get; set; }
}