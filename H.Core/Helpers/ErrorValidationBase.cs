using System.Collections;
using System.ComponentModel;
using H.Infrastructure;

namespace H.Core.Helpers;

/// <summary>
/// A class implementing the <see cref="INotifyDataErrorInfo"/> interface to be used by any class that needs input validation
/// collected from a view.
/// </summary>
public abstract class ErrorValidationBase : ModelBase, INotifyDataErrorInfo
{
    #region Fields

    private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    #endregion

    protected ErrorValidationBase()
    {
    }

    #region Properties

    public bool HasErrors => _errors.Any();

    IEnumerable INotifyDataErrorInfo.GetErrors(string? propertyName)
    {
        return GetErrors(propertyName ?? string.Empty)!;
    }

    #endregion

    #region Public Methods
    
    /// <summary>
    /// Adds an error message associated with the property name that has failed validation
    /// </summary>
    /// <param name="propertyName">The string representing the property name failing validation</param>
    /// <param name="error">A descriptive error message that will be presented to the user</param>
    public void AddError(string propertyName, string error)
    {
        if (!_errors.ContainsKey(propertyName))
        {
            _errors[propertyName] = new List<string>();
        }

        if (!_errors[propertyName].Contains(error))
        {
            _errors[propertyName].Add(error);
            OnErrorsChanged(propertyName);
            this.RaisePropertyChanged(nameof(HasErrors));
        }
    }

    /// <summary>
    /// Once the user has corrected the validation error, the error message should not be shown and should be removed
    /// from the collection of active errors.
    /// </summary>
    /// <param name="propertyName">The string representing the property that has passed validation</param>
    public void RemoveError(string propertyName)
    {
        if (_errors.ContainsKey(propertyName))
        {
            _errors[propertyName].Clear();
            _errors.Remove(propertyName);
            OnErrorsChanged(propertyName);
            this.RaisePropertyChanged(nameof(HasErrors));
        }
    }

    /// <summary>
    /// Raise an event so that UI elements can show/remove any relevant error messages
    /// </summary>
    /// <param name="propertyName"></param>
    public void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Get a collection of all active errors associated with the property
    /// </summary>
    /// <param name="propertyName">The string representing the property that might have errors</param>
    /// <returns>A collection of string errors associate with the property</returns>
    public IEnumerable? GetErrors(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName) || !_errors.ContainsKey(propertyName))
        {
            return null;
        }

        return _errors[propertyName];
    }

    #endregion
}