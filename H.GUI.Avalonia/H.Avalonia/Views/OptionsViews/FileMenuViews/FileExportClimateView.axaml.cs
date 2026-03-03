using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using H.Avalonia.ViewModels.OptionsViews.FileMenuViews;

namespace H.Avalonia.Views.OptionsViews.FileMenuViews;

public partial class FileExportClimateView : UserControl
{
    public FileExportClimateView()
    {
        InitializeComponent();
    }

    private void FarmsDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is FileExportClimateViewModel vm)
        {
            vm.SelectedFarms = Enumerable.Cast<H.Core.Models.Farm>(FarmsDataGrid.SelectedItems).ToList();
        }
    }

    private async void ExportButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is FileExportClimateViewModel vm)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel != null)
            {
                vm.SelectedFarms = vm.SelectedFarms.OrderBy(obj => obj.Name).ToList();
                foreach (H.Core.Models.Farm farm in vm.SelectedFarms)
                {
                    string fileName = $"{farm.Name} - Climate Data";
                    var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                    {
                        Title = H.Core.Properties.Resources.TitleExportClimate,
                        SuggestedStartLocation = await topLevel.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents),
                        FileTypeChoices = new FilePickerFileType[]
                    {
                        new("Holos Climate Export | *.csv")
                        {
                            Patterns = new[] { "*.csv" }
                        }
                    },
                        SuggestedFileName = fileName,
                    });

                    if (file != null)
                    {
                        var data = new ExportClimateData
                        {
                            File = file,
                            Farm = farm
                        };
                        vm.ExportClimate.Execute(data);
                    }
                }
                
            }
        }
    }

    public class ExportClimateData
    {
        public IStorageFile File { get; set; } = null!;
        public H.Core.Models.Farm Farm { get; set; } = null!;
    }
}