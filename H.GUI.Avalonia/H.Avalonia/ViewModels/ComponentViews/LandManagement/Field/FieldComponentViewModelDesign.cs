using System.Collections.ObjectModel;
using H.Core.Enumerations;
using H.Core.Factories;
using H.Core.Factories.Crops;
using H.Core.Factories.Fields;
using H.Core.Models;
using H.Core.Services.CropColorService;
using H.Core.Services.LandManagement.Fields;
using H.Core.Services.StorageService;
using Microsoft.Extensions.Logging;
using Prism.Events;
using Prism.Regions;

namespace H.Avalonia.ViewModels.ComponentViews.LandManagement.Field;

public class FieldComponentViewModelDesign : FieldComponentViewModel
{
    public FieldComponentViewModelDesign()
    {
        base.SelectedFieldSystemComponentDto = new FieldSystemComponentDto();
        base.SelectedFieldSystemComponentDto.Name = "A Field";
        this.SelectedFieldSystemComponentDto.FieldArea = 23.55;
        this.SelectedFieldSystemComponentDto.StartYear = 1980;
        this.SelectedFieldSystemComponentDto.EndYear = 2050;

        base.SelectedFieldSystemComponentDto.CropDtos = new ObservableCollection<ICropDto>()
        {
            new CropDto() { Year = 2021, CropType = CropType.Wheat },
            new CropDto() { Year = 2022, CropType = CropType.Barley },
            new CropDto() { Year = 2023, CropType = CropType.Oats },
        };

        base.StorageService = new DefaultStorageService(new H.Core.Storage());
        base.StorageService.Storage = new H.Core.Storage();
        base.StorageService.Storage.ApplicationData = new ApplicationData
        {
            DisplayUnitStrings = new DisplayUnitStrings()
            {
                HectaresString = "(ha)",
                MillimetersPerHectareString = "(mm ha⁻¹)",
                KilogramsPerHectareString = "(kg ha⁻¹)"
            }
        };

        base.SelectedCropDto = new CropDto();
        base.SelectedCropDto.AmountOfIrrigation = 100;
    }

    public FieldComponentViewModelDesign(IRegionManager regionManager, IEventAggregator eventAggregator, IStorageService storageService, IFieldFactory fieldFactory, ICropFactory cropFactory, IFieldComponentService fieldComponentService, ILogger logger, ICropColorService cropColorService) : base(regionManager, eventAggregator, storageService, fieldComponentService, logger, cropFactory, cropColorService)
    {
    }
}