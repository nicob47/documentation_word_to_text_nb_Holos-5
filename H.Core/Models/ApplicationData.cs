#region Imports

using Prism.Mvvm;

#endregion

namespace H.Core.Models
{
    /// <summary>
    /// </summary>
    public class ApplicationData : BindableBase
    {
        #region Fields

        private GlobalSettings _globalSettings = null!;
        private DisplayUnitStrings _displayDisplayUnitStrings = null!;
        private IList<Farm> _farms = null!;

        #endregion

        #region Constructors

        public ApplicationData()
        {
            this.GlobalSettings = new GlobalSettings();
            this.Farms = new List<Farm>();
            this.DisplayUnitStrings = new DisplayUnitStrings();
        }

        #endregion

        #region Properties

        public GlobalSettings GlobalSettings
        {
            get { return _globalSettings; }
            set { this.SetProperty(ref _globalSettings, value); }
        }

        public IList<Farm> Farms
        {
            get { return _farms; }
            set { this.SetProperty(ref _farms, value); }
        }

        public DisplayUnitStrings DisplayUnitStrings
        {
            get { return _displayDisplayUnitStrings; }
            set { SetProperty(ref _displayDisplayUnitStrings, value); }
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        #region Event Handlers

        #endregion
    }
}