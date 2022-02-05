using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Uno.UITest.Helpers;
using Uno.UITest.Helpers.Queries;

namespace ReactiveUIUnoSample.UITest
{
    public class Constants
    {
        public static readonly string WebAssemblyDefaultUri = "http://localhost:59036/";

        public static readonly Platform CurrentPlatform = Platform.Browser;

        /// <summary>
        /// Set this to the correct path of the directory that you put msedgedriver.exe in on your system if you want to use <see cref="Platform.Edge"/>.
        /// </summary>
        public static readonly string EdgeDriverPath = @"C:\EdgeDriver";

        /// <summary>
        /// Set this to the correct path of the directory that you put geckodriver.exe in on your system if you want to use <see cref="Platform.Firefox"/>.
        /// </summary>
        public static readonly string GeckoDriverPath = @"C:\GeckoDriver";
    }
}
