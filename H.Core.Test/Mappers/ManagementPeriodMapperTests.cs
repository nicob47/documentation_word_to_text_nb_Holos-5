using H.Core.Enumerations;
using H.Core.Factories.Animals;
using H.Core.Mappers;
using H.Core.Models.Animals;
using H.Core.Providers.Feed;

namespace H.Core.Test.Mappers;

[TestClass]
public class ManagementPeriodMapperTests
{
    private ManagementPeriodDtoToManagementPeriodDtoMapper _dtoToDtoMapper = null!;
    private ManagementPeriodToManagementPeriodDtoMapper _domainToDtoMapper = null!;
    private ManagementPeriodDtoToManagementPeriodMapper _dtoToDomainMapper = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _dtoToDtoMapper = new ManagementPeriodDtoToManagementPeriodDtoMapper();
        _domainToDtoMapper = new ManagementPeriodToManagementPeriodDtoMapper();
        _dtoToDomainMapper = new ManagementPeriodDtoToManagementPeriodMapper();
    }

    [TestMethod]
    public void DtoToDto_ClonesAllProperties()
    {
        var source = new ManagementPeriodDto
        {
            Name = "Summer Grazing",
            Start = new DateTime(2024, 5, 1),
            End = new DateTime(2024, 9, 30),
            NumberOfDays = 153,
            MethaneConversionFactor = 0.02,
            BeddingMaterialType = BeddingMaterialType.StrawLong
        };

        var result = _dtoToDtoMapper.Map(source);

        Assert.AreNotSame(source, result);
        Assert.AreEqual("Summer Grazing", result.Name);
        Assert.AreEqual(new DateTime(2024, 5, 1), result.Start);
        Assert.AreEqual(new DateTime(2024, 9, 30), result.End);
        Assert.AreEqual(153, result.NumberOfDays);
        Assert.AreEqual(0.02, result.MethaneConversionFactor);
        Assert.AreEqual(BeddingMaterialType.StrawLong, result.BeddingMaterialType);
    }

    [TestMethod]
    public void DtoToDomain_MapsNestedHousingProperties()
    {
        var source = new ManagementPeriodDto
        {
            Name = "Winter Housing",
            BeddingMaterialType = BeddingMaterialType.WoodChip,
            UserDefinedBeddingRate = 5.5,
            ActivityCoefficientOfFeedingSituation = 0.17,
            BaselineMaintenanceCoefficient = 0.386
        };

        var result = _dtoToDomainMapper.Map(source);

        Assert.AreEqual("Winter Housing", result.Name);
        Assert.AreEqual(BeddingMaterialType.WoodChip, result.HousingDetails.BeddingMaterialType);
        Assert.AreEqual(5.5, result.HousingDetails.UserDefinedBeddingRate);
        Assert.AreEqual(0.17, result.HousingDetails.ActivityCeofficientOfFeedingSituation);
        Assert.AreEqual(0.386, result.HousingDetails.BaselineMaintenanceCoefficient);
    }

    [TestMethod]
    public void DtoToDomain_MapsNestedManureProperties()
    {
        var source = new ManagementPeriodDto
        {
            MethaneConversionFactor = 0.02,
            VolatilizationFraction = 0.12,
            N2ODirectEmissionFactor = 0.005,
            LeachingFraction = 0.3,
            EmissionFactorVolatilization = 0.01,
            EmissionFactorLeaching = 0.0075,
            AshContentOfManure = 0.08,
            MethaneProducingCapacityOfManure = 0.24,
            VolatileSolidExcretion = 7.3,
            FractionOfNitrogenInManure = 0.004,
            FractionOfCarbonInManure = 0.04,
            FractionOfPhosphorusInManure = 0.0009
        };

        var result = _dtoToDomainMapper.Map(source);

        Assert.AreEqual(0.02, result.ManureDetails.MethaneConversionFactor);
        Assert.AreEqual(0.12, result.ManureDetails.VolatilizationFraction);
        Assert.AreEqual(0.005, result.ManureDetails.N2ODirectEmissionFactor);
        Assert.AreEqual(0.3, result.ManureDetails.LeachingFraction);
        Assert.AreEqual(0.01, result.ManureDetails.EmissionFactorVolatilization);
        Assert.AreEqual(0.0075, result.ManureDetails.EmissionFactorLeaching);
        Assert.AreEqual(0.08, result.ManureDetails.AshContentOfManure);
        Assert.AreEqual(0.24, result.ManureDetails.MethaneProducingCapacityOfManure);
        Assert.AreEqual(7.3, result.ManureDetails.VolatileSolidExcretion);
        Assert.AreEqual(0.004, result.ManureDetails.FractionOfNitrogenInManure);
        Assert.AreEqual(0.04, result.ManureDetails.FractionOfCarbonInManure);
        Assert.AreEqual(0.0009, result.ManureDetails.FractionOfPhosphorusInManure);
    }

    [TestMethod]
    public void DtoToDomain_MapsDietProperties()
    {
        var source = new ManagementPeriodDto
        {
            DietAdditiveType = DietAdditiveType.TwoPercentFat,
            SelectedDietType = DietType.Barley
        };

        var result = _dtoToDomainMapper.Map(source);

        Assert.AreEqual(DietAdditiveType.TwoPercentFat, result.DietAdditive);
        Assert.AreEqual(DietType.Barley, result.SelectedDiet.DietType);
    }

    [TestMethod]
    public void DomainToDto_FlattensNestedHousingProperties()
    {
        var source = new ManagementPeriod
        {
            Name = "Barn Period"
        };
        source.HousingDetails.BeddingMaterialType = BeddingMaterialType.Shavings;
        source.HousingDetails.UserDefinedBeddingRate = 3.2;
        source.HousingDetails.ActivityCeofficientOfFeedingSituation = 0.15;
        source.HousingDetails.BaselineMaintenanceCoefficient = 0.4;

        var result = _domainToDtoMapper.Map(source);

        Assert.AreEqual("Barn Period", result.Name);
        Assert.AreEqual(BeddingMaterialType.Shavings, result.BeddingMaterialType);
        Assert.AreEqual(3.2, result.UserDefinedBeddingRate);
        Assert.AreEqual(0.15, result.ActivityCoefficientOfFeedingSituation);
        Assert.AreEqual(0.4, result.BaselineMaintenanceCoefficient);
    }

    [TestMethod]
    public void DomainToDto_FlattensNestedManureProperties()
    {
        var source = new ManagementPeriod();
        source.ManureDetails.MethaneConversionFactor = 0.03;
        source.ManureDetails.VolatilizationFraction = 0.15;
        source.ManureDetails.N2ODirectEmissionFactor = 0.006;
        source.ManureDetails.LeachingFraction = 0.25;
        source.ManureDetails.AshContentOfManure = 0.09;
        source.ManureDetails.FractionOfNitrogenInManure = 0.005;
        source.ManureDetails.FractionOfCarbonInManure = 0.05;
        source.ManureDetails.FractionOfPhosphorusInManure = 0.001;

        var result = _domainToDtoMapper.Map(source);

        Assert.AreEqual(0.03, result.MethaneConversionFactor);
        Assert.AreEqual(0.15, result.VolatilizationFraction);
        Assert.AreEqual(0.006, result.N2ODirectEmissionFactor);
        Assert.AreEqual(0.25, result.LeachingFraction);
        Assert.AreEqual(0.09, result.AshContentOfManure);
        Assert.AreEqual(0.005, result.FractionOfNitrogenInManure);
        Assert.AreEqual(0.05, result.FractionOfCarbonInManure);
        Assert.AreEqual(0.001, result.FractionOfPhosphorusInManure);
    }

    [TestMethod]
    public void DomainToDto_FlattensDietProperties()
    {
        var source = new ManagementPeriod
        {
            DietAdditive = DietAdditiveType.FourPercentFat
        };
        source.SelectedDiet.DietType = DietType.HighEnergy;
        source.SelectedDiet.CrudeProtein = 16.5;
        source.SelectedDiet.Forage = 0.55;
        source.SelectedDiet.TotalDigestibleNutrient = 68.0;
        source.SelectedDiet.DailyDryMatterFeedIntakeOfFeed = 22.0;

        var result = _domainToDtoMapper.Map(source);

        Assert.AreEqual(DietAdditiveType.FourPercentFat, result.DietAdditiveType);
        Assert.AreEqual(DietType.HighEnergy, result.SelectedDietType);
        Assert.AreEqual(16.5, result.CrudeProtein);
        Assert.AreEqual(0.55, result.Forage);
        Assert.AreEqual(68.0, result.TotalDigestibleNutrient);
        Assert.AreEqual(22.0, result.DailyDryMatterFeedIntakeOfFeed);
    }
}
