using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using H.Avalonia.ViewModels.OptionsViews.FileMenuViews;

namespace H.Avalonia.Views.OptionsViews.FileMenuViews;

public partial class FileImportFarmView : UserControl
{
    

    public FileImportFarmView()
    {
        InitializeComponent();
    }

    #region Public Methods
    private void FarmsDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is FileImportFarmViewModel vm)
        {
            vm.SelectedFarms = Enumerable.Cast<H.Core.Models.Farm>(FarmsDataGrid.SelectedItems).ToList();
        }
    }
    private async void OnSelectFarmOption_Clicked(object? sender, RoutedEventArgs args)
    {
        // Get top level from the current control.
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
        {
            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = H.Core.Properties.Resources.TitleSelectFarmFile,
                AllowMultiple = false,
                SuggestedStartLocation = await topLevel.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents),
                FileTypeFilter = new FilePickerFileType[]
                {
                    new("Holos export files|*.json")
                    {
                        Patterns = new[] { "*.json" }
                    }
                }
            });

            if (files.Count >= 1)
            {
                if (DataContext is FileImportFarmViewModel vm)
                {
                    vm.Farms.Clear();
                    var farms = await vm.GetFarmsFromExportFileAsync(files[0].Path.LocalPath);
                    if (farms != null)
                    {
                        foreach (var farm in farms)
                        {
                            vm.Farms.Add(farm);
                        }
                    }
                    vm.ShowGrid = vm.Farms.Count > 0;
                }
            }
        }
    }

    private async void OnSelectFolderOption_Clicked(object? sender, RoutedEventArgs args)
    {
        // Get top level from the current control.
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
        {
            // Start async operation to open the dialog.
            var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = H.Core.Properties.Resources.TitleSelectFarmFolder,
                AllowMultiple = false,
                SuggestedStartLocation = await topLevel.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents)
            });
            if (folder.Count >= 1)
            {
                if (DataContext is FileImportFarmViewModel vm)
                {
                    vm.Farms.Clear();
                    var farms = await vm.GetExportedFarmsFromDirectoryRecursivelyAsync(folder[0].Path.LocalPath);
                    if (farms != null)
                    {
                        foreach (var farm in farms)
                        {
                            vm.Farms.Add(farm);
                        }
                    }
                    vm.ShowGrid = vm.Farms.Count > 0;
                }
            }
        }
        #endregion
    }
}