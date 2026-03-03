namespace H.Core.Providers.Soil
{
    public class CustomUserYieldData
    {
        public int Year { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public double Yield { get; set; }
        public string RotationName { get; set; } = string.Empty;
    }
}