using H.Avalonia.Infrastructure;
using H.Avalonia.Models;
using H.Avalonia.Models.ClassMaps;
using H.Avalonia.Services;
using H.Core.Services.Climate;
using H.Core.Services.StorageService;
using LiveChartsCore;
using Microsoft.Extensions.Logging;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiveChartsCore.SkiaSharpView;

namespace H.Avalonia.ViewModels.Results
{
    public class EmissionPieChartViewModel : ResultsViewModelBase
    {
        private IRegionNavigationJournal? _navigationJournal;
        private readonly ExportHelpers _exportHelpers;
        private readonly ClimateResultsViewItemMap _climateResultsViewItemMap;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger _logger;

        public IEnumerable<ISeries> PieChartSeries { get; set; }

        EmissionPieChartViewModel(IRegionManager regionManager, INotificationManagerService notificationManager, ExportHelpers exportHelpers, IStorageService storageService, ILogger logger) : base(regionManager, notificationManager, storageService)
        {
            if (logger != null)
            {
                _logger = logger;
            }
            else
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (exportHelpers != null)
            {
                _exportHelpers = exportHelpers;
            }
            else
            {
                throw new ArgumentNullException(nameof(exportHelpers));
            }

            _climateResultsViewItemMap = new ClimateResultsViewItemMap();

            PieChartSeries = new ISeries[]
            {
                new PieSeries<double> { Values = new double[] { 40 }, Name = "Enteric CH4" },
                new PieSeries<double> { Values = new double[] { 30 }, Name = "Manure CH4" },
                new PieSeries<double> { Values = new double[] { 20 }, Name = "Direct N20" },
                new PieSeries<double> { Values = new double[] { 10 }, Name = "Indirect N20" },
                new PieSeries<double> { Values = new double[] { 10 }, Name = "Energy CO2" }
            };
        }
    }
}
