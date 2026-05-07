using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using H.Avalonia.ViewModels.SupportingViews.DietFormulator;
using H.Avalonia.Views.SupportingViews.DietFormulator;
using H.Core.Enumerations;
using H.Core.Providers.Feed;
using H.Core.Services.DietService;
using H.Core.Services.StorageService;

namespace H.Avalonia.Services.DietFormulator;

/// <summary>
/// Opens the Diet Formulator modal window. Resolves the parent window from
/// <see cref="Application.Current"/>'s desktop lifetime at call time, so no
/// initialization step is required.
/// </summary>
public class DietFormulatorWindowService : IDietFormulatorWindowService
{
    private readonly IDietService _dietService;
    private readonly IFeedIngredientProvider _feedIngredientProvider;
    private readonly IStorageService _storageService;

    public DietFormulatorWindowService(
        IDietService dietService,
        IFeedIngredientProvider feedIngredientProvider,
        IStorageService storageService)
    {
        _dietService = dietService ?? throw new ArgumentNullException(nameof(dietService));
        _feedIngredientProvider = feedIngredientProvider ?? throw new ArgumentNullException(nameof(feedIngredientProvider));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
    }

    public async Task<DietDto?> ShowAsync(AnimalType animalType)
    {
        var vm = new DietFormulatorWindowViewModel(_dietService, _feedIngredientProvider, _storageService, animalType);
        var window = new DietFormulatorWindow { DataContext = vm };

        var owner = GetOwnerWindow();
        if (owner != null)
        {
            await window.ShowDialog(owner);
        }
        else
        {
            // Fallback: no desktop lifetime (test harness or non-desktop runtime).
            // Show as a non-modal window instead of throwing so headless tests don't fail.
            window.Show();
        }

        // After the modal closes, the VM's SavedDiet holds the working diet (Save) or null (Cancel/X).
        return vm.SavedDiet;
    }

    private static Window? GetOwnerWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }
}
