using Avalonia.Media;
using H.Core.Enumerations;
using H.Core.Factories.Crops;
using Prism.Mvvm;

namespace H.Avalonia.ViewModels.ComponentViews.LandManagement.Rotation
{
    /// <summary>
    /// Represents a single cell in the preview grid showing which crop grows in a specific field in a specific year.
    /// 
    /// This class implements INotifyPropertyChanged (via BindableBase) to support dynamic selection highlighting.
    /// When a user clicks a crop card in the timeline (Step 2), all cells with matching crop types in the
    /// preview grid (Step 3) are highlighted with a blue border.
    /// 
    /// Cell appearance is determined by:
    /// - Background color: Based on crop category (cereals=orange, oilseeds=green, pulses=blue, forages=purple, fallow=gray)
    /// - Border: Normal (1px gray) when not selected, highlighted (3px blue) when selected
    /// - Text: Crop name with emoji icon, truncated if too long with tooltip showing full name
    /// 
    /// Example cell data:
    /// - Year: "2020"
    /// - CropType: CropType.Wheat
    /// - CropDisplay: "Wheat ??"
    /// - CropBackground: "#FFF3E0" (light orange)
    /// - IsSelected: true (if user clicked the Wheat card in timeline)
    /// </summary>
    public class YearCropAssignment : BindableBase
    {
        /// <summary>
        /// Backing field for the IsSelected property
        /// </summary>
        private bool _isSelected;

        /// <summary>
        /// The calendar year for this assignment.
        /// Format: Four-digit year as string (e.g., "2020", "2025")
        /// Displayed in the top section of each cell in smaller, gray text.
        /// </summary>
        public string Year { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable crop name with emoji icon for display in the UI.
        /// Format: "{CropName} {Emoji}"
        /// Examples: "Wheat ??", "Canola ??", "Peas ??", "Alfalfa ??"
        /// 
        /// Provided by the ICropColorService.GetCropDisplayName() method.
        /// Falls back to CropType.ToString() if service is unavailable.
        /// 
        /// Displayed in the bottom section of each cell with text wrapping/trimming.
        /// </summary>
        public string CropDisplay { get; set; } = string.Empty;

        /// <summary>
        /// Background color brush for the cell, based on crop category.
        /// 
        /// Color scheme:
        /// - Cereals (Wheat, Barley, Oats, etc.): Orange (#FFF3E0)
        /// - Oilseeds (Canola, Flax, Sunflower, etc.): Green (#E8F5E9)
        /// - Pulses (Peas, Lentils, Chickpeas, etc.): Blue (#E3F2FD)
        /// - Forages (Alfalfa, Hay, Grasses, etc.): Purple (#F3E5F5)
        /// - Fallow: Gray (#FAFAFA)
        /// - Unknown/Default: Light Gray (#F5F5F5)
        /// 
        /// Provided by the ICropColorService.GetCropColorHex() method.
        /// This creates visual consistency across the timeline (Step 2) and preview (Step 3).
        /// </summary>
        public IBrush CropBackground { get; set; } = null!;
        
        /// <summary>
        /// The actual crop type enumeration value for this assignment.
        /// 
        /// This property is used for selection matching:
        /// - When user clicks a crop card in the timeline (Step 2)
        /// - All cells in the preview grid (Step 3) with matching CropType are highlighted
        /// - This allows users to visualize where a specific crop appears across all fields and years
        /// 
        /// Example: If user clicks "Wheat" card, all cells where CropType == CropType.Wheat
        /// will have IsSelected set to true, triggering the blue border highlight.
        /// </summary>
        public CropType CropType { get; set; }
        
        /// <summary>
        /// Reference to the actual crop DTO from the rotation sequence.
        /// This allows the cell to provide the crop data for editing when clicked.
        /// </summary>
        public ICropDto CropDto { get; set; } = null!;
        
        /// <summary>
        /// Indicates whether this cell represents the currently selected crop type from the timeline.
        /// 
        /// Selection behavior:
        /// - When true: Cell displays with blue border (3px thickness) via BoolToSelectionBorderBrushConverter
        /// - When false: Cell displays with normal gray border (1px thickness)
        /// 
        /// This property is updated by UpdatePreviewCellSelection() when:
        /// - User clicks a crop card in the timeline
        /// - All cells with matching CropType have IsSelected set to true
        /// - All cells with non-matching CropType have IsSelected set to false
        /// 
        /// Property change notifications ensure the UI updates immediately when selection changes.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
