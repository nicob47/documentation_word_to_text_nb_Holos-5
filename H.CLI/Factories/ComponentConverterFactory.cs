using H.CLI.Converters;
using H.CLI.Interfaces;
using System;
using H.Localization.Resources.Strings;

namespace H.CLI.Factories
{
    public class ComponentConverterFactory
    {
        #region Public Methods
        /// <summary>
        /// Based on the type of component, return the appropriate concrete converter. 
        /// If you are adding a new component, please add the appropriate concrete converter as a case below. The cases are
        /// strings that should always be written based on the list of keys in the DirectoryKeys class.
        /// </summary>
        public IConverter GetComponentConverter(string component)
        {
            if (component.ToUpper() == AppStrings.Filename_DefaultShelterbeltInput.ToUpper())
            {
                return new ShelterbeltConverter();
            }

            if (component.ToUpper() == AppStrings.Filename_DefaultFieldsInput.ToUpper())
            {
                return new FieldSystemInputConverter();
            }

            if (component.ToUpper() == AppStrings.Filename_DefaultSwineInput.ToUpper())
            {
                return new SwineConverter();
            }

            if (component.ToUpper() == AppStrings.Filename_DefaultDairyInput.ToUpper())
            {
                return new DairyConverter();
            }

            if (component.ToUpper() == AppStrings.Filename_DefaultSheepInput.ToUpper())
            {
                return new SheepConverter();
            }


            if (component.ToUpper() == AppStrings.Filename_DefaultBeefInput.ToUpper())
            {
                return new BeefConverter();
            }

            if (component.ToUpper() == AppStrings.Filename_DefaultPoultryInput.ToUpper())
            {
                return new PoultryConverter();
            }
            if (component.ToUpper() == AppStrings.Filename_DefaultOtherLivestockInput.ToUpper())
            {
                return new OtherLiveStockConverter();
            }

            else
            {
                throw new NotImplementedException();
            }
        } 
        #endregion
    }
}
