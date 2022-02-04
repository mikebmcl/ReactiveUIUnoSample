﻿using Uno.UITest.Helpers.Queries;

namespace Uno.UITests.Helpers
{
    /// <summary>
    /// Environment default values for <see cref="AppInitializer"/>
    /// </summary>
    public class AppInitializerEnvironment
    {
        internal AppInitializerEnvironment()
        {
        }

        /// <summary>
        /// Defines the iOS Device name or ID. Default value for <see cref="AppInitializer.UITEST_IOSDEVICE_ID"/>
        /// </summary>
        public string iOSDeviceNameOrId { get; set; }

        /// <summary>
        /// Defines the Application bundle ID to use. Default value for <see cref="AppInitializer.UITEST_IOSBUNDLE_PATH"/>
        /// </summary>
        public string iOSAppName { get; set; }

        /// <summary>
        ///Defines the Uri to use for WebAssembly tests
        /// </summary>
        public string WebAssemblyDefaultUri { get; set; }

        /// <summary>
        /// Defines the current tested platform. Defaut value for <see cref="AppInitializer.UNO_UITEST_PLATFORM"/>
        /// </summary>
        public Platform CurrentPlatform { get; set; }

        /// <summary>
        /// Defines the currently tested app name. Default value when <see cref="AppInitializer.UITEST_ANDROIDAPK_PATH"/> is not set.
        /// </summary>
        public string AndroidAppName { get; set; }

        /// <summary>
        /// Defines the location of chrome driver.
        /// </summary>
        /// <remarks>
        /// If not defined, the test engine will select the version based on
        /// the currently installed Chrome version.
        /// </remarks>
        public string ChromeDriverPath { get; set; }

        /// <summary>
        /// Defines if the browser tests are running in chrome without a window.
        /// </summary>
        public bool WebAssemblyHeadless { get; set; } = true;

        ///// <summary>
        ///// If this is not null, a RemoteWebDriver using the server specified by this Uri will be used instead of a local ChromeDriver using the <see cref="WebAssemblyDefaultUri"/>.
        ///// </summary>
        //public System.Uri WebAssemblyRemoteWebDriverUri { get; set; }
    }
}
