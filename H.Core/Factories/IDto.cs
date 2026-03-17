using System.ComponentModel;

namespace H.Core.Factories;

public interface IDto : INotifyPropertyChanged
{
    string? Name { get; set; }
    public Guid Guid { get; set; }

    /// <summary>
    /// Tracks which domain object this DTO was created from.
    /// Set by TransferService when transferring a domain object to a DTO.
    /// </summary>
    public Guid DomainObjectGuid { get; set; }
}