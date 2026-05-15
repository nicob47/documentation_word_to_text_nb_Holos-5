#region Imports

using System.Diagnostics;
using H.Core.Enumerations;
using NLog;

#endregion

namespace H.Core.Providers
{
    /// <summary>
    /// </summary>
    public static class YearlyAverageCalculator
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        #region Public Methods

        public static List<double> GetAverages(this List<double> values)
        {
            if (values.Count != 12)
            {
                var message = $"{nameof(YearlyAverageCalculator)}.{nameof(GetAverages)}: expected a list of 12 values but got {values.Count()} values instead.";
                _log.Info(message);

                throw new InvalidOperationException();
            }

            var yearlyAverages = new List<double>();
            var numberOfDaysInEachMonthList = new List<int>
            {
                (int) DaysInMonth.January,
                (int) DaysInMonth.February,
                (int) DaysInMonth.March,
                (int) DaysInMonth.April,
                (int) DaysInMonth.May,
                (int) DaysInMonth.June,
                (int) DaysInMonth.July,
                (int) DaysInMonth.August,
                (int) DaysInMonth.September,
                (int) DaysInMonth.October,
                (int) DaysInMonth.November,
                (int) DaysInMonth.December
            };

            for (var i = 0; i < numberOfDaysInEachMonthList.Count; i++)
            {
                var currentMonthAndDayCount = numberOfDaysInEachMonthList.ElementAt(i);
                var currentInputValue = values.ElementAt(i);
                var average = currentInputValue / currentMonthAndDayCount;
                //var average = currentInputValue;
                for (var j = 0; j < currentMonthAndDayCount; j++)
                {
                    yearlyAverages.Add(average);
                }
            }

            return yearlyAverages;
        }

        #endregion

        #region Constructors

        #endregion

        #region Fields

        #endregion

        #region Properties

        #endregion

        #region Private Methods

        #endregion

        #region Event Handlers

        #endregion
    }
}