﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Uno.UITest.Selenium
{
	public static class ConfigureApp
	{
		public static ChromeAppConfigurator WebAssembly =>
			new ChromeAppConfigurator();
	}
}
