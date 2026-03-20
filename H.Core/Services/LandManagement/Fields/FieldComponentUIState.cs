namespace H.Core.Services.LandManagement.Fields;

/// <summary>
/// Represents the UI state for a FieldComponent that needs to be preserved across ViewModel disposal cycles
/// </summary>
public class FieldComponentUIState : UIStateBase
{
    /// <summary>
    /// The GUID of the field component this state belongs to
    /// </summary>
    public Guid FieldComponentGuid { get; set; }

    /// <summary>
    /// The GUID of the currently selected crop (if any)
    /// </summary>
    public Guid? SelectedCropGuid { get; set; }
}