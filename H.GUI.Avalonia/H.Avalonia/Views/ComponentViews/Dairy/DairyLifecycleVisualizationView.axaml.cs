using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace H.Avalonia.Views.ComponentViews.Dairy;

/// <summary>
/// Visual diagram showing the dairy cattle lifecycle from birth through productive life.
/// Helps users understand the flow between lifecycle stages and how herd parameters affect animal distribution.
/// </summary>
public partial class DairyLifecycleVisualizationView : UserControl, INotifyPropertyChanged
{
    private string? _selectedStage;
    private bool _hasSelection;

    public DairyLifecycleVisualizationView()
    {
        InitializeComponent();
        DataContext = this;
    }

    public string? SelectedStage
    {
        get => _selectedStage;
        set
        {
            if (_selectedStage != value)
            {
                _selectedStage = value;
                HasSelection = !string.IsNullOrEmpty(value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedStageTitle));
                OnPropertyChanged(nameof(SelectedStageDescription));
                
                // Notify all selection state properties
                OnPropertyChanged(nameof(IsCalfSelected));
                OnPropertyChanged(nameof(IsHeiferSelected));
                OnPropertyChanged(nameof(IsLactatingSelected));
                OnPropertyChanged(nameof(IsDrySelected));
                OnPropertyChanged(nameof(IsCullingSelected));
                OnPropertyChanged(nameof(IsReplacementSelected));
            }
        }
    }

    public bool HasSelection
    {
        get => _hasSelection;
        set
        {
            if (_hasSelection != value)
            {
                _hasSelection = value;
                OnPropertyChanged();
            }
        }
    }

    public string SelectedStageTitle => SelectedStage switch
    {
        "Calf" => "Calf Stage (Birth - 4 months)",
        "Heifer" => "Heifer Stage (4 months - First Calving)",
        "Lactating" => "Lactating Cow Stage (Milk Production)",
        "Dry" => "Dry Cow Stage (Pre-Calving Rest)",
        "Culling" => "Culling - Removing Animals from the Herd",
        "Replacement" => "Replacement - Adding Heifers to the Herd",
        _ => ""
    };

    public string SelectedStageDescription => SelectedStage switch
    {
        "Calf" => "The calf stage begins at birth and lasts approximately 4 months. During this critical growth period, calves are fed milk replacer or whole milk along with starter grain. This stage focuses on building a strong foundation for future productivity. Proper nutrition and health management during this period are essential for developing healthy replacement heifers. Male calves are typically sold or raised for beef, while female calves that survive (accounting for mortality rate) become the future replacement heifers for the dairy herd.",
        
        "Heifer" => "The heifer stage spans from weaning (around 4 months) until first calving (typically 22-26 months of age). During this extended growing phase, heifers are developed to reach appropriate breeding weight and body condition. They are bred at around 13-15 months and go through their first pregnancy. This stage is crucial for proper skeletal and mammary development. The number of heifers needed is determined by your replacement rate - the percentage of the milking herd that needs to be replaced annually due to culling. Proper heifer development directly impacts future milk production and cow longevity.",
        
        "Lactating" => "The lactating cow stage is the productive heart of the dairy operation. After calving, cows enter lactation and produce milk for approximately 305 days (10 months). Milk production follows a lactation curve - peaking around 60-90 days after calving, then gradually declining. Lactating cows have the highest nutritional requirements and energy demands to support milk production. They also produce the most manure and have the highest greenhouse gas emissions, but these are offset by the valuable milk production. The majority of your milking herd will be in this stage at any given time, with the exact number depending on your dry period length.",
        
        "Dry" => "The dry period is a non-lactating rest period before the next calving, typically lasting 60 days. During this time, cows are not milked, allowing their mammary tissue to regenerate and prepare for the next lactation. Dry cows have lower nutritional requirements than lactating cows but need specific nutrition to support the developing calf and prepare for calving. This rest period is critical for maximizing milk production in the next lactation and maintaining cow health. The number of dry cows in your herd is determined by your dry period length - a typical 60-day dry period means approximately 16% of your milking herd will be dry at any time.",
        
        "Culling" => "Culling is the process of removing cows from the milking herd, typically due to low production, health issues, reproductive problems, or age. The replacement rate parameter you set determines how many cows are culled annually - for example, a 28% replacement rate means approximately 28% of your milking herd is replaced each year. Culled cows are usually sold for beef. This natural turnover is essential for maintaining herd health and productivity, as it allows you to remove underperforming animals and bring in fresh genetics through young heifers. The culling rate directly determines how many replacement heifers you need to raise.",
        
        "Replacement" => "Replacement refers to the process of adding bred heifers to the milking herd to replace culled cows. These heifers have completed their growth phase and are ready for their first calving. The number of replacements needed equals your culling rate - if you cull 28% of the herd annually, you need enough heifers to replace them. However, you must account for heifer mortality and breeding success, so you typically raise more heifers than the exact replacement number needed. The replacement rate is one of the most important parameters in dairy herd management, as it determines the size of your heifer population and impacts overall farm profitability and sustainability.",
        
        _ => ""
    };

    private void Calf_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        SelectedStage = "Calf";
    }

    private void Heifer_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        SelectedStage = "Heifer";
    }

    private void Lactating_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        SelectedStage = "Lactating";
    }

    private void Dry_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        SelectedStage = "Dry";
    }

    private void Culling_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        SelectedStage = "Culling";
    }

    private void Replacement_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        SelectedStage = "Replacement";
    }

    /// <summary>
    /// Returns true if the specified stage is currently selected
    /// </summary>
    public bool IsStageSelected(string stageName)
    {
        return SelectedStage == stageName;
    }

    public bool IsCalfSelected => IsStageSelected("Calf");
    public bool IsHeiferSelected => IsStageSelected("Heifer");
    public bool IsLactatingSelected => IsStageSelected("Lactating");
    public bool IsDrySelected => IsStageSelected("Dry");
    public bool IsCullingSelected => IsStageSelected("Culling");
    public bool IsReplacementSelected => IsStageSelected("Replacement");

    private void CloseInfo_Click(object? sender, RoutedEventArgs e)
    {
        SelectedStage = null;
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}


