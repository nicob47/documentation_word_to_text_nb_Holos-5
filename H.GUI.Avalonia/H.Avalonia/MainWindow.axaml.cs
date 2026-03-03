using Avalonia.Controls;
using H.Avalonia.ViewModels;
using System;

namespace H.Avalonia
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Opened += OnOpened;
        }

        private void OnOpened(object? sender, EventArgs e)
        {
            ViewModel?.NotificationManagerService.Initialize(this);
        }

        private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;
    }
}