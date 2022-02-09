using ReactiveUIUnoSample.Helpers;

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

using Uno.Extensions;
using ReactiveUIUnoSample.Interfaces;
using ReactiveUIUnoSample.ViewModels.Testing;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;

namespace ReactiveUIUnoSample.ViewModels.UnitConversions
{
    /*
     * Conversions to perform:
     * Study and do quizzes on unit conversions: F -> C, cm -> in, mi -> km, lb -> kg, ft -> m, gal -> L, oz -> ml,
     * shoe sizes (very difficult it seems), numbers by thousands -> numbers by 万, liquid measures (teaspoons, cups, fluid ounces), dry measures,
     * cooking measures, pressures (? *** names for all of these units *** - have quizzes work on close approximations where
     * appropriate (50km = 31.07mi so 31 or even 30) is acceptable for quick mental conversion. Teach tricks for quick mental
     * conversions by using one or more conversions that are close to whole numbers so you can extrapolate with simple mental
     * math, e.g.:
     * 50km ~= 30mi and 80km ~= 50mi so 20 mi ~= 65km,
     * 70km ~= 30mi + (2/3 of 30mi) ~= (70km * 2 / 3) ~= (140km / 3) ~= (90km / 3) + (30km / 3) + (20km / 3) ~= 30mi + 10mi + 7mi ~= 46-47mi
     */

    [Windows.UI.Xaml.Data.Bindable]
    public class UnitConversionsViewModel : UnitConversionsViewModelBase
    {
        //public const string TemperatureConversionsMainViewContract = "TemperatureConversionsMainView";
        private string m_errorValue = "(Error)";

        [Reactive]
        public object SelectedTemperatureConversion { get; set; }

        public IList<ValueDisplayGenericPair<TemperatureConversionDirection>> TemperatureConversions => new List<ValueDisplayGenericPair<TemperatureConversionDirection>>(new ValueDisplayGenericPair<TemperatureConversionDirection>[]
        {
            new ValueDisplayGenericPair<TemperatureConversionDirection>(TemperatureConversionDirection.CelsiusToFahrenheit, m_celsiusToFahrenheit)
            , new ValueDisplayGenericPair<TemperatureConversionDirection>(TemperatureConversionDirection.FahrenheitToCelsius, m_fahrenheitToCelsius)
        });

        // Temperature
        // Note: Initialize m_tempPickerItems and populate it with values (typically in the format "A -> B") in the ctor
        //private void UpdateTempEntryTextTwo(string value, ValueDisplayGenericPair<TemperatureConversionDirection> direction)
        //{
        //    string strToConvert = m_tempEntryOneText;
        //    if (string.IsNullOrWhiteSpace(strToConvert))
        //    {
        //        TempEntryTwoText = "";
        //        return;
        //    }
        //    TempEntryTwoText = "";
        //    double convertedValue;
        //    string tempPickerSelectedItem = m_tempPickerSelectedItem;
        //    if (double.TryParse(m_tempEntryOneText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double valueToConvert))
        //    {
        //        switch (tempPickerSelectedItem)
        //        {
        //            case m_fahrenheitToCelsius:
        //                convertedValue = MiscHelpers.ConvertTemperature(TemperatureConversionDirection.FahrenheitToCelsius, valueToConvert);
        //                break;
        //            case m_celsiusToFahrenheit:
        //                convertedValue = MiscHelpers.ConvertTemperature(TemperatureConversionDirection.CelsiusToFahrenheit, valueToConvert);
        //                break;
        //            case null:
        //                TempEntryTwoText = "";
        //                return;
        //            default:
        //                DiagnosticsHelpers.ReportProblem($"Unknown temperature picker value '{tempPickerSelectedItem ?? "(null)"}'", LogLevel.Error, null);
        //                TempEntryTwoText = m_errorValue;
        //                return;
        //        }
        //        if (convertedValue == double.PositiveInfinity)
        //        {
        //            TempEntryTwoText = m_errorValue;
        //            return;
        //        }
        //        TempEntryTwoText = convertedValue.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
        //    }
        //    else
        //    {
        //        TempEntryTwoText = m_errorValue;
        //        return;
        //    }
        //}

        //private string m_tempEntryOneText = "0";

        [Reactive]
        public string TempEntryOneText { get; set; }
        //{
        //    get => m_tempEntryOneText;
        //    set
        //    {
        //        if (m_tempEntryOneText != value)
        //        {
        //            m_tempEntryOneText = value;
        //            RaisePropertyChanged();
        //            UpdateTempEntryTextTwo();
        //        }
        //    }

        private readonly ObservableAsPropertyHelper<string> _tempEntryTwoText;
        public string TempEntryTwoText => _tempEntryTwoText.Value;

        //[Reactive]
        //public string TempEntryTwoText { get; set; }

        //private string m_tempPickerTitle = "Temperature";
        [Reactive]
        public string TempPickerTitle { get; set; }
        //{
        //    get => m_tempPickerTitle;
        //    set
        //    {
        //        if (m_tempPickerTitle != value)
        //        {
        //            m_tempPickerTitle = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        //private string m_tempPickerSelectedItem;
        //public string TempPickerSelectedItem
        //{
        //    get => m_tempPickerSelectedItem;
        //    set
        //    {
        //        if (m_tempPickerSelectedItem != value)
        //        {
        //            m_tempPickerSelectedItem = value;
        //            UpdateTempEntryTextTwo();
        //        }
        //    }
        //}

        private IUnitConversionsTesting m_temperatureTestingVM;
        public IUnitConversionsTesting TemperatureTestingVM
        {
            get => m_temperatureTestingVM;
            set
            {
                if (m_temperatureTestingVM != value)
                {
                    m_temperatureTestingVM = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string m_title = "Temperature Conversions!";
        public string Title
        {
            get => m_title; set { if (m_title != value) { m_title = value; RaisePropertyChanged(); } }
        }

        public override object HeaderContent { get; set; } = "Temperature Conversions";

        public UnitConversionsViewModel(IScreenWithContract hostScreen, ISchedulerProvider schedulerProvider, string urlPathSegment = null, bool useNullUrlPathSegment = false) : base(hostScreen, schedulerProvider, urlPathSegment, useNullUrlPathSegment)
        {
            TempEntryOneText = "0";
            TempPickerTitle = "Temperature";
            SelectedTemperatureConversion = TemperatureConversions[0];
            //m_temperaturePickerItems = new List<string> { m_fahrenheitToCelsius, m_celsiusToFahrenheit };
            m_temperatureTestingVM = new TemperatureConversionsTestingViewModel(hostScreen, schedulerProvider, urlPathSegment, useNullUrlPathSegment);
            this.WhenAnyValue(x => x.TempEntryOneText, x => x.SelectedTemperatureConversion, (value, directionAsObj) =>
            {
                string strToConvert = value;
                if (string.IsNullOrWhiteSpace(strToConvert) || directionAsObj == null || !(directionAsObj is ValueDisplayGenericPair<TemperatureConversionDirection> direction))
                {
                    return "";
                }
                double convertedValue;
                if (double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double valueToConvert))
                {
                    switch (direction.Value)
                    {
                        case TemperatureConversionDirection.CelsiusToFahrenheit:
                            convertedValue = MiscHelpers.ConvertTemperature(TemperatureConversionDirection.CelsiusToFahrenheit, valueToConvert);
                            break;
                        case TemperatureConversionDirection.FahrenheitToCelsius:
                            convertedValue = MiscHelpers.ConvertTemperature(TemperatureConversionDirection.FahrenheitToCelsius, valueToConvert);
                            break;
                        case TemperatureConversionDirection.Invalid:
                            return "";
                        default:
                            DiagnosticsHelpers.ReportProblem($"Unknown temperature picker enumerator value '{direction.Value}'", LogLevel.Error, null);
                            return m_errorValue;
                    }
                    //switch (direction.Value)
                    //{
                    //    case m_fahrenheitToCelsius:
                    //        convertedValue = MiscHelpers.ConvertTemperature(TemperatureConversionDirection.FahrenheitToCelsius, valueToConvert);
                    //        break;
                    //    case m_celsiusToFahrenheit:
                    //        convertedValue = MiscHelpers.ConvertTemperature(TemperatureConversionDirection.CelsiusToFahrenheit, valueToConvert);
                    //        break;
                    //    case null:
                    //        return "";
                    //    default:
                    //        DiagnosticsHelpers.ReportProblem($"Unknown temperature picker value '{tempPickerSelectedItem ?? "(null)"}'", LogLevel.Error, null);
                    //        return m_errorValue;
                    //}
                    if (convertedValue == double.PositiveInfinity)
                    {
                        return m_errorValue;
                    }
                    return convertedValue.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    return m_errorValue;
                }

            }).ToProperty(this, x => x.TempEntryTwoText, out _tempEntryTwoText);
        }
    }
}
