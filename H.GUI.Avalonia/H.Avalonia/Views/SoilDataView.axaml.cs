using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using H.Avalonia.ViewModels;
using H.Core.Enumerations;
using H.Core.Providers.Soil;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Limiting;
using Mapsui.Nts;
using Mapsui.Nts.Extensions;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Rendering;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Tiling.Layers;
using Mapsui.UI.Avalonia;
using Mapsui.Widgets.Zoom;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Point = NetTopologySuite.Geometries.Point;

namespace H.Avalonia.Views
{
    public partial class SoilDataView : UserControl, INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// A RasterizingTileLayer that goes on top of the map to display the polygons for a specific province.
        /// </summary>
        private RasterizingTileLayer? _polygonLayer;

        /// <summary>
        /// Central coordinate points for all provinces used for map navigation when a province is selected.
        /// </summary>
        private MPoint _coordinateBritishColumbia = new MPoint(-13928197, 7300000);
        private MPoint _coordinateAlberta = new MPoint(-12731248, 7300000);
        private MPoint _coordinateSaskatchewan = new MPoint(-11800000, 7300000);
        private MPoint _coordinateManitoba = new MPoint(-10900000, 7300000);
        private MPoint _coordinateOntario = new MPoint(-9510000, 6400000);
        private MPoint _coordinateQuebec = new MPoint(-7900000, 6300000);
        private MPoint _coordinateNewBrunswick = new MPoint(-7400000, 5850000);
        private MPoint _coordinatePrinceEdwardIsland = new MPoint(-7020000, 5830000);
        private MPoint _coordinateNovaScotia = new MPoint(-7030000, 5650000);
        private MPoint _coordinateNewfoundland = new MPoint(-6250000, 6250000);

        private ILogger _logger;

        #endregion

        #region Properties

        public SoilDataViewModel? _viewModel => DataContext as SoilDataViewModel;

        #endregion

        private TopLevel GetTopLevel() => TopLevel.GetTopLevel(this) ?? throw new NullReferenceException("Invalid Owner");

        /// <summary>
        /// A GenericCollectionLayer that goes on top of the map and holds the points that are displayed to indicate location.
        /// </summary>
        private GenericCollectionLayer<List<IFeature>> _pointsLayer = new()
        {
            Style = SymbolStyles.CreatePinStyle()
        };

        #region Constructors

        /// <summary>
        /// Parameterless constructor required by the Avalonia XAML loader.
        /// </summary>
        public SoilDataView() : this(Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance)
        {
        }

        public SoilDataView(ILogger logger)
        {
            _logger = logger;
            InitializeComponent();
            InitializeMap();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Is used to attach the windows manager for displaying notifications.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            if (_viewModel is not null) _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles certain behaviour related to the map and how it is affected based on user interaction.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs? args)
        {
            switch (args?.PropertyName)
            {
                case nameof(_viewModel.NavigationPoint):
                    {
                        // Navigate to the specific point on the map based on longitude and lat values.
                        NavigateToPoint();

                        // Call method to add point to map
                        AddPointToMap();
                        break;
                    }
                case nameof(_viewModel.SelectedProvince):
                    {
                        if (_polygonLayer != null) SoilTabMap.Map.Layers.Remove(_polygonLayer);
                        if (_viewModel is not null && _viewModel.SelectedProvince != Province.SelectProvince)
                        {
                            _polygonLayer = new RasterizingTileLayer(CreateLayer(_viewModel.SelectedProvince), minTiles: 400, maxTiles: 800, renderFormat: RenderFormat.WebP);
                            SoilTabMap.Map.Layers.Add(_polygonLayer);
                            SetCoordinatesOnProvinceSelected(_viewModel.SelectedProvince);
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// Sets the map coordinates and zoom level based on the selected province.
        /// </summary>
        /// <param name="selectedProvince">The province to center map on</param>
        private void SetCoordinatesOnProvinceSelected(Province selectedProvince)
        {
            _logger.LogDebug("New province " + selectedProvince + " selected in " + nameof(SoilDataView));
            switch (selectedProvince)
            {
                case (Province.BritishColumbia):
                {
                    SoilTabMap.Map.Navigator.CenterOnAndZoomTo(_coordinateBritishColumbia, resolution: 3500);
                    break;
                }
                case (Province.Alberta):
                {
                    SoilTabMap.Map.Navigator.CenterOnAndZoomTo(_coordinateAlberta, resolution: 3500);
                    break;
                }
                case (Province.Saskatchewan):
                {
                    SoilTabMap.Map.Navigator.CenterOnAndZoomTo(_coordinateSaskatchewan, resolution: 3500);
                    break;
                }
                case (Province.Manitoba):
                {
                    SoilTabMap.Map.Navigator.CenterOnAndZoomTo(_coordinateManitoba, resolution: 3500);
                    break;
                }
                case (Province.Ontario):
                {
                    SoilTabMap.Map.Navigator.CenterOnAndZoomTo(_coordinateOntario, resolution: 3900);
                    break;
                }
                case (Province.Quebec):
                {
                    SoilTabMap.Map.Navigator.CenterOnAndZoomTo(_coordinateQuebec, resolution: 3000);
                    break;
                }
                case (Province.NewBrunswick):
                {
                    SoilTabMap.Map.Navigator.CenterOnAndZoomTo(_coordinateNewBrunswick, resolution: 1200);
                    break;
                }
                case (Province.PrinceEdwardIsland):
                {
                    SoilTabMap.Map.Navigator.CenterOnAndZoomTo(_coordinatePrinceEdwardIsland, resolution: 600);
                    break;
                }
                case (Province.NovaScotia):
                {
                    SoilTabMap.Map.Navigator.CenterOnAndZoomTo(_coordinateNovaScotia, resolution: 1100);
                    break;
                }
                case (Province.Newfoundland):
                {
                    SoilTabMap.Map.Navigator.CenterOnAndZoomTo(_coordinateNewfoundland, resolution: 1400);
                    break;
                }
            }
        }

        #endregion

        /// <summary>
        /// Initialize the map displayed on the <see cref="SoilDataView"/>'s single coordinate tab.
        /// </summary>
        private void InitializeMap()
        {
            _logger.LogDebug("Attempting to initialize map in " + nameof(SoilDataView));

            SoilTabMap.Map.Layers.Add(OpenStreetMap.CreateTileLayer());
            SoilTabMap.Map.Navigator.Limiter = new ViewportLimiterKeepWithinExtent();

            SoilTabMap.Map.Widgets.Add(new ZoomInOutWidget
            {
                MarginX = 10,
                MarginY = 20,
                Size = 25,
                TextColor = Color.Black,
                BackColor = Color.White,
                Opacity = 1,
            });

            // Subscribe to when the map control is fully loaded and rendered
            SoilTabMap.LayoutUpdated += OnMapLayoutUpdated;

            _logger.LogDebug("Map initialized successfully in " + nameof(SoilData));
        }

        private bool _hasZoomedToCanada = false;

        private void OnMapLayoutUpdated(object? sender, EventArgs e)
        {
            // Check if viewport is initialized and we haven't zoomed yet
            if (_hasZoomedToCanada || SoilTabMap.Map.Navigator.Viewport.Width <= 0) return;

            _hasZoomedToCanada = true;
            SoilTabMap.LayoutUpdated -= OnMapLayoutUpdated;

            // Define Canada's bounding box in EPSG:4326 (lat/lon)
            var minX = -141.0;  // West longitude
            var minY = 41.7;    // South latitude
            var maxX = -52.6;   // East longitude
            var maxY = 83.1;    // North latitude

            // Convert to EPSG:3857 (Web Mercator)
            var min = SphericalMercator.FromLonLat(minX, minY);
            var max = SphericalMercator.FromLonLat(maxX, maxY);
            var canadaBounds = new MRect(min.x, min.y, max.x, max.y);

            // Zoom to Canada
            SoilTabMap.Map.Navigator.ZoomToBox(canadaBounds, MBoxFit.Fit);
        }

        /// <summary>
        /// Navigate to a specific point and zoom into that location.
        /// </summary>
        private void NavigateToPoint()
        {
            if (_viewModel is not null) SoilTabMap.Map.Navigator.CenterOnAndZoomTo(_viewModel.NavigationPoint, resolution: 9);
        }

        /// <summary>
        /// Adds a point to the map displayed to the user.
        /// </summary>
        private void AddPointToMap()
        {
            // Add the points layer to our current map
            SoilTabMap.Map.Layers.Add(_pointsLayer);
            // Clear the features collection of the points layer so that any previous points are removed
            _pointsLayer?.Features.Clear();

            // Add a new point to the map as a GeometryFeature
            _pointsLayer?.Features.Add(new GeometryFeature
            {
                Geometry = _viewModel is not null ? new Point(_viewModel.NavigationPoint.X, _viewModel.NavigationPoint.Y) : null
            });
            // To notify the map that a redraw is needed.
            _pointsLayer?.DataHasChanged();
        }

        /// <summary>
        /// Handles behaviour related to user mouse clicks on the map.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SoilMap_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            _logger.LogDebug("New location selected in " + nameof(SoilDataView));
            // Get the properties of the pointer event so that we can determine the type of click.
            var properties = e.GetCurrentPoint(this).Properties;
            if (!properties.IsRightButtonPressed) return;

            // Get the screen position and world position of the clicked point
            var screenPosition = e.GetPosition(SoilTabMap);
            var worldPosition = SoilTabMap.Map.Navigator.Viewport.ScreenToWorld(screenPosition.X, screenPosition.Y);

            // Update the navigation point based on the new world position
            _viewModel?.UpdateNavigationPointCommand.Execute(worldPosition);

            // Navigate to point and create marker for point
            NavigateToPoint();
            AddPointToMap();

            // Update the Address and Long/Lat values shown to user in the UI
            _viewModel?.UpdateInformationFromNavigationPointCommand.Execute();
        }

        /// <summary>
        /// Calls the appropriate command in the viewmodel when the user clicks the import data button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ImportDataButton_OnClick(object? sender, RoutedEventArgs e)
        {
            if (_viewModel is null) return;
            var storageProvider = GetTopLevel().StorageProvider;
            var item = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = Core.Properties.Resources.ImportDefaultName,
                AllowMultiple = false,
            });
            if (_viewModel.ImportFromCsvCommand.CanExecute(item))
            {
                _viewModel.ImportFromCsvCommand.Execute(item);
            }
        }

        /// <summary>
        /// Creates a layer for a polygon that will be placed on top of the map. We must have <see cref="ViewModel.WktPolygonMap"/> ready before using this method, otherwise an exception might be thrown.
        /// </summary>
        /// <param name="province"></param>
        /// <returns></returns>
        private ILayer CreateLayer(Province province)
        {
            _logger.LogDebug("Drawing " + province + " polygons on top of " + nameof(SoilDataView) + " map.");
            if (_viewModel is null) return new Layer("Polygons");
            var polygons = _viewModel.WktPolygonMap[province];
            return new Layer("Polygons")
            {
                DataSource = new MemoryProvider(polygons.ToFeatures()),
                Style = new VectorStyle
                {
                    Fill = new Brush(Color.Orange),
                    Opacity = 0.20f,
                    Outline = new Pen
                    {
                        Color = Color.Brown,
                        Width = 2,
                        PenStyle = PenStyle.Solid,
                        PenStrokeCap = PenStrokeCap.Round
                    }
                }
            };
        }
    }
}