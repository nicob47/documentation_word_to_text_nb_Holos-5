using H.Avalonia.ViewModels.Wizard;
using H.Core.Enumerations;
using H.Core.Models.LandManagement.Shelterbelt;
using H.Infrastructure;

namespace H.Avalonia.ViewModels.ComponentViews.LandManagement.Shelterbelt
{
    /// <summary>
    /// Represents one expanded tree group card shown in Step 5.
    /// Each card corresponds to a <see cref="H.Core.Models.LandManagement.Shelterbelt.TreeGroupData"/> entry on the row.
    /// </summary>
    public class TreeGroupCardViewModel : ModelBase
    {
        #region Fields

        private readonly TreeGroupData _treeGroupData;
        private TreeSpecies _species;
        private double _plantedCount;
        private double _liveCount;
        private double _spacing;
        private int _plantYear;
        private double _circumference;

        #endregion

        #region Constructors

        public TreeGroupCardViewModel(TreeGroupData treeGroupData)
        {
            _treeGroupData = treeGroupData;
            _species = treeGroupData.TreeSpecies;
            _plantedCount = treeGroupData.PlantedTreeCount;
            _liveCount = treeGroupData.LiveTreeCount;
            _spacing = treeGroupData.PlantedTreeSpacing;
            _plantYear = treeGroupData.PlantYear > 1 ? treeGroupData.PlantYear : System.DateTime.Now.Year - 10;
            _circumference = treeGroupData.CircumferenceData.UserCircumference;

            // Ensure model is in sync
            _treeGroupData.TreeSpecies = _species;
            _treeGroupData.PlantedTreeCount = _plantedCount;
            _treeGroupData.LiveTreeCount = _liveCount;
            _treeGroupData.PlantedTreeSpacing = _spacing;
            _treeGroupData.PlantYear = _plantYear;
            _treeGroupData.CircumferenceData.UserCircumference = _circumference;
        }

        #endregion

        #region Properties

        public TreeSpecies Species
        {
            get => _species;
            set
            {
                if (SetProperty(ref _species, value))
                    _treeGroupData.TreeSpecies = value;
            }
        }

        public double PlantedCount
        {
            get => _plantedCount;
            set
            {
                if (SetProperty(ref _plantedCount, value))
                    _treeGroupData.PlantedTreeCount = value;
            }
        }

        public double LiveCount
        {
            get => _liveCount;
            set
            {
                if (SetProperty(ref _liveCount, value))
                    _treeGroupData.LiveTreeCount = value;
            }
        }

        public double Spacing
        {
            get => _spacing;
            set
            {
                if (SetProperty(ref _spacing, value))
                    _treeGroupData.PlantedTreeSpacing = value;
            }
        }

        public int PlantYear
        {
            get => _plantYear;
            set
            {
                if (SetProperty(ref _plantYear, value))
                    _treeGroupData.PlantYear = value;
            }
        }

        public double Circumference
        {
            get => _circumference;
            set
            {
                if (SetProperty(ref _circumference, value))
                    _treeGroupData.CircumferenceData.UserCircumference = value;
            }
        }

        /// <summary>Display name for the card header (species description).</summary>
        public string DisplayName => _species.GetDescription();

        #endregion
    }
}
