using H.Core.CustomAttributes;
using H.Core.Enumerations;
using H.Core.Models.LandManagement.Fields;
using H.Core.Models.LandManagement.Rotation;
using System.ComponentModel;

namespace H.Core.Factories.Rotations;

/// <summary>
/// A class used to validate input as it relates to a <see cref="RotationComponent"/>. This class is used to valid input before any input
/// is transferred to the <see cref="RotationComponent"/>
/// </summary>
public class RotationComponentDto : DtoBase, IRotationComponentDto
{
    #region Fields

    private double _fieldArea;

    private int _startYear;
    private int _endYear;
    private int _numberOfFields;
    private bool _isSelected;

    #endregion

    #region Constructors

    public RotationComponentDto()
    {
        this.PropertyChanged += OnPropertyChanged;
    }

    #endregion

    #region Properties

    /// <summary>
    /// All fields that belong to this rotation must have the same area. This property defines that area
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

    public int NumberOfFields
    {
        get => _numberOfFields;
        set => SetProperty(ref _numberOfFields, value);
    }

    /// <summary>
    /// Indicates whether this rotation component is currently selected in the UI
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    /// <summary>
    /// Calculated property that returns the length of the rotation in years (EndYear - StartYear).
    /// Returns 0 if either StartYear or EndYear is not set or if EndYear is less than or equal to StartYear.
    /// </summary>
    public int RotationLength
    {
        get
        {
            if (this.StartYear > 0 && this.EndYear > 0 && this.EndYear > this.StartYear)
            {
                return this.EndYear - this.StartYear;
            }
            return 0;
        }
    }

    /// <summary>
    /// Calculated property that returns the total area of all fields in the rotation (FieldArea * NumberOfFields).
    /// 
    /// (ha)
    /// </summary>
    public double TotalRotationArea
    {
        get
        {
            return this.FieldArea * this.NumberOfFields;
        }
    }

    /// <summary>
    /// Calculated property that returns the total number of crop-years in the rotation (NumberOfFields * RotationLength).
    /// This represents the total number of individual crop instances across all fields and all years in the rotation.
    /// For example, if you have 3 fields and a 5-year rotation, you have 15 crop-years total.
    /// </summary>
    public int TotalCropYears
    {
        get
        {
            return this.NumberOfFields * this.RotationLength;
        }
    }

    #endregion

    #region Event Handlers

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
    /// Ensure all <see cref="RotationComponent"/>s will have a valid name specified by the user
    /// </summary>
    private void ValidateRotationName()
    {
        var key = nameof(Name);
        if (string.IsNullOrWhiteSpace(this.Name))
        {
            AddError(key, "Rotation name cannot be empty");
        }
        else
        {
            RemoveError(key);
        }
    }

    /// <summary>
    /// Ensure that the area of the rotation is a valid number
    /// </summary>
    private void ValidateFieldArea()
    {
        var key = nameof(FieldArea);
        if (this.FieldArea <= 0)
        {
            AddError(key, "Rotation size cannot be less than or equal to zero");
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
            ValidateRotationName();
        }
        else if (e.PropertyName != null && e.PropertyName.Equals(nameof(FieldArea)))
        {
            // Ensure the area of the field is valid
            ValidateFieldArea();
            // Notify that TotalRotationArea has changed
            RaisePropertyChanged(nameof(TotalRotationArea));
        }
        else if (e.PropertyName != null && e.PropertyName.Equals(nameof(NumberOfFields)))
        {
            // Notify that TotalRotationArea and TotalCropYears have changed
            RaisePropertyChanged(nameof(TotalRotationArea));
            RaisePropertyChanged(nameof(TotalCropYears));
        }
        else if (e.PropertyName != null && e.PropertyName.Equals(nameof(StartYear)))
        {
            // Ensure the start year is valid
            ValidateStartYear();
            // Re-validate end year in case it was previously invalid due to start year
            ValidateEndYear();
            // Notify that RotationLength and TotalCropYears have changed
            RaisePropertyChanged(nameof(RotationLength));
            RaisePropertyChanged(nameof(TotalCropYears));
        }
        else if (e.PropertyName != null && e.PropertyName.Equals(nameof(EndYear)))
        {
            // Ensure the end year is valid
            ValidateEndYear();
            // Re-validate start year in case it was previously invalid due to end year
            ValidateStartYear();
            // Notify that RotationLength and TotalCropYears have changed
            RaisePropertyChanged(nameof(RotationLength));
            RaisePropertyChanged(nameof(TotalCropYears));
        }
    }

    #endregion
}