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
        public readonly static string WebAssemblyDefaultUri = "http://localhost:59036/";

        public readonly static Platform CurrentPlatform = Platform.Browser;

        /// <summary>
        /// Set this to the correct path of the directory that you put msedgedriver.exe in your system if you intend to use <see cref="Platform.Edge"/>.
        /// </summary>
        public readonly static string EdgeDriverPath = @"C:\EdgeDriver";
    }
}
