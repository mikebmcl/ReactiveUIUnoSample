//using FluentAssertions.Common;
//using NUnit.Framework;

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using Uno.UITest.Helpers;
//using Uno.UITests.Helpers;
//using Uno.UITest.Helpers.Queries;
//using ReactiveUI.Testing;


//using Query = System.Func<Uno.UITest.IAppQuery, Uno.UITest.IAppQuery>;
//using FluentAssertions;
//using FluentAssertions.Execution;
//using Uno.UITest;
//using Windows.UI.Xaml;
//using Windows.UI.Xaml.Controls;
//using ReactiveUIUnoSample.ViewModels;

//namespace ReactiveUIUnoSample.UITest
//{
//    public class Given_FirstView : AppTestBase
//    {
//        // Note: Some testing tools will show you the results using the method name while others prefer a Description, so the Description should be set and both
//        // should be descriptive of what the test is testing and the expected result. We're following the Given-When-Then test pattern
//        // (see: https://martinfowler.com/bliki/GivenWhenThen.html ) and because this is UI testing, the Given should normally be the name of the view we are
//        // going to be testing.
//        [Test(Description = "When the app is queried for elements marked EnteredAmountTextBox then there should be exactly one found.")]
//        public void WhenQueriedForElementsNamedEnteredAmountTextBox_ThenThereShouldBeExactlyOneFound()
//        {
//            var app = App;
//            if (app == null)
//            {
//                AppInitializer.TestEnvironment.CurrentPlatform = Platform.Browser;
//                app = AppInitializer.AttachToApp(AppDataMode.Clear);
//            }
//            else
//            {
//                App.RefreshBrowser();
//            }
//            Query enteredAmountTextBox = q => q.All().Marked(FirstViewModel.EnteredAmountTextBoxAutomationId);
//            //using var _ = new AssertionScope();
//            var matchedElementCount = app.WaitForElement(enteredAmountTextBox).Length;
//            Assert.IsTrue(matchedElementCount == 1);
//        }

//        [Test(Description = "When the app is loaded then the value of EnteredAmountTextBox should contain the currency symbol of the converter that is should be used.")]
//        public void WhenLoaded_ThenEnteredAmountShouldBeContainTheExpectedCurrencySymbol()
//        {
//            var app = App;
//            if (app == null)
//            {
//                AppInitializer.TestEnvironment.CurrentPlatform = Platform.Browser;
//                app = AppInitializer.AttachToApp(AppDataMode.Clear);
//            }
//            else
//            {
//                App.RefreshBrowser();
//            }
//            Query enteredAmountTextBox = q => q.All().Marked(FirstViewModel.EnteredAmountTextBoxAutomationId);
//            app.WaitForElement(enteredAmountTextBox);
//            using var _ = new AssertionScope();
//            string result = app.Query(q => enteredAmountTextBox(q).GetDependencyPropertyValue(nameof(TextBox.Text)).Value<string>()).FirstOrDefault();
//            result.Should().Contain(Converters.DecimalToStringBindingTypeConverter.CultureInfoForConversion.NumberFormat.CurrencySymbol, "because the initial value should contain the currency symbol");
//        }

//        [Test(Description = "When valid pure numeric value then successful formatting.")]
//        public void WhenEnteredAmountTextBoxGetsValidPureNumericValue_ThenSuccessfulFormatting()
//        {
//            var app = App;
//            if (app == null)
//            {
//                AppInitializer.TestEnvironment.CurrentPlatform = Platform.Browser;
//                app = AppInitializer.AttachToApp(AppDataMode.Clear);
//            }
//            else
//            {
//                App.RefreshBrowser();
//            }
//            Query enteredAmountTextBox = q => q.All().Marked(FirstViewModel.EnteredAmountTextBoxAutomationId);
//            app.WaitForElement(enteredAmountTextBox);
//            decimal value = 100000.32M;
//            app.Query(q => enteredAmountTextBox(q).SetDependencyPropertyValue(nameof(TextBox.Text), value.ToString()));
//            // This switches focus away from the text box so that the value conversion that is hooked up to it will run.
//            app.EnterText(enteredAmountTextBox, OpenQA.Selenium.Keys.Tab);

//            app.Wait(1);
//            // Take a screenshot and add it to the test results
//            TakeScreenshot("successfulformatting");
//            using var _ = new AssertionScope();
//            string result = app.Query(q => enteredAmountTextBox(q).GetDependencyPropertyValue(nameof(TextBox.Text)).Value<string>()).FirstOrDefault();
//            result.Should().NotBeNullOrWhiteSpace($"because {FirstViewModel.EnteredAmountTextBoxAutomationId} should not be null or empty after setting its value and switching focus away from it");
//            var converter = new Converters.DecimalToStringBindingTypeConverter();
//            if (!converter.TryConvert(value, typeof(string), null, out object expected) || !(expected is string))
//            {
//                Assert.Fail($"value {value} should have converted successfully to a string.");
//            }
//            else
//            {
//                result.Should().Be(expected as string, $"because of conversion using {nameof(Converters.DecimalToStringBindingTypeConverter)}");
//            }
//        }
//    }
//}
