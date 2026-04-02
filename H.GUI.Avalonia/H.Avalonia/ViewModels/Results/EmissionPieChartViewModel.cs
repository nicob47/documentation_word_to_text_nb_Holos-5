using H.Infrastructure;
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
using Avalonia.Controls;
using H.Avalonia.Models.Results;
using H.Core.Services;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ShimSkiaSharp;
using SkiaSharp;

namespace H.Avalonia.ViewModels.Results
{
    public class EmissionPieChartViewModel : ResultsViewModelBase, INavigationAware
    {
        #region Fields

        private readonly ILogger _logger;
        private bool _showDetails = true;
        private ComboBoxItem _selectedEmissionType = new();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the collection of data series displayed in the pie chart.
        /// </summary>
        public IEnumerable<ISeries> PieChartSeries { get; set; }

        public ObservableCollection<ComboBoxItem> EmissionTypeOptions { get; set; } = new ObservableCollection<ComboBoxItem>
        {
            new ComboBoxItem { Content = "Kg CO2e" },
            new ComboBoxItem { Content = "Mg CO2e" },
            new ComboBoxItem { Content = "Kg GHGs" },
            new ComboBoxItem { Content = "Mg GHGs" }
        };

        /// <summary>
        /// Gets or sets the currently selected emission type in the user interface.
        /// </summary>
        public ComboBoxItem SelectedEmissionType
        {
            get => _selectedEmissionType;
            set => SetProperty(ref _selectedEmissionType, value);
        }

        /// <summary>
        /// Gets or sets the collection of years available for selection.
        /// </summary>
        public ObservableCollection<int> AvailableYears { get; set; } = new ObservableCollection<int>()
        {
            2026, 2025, 2024, 2023, 2022, 2021, 2020, 2019, 2018,
        };

        /// <summary>
        /// Gets or sets the year currently selected by the user.
        /// </summary>
        public int SelectedYear { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether detailed information is displayed in the user interface.
        /// This serves no function as is, just tried to mock V4 GUI
        /// </summary>
        public bool ShowDetails
        {
            get => _showDetails;
            set => SetProperty(ref _showDetails, value);
        }

        #endregion

        #region Constructors

        EmissionPieChartViewModel()
        {

            ConstructPieChartContent();

            // Set defaults to first item in each collection
            SelectedEmissionType = EmissionTypeOptions.FirstOrDefault();
            SelectedYear = AvailableYears.FirstOrDefault();
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        /// <summary>
        /// Constructs and populates the content for the pie chart representing emission sources and their respective values.
        /// </summary>
        private void ConstructPieChartContent()
        {
            PieChartSeries = new ISeries[] { };

            EmissionPieChartViewItem entericMethane = new EmissionPieChartViewItem { GroupName = "Enteric CH4", EmissionType = "CH4", Value = 100, Label = "Enteric CH4" };
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

            SetPercentOfOutputPerGroup(items);

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

        /// <summary>
        /// Sets the values of the PieChartViewItems to their respective percentage 
        /// </summary>
        /// <param name="emissionsList">The list of PieChartViewItems that will be calculating percentage of</param>
        private void SetPercentOfOutputPerGroup(List<EmissionPieChartViewItem> emissionsList)
        {
            var totalEmissions = 0;
            foreach (var item in emissionsList)
            {
                totalEmissions += (int)item.Value;
            }
            foreach (var item in emissionsList)
            {
                item.Value = Math.Round((item.Value / totalEmissions) * 100, 2);
            }
        }

        #endregion
    }
}
