using ReactiveUIUnoSample.Helpers;

using System;
using System.Collections.Generic;
using System.Text;

namespace ReactiveUIUnoSample.ViewModels.UnitConversions
{
    public class TemperatureConversionDirectionValueDisplayPair : ValueDisplayGenericPair<TemperatureConversionDirection>
    {
        public TemperatureConversionDirectionValueDisplayPair(TemperatureConversionDirection conversionDirection, string display) : base(conversionDirection, display)
        { }
    }
}
