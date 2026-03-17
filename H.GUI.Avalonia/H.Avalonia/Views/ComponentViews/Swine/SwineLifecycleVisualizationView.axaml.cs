using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace H.Avalonia.Views.ComponentViews.Swine;

public partial class SwineLifecycleVisualizationView : UserControl, INotifyPropertyChanged
{
    private string? _selectedStage;
    private bool _hasSelection;

    public SwineLifecycleVisualizationView()
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

                OnPropertyChanged(nameof(IsBreedingSelected));
                OnPropertyChanged(nameof(IsFarrowingSelected));
                OnPropertyChanged(nameof(IsNurserySelected));
                OnPropertyChanged(nameof(IsGrowingSelected));
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
        "Breeding" => "Breeding Stock",
        "Farrowing" => "Farrowing (Lactating Sows)",
        "Nursery" => "Nursery (Starters / Iso-Wean)",
        "Growing" => "Growing Phase",
        "Finishing" => "Finishing Phase",
        "FarrowToFinish" => "Farrow-to-Finish Production System",
        "FarrowToWean" => "Farrow-to-Wean Production System",
        "IsoWean" => "Iso-Wean Production System",
        "GrowerToFinish" => "Grower-to-Finish Production System",
        _ => ""
    };

    public string SelectedStageDescription => SelectedStage switch
    {
        "Breeding" => "The breeding stage includes boars, sows, gilts, and dry (non-lactating) sows that form the foundation of swine production. Boars are mature males used for breeding, while gilts are young females that have not yet had their first litter. Dry sows are between weaning and the next farrowing. Breeding stock is selected for traits such as litter size, growth rate, feed efficiency, and carcass quality. The gestation period for sows is approximately 114 days (3 months, 3 weeks, and 3 days).",

        "Farrowing" => "During the farrowing stage, lactating sows give birth to and nurse their piglets in specialized farrowing crates or pens designed to protect the piglets from being crushed. A sow typically produces 10-14 piglets per litter. Lactation lasts approximately 3-4 weeks, during which the sow has very high nutritional requirements to support milk production. Piglets receive colostrum within the first hours of life for immune protection. This stage has significant feed intake and manure output from the lactating sows.",

        "Nursery" => "The nursery stage begins at weaning (typically 3-4 weeks of age) when piglets are separated from the sow. Weaned piglets (starters) weigh approximately 5-7 kg and are moved to environmentally controlled nursery barns. This is a critical transition period where piglets must adapt to solid feed and a new social environment. Starter diets are highly digestible and nutrient-dense. In iso-wean systems, piglets are moved to a separate, isolated facility to break disease cycles between sow herds and young pigs.",

        "Growing" => "The growing phase follows the nursery period, typically starting when pigs reach 25 kg at around 10 weeks of age. Grower pigs are housed in group pens and fed diets formulated for lean tissue growth. During this phase, pigs gain approximately 0.7-0.9 kg per day. Feed efficiency is a key economic driver, as feed costs represent 60-70% of total production costs. This stage continues until pigs reach approximately 60 kg, at which point they transition to the finishing phase.",

        "Finishing" => "The finishing phase is the final stage before market, where pigs grow from approximately 60 kg to market weight of 110-130 kg. Finisher pigs have the highest absolute feed intake of any stage and produce the most manure per animal. Diets are adjusted to optimize the balance between growth rate and carcass leanness. Pigs are typically marketed at 5-6 months of age. Replacement gilts with desirable traits may be selected from the finishing herd and returned to the breeding program.",

        "FarrowToFinish" => "A farrow-to-finish operation encompasses the entire swine production cycle on a single farm, from breeding through to market-weight hogs. This integrated approach gives producers control over all stages: breeding, farrowing, nursery, growing, and finishing. While requiring more diverse facilities and management expertise, farrow-to-finish operations can optimize the flow of pigs and maintain consistent health status throughout the production chain.",

        "FarrowToWean" => "A farrow-to-wean operation specializes in breeding, farrowing, and weaning piglets. These operations maintain the breeding herd (boars, sows, gilts) and farrow sows in specialized facilities, then sell weaned piglets (typically at 3-4 weeks of age, weighing 5-7 kg) to nursery or grower-to-finish operations. This specialization allows producers to focus expertise on reproductive management and early piglet care.",

        "IsoWean" => "An iso-wean (isolated weaning) operation receives early-weaned piglets (typically at 2-3 weeks of age) and raises them in an isolated facility physically separated from the sow herd. The goal is to break the disease transmission cycle from sows to piglets by removing piglets before maternal antibodies wane. Iso-wean facilities maintain strict biosecurity protocols and all-in/all-out management to minimize pathogen exposure during this vulnerable stage.",

        "GrowerToFinish" => "A grower-to-finish operation receives weaned or nursery-stage pigs and raises them through the growing and finishing phases to market weight. These operations purchase pigs from farrow-to-wean or iso-wean operations, typically at 20-25 kg. The focus is on feed efficiency, growth rate, and carcass quality. Grower-to-finish barns are designed for high-volume production with automated feeding systems and climate control.",

        _ => ""
    };

    public bool IsBreedingSelected => SelectedStage == "Breeding";
    public bool IsFarrowingSelected => SelectedStage == "Farrowing";
    public bool IsNurserySelected => SelectedStage == "Nursery";
    public bool IsGrowingSelected => SelectedStage == "Growing";
    public bool IsFinishingSelected => SelectedStage == "Finishing";

    private void Breeding_PointerPressed(object? sender, PointerPressedEventArgs e) => SelectedStage = "Breeding";
    private void Farrowing_PointerPressed(object? sender, PointerPressedEventArgs e) => SelectedStage = "Farrowing";
    private void Nursery_PointerPressed(object? sender, PointerPressedEventArgs e) => SelectedStage = "Nursery";
    private void Growing_PointerPressed(object? sender, PointerPressedEventArgs e) => SelectedStage = "Growing";
    private void Finishing_PointerPressed(object? sender, PointerPressedEventArgs e) => SelectedStage = "Finishing";
    private void FarrowToFinish_PointerPressed(object? sender, PointerPressedEventArgs e) => SelectedStage = "FarrowToFinish";
    private void FarrowToWean_PointerPressed(object? sender, PointerPressedEventArgs e) => SelectedStage = "FarrowToWean";
    private void IsoWean_PointerPressed(object? sender, PointerPressedEventArgs e) => SelectedStage = "IsoWean";
    private void GrowerToFinish_PointerPressed(object? sender, PointerPressedEventArgs e) => SelectedStage = "GrowerToFinish";

    private void CloseInfo_Click(object? sender, RoutedEventArgs e) => SelectedStage = null;

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
