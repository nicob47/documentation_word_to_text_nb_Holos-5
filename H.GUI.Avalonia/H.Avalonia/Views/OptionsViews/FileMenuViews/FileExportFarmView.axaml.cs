using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using H.Avalonia.ViewModels.OptionsViews.FileMenuViews;

namespace H.Avalonia.Views.OptionsViews.FileMenuViews;

public partial class FileExportFarmView : UserControl
{

    public FileExportFarmView()
    {
        InitializeComponent();
    }

    private void FarmsDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is FileExportFarmViewModel vm)
        {
            vm.SelectedFarms = Enumerable.Cast<H.Core.Models.Farm>(FarmsDataGrid.SelectedItems).ToList();
        }
    }

    private async void SaveFileButton_Clicked(object? sender, RoutedEventArgs args)
    {
        
        if(DataContext is FileExportFarmViewModel vm)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel != null)
            {
                vm.SelectedFarms = vm.SelectedFarms.OrderBy(obj => obj.Name).ToList();
                string fileName = string.Join(",", vm.SelectedFarms.Select(farm => farm.Name));
                var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = H.Core.Properties.Resources.TitleExportFarm,
                    SuggestedStartLocation = await topLevel.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents),
                    FileTypeChoices = new FilePickerFileType[]
                    {
                        new("Holos export files (*.json)")
                        {
                            Patterns = new[] { "*.json" }
                        }
                    },
                    SuggestedFileName = fileName,
                });

                if (file != null)
                {
                    vm.ExportFarms.Execute(file);
                }
            }
        }
    }
}