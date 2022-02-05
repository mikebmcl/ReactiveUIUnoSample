using NUnit.Framework;

using System;
using System.IO;

using Uno.UITest;
using Uno.UITest.Selenium;
using Uno.UITests.Helpers;

namespace ReactiveUIUnoSample.UITest
{
    /// <summary>
    /// Use this as the base class for any tests that will be launching the app and using Uno.UITest to perform tests that involve entering data,
    /// checking values after actions are performed, etc. Note that only WASM is supported because of technical limitations.
    /// </summary>
    public class AppTestBase
    {
        private IApp _app;

        static AppTestBase()
        {
            AppInitializer.TestEnvironment.WebAssemblyDefaultUri = Constants.WebAssemblyDefaultUri;
            AppInitializer.TestEnvironment.CurrentPlatform = Constants.CurrentPlatform;
            AppInitializer.TestEnvironment.EdgeDriverPath = Constants.EdgeDriverPath;
            AppInitializer.TestEnvironment.GeckoDriverPath = Constants.GeckoDriverPath;

            // The CurrentPlatform defaults to Platform.Browser which is equivalent to Platform.Chrome. To use Edge or Firefox uncomment the appropriate line below. Only one Platform can be tested at a time so if you set CurrentPlatform multiple times you will get whatever the last value you set was.

            //AppInitializer.TestEnvironment.CurrentPlatform = Uno.UITest.Helpers.Queries.Platform.Edge;
            //AppInitializer.TestEnvironment.CurrentPlatform = Uno.UITest.Helpers.Queries.Platform.Firefox;
#if DEBUG
            // If you want to see what the tests are doing, keep this as false. If you're content to let them run without actually
            // showing a browser window you can just comment it out (true is the default).
            AppInitializer.TestEnvironment.WebAssemblyHeadless = false;
#endif
        }

        protected IApp App
        {
            get => _app;
            private set
            {
                _app = value;
                Uno.UITest.Helpers.Queries.Helpers.App = value;
            }
        }
        [OneTimeSetUp]
        public void SetUpBeforeTestsBegin()
        {
            // DoMotClear is the default if no argument is supplied. It's specified here to let you know you can change it.
            App = AppInitializer.AttachToApp(Uno.UITest.Helpers.AppDataMode.DoNotClear);
        }
        [SetUp]
        public void SetUpTest()
        {
        }

        [TearDown]
        public void TearDownTest()
        {
            TakeScreenshot("teardown");
        }

        [OneTimeTearDown]
        public void TearDownAfterAllTests()
        {
            // Close out the application, making sure that the web driver gets terminated by disposing of it explicitly.
            // Otherwise it might hang around running in the background on your computer.
            App?.Dispose();
            App = null;
        }

        public FileInfo TakeScreenshot(string stepName)
        {
            var title = $"{TestContext.CurrentContext.Test.Name}_{stepName}"
                .Replace(" ", "_")
                .Replace(".", "_");

            var fileInfo = _app.Screenshot(title);

            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileInfo.Name);
            if (fileNameWithoutExt != title)
            {
                var destFileName = Path
                    .Combine(Path.GetDirectoryName(fileInfo.FullName), title + Path.GetExtension(fileInfo.Name));

                if (File.Exists(destFileName))
                {
                    File.Delete(destFileName);
                }

                File.Move(fileInfo.FullName, destFileName);

                TestContext.AddTestAttachment(destFileName, stepName);

                fileInfo = new FileInfo(destFileName);
            }
            else
            {
                TestContext.AddTestAttachment(fileInfo.FullName, stepName);
            }

            return fileInfo;
        }
    }
}
