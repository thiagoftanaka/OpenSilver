using System;
using System.Windows;
using System.Windows.Controls;
using TestApplication.LegacyBasicHttpServiceReference;
using TestApplication.LegacyBinaryServiceReference;
using TestApplication.OpenSilver.Tests.ServiceReference;
#if OPENSILVER
using BasicHttpServiceReference;
using BinaryServiceReference;
#endif

namespace TestApplication.Tests
{
    public partial class ServiceReferenceTest : Page
    {
        public ServiceReferenceTest()
        {
            this.InitializeComponent();
        }

        private void LegacyBasicHttpGetTestStringButton_OnClick(object sender, RoutedEventArgs e)
        {
            LegacyBasicHttpServiceReference.BasicHttpServiceClient basicHttpServiceClient =
                new LegacyBasicHttpServiceReference.BasicHttpServiceClient();
            basicHttpServiceClient.GetTestStringCompleted +=
                (_, ee) =>
                    LegacyBasicHttpGetTestStringResultTextBlock.Text = ee.Error?.Message ?? ee.Result;
            basicHttpServiceClient.GetTestStringAsync();
        }

#if OPENSILVER
        private async void BasicHttpGetTestStringButton_OnClick(object sender, RoutedEventArgs e)
#else
        private void BasicHttpGetTestStringButton_OnClick(object sender, RoutedEventArgs e)
#endif
        {
#if OPENSILVER
            BasicHttpServiceReference.BasicHttpServiceClient basicHttpServiceClient = new BasicHttpServiceReference.BasicHttpServiceClient();
            string testString = await basicHttpServiceClient.GetTestStringAsync();
#else
            string testString = "Not possible to add modern ServiceReference in Silverlight.";
#endif
            BasicHttpGetTestStringResultTextBlock.Text = testString;
        }

        private void LegacyBinaryGetTestStringButton_OnClick(object sender, RoutedEventArgs e)
        {
            LegacyBinaryServiceReference.BinaryServiceClient binaryServiceClient =
                new LegacyBinaryServiceReference.BinaryServiceClient();
            binaryServiceClient.GetTestStringCompleted +=
                (_, ee) => LegacyBinaryGetTestStringResultTextBlock.Text = ee.Error?.Message ?? ee.Result;
            binaryServiceClient.GetTestStringAsync();
        }

#if OPENSILVER
        private async void BinaryGetTestStringButton_OnClick(object sender, RoutedEventArgs e)
#else
        private void BinaryGetTestStringButton_OnClick(object sender, RoutedEventArgs e)
#endif
        {
#if OPENSILVER
            BinaryServiceReference.BinaryServiceClient binaryServiceClient = new BinaryServiceReference.BinaryServiceClient();
            string testString = await binaryServiceClient.GetTestStringAsync();
#else
            string testString = "Not possible to add modern ServiceReference in Silverlight.";
#endif
            BinaryGetTestStringResultTextBlock.Text = testString;
        }

        private void CustomClientBasicHttpGetTestStringButton_OnClick(object sender, RoutedEventArgs e)
        {
            CustomLegacyBasicHttpServiceReferenceClient customLegacyBasicHttpServiceReferenceClient =
                new CustomLegacyBasicHttpServiceReferenceClient();
            customLegacyBasicHttpServiceReferenceClient.GetTestStringCompleted +=
                (_, ee) => CustomClientBasicHttpGetTestStringResultTextBlock.Text = ee.Error?.Message ?? ee.Result;
            customLegacyBasicHttpServiceReferenceClient.GetTestStringAsync();
        }

        private void CustomClientBinaryGetTestStringButton_OnClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
