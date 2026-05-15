using System.Diagnostics;
using H.Core.Models;
using H.Infrastructure;
using NLog;

namespace H.Core.Enumerations
{
    public static class ManureAnimalSourceTypeExtension
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public static ComponentCategory GetComponentCategory(this ManureAnimalSourceTypes manureAnimalSourceType)
        {
            switch (manureAnimalSourceType)
            {
                case ManureAnimalSourceTypes.BeefManure:
                    return ComponentCategory.BeefProduction;

                case ManureAnimalSourceTypes.DairyManure:
                    return ComponentCategory.Dairy;

                case ManureAnimalSourceTypes.SwineManure:
                    return ComponentCategory.Swine;

                case ManureAnimalSourceTypes.SheepManure:
                    return ComponentCategory.Sheep;

                case ManureAnimalSourceTypes.PoultryManure:
                    return ComponentCategory.Poultry;

                case ManureAnimalSourceTypes.OtherLivestockManure:
                    return ComponentCategory.OtherLivestock;

                default:
                {
                    _log.Error($"Unknown manure animal source type: {manureAnimalSourceType.GetDescription()}. Returning {ComponentCategory.BeefProduction.GetDescription()}");
                    
                    return ComponentCategory.BeefProduction;
                }
            }
        }
    }
}