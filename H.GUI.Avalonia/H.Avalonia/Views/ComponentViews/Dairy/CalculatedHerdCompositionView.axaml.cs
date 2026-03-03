using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using H.Avalonia.ViewModels.ComponentViews.Dairy;

namespace H.Avalonia.Views.ComponentViews.Dairy
{
    /// <summary>
    /// View for the Calculated Herd Composition section.
    /// This view displays the calculated distribution of animals across different lifecycle stages
    /// based on the herd parameters entered in the Herd Overview.
    /// </summary>
    public partial class CalculatedHerdCompositionView : UserControl
    {
        public CalculatedHerdCompositionView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles click event for the Calf herd stage card
        /// </summary>
        private void OnCalfCardClick(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is DairyComponentViewModel viewModel)
            {
                viewModel.SelectHerdStage("Calf");
            }
        }

        /// <summary>
        /// Handles click event for the Heifer herd stage card
        /// </summary>
        private void OnHeiferCardClick(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is DairyComponentViewModel viewModel)
            {
                viewModel.SelectHerdStage("Heifer");
            }
        }

        /// <summary>
        /// Handles click event for the Lactating herd stage card
        /// </summary>
        private void OnLactatingCardClick(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is DairyComponentViewModel viewModel)
            {
                viewModel.SelectHerdStage("Lactating");
            }
        }

        /// <summary>
        /// Handles click event for the Dry herd stage card
        /// </summary>
        private void OnDryCardClick(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is DairyComponentViewModel viewModel)
            {
                viewModel.SelectHerdStage("Dry");
            }
        }
    }
}

