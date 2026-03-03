using System;
using Avalonia.Controls;
using H.Avalonia.ViewModels.SupportingViews.MeasurementProvince;

namespace H.Avalonia.Views.SupportingViews.MeasurementProvince
{
    public partial class MeasurementProvinceView : UserControl
    {
        #region Fields

        private MeasurementProvinceViewModel? _measurementProvinceViewModel;

        #endregion

        #region Constructors

        /// <summary>
        /// Parameterless constructor required by the Avalonia XAML loader.
        /// </summary>
        public MeasurementProvinceView()
        {
            InitializeComponent();
        }

        public MeasurementProvinceView(MeasurementProvinceViewModel measurementProvinceViewModel)
        {
            InitializeComponent();
            _measurementProvinceViewModel = measurementProvinceViewModel ?? throw new ArgumentNullException(nameof(measurementProvinceViewModel));
        }

        #endregion
    }
}