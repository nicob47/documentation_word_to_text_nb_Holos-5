using Avalonia.Controls;
using System;
using H.Avalonia.ViewModels.FarmCreationViews;


namespace H.Avalonia.Views.FarmCreationViews;

public partial class FarmOpenExistingView : UserControl
{

    #region Fields

    private FarmOpenExistingViewmodel? _farmOpenExistingViewmodel;

    #endregion

    #region Constructors
    public FarmOpenExistingView(FarmOpenExistingViewmodel farmOpenExistingViewmodel)
    {
        InitializeComponent();
        _farmOpenExistingViewmodel = farmOpenExistingViewmodel ?? throw new ArgumentNullException(nameof(farmOpenExistingViewmodel));
    }
    public FarmOpenExistingView()
    {
        InitializeComponent();
    }
    #endregion
}