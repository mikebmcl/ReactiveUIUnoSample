using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Reflection;
using System.Windows.Input;

using Windows.System;

namespace ReactiveUIUnoSample.ViewModels
{
    public class AboutViewModel : DisplayViewModelBase
    {
        public AboutViewModel(IScreenWithContract hostScreen, ISchedulerProvider schedulerProvider, string urlPathSegment = null, bool useNullUrlPathSegment = false) : base(hostScreen, schedulerProvider, urlPathSegment, useNullUrlPathSegment)
        {
            // Note: For UWP, Skia.Gtk, and Skia.Tizen these properties should be extracted from the App Manifest or hard-coded here.
            HeaderContent = "About";
            var assembly = Assembly.GetExecutingAssembly();
            AssemblyProductAttribute productAttribute = (AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute));
            AppProductNameText = productAttribute?.Product ?? assembly.GetName()?.Name ?? "ReactiveUI with Uno";
            AssemblyCopyrightAttribute copyrightAttribute = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute));
            AppCopyrightText = copyrightAttribute?.Copyright ?? "";
            AssemblyVersionAttribute versionAttribute = (AssemblyVersionAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyVersionAttribute));
            var appVersionText = versionAttribute?.Version;
            if (appVersionText == null)
            {
                var version = assembly.GetName()?.Version;
                if (version != null)
                {
                    if (version.Major < 0)
                    {
                        appVersionText = "";
                    }
                    else
                    {
                        appVersionText = $"Version {version.Major}";
                        if (version.Minor >= 0)
                        {
                            appVersionText += $".{version.Minor}";
                            if (version.Build >= 0)
                            {
                                appVersionText += $".{version.Build}";
                                if (version.Revision >= 0)
                                {
                                    appVersionText += $"{version.Revision}";
                                }
                            }
                        }
                    }
                }
                else
                {
                    appVersionText = "";
                }
            }
            else
            {
                appVersionText = "Version " + appVersionText;
            }
            //}?? assembly.GetName()?.Version?.ToString(3) ?? "";
            AppVersionText = appVersionText;
            ViewSILLicensePageCommand = ReactiveCommand.CreateFromObservable(() => HostScreenWithContract.Router.Navigate.Execute(new SILOpenFontLicense1_1ViewModel(HostScreenWithContract, SchedulerProvider)));
        }

        [Reactive]
        public string AppProductNameText { get; set; }

        [Reactive]
        public string AppCopyrightText { get; set; }

        [Reactive]
        public string AppVersionText { get; set; }

        public override object HeaderContent { get; set; }

        public ICommand ViewSILLicensePageCommand { get; set; }

        public string NotoSILLicenseLinkAddress => "https://scripts.sil.org/OFL";

        public string NotoCopyrightStatementLinkAddress => "http://www.google.com/get/noto";
    }
}
