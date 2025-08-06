
/*===================================================================================
* 
*   Copyright (c) Userware/OpenSilver.net
*      
*   This file is part of the OpenSilver Runtime (https://opensilver.net), which is
*   licensed under the MIT license: https://opensource.org/licenses/MIT
*   
*   As stated in the MIT license, "the above copyright notice and this permission
*   notice shall be included in all copies or substantial portions of the Software."
*  
\*====================================================================================*/


using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Windows;
using System.Windows.Controls;
using TestApplication.Tests.ServiceReference;

namespace TestApplication.Tests
{
    public partial class ServiceReferenceTest : Page
    {
        private const string BasicHttpAddress = "http://localhost:50506/BasicHttpService.svc";
        private const string BinaryAddress = "http://localhost:50506/BinaryService.svc";

        public ServiceReferenceTest()
        {
            this.InitializeComponent();
        }

        #region Legacy

        private void LegacyBasicHttpEchoButton_OnClick(object sender, RoutedEventArgs e)
        {
#if OPENSILVER
            LegacyBasicHttpServiceReference.BasicHttpServiceClient client = new LegacyBasicHttpServiceReference.BasicHttpServiceClient(
                new CustomBinding(), new EndpointAddress(BasicHttpAddress));
#else
            LegacyBasicHttpServiceReference.BasicHttpServiceClient client = new LegacyBasicHttpServiceReference.BasicHttpServiceClient();
#endif
            client.EchoCompleted +=
                (_, ee) => LegacyBasicHttpEchoTextBlock.Text = ee.Error?.Message ?? ee.Result;
            client.EchoAsync(LegacyBasicHttpEchoTextBox.Text);
        }

        private void LegacyBasicHttpBodyMemberButton_OnClick(object sender, RoutedEventArgs e)
        {
#if OPENSILVER
            LegacyBasicHttpServiceReference.BasicHttpServiceClient client = new LegacyBasicHttpServiceReference.BasicHttpServiceClient(
                new CustomBinding(), new EndpointAddress(BasicHttpAddress));
#else
            LegacyBasicHttpServiceReference.BasicHttpServiceClient client = new LegacyBasicHttpServiceReference.BasicHttpServiceClient();

            using (new System.ServiceModel.OperationContextScope(client.InnerChannel))
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
                ICollection<MessageHeader> outgoingMessageHeaders =
                    (ICollection<MessageHeader>)outgoingMessageHeadersProperty.GetValue(client);
                outgoingMessageHeaders.Add(customHeader);
#else
                    System.ServiceModel.OperationContext.Current.OutgoingMessageHeaders.Add(customHeader);
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
#if OPENSILVER
            LegacyBinaryServiceReference.BinaryServiceClient client = new LegacyBinaryServiceReference.BinaryServiceClient(
                new CustomBinding([new BinaryMessageEncodingBindingElement(),
                    new HttpTransportBindingElement { MaxBufferSize = 2147483647, MaxReceivedMessageSize = 2147483647 }]),
                new EndpointAddress(BinaryAddress));
#else
            LegacyBinaryServiceReference.BinaryServiceClient client = new LegacyBinaryServiceReference.BinaryServiceClient();
#endif
            client.EchoCompleted +=
                (_, ee) => LegacyBinaryEchoTextBlock.Text = ee.Error?.Message ?? ee.Result;
            client.EchoAsync(LegacyBinaryEchoTextBox.Text);
        }

        private void LegacyBinaryBodyMemberButton_OnClick(object sender, RoutedEventArgs e)
        {
#if OPENSILVER
            LegacyBinaryServiceReference.BinaryServiceClient client = new LegacyBinaryServiceReference.BinaryServiceClient(
                new CustomBinding([new BinaryMessageEncodingBindingElement(),
                    new HttpTransportBindingElement { MaxBufferSize = 2147483647, MaxReceivedMessageSize = 2147483647 }]),
                new EndpointAddress(BinaryAddress));
#else
            LegacyBinaryServiceReference.BinaryServiceClient client = new LegacyBinaryServiceReference.BinaryServiceClient();
#endif
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
                ICollection<MessageHeader> outgoingMessageHeaders =
                    (ICollection<MessageHeader>)outgoingMessageHeadersProperty.GetValue(client);
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
            BinaryServiceReference.BinaryServiceClient client = new(
                new CustomBinding([new BinaryMessageEncodingBindingElement(),
                    new HttpTransportBindingElement { MaxBufferSize = 2147483647, MaxReceivedMessageSize = 2147483647 }]),
                new EndpointAddress(BinaryAddress));
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
            BinaryServiceReference.BinaryServiceClient client = new(
                new CustomBinding([new BinaryMessageEncodingBindingElement(),
                    new HttpTransportBindingElement { MaxBufferSize = 2147483647, MaxReceivedMessageSize = 2147483647 }]),
                new EndpointAddress(BinaryAddress));
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
#if OPENSILVER
            CustomLegacyBasicHttpClient client = new CustomLegacyBasicHttpClient(
                new CustomBinding(), new EndpointAddress(BasicHttpAddress));
#else
            CustomLegacyBasicHttpClient client = new CustomLegacyBasicHttpClient();

            using (new System.ServiceModel.OperationContextScope(client.InnerChannel))
            {
#endif
            if (!string.IsNullOrEmpty(CustomLegacyBasicHttpHeaderTextBox.Text))
            {
                MessageHeader customHeader =
                    MessageHeader.CreateHeader("CustomHeader", "", CustomLegacyBasicHttpHeaderTextBox.Text);

#if OPENSILVER
                client.OutgoingMessageHeaders.Add(customHeader);
#else
                System.ServiceModel.OperationContext.Current.OutgoingMessageHeaders.Add(customHeader);
#endif
            }

            client.BodyMemberCompleted +=
                (_, ee) => CustomBasicHttpBodyMemberTextBlock.Text = ee.Error?.Message ?? ee.Result;
            // Tests if headers from the Message instance are sent
            client.BodyMemberAsync(CustomLegacyBasicHttpBodyMemberTextBox.Text);
#if !OPENSILVER
            }
#endif

        }

        #endregion
    }
}
