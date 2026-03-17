using System;
using H.CLI.Interfaces;
using H.CLI.TemporaryComponentStorage;
using H.Localization.Resources.Strings;

namespace H.CLI.Factories
{
    public class ComponentTemporaryInputFactory
    {
        #region Public Methods
        /// Based on the type of component, return the appropriate concrete TemporaryInput class. If you are adding a new component,
        /// please add the appropriate case below. The cases below are strings that should always 
        /// be based on the list of keys in the DirectoryKeys class
        /// </summary>
        public IComponentTemporaryInput CreateComponentTemporaryInputs(string component)
        {
            
            if (component.ToUpper() == AppStrings.Filename_DefaultShelterbeltInput.ToUpper())
            {
                return new ShelterBeltTemporaryInput();
            }

            if (component.ToUpper() == AppStrings.Filename_DefaultFieldsInput.ToUpper())
            {
                return new FieldTemporaryInput();
            }

            if (component.ToUpper() == AppStrings.Filename_DefaultSwineInput.ToUpper())
            {
                return new SwineTemporaryInput();
            }

            if (component.ToUpper() == AppStrings.Filename_DefaultDairyInput.ToUpper())
            {
                return new DairyTemporaryInput();
            }

            if (component.ToUpper() == AppStrings.Filename_DefaultSheepInput.ToUpper())
            {
                return new SheepTemporaryInput();
            }


            if (component.ToUpper() == AppStrings.Filename_DefaultBeefInput.ToUpper())
            {
                return new BeefCattleTemporaryInput();
            }

            if (component.ToUpper() == AppStrings.Filename_DefaultPoultryInput.ToUpper())
            {
                return new PoultryTemporaryInput();
            }
            if (component.ToUpper() == AppStrings.Filename_DefaultOtherLivestockInput.ToUpper())
            {
                return new OtherLiveStockTemporaryInput();
            }

            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
        #endregion

