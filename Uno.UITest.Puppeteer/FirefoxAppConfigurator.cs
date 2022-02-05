using System;
using System.Collections.Generic;
using System.Text;

namespace Uno.UITest.Selenium
{
    public class FirefoxAppConfigurator
    {
        internal string GeckoDriverPath { get; private set; }
        internal Uri SiteUri { get; private set; }
        internal string InternalScreenShotsPath { get; private set; } = "";
        internal bool InternalHeadless { get; private set; } = true;
        internal int InternalWindowWidth { get; private set; } = 1024;
        internal int InternalWindowHeight { get; private set; } = 768;
        internal string InternalBrowserBinaryPath { get; private set; }
        internal List<string> InternalSeleniumArgument = new List<string>();
        //internal bool InternalDetectDockerEnvironment = true;
        //internal Uri InternalRemoteDriverUri { get; private set; }

        public FirefoxAppConfigurator()
        {
        }

        public FirefoxAppConfigurator Uri(Uri uri) { SiteUri = uri; return this; }

        public FirefoxAppConfigurator GeckoDriverLocation(string edgeDriverPath) { GeckoDriverPath = edgeDriverPath; return this; }

        public FirefoxAppConfigurator ScreenShotsPath(string path) { InternalScreenShotsPath = path; return this; }

        public FirefoxAppConfigurator BrowserBinaryPath(string path) { InternalBrowserBinaryPath = path; return this; }

        /// <summary>
        /// This parameters allows to provide a set of additional parameters to be provided to the WebDriver.
        /// </summary>
        public FirefoxAppConfigurator SeleniumArgument(string argument) { InternalSeleniumArgument.Add(argument); return this; }

        // Disabled until I find time to test it.
        ///// <summary>
        ///// Enables the detection of the docker environment to configure the WebDriver accordingly. Enabled by default.
        ///// </summary>
        //public FirefoxAppConfigurator DetectDockerEnvironment(bool enabled) { InternalDetectDockerEnvironment = enabled; return this; }

        /// <summary>
        /// Runs the browser as headless. Defaults to true.
        /// </summary>
        /// <param name="isHeadless"></param>
        /// <returns></returns>
        public FirefoxAppConfigurator Headless(bool isHeadless) { InternalHeadless = isHeadless; return this; }

        ///// <summary>
        ///// If this is not null, the app will be configured to use a RemoteWebDriver to connect to at the supplied Uri.
        ///// </summary>
        ///// <param name="isRemote"></param>
        ///// <returns></returns>
        //public FirefoxAppConfigurator RemoteDriverUri(Uri remoteDriverUri) { InternalRemoteDriverUri = remoteDriverUri; return this; }

        /// <summary>
        /// Sets the window size. Defaults to 1024x768
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public FirefoxAppConfigurator WindowSize(int width, int height)
        {
            InternalWindowWidth = width;
            InternalWindowHeight = height;
            return this;
        }

        public IApp StartApp() => new SeleniumApp(this);

    }
}
