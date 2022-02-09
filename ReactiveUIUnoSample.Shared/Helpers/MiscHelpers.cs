using System;

using Uno.Extensions;
using Microsoft.Extensions.Logging;

namespace ReactiveUIUnoSample.Helpers
{
    public enum TemperatureConversionDirection
    {
        Invalid,
        FahrenheitToCelsius,
        CelsiusToFahrenheit
    }

    public enum RoundedToResultType
    {
        None,
        Half,
        One
    }

    public static class MiscHelpers
    {
        public static double ConvertTemperature(TemperatureConversionDirection conversionDirection, double valueToConvert, MidpointRounding midpointRounding = MidpointRounding.AwayFromZero)
        {
            switch (conversionDirection)
            {
                case TemperatureConversionDirection.FahrenheitToCelsius:
                    return Math.Round((valueToConvert - 32) * 5 / 9, 10, MidpointRounding.AwayFromZero);
                case TemperatureConversionDirection.CelsiusToFahrenheit:
                    return Math.Round((valueToConvert * 9 / 5) + 32, 10, MidpointRounding.AwayFromZero);
                default:
                    DiagnosticsHelpers.ReportProblem($"Unknown {nameof(TemperatureConversionDirection)} enumerator value '{conversionDirection}'", LogLevel.Error, null);
                    return double.PositiveInfinity;
            }
        }

        public static (double Result, RoundedToResultType ResultType) RoundToNearestHalf(double value)
        {
            const double twoThirds = 2.0 / 3.0;
            const double oneThird = 1.0 / 3.0;
            double truncatedValue = Math.Truncate(value);
            double absRemainderValue = Math.Abs(value - truncatedValue);
            if (absRemainderValue > twoThirds)
            {
                return (value > 0 ? truncatedValue + 1.0 : truncatedValue - 1.0, RoundedToResultType.One);
            }
            else
            {
                if (absRemainderValue > oneThird)
                {
                    return (value > 0 ? truncatedValue + 0.5 : truncatedValue - 0.5, RoundedToResultType.Half);
                }
                else
                {
                    return (truncatedValue, RoundedToResultType.None);
                }
            }
        }
    }
}
