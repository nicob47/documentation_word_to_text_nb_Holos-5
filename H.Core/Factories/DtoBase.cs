using H.Core.Helpers;

namespace H.Core.Factories;

/// <summary>
/// A base class to be used with any other classes that must validate user input. The properties in this class and subclasses are properties
/// that are bound to GUI controls and should therefore be validated before passing on the input values to the domain/business class objects.
/// </summary>
public abstract class DtoBase : ErrorValidationBase, IDto
{
    #region Fields

    private string? _name;
    private Guid _guid = Guid.NewGuid();
    private Guid _domainObjectGuid;

    #endregion

    #region Properties

    public new string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public new Guid Guid
    {
        get => _guid;
        set => SetProperty(ref _guid, value);
    }

    /// <summary>
    /// Tracks which domain object this DTO was created from.
    /// Set by TransferService when transferring a domain object to a DTO.
    /// </summary>
    public Guid DomainObjectGuid
    {
        get => _domainObjectGuid;
        set => SetProperty(ref _domainObjectGuid, value);
    }

    #endregion
}