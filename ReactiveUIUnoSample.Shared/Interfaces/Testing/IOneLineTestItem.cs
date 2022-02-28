using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

using Windows.UI.Xaml;

namespace ReactiveUIUnoSample.Interfaces.Testing
{
    /// <summary>
    /// Provides an abstracted representation of all data needed for a test item that can be used to populate a <see cref="IOneLineTest"/> in order to use <see cref="VocabTestContentPage"/>.
    /// </summary>
    public interface IOneLineTestItem : INotifyPropertyChanged
    {
        /// <summary>
        /// Needs to raise that it's been changed when it changes value.
        /// </summary>
        IThreeStateTestAnswer SelectedItem { get; set; }
        string FirstLine { get; set; }
        string SecondLine { get; set; }

        IList<IThreeStateTestAnswer> Answers { get; set; }
        IThreeStateTestAnswer CorrectAnswer { get; set; }
        string UserAnswer();
        bool UserAnswerIsCorrect();
        FrameworkElement CorrectAnswerFrameworkElement { get; }
        bool HasCorrectAnswerFrameworkElement { get; }
        bool NoCorrectAnswerFrameworkElement { get; }
        ICommand ViewCorrectAnswerFrameworkElementCommand { get; set; }
    }
}
