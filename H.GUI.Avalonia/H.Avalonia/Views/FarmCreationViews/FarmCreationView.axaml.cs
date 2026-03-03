using Avalonia.Controls;
using System;
using H.Avalonia.ViewModels.FarmCreationViews;

namespace H.Avalonia.Views.FarmCreationViews
{
    public partial class FarmCreationView : UserControl
    {
        #region Fields

        private FarmCreationViewModel? _farmCreationViewModel;

        #endregion

        #region Constructors

        public FarmCreationView()
        {
            InitializeComponent();
        }

        public FarmCreationView(FarmCreationViewModel farmCreationViewModel)
        {
            InitializeComponent();
            _farmCreationViewModel = farmCreationViewModel ?? throw new ArgumentNullException(nameof(farmCreationViewModel));
        }

        #endregion
    }
}