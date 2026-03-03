using System.Collections.ObjectModel;
using System.Linq;
using H.Core.Providers.Feed;
using H.Core.Services.DietService;
using H.Core.Services.StorageService;
using Prism.Regions;

namespace H.Avalonia.ViewModels.SupportingViews
{
    public class DietFormulatorViewModel : ViewModelBase
    {
        #region Fields

        private readonly IDietService _dietService = null!;
        private ObservableCollection<IDietDto> _diets = null!;
        private IDietDto _selectedDiet = null!;
        private string _searchText = string.Empty;

        #endregion

        #region Constructors

        public DietFormulatorViewModel()
        {
            // Design-time constructor
            Diets = new ObservableCollection<IDietDto>();
        }

        public DietFormulatorViewModel(
            IRegionManager regionManager, 
            IStorageService storageService,
            IDietService dietService) : base(regionManager, storageService)
        {
            _dietService = dietService ?? throw new System.ArgumentNullException(nameof(dietService));
            Diets = new ObservableCollection<IDietDto>();
            
            LoadDiets();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Collection of all available diets to display in the grid
        /// </summary>
        public ObservableCollection<IDietDto> Diets
        {
            get => _diets;
            set => SetProperty(ref _diets, value);
        }

        /// <summary>
        /// Currently selected diet in the grid
        /// </summary>
        public IDietDto SelectedDiet
        {
            get => _selectedDiet;
            set => SetProperty(ref _selectedDiet, value);
        }

        /// <summary>
        /// Search text for filtering diets
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set 
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterDiets();
                }
            }
        }

        #endregion

        #region Public Methods

        public override void InitializeViewModel()
        {
            base.InitializeViewModel();
            LoadDiets();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads all available diets from the diet service
        /// </summary>
        private void LoadDiets()
        {
            if (_dietService == null)
            {
                return;
            }

            var allDiets = _dietService.GetDiets();
            
            Diets.Clear();
            foreach (var diet in allDiets)
            {
                Diets.Add(diet);
            }
        }

        /// <summary>
        /// Filters diets based on search text
        /// </summary>
        private void FilterDiets()
        {
            if (_dietService == null)
            {
                return;
            }

            var allDiets = _dietService.GetDiets();
            
            Diets.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // Show all diets if no search text
                foreach (var diet in allDiets)
                {
                    Diets.Add(diet);
                }
            }
            else
            {
                // Filter diets based on name, animal type, or diet type
                var filteredDiets = allDiets.Where(d => 
                    (d.Name?.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase) == true) ||
                    (d.AnimalType.ToString().Contains(SearchText, System.StringComparison.OrdinalIgnoreCase)) ||
                    (d.DietType.ToString().Contains(SearchText, System.StringComparison.OrdinalIgnoreCase))).ToList();

                foreach (var diet in filteredDiets)
                {
                    Diets.Add(diet);
                }
            }
        }

        #endregion
    }
}