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

namespace ReactiveUIUnoSample.UITest
{
    public class Given_TemperatureConversionsMainView : AppTestBase
    {
        [TestCase(Description = "When Loaded, Then there should be exactly one control with AutomationId TempInputAutomationId")]
        public void WhenLoaded_ThenThereShouldBeExactlyOneControlWithAutomationIdTempInputAutomationId()
        {
            App.RefreshBrowser();
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
            App.RefreshBrowser();
            Query someControl = q => q.All().Marked(TemperatureConversionsMainView.TempConversionResultAutomationId);
            var matchedElementCount = App.WaitForElement(someControl).Length;
            Assert.That(matchedElementCount, Is.EqualTo(1));
        }

    }
}
