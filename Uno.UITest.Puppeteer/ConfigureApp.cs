using System;
using System.Collections.Generic;
using System.Text;

namespace Uno.UITest.Selenium
{
    public static class ConfigureApp
    {
        public static EdgeAppConfigurator EdgeWebAssembly =>
            new EdgeAppConfigurator();

        public static ChromeAppConfigurator WebAssembly =>
            new ChromeAppConfigurator();
    }
}
