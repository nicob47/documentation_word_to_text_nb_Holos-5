using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace H.Avalonia.Views.ComponentViews.Beef;

public partial class BeefLifecycleVisualizationView : UserControl, INotifyPropertyChanged
{
    private string? _selectedStage;
    private bool _hasSelection;

    public BeefLifecycleVisualizationView()
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

                OnPropertyChanged(nameof(IsCowCalfSelected));
                OnPropertyChanged(nameof(IsBackgroundingSelected));
                OnPropertyChanged(nameof(IsFinishingSelected));
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
        "CowCalf" => "Cow-Calf Stage",
        "Backgrounding" => "Backgrounding Stage",
        "Finishing" => "Finishing (Feedlot) Stage",
        "CowCalfSystem" => "Cow-Calf Production System",
        "BackgroundingSystem" => "Backgrounding Production System",
        "FinishingSystem" => "Finishing (Feedlot) Production System",
        "FullCycle" => "Full Beef Production Cycle",
        _ => ""
    };

    public string SelectedStageDescription => SelectedStage switch
    {
        "CowCalf" => "The cow-calf stage is the foundation of beef production. A breeding herd of cows and bulls produces calves each year, typically calving in spring. The herd includes lactating cows nursing calves, dry cows between weaning and calving, bulls for breeding, and replacement heifers being developed for the breeding herd. Calves are raised on pasture alongside their mothers until weaning at 6-8 months of age (200-300 kg). Cow-calf operations are primarily pasture-based and represent the largest source of beef cattle greenhouse gas emissions due to enteric methane from forage digestion.",

        "Backgrounding" => "Backgrounding (also called stocker or growing) is a transitional phase where weaned calves are grown on forage-based diets before entering the feedlot. Backgrounded cattle include steers, heifers, and stocker animals that gain weight at a moderate rate (0.5-1.0 kg/day) on pasture, silage, or hay-based rations. This phase typically lasts 3-8 months and adds 100-200 kg of body weight. Backgrounding allows calves to grow frame size before being placed on high-energy finishing diets, improving overall feed efficiency in the feedlot.",

        "Finishing" => "The finishing (feedlot) stage is where cattle are fed high-energy grain-based diets to reach market weight of 550-650 kg. Finishing steers and heifers are housed in feedlot pens and typically gain 1.2-1.8 kg per day over a 120-200 day feeding period. Feed conversion ratios are approximately 6:1 (kg feed per kg gain). This stage has the highest daily feed intake and the most concentrated manure output. Diet composition, feed additives, and management practices significantly influence both enteric methane and manure emissions during finishing.",

        "CowCalfSystem" => "A cow-calf operation maintains a breeding herd to produce weaned calves for sale to backgrounding or finishing operations. The operation manages the full breeding cycle: bull selection and breeding (natural service or AI), gestation, calving, lactation, and weaning. Calves are typically sold at weaning (6-8 months) or after a short preconditioning period. Cow-calf operations are predominantly pasture-based, with supplemental feeding during winter months. Herd size, calving rate, weaning weight, and forage quality are key production and emission factors.",

        "BackgroundingSystem" => "A backgrounding operation purchases weaned calves and grows them on forage-based diets before they enter the feedlot. These operations add frame size and weight to young cattle at lower cost than feedlot finishing. Backgrounding can occur on pasture (stocker operations), in drylots with harvested feeds, or a combination. The goal is to develop skeletal and muscular growth while cattle are physiologically efficient at converting forage. Animals are sold to finishing operations when they reach an appropriate weight and frame size.",

        "FinishingSystem" => "A finishing (feedlot) operation purchases backgrounded or weaned cattle and feeds them high-energy diets to reach slaughter weight. Feedlots range from small farmer-feeders to large commercial operations with thousands of head. Cattle are penned in groups and fed total mixed rations based on barley or corn grain with silage and supplements. Implants, ionophores, and other feed additives are commonly used to improve feed efficiency and growth rate. Manure management (collection, storage, and land application) is a major component of the environmental footprint.",

        "FullCycle" => "The full beef production cycle encompasses all three stages: cow-calf, backgrounding, and finishing. Some operations integrate all stages on a single farm, while most cattle move between specialized operations. A typical animal's lifecycle spans 18-24 months from birth to slaughter. The cow-calf stage accounts for the largest share of lifetime greenhouse gas emissions (primarily enteric methane from cows on forage), while the finishing stage has the highest daily emissions intensity. Understanding the full cycle is important for accurately estimating total emissions and identifying reduction opportunities at each stage.",

        _ => ""
    };

    public bool IsCowCalfSelected => SelectedStage == "CowCalf";
    public bool IsBackgroundingSelected => SelectedStage == "Backgrounding";
    public bool IsFinishingSelected => SelectedStage == "Finishing";

    private void Card_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Space && sender is Border { Tag: string stage })
            SelectedStage = stage;
    }

    private void CowCalf_PointerPressed(object? sender, PointerPressedEventArgs e) => SelectedStage = "CowCalf";
    private void Backgrounding_PointerPressed(object? sender, PointerPressedEventArgs e) => SelectedStage = "Backgrounding";
    private void Finishing_PointerPressed(object? sender, PointerPressedEventArgs e) => SelectedStage = "Finishing";
    private void CowCalfSystem_PointerPressed(object? sender, PointerPressedEventArgs e) => SelectedStage = "CowCalfSystem";
    private void BackgroundingSystem_PointerPressed(object? sender, PointerPressedEventArgs e) => SelectedStage = "BackgroundingSystem";
    private void FinishingSystem_PointerPressed(object? sender, PointerPressedEventArgs e) => SelectedStage = "FinishingSystem";
    private void FullCycle_PointerPressed(object? sender, PointerPressedEventArgs e) => SelectedStage = "FullCycle";

    private void CloseInfo_Click(object? sender, RoutedEventArgs e) => SelectedStage = null;

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
