using System;
using System.Collections.Generic;
using System.Linq;
using H.Avalonia.ViewModels.Wizard;
using H.Core.Enumerations;
using H.Core.Models.LandManagement.Shelterbelt;

namespace H.Avalonia.ViewModels.ComponentViews.LandManagement.Shelterbelt
{
    /// <summary>
    /// Step 2 — The user selects a hardiness zone and optionally sets an eco-district ID.
    /// </summary>
    public class ShelterbeltWizardStep2LocationViewModel : WizardStepViewModelBase
    {
        #region Fields

        private readonly ShelterbeltComponent _component;
        private HardinessZone _selectedHardinessZone;
        private int _ecoDistrictId;

        #endregion

        #region Constructors

        public ShelterbeltWizardStep2LocationViewModel(ShelterbeltComponent component)
        {
            _component = component;
            _selectedHardinessZone = component.HardinessZone == HardinessZone.NotAvailable
                ? HardinessZone.H3b  // sensible prairie default
                : component.HardinessZone;
            _ecoDistrictId = component.EcoDistrictId;

            // Push the default back to the model so it's not left as NotAvailable
            _component.HardinessZone = _selectedHardinessZone;

            CanGoNext = true;
        }

        #endregion

        #region IWizardStepViewModel

        public override string StepLabel => "Step 2 of 6";
        public override string StepTitle => "Location";

        #endregion

        #region Properties

        /// <summary>All usable hardiness zone values (excludes NotAvailable).</summary>
        public IReadOnlyList<HardinessZone> HardinessZones { get; } =
            Enum.GetValues<HardinessZone>()
                .Where(z => z != HardinessZone.NotAvailable)
                .ToList();

        public HardinessZone SelectedHardinessZone
        {
            get => _selectedHardinessZone;
            set
            {
                if (SetProperty(ref _selectedHardinessZone, value))
                    _component.HardinessZone = value;
            }
        }

        public int EcoDistrictId
        {
            get => _ecoDistrictId;
            set
            {
                if (SetProperty(ref _ecoDistrictId, value))
                    _component.EcoDistrictId = value;
            }
        }

        #endregion
    }
}
