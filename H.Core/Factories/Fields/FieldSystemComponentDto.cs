using System.Collections.ObjectModel;
using System.ComponentModel;
using H.Core.CustomAttributes;
using H.Core.Enumerations;
using H.Core.Factories.Crops;
using H.Core.Models.LandManagement.Fields;

namespace H.Core.Factories.Fields;

/// <summary>
/// A class used to validate input as it relates to a <see cref="FieldSystemComponent"/>. This class is used to valid input before any input
/// is transferred to the <see cref="FieldSystemComponent"/>
/// </summary>
public class FieldSystemComponentDto : DtoBase, IFieldComponentDto
{
    #region Fields

    private double _fieldArea;

    private int _startYear;
    private int _endYear;

    private ObservableCollection<ICropDto>? _cropDtoModels;

    #endregion

    #region Constructors

    public FieldSystemComponentDto()
    {
        this.CropDtos = new ObservableCollection<ICropDto>();

        this.PropertyChanged += OnPropertyChanged;
    }

    #endregion

    #region Properties

    /// <summary>
    /// A collection of <see cref="CropDto"/>. Each <see cref="CropDto"/> in the collection represents the crop data input for one particular year on the
    /// given <see cref="FieldSystemComponentDto"/>
    /// </summary>
    public ObservableCollection<ICropDto>? CropDtos
    {
        get => _cropDtoModels;
        set => SetProperty(ref _cropDtoModels, value);
    }

    /// <summary>
    /// The total size of the field
    ///
    /// (ha)
    /// </summary>
    [Units(MetricUnitsOfMeasurement.Hectares)]
    public double FieldArea
    {
        get => _fieldArea;
        set => SetProperty(ref _fieldArea, value);
    }

    public int StartYear
    {
        get => _startYear;
        set => SetProperty(ref _startYear, value);
    }

    public int EndYear
    {
        get => _endYear;
        set => SetProperty(ref _endYear, value);
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Ensure all <see cref="FieldSystemComponent"/>s will have a valid name specified by the user
    /// </summary>
    private void ValidateFieldName()
    {
        var key = nameof(Name);
        if (string.IsNullOrWhiteSpace(this.Name))
        {
            AddError(key, "Field name cannot be empty");
        }
        else
        {
            RemoveError(key);
        }
    }

    /// <summary>
    /// Ensure that the area of the field is a valid number
    /// </summary>
    private void ValidateFieldArea()
    {
        var key = nameof(FieldArea);
        if (this.FieldArea <= 0)
        {
            AddError(key, "Field size cannot be less than or equal to zero");
        }
        else
        {
            RemoveError(key);
        }
    }

    /// <summary>
    /// Ensure that the start year is within a valid range (greater than 1900 and less than 100 years into the future)
    /// and that it is before the end year
    /// </summary>
    private void ValidateStartYear()
    {
        var key = nameof(StartYear);
        var currentYear = DateTime.Now.Year;
        var maxYear = currentYear + 100;

        if (this.StartYear <= 1900)
        {
            AddError(key, "Start year must be greater than 1900");
        }
        else if (this.StartYear > maxYear)
        {
            AddError(key, $"Start year cannot be more than 100 years in the future (maximum: {maxYear})");
        }
        else if (this.EndYear > 0 && this.StartYear >= this.EndYear)
        {
            AddError(key, "Start year must be before end year");
        }
        else
        {
            RemoveError(key);
        }
    }

    /// <summary>
    /// Ensure that the end year is within a valid range (greater than 1900 and less than 100 years into the future)
    /// and that it is after the start year
    /// </summary>
    private void ValidateEndYear()
    {
        var key = nameof(EndYear);
        var currentYear = DateTime.Now.Year;
        var maxYear = currentYear + 100;

        if (this.EndYear <= 1900)
        {
            AddError(key, "End year must be greater than 1900");
        }
        else if (this.EndYear > maxYear)
        {
            AddError(key, $"End year cannot be more than 100 years in the future (maximum: {maxYear})");
        }
        else if (this.StartYear > 0 && this.EndYear <= this.StartYear)
        {
            AddError(key, "End year must be after start year");
        }
        else
        {
            RemoveError(key);
        }
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != null && e.PropertyName.Equals(nameof(Name)))
        {
            // Ensure the field name is valid
           ValidateFieldName();
        }
        else if (e.PropertyName != null && e.PropertyName.Equals(nameof(FieldArea)))
        {
            // Ensure the area of the field is valid
            ValidateFieldArea();
        }
        else if (e.PropertyName != null && e.PropertyName.Equals(nameof(StartYear)))
        {
            // Ensure the start year is valid
            ValidateStartYear();
            // Re-validate end year in case it was previously invalid due to start year
            ValidateEndYear();
        }
        else if (e.PropertyName != null && e.PropertyName.Equals(nameof(EndYear)))
        {
            // Ensure the end year is valid
            ValidateEndYear();
            // Re-validate start year in case it was previously invalid due to end year
            ValidateStartYear();
        }
    }

    #endregion
}