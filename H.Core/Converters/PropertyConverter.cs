using System.Diagnostics;
using System.Reflection;
using H.Core.Calculators.UnitsOfMeasurement;
using H.Core.CustomAttributes;
using H.Core.Enumerations;
using H.Core.Properties;

namespace H.Core.Converters
{
    /// <summary>
    /// A class used to convert properties of a class from one unit of measurement system to another
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PropertyConverter<T> : IPropertyConverter
    {
        #region Fields

        private readonly UnitsOfMeasurementCalculator _unitsCalculator = null!;

        #endregion

        #region Constructors

        /// <summary>
        /// A converter to be used alongside the <see cref="UnitsAttribute"/>
        /// </summary>
        /// <param name="instance">any type that has properties using attribute <see cref="UnitsAttribute"/></param>
        public PropertyConverter(T instance)
        {
            //sometimes we will get null instances so lets handle that
            if (instance != null)
            {
                this.Instance = instance;
                this.Type = instance.GetType();
                this.PropertyInfos = this.Type.GetProperties().Where(x => Attribute.IsDefined(x, typeof(UnitsAttribute))).ToList();
                _unitsCalculator = new UnitsOfMeasurementCalculator();
            }
        }

        #endregion

        #region Properties

        public T Instance { get; set; } = default!;
        public Type Type { get; set; } = null!;
        public List<PropertyInfo> PropertyInfos { get; set; } = null!;

        #endregion

        #region Public Methods

        public double GetSystemValueFromBinding(string propertyName)
        {
            if (PropertyInfos != null)
            {
                //check that the property is something that we can work on
                var prop = this.PropertyInfos.FirstOrDefault(x => x.Name == propertyName);
                if (prop == null)
                {
                    Trace.TraceInformation($"{nameof(PropertyConverter<T>)}.{nameof(GetSystemValueFromBinding)}: {propertyName} cannot be converted or doesn't exist. Returning 0");
                    return 0;
                }

                //using reflection we will set the value of the property prop
                return this.GetSystemValueFromBinding(prop);
            }

            Trace.TraceInformation($"{nameof(PropertyConverter<T>)}.{nameof(GetSystemValueFromBinding)}: {nameof(PropertyInfos)} is null. Returning 0");
            return 0;
        }

        public double GetBindingValueFromSystem(string propertyName)
        {
            if (PropertyInfos != null)
            {
                var prop = this.PropertyInfos.FirstOrDefault(x => x.Name == propertyName);
                if (prop == null)
                {
                    Trace.TraceInformation($"{nameof(PropertyConverter<T>)}.{nameof(GetBindingValueFromSystem)}: {propertyName} cannot be converted or doesn't exist. Returning 0");
                    return 0;
                }

                return this.GetBindingValueFromSystem(prop);
            }

            Trace.TraceInformation($"{nameof(PropertyConverter<T>)}.{nameof(GetSystemValueFromBinding)}: {nameof(PropertyInfos)} is null. Returning 0");
            return 0;
        }


        public double GetBindingValueFromSystem(PropertyInfo prop, MeasurementSystemType measurementSystemType)
        {
            // Get the list of attributes adorned on the property
            var attrs = prop.GetCustomAttributes(typeof(UnitsAttribute), false);

            if (measurementSystemType == MeasurementSystemType.Metric)
            {
                // The GUI is in metric so just return system value
                return (double)(prop.GetValue(this.Instance) ?? 0.0);
            }
            // Convert for imperial
            if (this.Instance != null && attrs.Length > 0)
            {
                // We now have the metric unit of the property from the system
                var metricUnit = ((UnitsAttribute)attrs[0]).SourceUnit;

                // Now we need to get the value of the property
                var propValue = (double)(prop.GetValue(this.Instance) ?? 0.0);

                // Convert to an imperial value for the binding
                var imperialValue = _unitsCalculator.ConvertValueToImperialFromMetric(metricUnit, propValue);

                return imperialValue;
            }

            Trace.TraceError($"{nameof(PropertyConverter<T>)}.{nameof(GetSystemValueFromBinding)}: unable to convert {prop.Name} value, returning 0.");

            return 0;
        }

        public double GetBindingValueFromSystem(PropertyInfo prop)
        {
            //the list of attributes
            var attrs = prop.GetCustomAttributes(typeof(UnitsAttribute), false);

            if (Settings.Default.MeasurementSystem == MeasurementSystemType.Metric)
            {
                //the gui is in metric so just return system value
                return (double)(prop.GetValue(this.Instance) ?? 0.0);
            }
            //convert for imperial
            if (this.Instance != null && attrs.Length > 0)
            {
                //I now have the metricUnit of the property in the system
                var metricUnit = ((UnitsAttribute)attrs[0]).SourceUnit;

                //now I need to get the value of the property
                var propValue = (double)(prop.GetValue(this.Instance) ?? 0.0);

                //the imperial value for the binding
                var imperialValue = _unitsCalculator.ConvertValueToImperialFromMetric(metricUnit, propValue);

                return imperialValue;
            }
            Trace.TraceError($"{nameof(PropertyConverter<T>)}.{nameof(GetSystemValueFromBinding)}: unable to convert {prop.Name} value, returning 0.");
            return 0;
        }


        public double GetSystemValueFromBinding(PropertyInfo prop)
        {
            //nothing to convert and return
            if (Settings.Default.MeasurementSystem == MeasurementSystemType.Metric)
            {
                return (double)(prop.GetValue(this.Instance) ?? 0.0);
            }

            //get the attribute on the property first
            var attrs = prop.GetCustomAttributes(typeof(UnitsAttribute), false);
            if (this.Instance != null && attrs.Length > 0)
            {
                //I now have the metricUnit of the property
                var metricUnit = ((UnitsAttribute)attrs[0]).SourceUnit;

                //the unit to convert from (i.e. lbs -> kg)
                var imperialUnit = _unitsCalculator.GetImperialUnitsOfMeasurement(metricUnit);

                //now I need to get the value of the property
                var propValue = (double)(prop.GetValue(this.Instance) ?? 0.0);

                //Convert to Metric the value entered from imperial to metric
                var convertedValue = _unitsCalculator.ConvertValueToMetricFromImperial(imperialUnit, propValue, metricUnit);
                return convertedValue;
            }
            Trace.TraceError($"{nameof(PropertyConverter<T>)}.{nameof(GetSystemValueFromBinding)}: unable to convert {prop.Name} value, returning 0.");
            return 0;
        }

        public double GetSystemValueFromBinding(PropertyInfo prop, MeasurementSystemType measurementSystemType)
        {
            // All values are stored internally as metric so we have nothing to convert
            if (measurementSystemType == MeasurementSystemType.Metric)
            {
                return (double)(prop.GetValue(this.Instance) ?? 0.0);
            }

            // Get the units of measurement attribute on the property so we know which conversion to perform
            var attrs = prop.GetCustomAttributes(typeof(UnitsAttribute), false);
            if (this.Instance != null && attrs.Length > 0)
            {
                // We now have the metric unit of the property
                var metricUnit = ((UnitsAttribute)attrs[0]).SourceUnit;

                // Get the complementary imperial unit to convert from (i.e. lbs -> kg)
                var imperialUnit = _unitsCalculator.GetImperialUnitsOfMeasurement(metricUnit);

                // Get the value of the property
                var propValue = (double)(prop.GetValue(this.Instance) ?? 0.0);

                // Convert the value entered from imperial to metric
                var convertedValue = _unitsCalculator.ConvertValueToMetricFromImperial(imperialUnit, propValue, metricUnit);
            
                return convertedValue;
            }
            Trace.TraceError($"{nameof(PropertyConverter<T>)}.{nameof(GetSystemValueFromBinding)}: unable to convert {prop.Name} value, returning 0.");

            return 0;
        }

        #endregion
    }
}
