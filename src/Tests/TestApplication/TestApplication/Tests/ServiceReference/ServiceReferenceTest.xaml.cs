using System;
using System.ServiceModel.Channels;
using System.Windows;
using System.Windows.Controls;
using TestApplication.LegacyBasicHttpServiceReference;
using TestApplication.LegacyBinaryServiceReference;
using TestApplication.OpenSilver.Tests.ServiceReference;
using static TestApplication.OpenSilver.Tests.ServiceReference.CustomLegacyBasicHttpServiceReferenceClient;

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

        #region Legacy
        private void LegacyBasicHttpGetTestStringButton_OnClick(object sender, RoutedEventArgs e)
        {
            LegacyBasicHttpServiceReference.BasicHttpServiceClient basicHttpServiceClient =
                new LegacyBasicHttpServiceReference.BasicHttpServiceClient();
            basicHttpServiceClient.GetTestStringCompleted +=
                (_, ee) =>
                    LegacyBasicHttpGetTestStringResultTextBlock.Text = ee.Error?.Message ?? ee.Result;
            basicHttpServiceClient.GetTestStringAsync();
        }

        private void LegacyBasicHttpEchoButton_OnClick(object sender, RoutedEventArgs e)
        {
            LegacyBasicHttpServiceReference.BasicHttpServiceClient basicHttpServiceClient =
                new LegacyBasicHttpServiceReference.BasicHttpServiceClient();
            basicHttpServiceClient.EchoCompleted +=
                (_, ee) =>
                    LegacyBasicHttpEchoResultTextBlock.Text = ee.Error?.Message ?? ee.Result.GetBody<EchoMessage>().Message;

            Message message = Message.CreateMessage(
#if OPENSILVER
                // BasicHttpBinding is transformed into a CustomBinding without MessageVersion, so it has to be set explicitly here
                MessageVersion.Soap11,
#else
                basicHttpServiceClient.Endpoint.Binding.MessageVersion,
#endif
                "urn:BasicHttpService/Echo", new EchoMessage(LegacyBasicHttpEchoMessageTextBox.Text));
            basicHttpServiceClient.EchoAsync(message);
        }

        private void LegacyBinaryGetTestStringButton_OnClick(object sender, RoutedEventArgs e)
        {
            LegacyBinaryServiceReference.BinaryServiceClient binaryServiceClient =
                new LegacyBinaryServiceReference.BinaryServiceClient();
            binaryServiceClient.GetTestStringCompleted +=
                (_, ee) => LegacyBinaryGetTestStringResultTextBlock.Text = ee.Error?.Message ?? ee.Result;
            binaryServiceClient.GetTestStringAsync();
        }
#endregion

        #region Modern
#if OPENSILVER
        private async void BasicHttpGetTestStringButton_OnClick(object sender, RoutedEventArgs e)
        {
            BasicHttpServiceReference.BasicHttpServiceClient basicHttpServiceClient = new BasicHttpServiceReference.BasicHttpServiceClient();
            string testString = await basicHttpServiceClient.GetTestStringAsync();
#else
        private void BasicHttpGetTestStringButton_OnClick(object sender, RoutedEventArgs e)
        {
            string testString = "Not possible to add modern ServiceReference in Silverlight.";
#endif
            BasicHttpGetTestStringResultTextBlock.Text = testString;
        }

#if OPENSILVER
        private async void BasicHttpEchoButton_OnClick(object sender, RoutedEventArgs e)
        {
            BasicHttpServiceReference.BasicHttpServiceClient basicHttpServiceClient =
                new BasicHttpServiceReference.BasicHttpServiceClient();
            Message message = Message.CreateMessage(
#if OPENSILVER
                // BasicHttpBinding is transformed into a CustomBinding without MessageVersion, so it has to be set explicitly here
                MessageVersion.Soap11,
#else
                basicHttpServiceClient.Endpoint.Binding.MessageVersion,
#endif
                "urn:BasicHttpService/Echo", new EchoBodyWriter(BasicHttpEchoMessageTextBox.Text));

            Message response = await basicHttpServiceClient.EchoAsync(message);

            string responseString = EchoResponseSerializer.Serialize(response);
#else
        private void BasicHttpEchoButton_OnClick(object sender, RoutedEventArgs e)
        {
            string responseString = "Not possible to add modern ServiceReference in Silverlight.";
#endif
            BasicHttpEchoResultTextBlock.Text = responseString;
        }

#if OPENSILVER
        private async void BinaryGetTestStringButton_OnClick(object sender, RoutedEventArgs e)
        {
            BinaryServiceReference.BinaryServiceClient binaryServiceClient = new BinaryServiceReference.BinaryServiceClient();
            string testString = await binaryServiceClient.GetTestStringAsync();
#else
        private void BinaryGetTestStringButton_OnClick(object sender, RoutedEventArgs e)
        {
            string testString = "Not possible to add modern ServiceReference in Silverlight.";
#endif
            BinaryGetTestStringResultTextBlock.Text = testString;
        }
#endregion

        #region Custom Client
        private void CustomClientLegacyBasicHttpGetTestStringButton_OnClick(object sender, RoutedEventArgs e)
        {
            CustomLegacyBasicHttpServiceReferenceClient customLegacyBasicHttpServiceReferenceClient =
                new CustomLegacyBasicHttpServiceReferenceClient();
            customLegacyBasicHttpServiceReferenceClient.GetTestStringCompleted +=
                (_, ee) => CustomClientBasicHttpGetTestStringResultTextBlock.Text = ee.Error?.Message ?? ee.Result;
            customLegacyBasicHttpServiceReferenceClient.GetTestStringAsync();
        }

        private void CustomClientLegacyBinaryGetTestStringButton_OnClick(object sender, RoutedEventArgs e)
        {

        }

        private void CustomClientLegacyBasicHttpEchoButton_OnClick(object sender, RoutedEventArgs e)
        {
            CustomLegacyBasicHttpServiceReferenceClient customLegacyBasicHttpServiceReferenceClient =
                new CustomLegacyBasicHttpServiceReferenceClient();
            customLegacyBasicHttpServiceReferenceClient.EchoCompleted +=
                (_, ee) => CustomClientBasicHttpEchoResultTextBlock.Text = ee.Error?.Message ?? ee.Result;
            customLegacyBasicHttpServiceReferenceClient.EchoUsingMessageAsync(CustomClientLegacyBasicHttpEchoMessageTextBox.Text);
        }
        #endregion
    }
}
