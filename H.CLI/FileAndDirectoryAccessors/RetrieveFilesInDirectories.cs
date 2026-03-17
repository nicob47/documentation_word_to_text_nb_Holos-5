using System;
using System.IO;
using H.Localization.Resources.Strings;

namespace H.CLI.FileAndDirectoryAccessors
{
    public class RetrieveFilesInDirectories
    {
        #region Public Methods
        /// <summary>
        /// Used in the ParserHandler to retrieve a list of files from a component's directory path. (ie, all the files in the Shelterbelt directory)
        /// </summary>
        public string[] GetSubDirectoryFiles(string path)
        {
            try
            {
                var files = Directory.GetFiles(path);
                return files;
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine(String.Format(AppStrings.Error_DirectoryNotFound, ex.Message));
                throw new DirectoryNotFoundException();
            }

        } 
        #endregion
    }
}
