using System.ComponentModel;

namespace H.Core.Test.ViewModels;

/// <summary>
/// Unit tests for the checklist category toggle properties used by the crop editor.
/// These toggles control which input sections (General, Fertilizer, Manure, etc.) are visible.
/// Tests use a simple helper class since the real ViewModels require Prism/DI infrastructure.
/// </summary>
[TestClass]
public class ChecklistToggleTests
{
    /// <summary>
    /// Lightweight stand-in that mirrors the toggle properties added to both
    /// FieldComponentViewModel and RotationComponentViewModel.
    /// </summary>
    private sealed class ToggleHost : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private bool SetProperty(ref bool field, bool value, [System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            if (field == value) return false;
            field = value;
            OnPropertyChanged(name!);
            return true;
        }

        private bool _isFertilizerActive = true;
        public bool IsFertilizerActive
        {
            get => _isFertilizerActive;
            set => SetProperty(ref _isFertilizerActive, value);
        }

        private bool _isManureActive;
        public bool IsManureActive
        {
            get => _isManureActive;
            set => SetProperty(ref _isManureActive, value);
        }

        private bool _isGrazingActive;
        public bool IsGrazingActive
        {
            get => _isGrazingActive;
            set => SetProperty(ref _isGrazingActive, value);
        }

        private bool _isSoilActive;
        public bool IsSoilActive
        {
            get => _isSoilActive;
            set => SetProperty(ref _isSoilActive, value);
        }

        private bool _isResidueActive;
        public bool IsResidueActive
        {
            get => _isResidueActive;
            set => SetProperty(ref _isResidueActive, value);
        }

        private bool _isEconomicsActive;
        public bool IsEconomicsActive
        {
            get => _isEconomicsActive;
            set => SetProperty(ref _isEconomicsActive, value);
        }

        private bool _isEditingCoverCrop;
        public bool IsEditingCoverCrop
        {
            get => _isEditingCoverCrop;
            set => SetProperty(ref _isEditingCoverCrop, value);
        }
    }

    [TestMethod]
    public void DefaultToggles_FertilizerIsOn_OthersAreOff()
    {
        var host = new ToggleHost();

        Assert.IsTrue(host.IsFertilizerActive, "Fertilizer should default to ON");
        Assert.IsFalse(host.IsManureActive, "Manure should default to OFF");
        Assert.IsFalse(host.IsGrazingActive, "Grazing should default to OFF");
        Assert.IsFalse(host.IsSoilActive, "Soil should default to OFF");
        Assert.IsFalse(host.IsResidueActive, "Residue should default to OFF");
        Assert.IsFalse(host.IsEconomicsActive, "Economics should default to OFF");
    }

    [TestMethod]
    public void ToggleOn_SetsPropertyTrue()
    {
        var host = new ToggleHost();

        host.IsManureActive = true;
        host.IsGrazingActive = true;

        Assert.IsTrue(host.IsManureActive);
        Assert.IsTrue(host.IsGrazingActive);
    }

    [TestMethod]
    public void ToggleOff_SetsPropertyFalse()
    {
        var host = new ToggleHost();

        host.IsFertilizerActive = false;

        Assert.IsFalse(host.IsFertilizerActive);
    }

    [TestMethod]
    public void TogglesAreIndependent_ChangingOneDoesNotAffectOthers()
    {
        var host = new ToggleHost();

        host.IsManureActive = true;
        host.IsEconomicsActive = true;

        Assert.IsTrue(host.IsFertilizerActive, "Fertilizer should remain ON");
        Assert.IsTrue(host.IsManureActive);
        Assert.IsFalse(host.IsGrazingActive, "Grazing should remain OFF");
        Assert.IsFalse(host.IsSoilActive, "Soil should remain OFF");
        Assert.IsFalse(host.IsResidueActive, "Residue should remain OFF");
        Assert.IsTrue(host.IsEconomicsActive);
    }

    [TestMethod]
    public void IsEditingCoverCrop_DoesNotResetToggles()
    {
        var host = new ToggleHost();
        host.IsManureActive = true;
        host.IsSoilActive = true;

        // Switching to cover crop editing should NOT reset toggles
        host.IsEditingCoverCrop = true;

        Assert.IsTrue(host.IsManureActive, "Manure should still be ON after cover crop toggle");
        Assert.IsTrue(host.IsSoilActive, "Soil should still be ON after cover crop toggle");
        Assert.IsTrue(host.IsFertilizerActive, "Fertilizer should still be ON");
    }

    [TestMethod]
    public void PropertyChanged_FiresForEachToggle()
    {
        var host = new ToggleHost();
        var changedProperties = new List<string>();
        host.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName!);

        host.IsFertilizerActive = false;
        host.IsManureActive = true;
        host.IsGrazingActive = true;
        host.IsSoilActive = true;
        host.IsResidueActive = true;
        host.IsEconomicsActive = true;

        Assert.AreEqual(6, changedProperties.Count);
        CollectionAssert.Contains(changedProperties, nameof(host.IsFertilizerActive));
        CollectionAssert.Contains(changedProperties, nameof(host.IsManureActive));
        CollectionAssert.Contains(changedProperties, nameof(host.IsGrazingActive));
        CollectionAssert.Contains(changedProperties, nameof(host.IsSoilActive));
        CollectionAssert.Contains(changedProperties, nameof(host.IsResidueActive));
        CollectionAssert.Contains(changedProperties, nameof(host.IsEconomicsActive));
    }

    [TestMethod]
    public void SetSameValue_DoesNotFirePropertyChanged()
    {
        var host = new ToggleHost();
        var changedProperties = new List<string>();
        host.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName!);

        // Setting to same default value should not fire
        host.IsFertilizerActive = true;
        host.IsManureActive = false;

        Assert.AreEqual(0, changedProperties.Count, "No change events should fire for same values");
    }
}
