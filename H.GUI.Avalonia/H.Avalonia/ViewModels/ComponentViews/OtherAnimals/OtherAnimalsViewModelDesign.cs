using H.Core.Enumerations;
using H.Core.Factories.Animals;
using System;
using System.Collections.ObjectModel;

namespace H.Avalonia.ViewModels.ComponentViews.OtherAnimals
{
    public class OtherAnimalsViewModelDesign : OtherAnimalsViewModelBase
    {
        public OtherAnimalsViewModelDesign()
        {
            ViewName = "Bison";

            var validAnimalTypes = new ObservableCollection<AnimalType>(
            [
                AnimalType.NotSelected,
                AnimalType.Bison,
                AnimalType.Goats,
                AnimalType.Alpacas,
                AnimalType.Deer,
                AnimalType.Elk,
                AnimalType.Llamas,
                AnimalType.Horses,
                AnimalType.Mules
            ]);

            var group = new AnimalGroupDto
            {
                ValidAnimalTypes = validAnimalTypes,
                GroupType = AnimalType.Bison,
            };

            group.ManagementPeriodDtos.Add(new ManagementPeriodDto
            {
                Name = "Practice 1",
                Start = new DateTime(DateTime.Now.Year, 1, 1),
                End = new DateTime(DateTime.Now.Year, 6, 30),
                NumberOfDays = 181,
                NumberOfAnimals = 20,
            });

            group.ManagementPeriodDtos.Add(new ManagementPeriodDto
            {
                Name = "Practice 2",
                Start = new DateTime(DateTime.Now.Year, 7, 1),
                End = new DateTime(DateTime.Now.Year, 12, 31),
                NumberOfDays = 184,
                NumberOfAnimals = 15,
            });

            base.AnimalGroupDtos.Add(group);
            base.AnimalGroupDtos.Add(new AnimalGroupDto { ValidAnimalTypes = validAnimalTypes, GroupType = AnimalType.Alpacas });

            SelectedAnimalGroup = group;
        }
    }
}
