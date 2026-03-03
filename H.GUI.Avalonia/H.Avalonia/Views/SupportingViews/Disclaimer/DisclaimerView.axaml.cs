using Avalonia.Controls;
using Avalonia.Interactivity;
using H.Avalonia.ViewModels.SupportingViews.Disclaimer;

namespace H.Avalonia.Views.SupportingViews.Disclaimer
{

    public partial class DisclaimerView : UserControl
    {
        #region Constructors
        
        public DisclaimerView()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties

        public DisclaimerViewModel? ViewModel => DataContext as DisclaimerViewModel;

        #endregion

        #region Event Handlers

        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            if (this.ViewModel?.OkCommand.CanExecute(null!) == true)
            {
                this.ViewModel.OkCommand.Execute(null!);
            }
        } 

        #endregion
    }
}