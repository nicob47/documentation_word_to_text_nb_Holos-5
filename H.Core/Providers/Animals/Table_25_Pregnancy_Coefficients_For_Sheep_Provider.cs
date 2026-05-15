using NLog;

namespace H.Core.Providers.Animals
{
    /// <summary>
    /// Table 25. Pregnancy coefficients for sheep
    /// <para>Source: IPCC 2019, Table 10.7</para>
    /// </summary>
    public class Table_25_Pregnancy_Coefficients_For_Sheep_Provider
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();


        public Table_25_Pregnancy_Coefficients_For_Sheep_Provider()
        {
        }
        public double GetPregnancyCoefficient(double lambRatio)
        {
            if (lambRatio <= 1)
            {
                return 0.077;
            }

            if (lambRatio > 1 && lambRatio <= 2)
            {
                return 0.077 * (2 - lambRatio) + 0.126 * (lambRatio - 1);
            }

            else if (lambRatio > 2 && lambRatio <= 3)
            {
                return 0.126 * (3 - lambRatio) + 0.150 * (lambRatio - 2);
            }

            else
            {
                _log.Error($"{nameof(Table_25_Pregnancy_Coefficients_For_Sheep_Provider)}.{nameof(Table_25_Pregnancy_Coefficients_For_Sheep_Provider.GetPregnancyCoefficient)}" +
                    $" unable to get data for lamb ratio: {lambRatio}." +
                    $" Returning default value of {0}.");
                return 0;
            }
        }

    }
}
