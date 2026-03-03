using System.Collections.ObjectModel;

namespace H.Avalonia.ViewModels.ComponentViews.LandManagement.Rotation
{
    /// <summary>
    /// Represents a single row in the field assignment preview grid (Step 3).
    /// 
    /// Each row corresponds to one physical field on the farm and contains:
    /// - Field identifier (e.g., "Field 1")
    /// - Collection of year/crop assignments showing what crop grows in each year
    /// 
    /// The grid is structured as:
    /// - Rows = Fields (vertical axis)
    /// - Columns = Years (horizontal axis)
    /// - Cell values = Crop types with color coding
    /// 
    /// Example with 3 fields over 4 years:
    /// | Field Name | 2020  | 2021   | 2022 | 2023  |
    /// |------------|-------|--------|------|-------|
    /// | Field 1    | Wheat | Barley | Oats | Corn  |
    /// | Field 2    | Barley| Oats   | Corn | Wheat |
    /// | Field 3    | Oats  | Corn   | Wheat| Barley|
    /// </summary>
    public class FieldAssignmentRow
    {
        /// <summary>
        /// Display name for this field including the field number.
        /// Format: "Field {number}"
        /// Example: "Field 1"
        /// </summary>
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// Collection of year/crop assignments for this field, ordered chronologically.
        /// Each assignment represents one year in the rotation period and specifies which crop grows that year.
        /// The length of this collection equals (EndYear - StartYear + 1) from the rotation parameters.
        /// </summary>
        public ObservableCollection<YearCropAssignment> YearAssignments { get; set; } = new();
    }
}
