using Avalonia.Controls;

namespace H.Avalonia.Views
{
    public partial class FooterView : UserControl
    {
        private string _footerImageTitle = "CanadaLogo";

        public FooterView()
        {
            InitializeComponent();
            InitializeFooterImage();
        }

        public void InitializeFooterImage()
        {
            this.TryGetResource(_footerImageTitle, this.ActualThemeVariant, out var footerImage);
            this.DynamicImage.Source = (global::Avalonia.Media.IImage?)footerImage;
        }
    }
}