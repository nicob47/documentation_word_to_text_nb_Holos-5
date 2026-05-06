using System.Threading.Tasks;
using H.Core.Enumerations;

namespace H.Avalonia.Services.DietFormulator;

/// <summary>
/// Opens the Diet Formulator modal window. Encapsulates the UI concern of getting
/// a parent <c>TopLevel</c> so view models stay UI-agnostic.
/// </summary>
public interface IDietFormulatorWindowService
{
    /// <summary>
    /// Opens the formulator window as a modal scoped to the given animal type.
    /// Awaitable so callers can react after the user closes the dialog.
    /// </summary>
    Task ShowAsync(AnimalType animalType);
}
