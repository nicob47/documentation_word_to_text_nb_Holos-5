using H.Core.Enumerations;
using H.Core.Mappers;
using H.Core.Models;
using H.Core.Models.LandManagement.Fields;
using H.Core.Providers.Economics;
using H.Core.Providers.Fertilizer;
using H.Core.Providers.Soil;

namespace H.Core.Services
{
    public class FieldComponentHelper : IFieldComponentHelper
    {
        #region Fields

        #endregion

        #region Constructors

        public FieldComponentHelper()
        {
        }

        #endregion

        #region Public Methods

        public void InitializeComponent(FieldSystemComponent component, Farm farm)
        {
            component.StartYear = farm.CarbonModellingEquilibriumYear;
            component.EndYear = DateTime.Now.Year;
            component.YearOfObservation = DateTime.Now.Year;

            var fieldName = this.GetUniqueFieldName(farm.FieldSystemComponents);
            component.Name = fieldName;
            component.ComponentDescriptionString = fieldName;
            component.GroupPath = fieldName;

            component.IsInitialized = true;

            // Assign soil types to field
            this.InitializeSoilAvailableSoilTypes(farm, component);
        }

        public void InitializeSoilAvailableSoilTypes(Farm farm, FieldSystemComponent component)
        {
            if (farm.GeographicData?.SoilDataForAllComponentsWithinPolygon == null) return;
            if (component.SoilDataAvailableForField == null) return;
            foreach (var soilData in farm.GeographicData.SoilDataForAllComponentsWithinPolygon)
            {
                // Add this type of soil if it does not already exist
                if (!component.SoilDataAvailableForField.Any(x => x.SoilGreatGroup == soilData.SoilGreatGroup))
                {
                    // We don't model organic soil at this time
                    if (soilData.SoilFunctionalCategory != SoilFunctionalCategory.Organic)
                    {
                        var copiedSoil = new SoilData();
                        PropertyMapper.CopyTo(soilData, copiedSoil);

                        component.SoilDataAvailableForField.Add(copiedSoil);
                    }
                }
            }
        }

        public string GetUniqueFieldName(IEnumerable<FieldSystemComponent> components)
        {
            var i = 1;
            var fieldSystemComponents = components;

            var totalCount = fieldSystemComponents.Count();
            var proposedName = string.Format(Properties.Resources.InterpolatedFieldNumber, i);

            //while proposedName isn't unique create a uniqe name for this component so we don't have duplicate named components
            while (fieldSystemComponents.Any(x => x.Name == proposedName))
            {
                proposedName = string.Format(Properties.Resources.InterpolatedFieldNumber, ++i);
            }

            return proposedName;
        }

        public FieldSystemComponent Replicate(FieldSystemComponent component)
        {
            var fieldSystemComponent = (FieldSystemComponent)Activator.CreateInstance(typeof(FieldSystemComponent))!;

            this.Replicate(component, fieldSystemComponent);

            fieldSystemComponent.Name = component.Name;

            /*
             * Must be true else Holos will reinitialize the component 
             * causing field components in the components view to behave oddly
             * when clicked upon.
             */
            fieldSystemComponent.IsInitialized = true;

            return fieldSystemComponent;
        }

        public void Replicate(ComponentBase copyFrom, ComponentBase copyTo)
        {
            var to = copyTo as FieldSystemComponent;
            var from = copyFrom as FieldSystemComponent;
            if (to == null || from == null) return;

            to.FieldArea = from.FieldArea;
            to.StartYear = from.StartYear;
            to.EndYear = from.EndYear;

            foreach (var cropViewItem in from.CropViewItems)
            {
                var copiedViewItem = new CropViewItem();

                PropertyMapper.CopyTo(cropViewItem, copiedViewItem);

                // CopyTo above copies the CropEconomicData reference — create a fresh instance so the copy is independent.
                copiedViewItem.CropEconomicData = new CropEconomicData();
                PropertyMapper.CopyTo(cropViewItem.CropEconomicData, copiedViewItem.CropEconomicData);

                to.CropViewItems.Add(copiedViewItem);

                foreach (var manureApplicationViewItem in cropViewItem.ManureApplicationViewItems)
                {
                    var copiedManureApplicationViewItem = new ManureApplicationViewItem();

                    PropertyMapper.CopyTo(manureApplicationViewItem, copiedManureApplicationViewItem);
                    copiedViewItem.ManureApplicationViewItems.Add(copiedManureApplicationViewItem);
                }

                foreach (var harvestViewItem in cropViewItem.HarvestViewItems)
                {
                    var copiedHarvestViewItem = new HarvestViewItem();

                    PropertyMapper.CopyTo(harvestViewItem, copiedHarvestViewItem);
                    copiedViewItem.HarvestViewItems.Add(copiedHarvestViewItem);
                }

                foreach (var hayImportViewItem in cropViewItem.HayImportViewItems)
                {
                    var copiedHayImportItem = new HayImportViewItem();
                }

                foreach (var grazingViewItem in cropViewItem.GrazingViewItems)
                {
                    var copiedGrazingViewItem = new GrazingViewItem();

                    PropertyMapper.CopyTo(grazingViewItem, copiedGrazingViewItem);
                    copiedViewItem.GrazingViewItems.Add(copiedGrazingViewItem);
                }

                foreach (var fertilizerApplicationViewItem in cropViewItem.FertilizerApplicationViewItems)
                {
                    var copiedFertilizerApplicationViewItem = new FertilizerApplicationViewItem();

                    PropertyMapper.CopyTo(fertilizerApplicationViewItem, copiedFertilizerApplicationViewItem);
                    copiedViewItem.FertilizerApplicationViewItems.Add(copiedFertilizerApplicationViewItem);
                }
            }
        }

        #endregion
    }
}