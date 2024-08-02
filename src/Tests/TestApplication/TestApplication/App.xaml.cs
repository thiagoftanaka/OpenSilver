using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace TestApplication
{
    public sealed partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();

            // Enter construction logic here...

#if OPENSILVER
            var resourceStream = System.Reflection.Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("TestApplication.OpenSilver.ServiceReferences.ClientConfig");
            using (var sr = new StreamReader(resourceStream))
            {
                var sourceCode = sr.ReadToEnd();
                global::OpenSilver.Interop.ExecuteJavaScript(@"window.ServiceReferencesClientConfig = $0", sourceCode);
            }
#endif

            this.RootVisual = new MainPage();
        }
    }
}
