namespace Uno.UITest.Selenium
{
    public static class ConfigureApp
    {
        public static FirefoxAppConfigurator FirefoxWebAssembly =>
            new FirefoxAppConfigurator();

        public static EdgeAppConfigurator EdgeWebAssembly =>
            new EdgeAppConfigurator();

        /// <summary>
        /// Tbis is used to configure WebAssembly for UI testing using Chrome.
        /// </summary>
        public static ChromeAppConfigurator ChromeWebAssembly =>
            new ChromeAppConfigurator();

        /// <summary>
        /// Tbis is used to configure WebAssembly for UI testing using Chrome. Its name does not reflect that it is
        /// only used for Chrome for compatibility reasons. This redirects to <see cref="ChromeWebAssembly"/> so you 
        /// can use either as they both are the same static instance of <see cref="ChromeAppConfigurator"/>.
        /// </summary>
        public static ChromeAppConfigurator WebAssembly => ChromeWebAssembly;
    }
}
