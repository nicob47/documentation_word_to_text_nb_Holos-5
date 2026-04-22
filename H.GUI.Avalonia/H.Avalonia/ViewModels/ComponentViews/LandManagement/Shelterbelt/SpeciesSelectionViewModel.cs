using H.Core.Enumerations;
using H.Infrastructure;

namespace H.Avalonia.ViewModels.ComponentViews.LandManagement.Shelterbelt
{
    /// <summary>
    /// Represents a species chip in the species picker grid on Step 5.
    /// </summary>
    public class SpeciesSelectionViewModel : ModelBase
    {
        private bool _isSelected;

        public SpeciesSelectionViewModel(TreeSpecies species, bool isSelected = false)
        {
            Species = species;
            _isSelected = isSelected;
        }

        public TreeSpecies Species { get; }

        public string DisplayName => Species.GetDescription();

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
