using Avalonia.Controls;
using Avalonia.Interactivity;
using H.Avalonia.ViewModels.ComponentViews;
using H.Core.Models;

namespace H.Avalonia.Views.ComponentViews;

public partial class ChooseComponentsView : UserControl
{
    public ChooseComponentsView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handles a category card click — navigates from step 1 (categories) to step 2 (components).
    /// </summary>
    private void OnCategoryClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ComponentGroup group)
        {
            // Update the category title
            CategoryTitle.Text = group.CategoryName;

            // Set the ItemsSource for the component cards
            ComponentItems.ItemsSource = group.Components;

            // Update subtitle
            SubtitleText.Text = "Click + Add to Farm to include a component, then go back or finish";

            // Toggle visibility: hide categories, show components
            CategoryPanel.IsVisible = false;
            ComponentPanel.IsVisible = true;
        }
    }

    /// <summary>
    /// Handles the back button click — returns from step 2 (components) to step 1 (categories).
    /// </summary>
    private void OnBackToCategoriesClick(object? sender, RoutedEventArgs e)
    {
        // Update subtitle
        SubtitleText.Text = "Select a category to browse available components";

        // Toggle visibility: show categories, hide components
        CategoryPanel.IsVisible = true;
        ComponentPanel.IsVisible = false;
    }

    /// <summary>
    /// Handles the "+ Add to Farm" button click on a component card.
    /// Sets the selected component on the ViewModel and then triggers the add action.
    /// </summary>
    private void OnAddComponentClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button &&
            button.Tag is ComponentBase component &&
            this.DataContext is ChooseComponentsViewModel viewModel)
        {
            viewModel.SelectedComponent = component;
            viewModel.OnAddComponentExecute();
        }
    }
}
