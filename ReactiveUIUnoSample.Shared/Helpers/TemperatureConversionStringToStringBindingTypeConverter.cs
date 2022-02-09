using System;
using System.Collections.Generic;
using System.Text;

using ReactiveUI;

namespace ReactiveUIUnoSample.Helpers
{
    class TemperatureConversionStringToStringBindingTypeConverter : IBindingTypeConverter
    {
        public int GetAffinityForObjects(Type fromType, Type toType)
        {
            return fromType == typeof(string) && toType == typeof(string) ? 10 : 0;
        }

        public bool TryConvert(object from, Type toType, object conversionHint, out object result)
        {
            if (from is string input && toType == typeof(string) && conversionHint is TemperatureConversionDirection conversionDirection)
            {

            }
            result = null;
            return false;
        }
    }
}
