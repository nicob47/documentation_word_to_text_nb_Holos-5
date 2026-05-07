using H.Core.Enumerations.LocationEnumerationsProvinces;

namespace H.Core.Services.Provinces
{
    public class ProvincesService : IProvinces
    {
        public IEnumerable<object> GetProvinces()
        {
            return Enum.GetValues(typeof(ProvinceCanada)).Cast<object>();
        }
    }
}