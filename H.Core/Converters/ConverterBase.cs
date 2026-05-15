namespace H.Core.Converters
{
	public abstract class ConverterBase
	{
		public ConverterBase()
		{
		}

		// Does not deal with removing accents since we will be parsing from the english values of the csv
		protected string GetLettersAsLowerCase(string input)
		{
			var cleanedInput = "";
			foreach (var a in input)
			{
				if (char.IsLetter(a))
				{
					cleanedInput += a;
				}
			}

			return cleanedInput.ToLowerInvariant()
							   .Trim();
		}

        protected string GetLowerCase(string input)
        {
            return input.ToLowerInvariant();
        }
	}
}