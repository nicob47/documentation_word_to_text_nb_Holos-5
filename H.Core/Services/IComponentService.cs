using H.Core.Models;

namespace H.Core.Services;

public interface IComponentService
{
    string GetUniqueComponentName(Farm farm, ComponentBase component);
    void InitializeComponent(Farm farm, ComponentBase? component);
}