using System;
using Avalonia.Controls;
using H.Avalonia.ViewModels.SupportingViews.CountrySelection;

namespace H.Avalonia.Views.SupportingViews.CountrySelection
{
    public partial class CountrySelectionView : UserControl
    {
        #region Fields

        private CountrySelectionViewModel? _countrySelectionViewModel;

        #endregion

        #region Constructors

        /// <summary>
        /// Parameterless constructor required by the Avalonia XAML loader.
        /// </summary>
        public CountrySelectionView()
        {
            InitializeComponent();
        }

        public CountrySelectionView(CountrySelectionViewModel countrySelectionViewModel)
        {
            InitializeComponent();
            _countrySelectionViewModel = countrySelectionViewModel ?? throw new ArgumentNullException(nameof(countrySelectionViewModel));
        }

        #endregion
    }
}