using System.Collections.Generic;
using H.Localization.Resources.Strings;

namespace H.CLI.FileAndDirectoryAccessors
{
    public class DirectoryKeys 
    {
        #region Fields
        /// <summary>
        /// A dictionary that contains the name of the Directory and the weight associated with that directory
        /// Fields and Shelterbelts are a higher weight because we need to process them first incase they are
        /// used in the other Components. If you would like something to be processed first, please change the weight
        /// to be higher and just the Component weights accordingly.
        /// </summary>
        public Dictionary<string, int> directoryWeights { get; set; } = new Dictionary<string, int>
        {
            {AppStrings.Filename_DefaultShelterbeltInput, 2 },
            {AppStrings.Filename_DefaultFieldsInput, 2 },
            {AppStrings.Filename_DefaultBeefInput, 1 },
            {AppStrings.Filename_DefaultSheepInput, 1 },
            {AppStrings.Filename_DefaultDairyInput, 1 },
            {AppStrings.Filename_DefaultPoultryInput, 1 },
            {AppStrings.Filename_DefaultSwineInput, 1 },
            {AppStrings.Filename_DefaultOtherLivestockInput, 1 }
        };   
        #endregion
    }
}
