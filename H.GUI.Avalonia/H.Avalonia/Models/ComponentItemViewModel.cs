using System;
using System.ComponentModel;
using H.Core.Models;

namespace H.Avalonia.Models;

/// <summary>
/// Wrapper class for ComponentBase to add UI-specific properties like IsSelected
/// </summary>
public class ComponentItemViewModel : ModelBase
{
    private ComponentBase? _component;
    private bool _isSelected;

    public ComponentItemViewModel(ComponentBase component)
    {
        _component = component ?? throw new ArgumentNullException(nameof(component));
        
        // Forward property change notifications from the wrapped component
        _component.PropertyChanged += OnComponentPropertyChanged;
    }

    /// <summary>
    /// The wrapped ComponentBase instance
    /// </summary>
    public ComponentBase? Component
    {
        get => _component;
        set => SetProperty(ref _component, value);
    }

    /// <summary>
    /// Indicates whether this component is currently selected in the UI
    /// </summary>
    public new bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    /// <summary>
    /// Proxy properties to access ComponentBase properties directly
    /// </summary>
    public string Name
    {
        get => _component?.Name ?? string.Empty;
        set
        {
            if (_component is not null)
            {
                _component.Name = value;
            }
        }
    }

    public string ComponentNameDisplayString
    {
        get => _component?.ComponentNameDisplayString ?? string.Empty;
    }

    public string ComponentDescriptionString
    {
        get => _component?.ComponentDescriptionString ?? string.Empty;
    }

    public string ComponentTypeString
    {
        get => _component?.ComponentTypeString ?? string.Empty;
    }

    private void OnComponentPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Forward specific property change notifications to update the UI
        switch (e.PropertyName)
        {
            case nameof(ComponentBase.Name):
                RaisePropertyChanged(nameof(Name));
                break;
            case nameof(ComponentBase.ComponentNameDisplayString):
                RaisePropertyChanged(nameof(ComponentNameDisplayString));
                break;
            case nameof(ComponentBase.ComponentDescriptionString):
                RaisePropertyChanged(nameof(ComponentDescriptionString));
                break;
            case nameof(ComponentBase.ComponentTypeString):
                RaisePropertyChanged(nameof(ComponentTypeString));
                break;
        }
    }

    public void Cleanup()
    {
        if (_component is not null)
        {
            _component.PropertyChanged -= OnComponentPropertyChanged;
        }
    }
}