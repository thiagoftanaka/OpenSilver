
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
            client.BodyMemberCompleted +=
                (_, ee) => LegacyBasicHttpBodyMemberTextBlock.Text = ee.Error?.Message ?? ee.Result;

            client.BodyMemberAsync(LegacyBasicHttpBodyMemberTextBox.Text);
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

        #endregion

        #region Custom Client

        private void CustomLegacyBasicHttpBodyMemberButton_OnClick(object sender, RoutedEventArgs e)
        {
            CustomLegacyBasicHttpClient client = new CustomLegacyBasicHttpClient();
            client.BodyMemberCompleted +=
                (_, ee) => CustomBasicHttpBodyMemberTextBlock.Text = ee.Error?.Message ?? ee.Result;
            // Tests if headers from the Message instance are sent
            client.BodyMemberAsync(CustomLegacyBasicHttpBodyMemberTextBox.Text);
        }

        #endregion
    }
}
