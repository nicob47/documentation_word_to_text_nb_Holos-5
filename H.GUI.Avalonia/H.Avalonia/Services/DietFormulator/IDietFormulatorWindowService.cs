using System.Threading.Tasks;
using H.Core.Enumerations;
using H.Core.Providers.Feed;

namespace H.Avalonia.Services.DietFormulator;

/// <summary>
/// Opens the Diet Formulator modal window. Encapsulates the UI concern of getting
/// a parent <c>TopLevel</c> so view models stay UI-agnostic.
/// </summary>
public interface IDietFormulatorWindowService
{
    /// <summary>
    /// Opens the formulator window as a modal scoped to the given animal type.
    /// Awaits user action; returns the saved <see cref="DietDto"/> on Save, or
    /// <c>null</c> when the user cancels or closes the window without saving.
    /// </summary>
    Task<DietDto?> ShowAsync(AnimalType animalType);
}
