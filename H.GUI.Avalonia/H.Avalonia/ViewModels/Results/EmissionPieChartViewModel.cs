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
using H.Avalonia.Models.Results;
using H.Core.Services;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ShimSkiaSharp;
using SkiaSharp;

namespace H.Avalonia.ViewModels.Results
{
    public class EmissionPieChartViewModel : ResultsViewModelBase, INavigationAware
    {
        #region Fields

        private readonly ILogger _logger;
        private bool _showDetails = true;

        #endregion

        #region Properties

        public IEnumerable<ISeries> PieChartSeries { get; set; }

        public bool ShowDetails
        {
            get => _showDetails;
            set => SetProperty(ref _showDetails, value);
        }

        #endregion

        #region Constructors

        EmissionPieChartViewModel(IRegionManager regionManager, INotificationManagerService notificationManager, IStorageService storageService, ILogger logger) : base(regionManager, notificationManager, storageService)
        {
            if (logger != null)
            {
                _logger = logger;
            }
            else
            {
                throw new ArgumentNullException(nameof(logger));
            }


            ConstructPieChartContent();

        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        private void ConstructPieChartContent()
        {
            PieChartSeries = new ISeries[] { };

            EmissionPieChartViewItem entericMethane = new EmissionPieChartViewItem { GroupName = "Enteric CH4", EmissionType = "CH4", Value = 35, Label = "Enteric CH4" };
            EmissionPieChartViewItem manureMethane = new EmissionPieChartViewItem { GroupName = "Manure CH4", EmissionType = "CH4", Value = 15, Label = "Manure CH4" };
            EmissionPieChartViewItem directNitrousOxide = new EmissionPieChartViewItem { GroupName = "Direct N20", EmissionType = "N20", Value = 20, Label = "Direct N20" };
            EmissionPieChartViewItem indirectNitrousOxide = new EmissionPieChartViewItem { GroupName = "Indirect N20", EmissionType = "N20", Value = 15, Label = "Indirect N20" };
            EmissionPieChartViewItem energyCarbonDioxide= new EmissionPieChartViewItem { GroupName = "Energy CO2", EmissionType = "CO2", Value = 15, Label = "Energy CO2" };

            List<EmissionPieChartViewItem> items = new List<EmissionPieChartViewItem>
            {
                entericMethane,
                manureMethane,
                directNitrousOxide,
                indirectNitrousOxide,
                energyCarbonDioxide
            };

            PieChartSeries = items.Select(item =>
                new PieSeries<EmissionPieChartViewItem>
                {
                    Values = new[] { item },
                    Name = item.Label,
                    Mapping = (viewItem, point) => point.PrimaryValue = viewItem.Value,
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsSize = 17,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                    DataLabelsFormatter = point => $"{point.Context.Series.Name}" + Environment.NewLine + string.Format("{0:0.##}", point.PrimaryValue) + "%",
                    IsHoverable = false,
                }
            ).ToArray();
        }

        #endregion
    }
}
