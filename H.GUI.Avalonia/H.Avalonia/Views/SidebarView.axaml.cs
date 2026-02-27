using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace H.Avalonia.Views
{
    public partial class SidebarView : UserControl
    {
        #region Fields
        
        private Button? _navigationButton; 

        #endregion

        public SidebarView()
        {
            InitializeComponent();
            HighlightButton(this.AboutButton);
            _navigationButton = this.AboutButton;
        }

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

    }
}