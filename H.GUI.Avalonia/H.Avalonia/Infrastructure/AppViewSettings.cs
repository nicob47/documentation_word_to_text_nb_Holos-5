using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace H.Avalonia.Infrastructure;

/// <summary>
/// Shared UI settings observable across regions (sidebar + content).
/// </summary>
public sealed class AppViewSettings : INotifyPropertyChanged
{
    public static AppViewSettings Instance { get; } = new();

    private bool _showAdvancedOptions;

    public bool ShowAdvancedOptions
    {
        get => _showAdvancedOptions;
        set
        {
            if (_showAdvancedOptions != value)
            {
                _showAdvancedOptions = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
