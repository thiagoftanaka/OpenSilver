using System.Reflection;
using System.ServiceModel.Channels;
using System.Windows;
using System.Windows.Controls;
using TestApplication.Tests.ServiceReference;

namespace TestApplication.Tests
{
    public partial class ServiceReferenceTest : Page
    {
        public ServiceReferenceTest()
        {
            this.InitializeComponent();
        }

        #region Legacy

        private void LegacyBasicHttpEchoButton_OnClick(object sender, RoutedEventArgs e)
        {
            LegacyBasicHttpServiceReference.BasicHttpServiceClient client = new LegacyBasicHttpServiceReference.BasicHttpServiceClient();
            client.EchoCompleted +=
                (_, ee) => LegacyBasicHttpEchoTextBlock.Text = ee.Error?.Message ?? ee.Result;
            client.EchoAsync(LegacyBasicHttpEchoTextBox.Text);
        }

        private void LegacyBasicHttpBodyMemberButton_OnClick(object sender, RoutedEventArgs e)
        {
            LegacyBasicHttpServiceReference.BasicHttpServiceClient client = new LegacyBasicHttpServiceReference.BasicHttpServiceClient();

#if !OPENSILVER
            using (new OperationContextScope(client.InnerChannel))
            {
#endif
                if (!string.IsNullOrEmpty(LegacyBasicHttpHeaderTextBox.Text))
                {
                    MessageHeader customHeader =
                        MessageHeader.CreateHeader("CustomHeader", "", LegacyBasicHttpHeaderTextBox.Text);

#if OPENSILVER
                    // The client only inherits from CSHTML5_ClientBase after compilation, 
                    // so reflection is used here to access its members.
                    PropertyInfo outgoingMessageHeadersProperty = client.GetType().GetProperty("OutgoingMessageHeaders");
                    MessageHeaders outgoingMessageHeaders = (MessageHeaders)outgoingMessageHeadersProperty.GetValue(client);
                    outgoingMessageHeaders.Add(customHeader);
#else
                    OperationContext.Current.OutgoingMessageHeaders.Add(customHeader);
#endif
                }

                client.BodyMemberCompleted +=
                    (_, ee) => LegacyBasicHttpBodyMemberTextBlock.Text = ee.Error?.Message ?? ee.Result;

                client.BodyMemberAsync(LegacyBasicHttpBodyMemberTextBox.Text);
#if !OPENSILVER
            }
#endif
        }

        private void LegacyBinaryEchoButton_OnClick(object sender, RoutedEventArgs e)
        {
            LegacyBinaryServiceReference.BinaryServiceClient client = new LegacyBinaryServiceReference.BinaryServiceClient();
            client.EchoCompleted +=
                (_, ee) => LegacyBinaryEchoTextBlock.Text = ee.Error?.Message ?? ee.Result;
            client.EchoAsync(LegacyBinaryEchoTextBox.Text);
        }

        private void LegacyBinaryBodyMemberButton_OnClick(object sender, RoutedEventArgs e)
        {
            LegacyBinaryServiceReference.BinaryServiceClient client = new LegacyBinaryServiceReference.BinaryServiceClient();
            client.BodyMemberCompleted +=
                (_, ee) => LegacyBinaryBodyMemberTextBlock.Text = ee.Error?.Message ?? ee.Result;
            client.BodyMemberAsync(LegacyBinaryBodyMemberTextBox.Text);
        }

#endregion

        #region Modern

#if OPENSILVER
        private async void BasicHttpEchoButton_OnClick(object sender, RoutedEventArgs e)
        {
            BasicHttpServiceReference.BasicHttpServiceClient client = new();
            string testString = await client.EchoAsync(BasicHttpEchoTextBox.Text);
#else
        private void BasicHttpEchoButton_OnClick(object sender, RoutedEventArgs e)
        {
            string testString = "Ignore this case: not possible to add modern ServiceReference in Silverlight.";
#endif
            BasicHttpEchoTextBlock.Text = testString;
        }

#if OPENSILVER
        private async void BasicHttpEndpointConfigEchoButton_OnClick(object sender, RoutedEventArgs e)
        {
            BasicHttpServiceReference.BasicHttpServiceClient client = new(
                    BasicHttpServiceReference.BasicHttpServiceClient.EndpointConfiguration.BasicHttpBinding_BasicHttpService);
            string testString = await client.EchoAsync(BasicHttpEndpointConfigEchoTextBox.Text);
#else
        private void BasicHttpEndpointConfigEchoButton_OnClick(object sender, RoutedEventArgs e)
        {
            string testString = "Ignore this case: not possible to add modern ServiceReference in Silverlight.";
#endif
            BasicHttpEndpointConfigEchoTextBlock.Text = testString;
        }

#if OPENSILVER
        private async void BasicHttpBodyMemberButton_OnClick(object sender, RoutedEventArgs e)
        {
            BasicHttpServiceReference.BasicHttpServiceClient client = new();

            if (!string.IsNullOrEmpty(BasicHttpHeaderTextBox.Text))
            {
                MessageHeader customHeader = MessageHeader.CreateHeader("CustomHeader", "", BasicHttpHeaderTextBox.Text);

                // The client only inherits from CSHTML5_ClientBase after compilation, 
                // so reflection is used here to access its members.
                PropertyInfo outgoingMessageHeadersProperty = client.GetType().GetProperty("OutgoingMessageHeaders");
                MessageHeaders outgoingMessageHeaders = (MessageHeaders)outgoingMessageHeadersProperty.GetValue(client);
                outgoingMessageHeaders.Add(customHeader);
            }

            BasicHttpServiceReference.BodyMemberResponseMessage responseMessage =
                await client.BodyMemberAsync(BasicHttpBodyMemberTextBox.Text);
            string testString = responseMessage.BodyMemberResponse;
#else
        private void BasicHttpBodyMemberButton_OnClick(object sender, RoutedEventArgs e)
        {
            string testString = "Ignore this case: not possible to add modern ServiceReference in Silverlight.";
#endif
            BasicHttpBodyMemberTextBlock.Text = testString;
        }

#if OPENSILVER
        private async void BinaryEchoButton_OnClick(object sender, RoutedEventArgs e)
        {
            BinaryServiceReference.BinaryServiceClient client = new();
            string testString = await client.EchoAsync(BinaryEchoTextBox.Text);
#else
        private void BinaryEchoButton_OnClick(object sender, RoutedEventArgs e)
        {
            string testString = "Ignore this case: not possible to add modern ServiceReference in Silverlight.";
#endif
            BinaryEchoTextBlock.Text = testString;
        }

#if OPENSILVER
        private async void BinaryBodyMemberButton_OnClick(object sender, RoutedEventArgs e)
        {
            BinaryServiceReference.BinaryServiceClient client = new();
            BinaryServiceReference.BodyMemberResponseMessage responseMessage =
                await client.BodyMemberAsync(BinaryBodyMemberTextBox.Text);
            string testString = responseMessage.BodyMemberResponse;
#else
        private void BinaryBodyMemberButton_OnClick(object sender, RoutedEventArgs e)
        {
            string testString = "Ignore this case: not possible to add modern ServiceReference in Silverlight.";
#endif
            BinaryBodyMemberTextBlock.Text = testString;
        }

        #endregion

        #region Custom Client

        private void CustomLegacyBasicHttpBodyMemberButton_OnClick(object sender, RoutedEventArgs e)
        {
            CustomLegacyBasicHttpClient client = new CustomLegacyBasicHttpClient();

            MessageHeader customHeader = null;
            if (!string.IsNullOrEmpty(CustomLegacyBasicHttpHeaderTextBox.Text))
            {
                customHeader =
                    MessageHeader.CreateHeader("CustomHeader", "", CustomLegacyBasicHttpHeaderTextBox.Text);
            }

            client.BodyMemberCompleted +=
                (_, ee) => CustomBasicHttpBodyMemberTextBlock.Text = ee.Error?.Message ?? ee.Result;
            // Tests if headers from the Message instance are sent
            client.BodyMemberAsync(CustomLegacyBasicHttpBodyMemberTextBox.Text, customHeader);
        }

        #endregion
    }
}
