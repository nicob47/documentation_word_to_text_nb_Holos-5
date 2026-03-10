using System;
using H.Localization.Resources.Strings;

namespace H.CLI.TemporaryComponentStorage
{
    public class InputHelper
    {
        public bool IsNotApplicableInput(string input)
        {
            return input.Equals("N/A", StringComparison.InvariantCultureIgnoreCase);
        }

        public bool IsYesResponse(string input)
        {
            return input.Equals("y", StringComparison.InvariantCultureIgnoreCase) ||
                   input.Equals("o", StringComparison.InvariantCultureIgnoreCase) || // French "Oui"
                   input.Equals(AppStrings.Label_Yes, StringComparison.InvariantCultureIgnoreCase);
        }

        public bool IsNoResponse(string input)
        {
            return input.Equals("n", StringComparison.InvariantCultureIgnoreCase) ||                   
                   input.Equals(AppStrings.Label_No, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
