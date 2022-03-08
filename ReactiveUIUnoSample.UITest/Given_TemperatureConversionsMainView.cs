using FluentAssertions.Common;
using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Uno.UITest.Helpers;
using Uno.UITests.Helpers;
using Uno.UITest.Helpers.Queries;
using ReactiveUI.Testing;


using Query = System.Func<Uno.UITest.IAppQuery, Uno.UITest.IAppQuery>;
using FluentAssertions;
using FluentAssertions.Execution;
using Uno.UITest;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using ReactiveUIUnoSample.ViewModels;
using ReactiveUIUnoSample.ViewModels.Testing;
using ReactiveUIUnoSample.Views;
using ReactiveUIUnoSample.ViewModels.UnitConversions;

namespace ReactiveUIUnoSample.UITest
{
    public class Given_TemperatureConversionsMainView : AppTestBase
    {
        [SetUp]
        public void SetUp()
        {
            App.RefreshBrowser();
        }

        [TestCase(Description = "When Loaded, Then there should be exactly one control with AutomationId TempInputAutomationId")]
        public void WhenLoaded_ThenThereShouldBeExactlyOneControlWithAutomationIdTempInputAutomationId()
        {
            Query tempInputControl = q => q.All().Marked(TemperatureConversionsMainView.TempInputAutomationId);
            var matchedElementCount = App.WaitForElement(tempInputControl).Length;
            Assert.That(matchedElementCount, Is.EqualTo(1));
        }

        [TestCase(Description = "When Loaded, Then there should be exactly one control with AutomationId TempConversionResultAutomationId"
            // To change the base name that will be used for screenshot names from the method name to something you choose explicitly, set TestName = "..." such as below
            , TestName = "VerifyOnlyOneTempConversionResultAutomationIdControl"
            )]
        public void WhenLoaded_ThenThereShouldBeExactlyOneControlWithAutomationIdTempConversionResultAutomationId()
        {
            Query someControl = q => q.All().Marked(TemperatureConversionsMainView.TempConversionResultAutomationId);
            var matchedElementCount = App.WaitForElement(someControl).Length;
            Assert.That(matchedElementCount, Is.EqualTo(1));
        }

        [TestCase(Description = "When A TestType has been chosen, Then the selected TestType should not be empty.")]
        public void WhenATestTypeHasBeenChosen_ThenTheSelectedTestTypeShouldNotBeEmpty()
        {
            var comboBoxAutomationId = TemperatureConversionsMainView.TempTestTypeAutomationId;
            Query testType = q => q.Marked(comboBoxAutomationId);
            var matchedElements = App.WaitForElement(testType);
            Assert.That(matchedElements.Length, Is.EqualTo(1));
            // This call to App.Wait for 1 second seems to give enough time for everything to finish loading such that Tap will succeed on the first try. This is more than a bit hackish though; perhaps there's some better way to ensure that the browser is finished loading and is ready for input?
            App.Wait(1);
            try
            {
                App.Tap(testType);
                matchedElements = App.WaitForElement(q => q.Class(nameof(ComboBoxItem)), timeoutMessage: "Timed out waiting for ComboBoxItems to be loaded.", timeout: TimeSpan.FromSeconds(15));
            }
            catch (TimeoutException)
            {
                TestContext.WriteLine($"Retrying Tap on '{comboBoxAutomationId}' because search for its ComboBoxItems timed out...");
                // Try a second time because sometimes the first tap doesn't register
                App.Tap(testType);
                matchedElements = App.WaitForElement(q => q.Class(nameof(ComboBoxItem)), timeoutMessage: "Timed out waiting for ComboBoxItems to be loaded.", timeout: TimeSpan.FromSeconds(15));
            }

            Assert.That(matchedElements.Length, Is.GreaterThan(0));

            // Arbitrarily choose the first result as the one we will tap after writing out some diagnostic data because we know there is at least 1 item.
            Query firstTestType = q => q.Id(matchedElements[0].Id);

            foreach (var comboBoxItem in matchedElements)
            {
                string testTypeDisplay = App.Query(q => q.Id(comboBoxItem.Id).GetDependencyPropertyValue(nameof(ComboBoxItem.Content)).Value<string>()).FirstOrDefault();
                Assert.That(testTypeDisplay, Is.Not.Empty);
                TestContext.Out.WriteLine($"For ComboBox with AutomationId '{comboBoxAutomationId}' found ComboBoxItem with Content '{testTypeDisplay}'.");
            }

            App.Tap(firstTestType);
            // In case it needs some time after the tap to set the value.
            App.Wait(1);

            string result = App.Query(q => testType(q).GetDependencyPropertyValue(nameof(ComboBox.SelectedItem)).Value<string>()).FirstOrDefault();
            // We aren't concerned
            Assert.That(result, Is.Not.Empty);
            TestContext.Out.WriteLine($"Selected test type '{result}'.");
        }

        //[TestCase(Description = "When Navigate to Temperature Test, Then the view should change to a OneLineTestView")]
        //public void WhenNavigateToTemperatureTest_ThenTheViewShouldChangeToAOneLineTestView()
        //{
        //}
    }
}
