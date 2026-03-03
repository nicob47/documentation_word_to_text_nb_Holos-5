using Prism.Regions;

namespace H.Avalonia.ViewModels.OptionsViews
{
    public class SelectOptionViewModel : ViewModelBase
    {
        private readonly IRegionManager _regionManager = null!;
        public SelectOptionViewModel()
        {

        }
        public SelectOptionViewModel(IRegionManager regionManager) : base(regionManager)
        {
            _regionManager = regionManager ?? throw new System.ArgumentNullException(nameof(regionManager));
        }
    }
}
