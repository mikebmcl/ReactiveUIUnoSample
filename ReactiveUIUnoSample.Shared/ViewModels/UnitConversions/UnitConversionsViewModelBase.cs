﻿using ReactiveUIUnoSample.Helpers;

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

using Uno.Extensions;
using ReactiveUIUnoSample.Interfaces.Testing;
using ReactiveUI.Fody.Helpers;

namespace ReactiveUIUnoSample.ViewModels.UnitConversions
{
    [Windows.UI.Xaml.Data.Bindable]
    public abstract class UnitConversionsViewModelBase : DisplayViewModelBase
    {
        protected UnitConversionsViewModelBase(IScreenWithContract hostScreen, ISchedulerProvider schedulerProvider, string urlPathSegment = null, bool useNullUrlPathSegment = false) : base(hostScreen, schedulerProvider, urlPathSegment, useNullUrlPathSegment)
        {
            //TemperaturePickerItems = m_temperaturePickerItems;
        }

        private static readonly List<TestDifficultyValueDisplayPair> m_testDifficulties = new List<TestDifficultyValueDisplayPair>(new TestDifficultyValueDisplayPair[] { new TestDifficultyValueDisplayPair(TestDifficulty.Easy, "Easy"), new TestDifficultyValueDisplayPair(TestDifficulty.Medium, "Medium"), new TestDifficultyValueDisplayPair(TestDifficulty.Hard, "Hard") });
        public List<TestDifficultyValueDisplayPair> TestDifficulties => m_testDifficulties;

        //// Temperature
        //public static TemperatureConversionDirection TemperatureStringToTemperatureConversionDirection(string value, [System.Runtime.CompilerServices.CallerMemberName] string callerMemberName = null, [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = null, [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
        //{
        //    switch (value)
        //    {
        //        case m_celsiusToFahrenheit:
        //            return TemperatureConversionDirection.CelsiusToFahrenheit;
        //        case m_fahrenheitToCelsius:
        //            return TemperatureConversionDirection.FahrenheitToCelsius;
        //        default:
        //            DiagnosticsHelpers.ReportProblem($"Unexpected string value {value ?? "(null)"}.", LogLevel.Error, null, callerMemberName: callerMemberName, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber);
        //            return TemperatureConversionDirection.Invalid;
        //    }
        //}
        protected const string m_fahrenheitToCelsius = "F to C";
        protected const string m_celsiusToFahrenheit = "C to F";
        //protected List<string> m_temperaturePickerItems = new List<string> { m_celsiusToFahrenheit, m_fahrenheitToCelsius };
        //[Reactive]
        //public List<string> TemperaturePickerItems { get; set; }

        public static double GetUniqueOffset(Random random, int adjustedDifferencePlusOne, HashSet<int> existingQuestionOffsets, double testValueOffsetFromMinimum)
        {
            // We want to prevent duplicates since the number of possible values to test is greater than the number of questions. We do this before adjusting for half degree values (if we are doing half degrees) since those half degree values are part of the possible values.
            if (!existingQuestionOffsets.Add((int)testValueOffsetFromMinimum))
            {
                // If there isn't a large difference between possible values and number of questions, then we could run into issues with it bogging down trying to get a value from random that isn't already in the HashSet, especially towards the end of the question generation. So all of this code that follows exists to cut that off by capping the number of calls to random and if we hit the cap then incrementally checking values in the HashSet until we find an unused one.
                const int infiniteLoopPreventionMax = 100;
                bool foundValue = false;
                for (int infiniteLoopPreventionCounter = 0; infiniteLoopPreventionCounter < infiniteLoopPreventionMax; infiniteLoopPreventionCounter++)
                {
                    testValueOffsetFromMinimum = random.Next(adjustedDifferencePlusOne);
                    if (existingQuestionOffsets.Add((int)testValueOffsetFromMinimum))
                    {
                        foundValue = true;
                        break;
                    }
                    infiniteLoopPreventionCounter++;
                }
                if (!foundValue)
                {
                    double fallback = testValueOffsetFromMinimum;
                    for (int checkIfNotUsed = 0; checkIfNotUsed < adjustedDifferencePlusOne; checkIfNotUsed++)
                    {
                        if (existingQuestionOffsets.Add(checkIfNotUsed))
                        {
                            foundValue = true;
                            testValueOffsetFromMinimum = checkIfNotUsed;
                            break;
                        }
                    }
                    if (!foundValue)
                    {
                        // We should never get here because there should be more possible values than number of questions so we should've found a possible value.
                        testValueOffsetFromMinimum = fallback;
                    }
                }
            }

            return testValueOffsetFromMinimum;
        }
    }
}
