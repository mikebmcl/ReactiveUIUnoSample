using ReactiveUI;

using System;
using System.Globalization;

namespace ReactiveUIUnoSample.Converters
{
    // Rather than IValueConverter, it's better to use IBindingTypeConverter with ReactiveUI since that lets us take advantage
    // of the powerful binding mechanisms that ReactiveUI provides. It also doesn't require instantiating it in the XAML itself; it
    // is just used in the code-behind, which can have some performance benefits. We're using this in FirstViewModel. We could also
    // register it globally and it would then automatically be used for all value conversions for the types it supports. We're just
    // using it locally in FirstViewModel since we might want to use a different converter as the default for decimal <-> string instead
    // of this converter or possibly no converter at all. See: https://www.reactiveui.net/docs/handbook/data-binding/value-converters
    class DecimalToStringBindingTypeConverter : IBindingTypeConverter
    {
        public int GetAffinityForObjects(Type fromType, Type toType)
        {
            if ((fromType == typeof(decimal) && toType == typeof(string)) || fromType == typeof(string) && toType == typeof(decimal))
            {
                // See, e.g., https://github.com/reactiveui/ReactiveUI/blob/main/src/ReactiveUI.Uwp/Common/BooleanToVisibilityTypeConverter.cs
                return 10;
            }
            return 0;
        }

        /// <summary>
        /// Defaults to <see cref="CultureInfo.CurrentUICulture"/>.
        /// </summary>
        public static CultureInfo CultureInfoForConversion { get; set; } = CultureInfo.CurrentUICulture;

        public bool TryConvert(object from, Type toType, object conversionHint, out object result)
        {
            if (from is decimal d && toType == typeof(string))
            {
                result = d.ToString("C", CultureInfoForConversion);
                return true;
            }
            if (from is string s && toType == typeof(decimal))
            {
                // Note: Using CultureInfo.CurrentUICulture will mean that the parse will fail if the user's default culture is, e.g., en-GB or fr-FR, but inputs a value with the $ sign instead of the Pounds sign or the Euro sign.
                // Alternatively, if the user uses a comma as a decimal point, which is what is used in France and many other countries, without any currency symbol you could end up with a really weird value that is completely different from what the user intended.
                // For more info, see:
                // https://docs.microsoft.com/en-us/dotnet/standard/base-types/formatting-types?view=netframework-4.7.2#culture-sensitive-formatting-with-format-providers
                // https://docs.microsoft.com/en-us/dotnet/core/extensions/globalization#numeric-values
                if (decimal.TryParse(s, NumberStyles.Currency, CultureInfoForConversion, out decimal val))
                {
                    result = val;
                    return true;
                }
            }
            result = null;
            return false;
        }
    }
}
