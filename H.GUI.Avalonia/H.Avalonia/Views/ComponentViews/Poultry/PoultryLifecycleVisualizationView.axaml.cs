using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace H.Avalonia.Views.ComponentViews.Poultry;

/// <summary>
/// Visual diagram showing the poultry production chain from breeding through egg/meat production.
/// Helps users understand how the 7 poultry component types relate to each other in the industry pipeline.
/// </summary>
public partial class PoultryLifecycleVisualizationView : UserControl, INotifyPropertyChanged
{
    private string? _selectedStage;
    private bool _hasSelection;

    public PoultryLifecycleVisualizationView()
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

                OnPropertyChanged(nameof(IsBreederSelected));
                OnPropertyChanged(nameof(IsHatcherySelected));
                OnPropertyChanged(nameof(IsPulletSelected));
                OnPropertyChanged(nameof(IsEggSelected));
                OnPropertyChanged(nameof(IsChickenMeatSelected));
                OnPropertyChanged(nameof(IsTurkeyBreederSelected));
                OnPropertyChanged(nameof(IsTurkeyMeatSelected));
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
        "Breeder" => "Chicken Multiplier Breeder",
        "Hatchery" => "Multiplier Hatchery",
        "Pullet" => "Pullet Farm (Rearing)",
        "Egg" => "Chicken Egg Production (Layers)",
        "ChickenMeat" => "Chicken Meat Production (Broilers)",
        "TurkeyBreeder" => "Turkey Multiplier Breeder",
        "TurkeyMeat" => "Turkey Meat Production",
        _ => ""
    };

    public string SelectedStageDescription => SelectedStage switch
    {
        "Breeder" => "The chicken multiplier breeder operation maintains parent breeding stock that produces fertilized hatching eggs. These are not table eggs but specialized genetic stock. Breeder flocks typically consist of hens and roosters housed together. The fertilized eggs are collected and transferred to the multiplier hatchery. Breeder birds are selected for traits like growth rate, feed efficiency, and egg production depending on whether they supply the meat or egg production chains. This is the starting point of the chicken production pyramid.",

        "Hatchery" => "The multiplier hatchery receives fertilized eggs from both chicken and turkey breeder operations and incubates them under controlled temperature and humidity conditions. Chicken eggs hatch after approximately 21 days of incubation, while turkey eggs require about 28 days. The resulting day-old chicks and poults are then distributed to pullet rearing farms (for future layers) or directly to meat production operations (for broilers and turkeys). The hatchery is a critical biosecurity point in the production chain.",

        "Pullet" => "The pullet farm raises day-old chicks from the hatchery through to point-of-lay age, typically around 19 weeks for chickens. During this rearing period, birds progress through brooding (0-3 weeks with supplemental heat), growing (3-10 weeks), and developing (10-19 weeks) phases. Proper nutrition and lighting programs during rearing are essential for future egg production performance. Once pullets reach sexual maturity and begin laying, they are transferred to egg production facilities.",

        "Egg" => "Chicken egg production (layer) operations house hens that produce table eggs for human consumption. Hens typically begin laying around 19-20 weeks of age and continue for 12-14 months before being culled. Peak production occurs around 25-30 weeks of age, after which egg production gradually declines. Layer operations can be cage-free, free-range, or conventional housing systems. This component tracks the emissions associated with feed consumption, manure management, and energy use during the laying period.",

        "ChickenMeat" => "Chicken meat production (broiler) operations raise birds specifically for meat. Broiler chickens grow rapidly, typically reaching market weight of 2-3 kg in just 5-7 weeks. These operations have high feed conversion efficiency but also significant manure output in a short period. Birds are raised in climate-controlled barns and fed high-energy diets optimized for growth. The short production cycle means multiple flocks can be raised per year in the same facility.",

        "TurkeyBreeder" => "The turkey multiplier breeder operation maintains parent breeding stock that produces fertilized turkey hatching eggs. Turkey breeding is highly specialized, with artificial insemination commonly used due to the large size difference between toms and hens. Breeder hens typically produce eggs for 20-25 weeks. The fertilized eggs are transferred to the hatchery for incubation. Turkey breeding stock is selected for meat yield, feed efficiency, and overall health traits.",

        "TurkeyMeat" => "Turkey meat production operations raise turkeys from poults (day-old birds from the hatchery) to market weight. Turkey production cycles are longer than broiler chicken cycles, typically 10-14 weeks for hens and 16-20 weeks for toms. Turkeys require more space and different management compared to chickens. They are raised in climate-controlled barns with specialized feeding programs that change as the birds grow. Seasonal demand peaks around holidays significantly influence production scheduling.",

        _ => ""
    };

    public bool IsBreederSelected => SelectedStage == "Breeder";
    public bool IsHatcherySelected => SelectedStage == "Hatchery";
    public bool IsPulletSelected => SelectedStage == "Pullet";
    public bool IsEggSelected => SelectedStage == "Egg";
    public bool IsChickenMeatSelected => SelectedStage == "ChickenMeat";
    public bool IsTurkeyBreederSelected => SelectedStage == "TurkeyBreeder";
    public bool IsTurkeyMeatSelected => SelectedStage == "TurkeyMeat";

    private void Breeder_PointerPressed(object? sender, PointerPressedEventArgs e) => SelectedStage = "Breeder";
    private void Hatchery_PointerPressed(object? sender, PointerPressedEventArgs e) => SelectedStage = "Hatchery";
    private void Pullet_PointerPressed(object? sender, PointerPressedEventArgs e) => SelectedStage = "Pullet";
    private void Egg_PointerPressed(object? sender, PointerPressedEventArgs e) => SelectedStage = "Egg";
    private void ChickenMeat_PointerPressed(object? sender, PointerPressedEventArgs e) => SelectedStage = "ChickenMeat";
    private void TurkeyBreeder_PointerPressed(object? sender, PointerPressedEventArgs e) => SelectedStage = "TurkeyBreeder";
    private void TurkeyMeat_PointerPressed(object? sender, PointerPressedEventArgs e) => SelectedStage = "TurkeyMeat";

    private void CloseInfo_Click(object? sender, RoutedEventArgs e) => SelectedStage = null;

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
