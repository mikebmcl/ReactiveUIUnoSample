using ReactiveUIUnoSample.Helpers;
using ReactiveUIUnoSample.ViewModels;

using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace ReactiveUIUnoSample.Interfaces
{
    public interface IUnitConversionsTesting
    {
        string NumberOfQuestions { get; set; }
        IList<ValueDisplayGenericPair<Enum>> TestTypes { get; }
        ValueDisplayGenericPair<Enum> SelectedTestType { get; set; }
        TestDifficultyValueDisplayPair SelectedDifficulty { get; set; }

        ICommand RunTestCommand { get; set; }
    }
}
