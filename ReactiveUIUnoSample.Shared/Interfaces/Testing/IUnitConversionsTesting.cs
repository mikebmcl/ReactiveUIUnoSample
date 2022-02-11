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
        /// <summary>
        /// This is expected to be an object of the type that is held in <see cref="TestTypes"/>. It's given here as <see cref="object"/> because
        /// ReactiveUI will try (and fail) to do type conversion using its default <see cref="ReactiveUI.IBindingTypeConverter"/>:
        /// <see cref="ReactiveUI.EqualityTypeConverter"/>. It fails because this needs to be a two-way binding since it's set by the UI and so it needs to convert
        /// from <see cref="object"/> to the type. However this is likely to be null initially because no items is selected and you can't programmatically determine
        /// the type of a null value. The alternative would be to create and always use a binding type converter that would handle this conversion when creating the
        /// bindings but that seems like over kill since this is, in fact, meant to be bound to a SelectedItem in a ComboBox or something like it, and the type for
        /// those is <see cref="object"/>.
        /// </summary>
        object SelectedTestType { get; set; }

        /// <summary>
        /// Expected to be of type <see cref="TestDifficultyValueDisplayPair"/> and is of type object for the same reasons that <see cref="SelectedTestType"/> is.
        /// </summary>
        object SelectedDifficulty { get; set; }
        ICommand RunTestCommand { get; set; }
    }
}
