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
    [Windows.UI.Xaml.Data.Bindable]
    public class UnitConversionsViewModel : UnitConversionsViewModelBase
    {
        private string m_errorValue = "(Error)";

        [Reactive]
        public object SelectedTemperatureConversion { get; set; }

        [Reactive]
        public string TempEntryOneText { get; set; }

        private readonly ObservableAsPropertyHelper<string> _tempEntryTwoText;
        public string TempEntryTwoText => _tempEntryTwoText.Value;

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
            SelectedTemperatureConversion = ConversionDirections[0];
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
