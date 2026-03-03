using System.Collections.ObjectModel;
using H.Core.Enumerations;
using H.Core.Providers.Feed;

namespace H.Avalonia.ViewModels.SupportingViews
{
    /// <summary>
    /// Design-time view model for the Feed Ingredients view
    /// </summary>
    public class FeedIngredientsViewModelDesign : FeedIngredientsViewModel
    {
        public FeedIngredientsViewModelDesign()
        {
            // Create sample data for design-time
            Ingredients = new ObservableCollection<IFeedIngredient>
            {
                new DesignTimeFeedIngredient
                {
                    IngredientType = IngredientType.AlfalfaHay,
                    IngredientTypeString = "Alfalfa Hay",
                    DryMatter = 89.0,
                    CrudeProtein = 18.5,
                    TotalDigestibleNutrient = 58.0,
                    Fat = 2.5,
                    CrudeFiber = 28.0,
                    Forage = 100.0,
                    IFN = "1-00-063",
                    Cost = 0.15
                },
                new DesignTimeFeedIngredient
                {
                    IngredientType = IngredientType.CornGrain,
                    IngredientTypeString = "Corn Grain",
                    DryMatter = 88.0,
                    CrudeProtein = 8.8,
                    TotalDigestibleNutrient = 88.0,
                    Fat = 3.9,
                    CrudeFiber = 2.3,
                    Forage = 0.0,
                    IFN = "4-02-935",
                    Cost = 0.20
                },
                new DesignTimeFeedIngredient
                {
                    IngredientType = IngredientType.SoybeanMealExpelled,
                    IngredientTypeString = "Soybean Meal",
                    DryMatter = 89.0,
                    CrudeProtein = 48.5,
                    TotalDigestibleNutrient = 82.0,
                    Fat = 1.9,
                    CrudeFiber = 7.0,
                    Forage = 0.0,
                    IFN = "5-04-604",
                    Cost = 0.45
                },
                new DesignTimeFeedIngredient
                {
                    IngredientType = IngredientType.BarleySilage,
                    IngredientTypeString = "Barley Silage",
                    DryMatter = 35.0,
                    CrudeProtein = 9.2,
                    TotalDigestibleNutrient = 60.0,
                    Fat = 2.8,
                    CrudeFiber = 25.0,
                    Forage = 100.0,
                    IFN = "4-00-549",
                    Cost = 0.08
                },
                new DesignTimeFeedIngredient
                {
                    IngredientType = IngredientType.WheatBran,
                    IngredientTypeString = "Wheat Bran",
                    DryMatter = 89.0,
                    CrudeProtein = 15.5,
                    TotalDigestibleNutrient = 69.0,
                    Fat = 4.0,
                    CrudeFiber = 10.0,
                    Forage = 0.0,
                    IFN = "4-05-190",
                    Cost = 0.18
                }
            };
            
            SelectedAnimalType = AnimalType.Beef;
        }
    }

    /// <summary>
    /// Design-time implementation of IFeedIngredient for sample data
    /// </summary>
    public class DesignTimeFeedIngredient : IFeedIngredient
    {
        public string? IngredientTypeString { get; set; } = string.Empty;
        public IngredientType IngredientType { get; set; }
        public double Sugars { get; set; }
        public double SolubleCrudeProtein { get; set; }
        public double DryMatter { get; set; }
        public double CrudeProtein { get; set; }
        public double CrudeFiber { get; set; }
        public double AcidEtherExtract { get; set; }
        public double GrossEnergy { get; set; }
        public double Fat { get; set; }
        public double TotalDigestibleNutrient { get; set; }
        public double Starch { get; set; }
        public string? IFN { get; set; } = string.Empty;
        public double Cost { get; set; }
        public double Forage { get; set; }
        public double RDP { get; set; }
        public double ADF { get; set; }
        public double ADL { get; set; }
        public double Hemicellulose { get; set; }
        public double TDF { get; set; }
        public double IDF { get; set; }
        public double SDF { get; set; }
        public double Sucrose { get; set; }
        public double Lactose { get; set; }
        public double Glucose { get; set; }
        public double Oligosaccharides { get; set; }
        public double Raffinose { get; set; }
        public double Stachyose { get; set; }
        public double Verbascose { get; set; }
        public double Mo { get; set; }
        public double DE { get; set; }
        public double DeSwine { get; set; }
        public double SP { get; set; }
        public double ADICP { get; set; }
        public double OA { get; set; }
        public double Ash { get; set; }
        public double NDF { get; set; }
        public double Lignin { get; set; }
        public double CR { get; set; }
        public double Ca { get; set; }
        public double P { get; set; }
        public double K { get; set; }
        public double S { get; set; }
        public double Mg { get; set; }
        public double Na { get; set; }
        public double Cl { get; set; }
        public double Co { get; set; }
        public double Cu { get; set; }
        public double I { get; set; }
        public double Fe { get; set; }
        public double Mn { get; set; }
        public double Se { get; set; }
        public double Zn { get; set; }
        public double VitA { get; set; }
        public double VitD { get; set; }
        public double VitE { get; set; }
        public double VitD3 { get; set; }
        public double vitB1 { get; set; }
        public double VitB2 { get; set; }
        public double VitB3 { get; set; }
        public double VitB5 { get; set; }
        public double VitB6 { get; set; }
        public double VitB7 { get; set; }
        public double VitB12 { get; set; }
        public double KdPb { get; set; }
        public double Pef { get; set; }
        public double ME { get; set; }
        public double NEma { get; set; }
        public double NEga { get; set; }
        public double RUP { get; set; }
        public double kdPB { get; set; }
        public double KdCB1 { get; set; }
        public double KdCB2 { get; set; }
        public double KdCB3 { get; set; }
        public double PBID { get; set; }
        public double CB1ID { get; set; }
        public double CB2ID { get; set; }
        public double ARG { get; set; }
        public double HIS { get; set; }
        public double ILE { get; set; }
        public double LEU { get; set; }
        public double LYS { get; set; }
        public double MET { get; set; }
        public double CYS { get; set; }
        public double PHE { get; set; }
        public double TYR { get; set; }
        public double THR { get; set; }
        public double TRP { get; set; }
        public double VAL { get; set; }
        public double ALA { get; set; }
        public double ASP { get; set; }
        public double GLU { get; set; }
        public double GLY { get; set; }
        public double PRO { get; set; }
        public double SER { get; set; }
        public string? FeedNumber { get; set; } = string.Empty;
        public DairyFeedClassType DairyFeedClass { get; set; }
        public double PAF { get; set; }
        public double NEL_ThreeX { get; set; }
        public double NEL_FourX { get; set; }
        public double NEM { get; set; }
        public double NEG { get; set; }
        public double NDICP { get; set; }
        public double Paf { get; set; }
        public double Nel_threex { get; set; }
        public double Nel_fourx { get; set; }
        public double Nem { get; set; }
        public double Neg { get; set; }
        public double Ndicp { get; set; }
        public double EE { get; set; }
        public double Phytate { get; set; }
        public double PhytatePhosphorus { get; set; }
        public double NonPhytatePhosphorus { get; set; }
        public double NonphytatePhosphorus { get; set; }
        public double BetaCarotene { get; set; }
        public double Choline { get; set; }
        public double NE { get; set; }
        public double Ne { get; set; }
        public double CpDigestAID { get; set; }
        public double ArgDigestAID { get; set; }
        public double HisDigestAID { get; set; }
        public double IleDigestAID { get; set; }
        public double LeuDigestAID { get; set; }
        public double LysDigestAID { get; set; }
        public double MetDigestAID { get; set; }
        public double PheDigestAID { get; set; }
        public double ThrDigestAID { get; set; }
        public double TrpDigestAID { get; set; }
        public double ValDigestAID { get; set; }
        public double AlaDigestAID { get; set; }
        public double AspDigestAID { get; set; }
        public double CysDigestAID { get; set; }
        public double GluDigestAID { get; set; }
        public double GlyDigestAID { get; set; }
        public double ProDigestAID { get; set; }
        public double SerDigestAID { get; set; }
        public double TyrDigestAID { get; set; }
        public double CpDigestSID { get; set; }
        public double ArgDigestSID { get; set; }
        public double HisDigestSID { get; set; }
        public double IleDigestSID { get; set; }
        public double LeuDigestSID { get; set; }
        public double LysDigestSID { get; set; }
        public double MetDigestSID { get; set; }
        public double PheDigestSID { get; set; }
        public double ThrDigestSID { get; set; }
        public double TrpDigestSID { get; set; }
        public double ValDigestSID { get; set; }
        public double AlaDigestSID { get; set; }
        public double AspDigestSID { get; set; }
        public double CysDigestSID { get; set; }
        public double GluDigestSID { get; set; }
        public double GlyDigestSID { get; set; }
        public double ProDigestSID { get; set; }
        public double SerDigestSID { get; set; }
        public double TyrDigestSID { get; set; }
        public string? AAFCO { get; set; } = string.Empty;
        public string? AAFCO2010 { get; set; } = string.Empty;
        public double ATTDPhosphorus { get; set; }
        public double STTDPhosphorus { get; set; }
        public double Biotin { get; set; }
        public double Folacin { get; set; }
        public double Niacin { get; set; }
        public double PantothenicAcid { get; set; }
        public double Riboflavin { get; set; }
        public double Thiamin { get; set; }
        public double EtherExtractDup { get; set; }
        public double C120 { get; set; }
        public double C140 { get; set; }
        public double C160 { get; set; }
        public double C161 { get; set; }
        public double C180 { get; set; }
        public double C181 { get; set; }
        public double C182 { get; set; }
        public double C183 { get; set; }
        public double C184 { get; set; }
        public double C200 { get; set; }
        public double C201 { get; set; }
        public double C204 { get; set; }
        public double C205 { get; set; }
        public double C220 { get; set; }
        public double C221 { get; set; }
        public double C225 { get; set; }
        public double C226 { get; set; }
        public double C240 { get; set; }
        public double SFA { get; set; }
        public double MUFA { get; set; }
        public double PUFA { get; set; }
        public double IV { get; set; }
        public double IVP { get; set; }
        public double Nemf { get; }
        public bool IsReadonly { get; set; }
        public double PercentageInDiet { get; set; }
    }
}