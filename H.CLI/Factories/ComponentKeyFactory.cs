using System;
using H.CLI.ComponentKeys;
using H.CLI.Interfaces;
using H.Localization.Resources.Strings;

namespace H.CLI.Factories
{
    public class ComponentKeyFactory
    {
        #region Public Methods
        /// <summary>
        /// Based on the type of component, return the appropriate concrete ComponentKeys class. If you are adding a new component,
        /// please add the appropriate case below. The name of the case should correspond with the spelling in the DirectoryKeys.
        /// The cases below are strings that shuold always be based on the list of keys in the DirectoryKeys class.
        /// </summary>
        public IComponentKeys ComponentKeysCreator(string component)
        {
            if (component.ToUpper() == AppStrings.Filename_DefaultShelterbeltInput.ToUpper())
            {
                return new ShelterBeltKeys();
            }

            if (component.ToUpper() == AppStrings.Filename_DefaultFieldsInput.ToUpper())
            {
                return new FieldKeys();
            }

            if (component.ToUpper() == AppStrings.Filename_DefaultSwineInput.ToUpper())
            {
                return new SwineKeys();
            }

            if (component.ToUpper() == AppStrings.Filename_DefaultDairyInput.ToUpper())
            {
                return new DairyCattleKeys();
            }

            if (component.ToUpper() == AppStrings.Filename_DefaultSheepInput.ToUpper())
            {
                return new SheepKeys();
            }


            if (component.ToUpper() == AppStrings.Filename_DefaultBeefInput.ToUpper())
            {
                return new BeefCattleKeys();
            }

            if (component.ToUpper() == AppStrings.Filename_DefaultPoultryInput.ToUpper())
            {
                return new PoultryKeys();
            }
            if (component.ToUpper() == AppStrings.Filename_DefaultOtherLivestockInput.ToUpper())
            {
                return new OtherLivestockKeys();
            }

            else
            {
                throw new NotImplementedException();
            }
           
        } 
        #endregion
    }
}
