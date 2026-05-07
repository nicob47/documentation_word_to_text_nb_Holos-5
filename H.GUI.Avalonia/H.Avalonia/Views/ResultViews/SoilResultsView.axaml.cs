using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using H.Avalonia.ViewModels.Results;

namespace H.Avalonia.Views.ResultViews
{
    public partial class SoilResultsView : UserControl
    {
        #region Fields

        private Button? _navigationButton;

        #endregion

        #region Constructors

        public SoilResultsView()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties

        private SoilResultsViewModel? ViewModel
        {
            get { return DataContext as SoilResultsViewModel; }
        }

        #endregion

        #region Private Methods

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private TopLevel GetTopLevel()
        {
            return TopLevel.GetTopLevel(this) ?? throw new NullReferenceException("Invalid Owner");
        }

        #endregion

        #region Event Handlers

        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            if (_navigationButton != null)
            {
                _navigationButton.Background = Brushes.Transparent;
            }
            _navigationButton = button;
            HighlightButton(button);
        }

        private void HighlightButton(Button button)
        {
            button.Background = new SolidColorBrush()
            {
                Color = Color.Parse("#d6e4f5"),
                Opacity = 0.25,
            };
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
        }

        private async void ExportDataButton_OnClick(object? sender, RoutedEventArgs e)
        {
            if (ViewModel is null) return;
            var storageProvider = GetTopLevel().StorageProvider;
            var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
            {
                Title = Core.Properties.Resources.ExportDefaultName,
                SuggestedFileName = Core.Properties.Resources.SoilDataExportDefaultName,
                DefaultExtension = "csv",
                ShowOverwritePrompt = true,
            });

            if (file is not null && ViewModel.ExportToCsvCommand.CanExecute(file))
            {
                ViewModel.ExportToCsvCommand.Execute(file);
            }
        }

        private void ChooseSelectedSoilButton_OnClick(object? sender, RoutedEventArgs e)
        {
            this.ViewModel?.ChooseSelectedSoilCommand.Execute();
        }

        #endregion
    }
}