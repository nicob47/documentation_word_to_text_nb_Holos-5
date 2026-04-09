using Avalonia.Controls;
using System;
using H.Avalonia.ViewModels.FarmCreationViews;


namespace H.Avalonia.Views.FarmCreationViews;

public partial class FarmOpenExistingView : UserControl
{

    #region Fields

    private FarmOpenExistingViewModel? _farmOpenExistingViewmodel;

    #endregion

    #region Constructors
    public FarmOpenExistingView(FarmOpenExistingViewModel farmOpenExistingViewModel)
    {
        InitializeComponent();
        _farmOpenExistingViewmodel = farmOpenExistingViewModel ?? throw new ArgumentNullException(nameof(farmOpenExistingViewModel));
    }
    public FarmOpenExistingView()
    {
        InitializeComponent();
    }
    #endregion
}