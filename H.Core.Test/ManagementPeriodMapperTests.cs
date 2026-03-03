using AutoMapper;
using H.Core.Factories;
using H.Core.Factories.Animals;
using H.Core.Mappers;
using H.Core.Models.Animals;

namespace H.Core.Test;

[TestClass]
public class ManagementPeriodMapperTests
{
    private IMapper _dtoToDtoMapper = null!;
    private IMapper _domainToDtoMapper = null!;
    private IMapper _dtoToDomainMapper = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ManagementPeriodDtoToManagementPeriodDtoMapper>();
            cfg.AddProfile<ManagementPeriodToManagementPeriodDtoMapper>();
            cfg.AddProfile<ManagementPeriodDtoToManagementPeriodMapper>();
        });

        _dtoToDtoMapper = new Mapper(config);
        _domainToDtoMapper = new Mapper(config);
        _dtoToDomainMapper = new Mapper(config);
    }

    [TestMethod]
    public void ManagementPeriodDtoToManagementPeriodDtoMapper_MapsCorrectly()
    {
        // Arrange
        var source = new ManagementPeriodDto();
        source.Name = "Test Period";
        source.Start = new DateTime(2024, 1, 1);
        source.End = new DateTime(2024, 12, 31);
        source.NumberOfDays = 366;

        // Act
        var result = _dtoToDtoMapper.Map<ManagementPeriodDto>(source);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(source.Name, result.Name);
        Assert.AreEqual(source.Start, result.Start);
        Assert.AreEqual(source.End, result.End);
        Assert.AreEqual(source.NumberOfDays, result.NumberOfDays);
    }

    [TestMethod]
    public void ManagementPeriodToManagementPeriodDtoMapper_MapsCorrectly()
    {
        // Arrange
        var source = new ManagementPeriod();
        source.Name = "Test Domain Period";
        source.Start = new DateTime(2024, 7, 1);
        source.End = new DateTime(2024, 9, 30);
        source.NumberOfDays = 92;

        // Act
        var result = _domainToDtoMapper.Map<ManagementPeriodDto>(source);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(source.Name, result.Name);
        Assert.AreEqual(source.Start, result.Start);
        Assert.AreEqual(source.End, result.End);
        Assert.AreEqual(source.NumberOfDays, result.NumberOfDays);
    }

    [TestMethod]
    public void ManagementPeriodDtoToManagementPeriodMapper_MapsCorrectly()
    {
        // Arrange
        var source = new ManagementPeriodDto();
        source.Name = "Test DTO to Domain Period";
        source.Start = new DateTime(2024, 4, 1);
        source.End = new DateTime(2024, 6, 30);
        source.NumberOfDays = 91;

        // Act
        var result = _dtoToDomainMapper.Map<ManagementPeriod>(source);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(source.Name, result.Name);
        Assert.AreEqual(source.Start, result.Start);
        Assert.AreEqual(source.End, result.End);
        Assert.AreEqual(source.NumberOfDays, result.NumberOfDays);
    }
}
