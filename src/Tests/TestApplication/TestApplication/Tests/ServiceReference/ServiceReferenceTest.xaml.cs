using System.Windows;
using System.Windows.Controls;
using TestApplication.Tests.ServiceReference;

#if OPENSILVER
using BasicHttpServiceReference;
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

        private void LegacyBasicHttpEchoButton_OnClick(object sender, RoutedEventArgs e)
        {
            LegacyBasicHttpServiceReference.BasicHttpServiceClient basicHttpServiceClient =
                new LegacyBasicHttpServiceReference.BasicHttpServiceClient();
            basicHttpServiceClient.EchoCompleted +=
                (_, ee) =>
                    LegacyBasicHttpEchoTextBlock.Text = ee.Error?.Message ?? ee.Result;
            basicHttpServiceClient.EchoAsync(LegacyBasicHttpEchoTextBox.Text);
        }

        private void LegacyBasicHttpBodyMemberButton_OnClick(object sender, RoutedEventArgs e)
        {
            LegacyBasicHttpServiceReference.BasicHttpServiceClient basicHttpServiceClient =
                new LegacyBasicHttpServiceReference.BasicHttpServiceClient();
            basicHttpServiceClient.BodyMemberCompleted +=
                (_, ee) =>
                    LegacyBasicHttpBodyMemberTextBlock.Text = ee.Error?.Message ?? ee.Result;
            basicHttpServiceClient.BodyMemberAsync(LegacyBasicHttpBodyMemberTextBox.Text);
        }

        private void LegacyBinaryEchoButton_OnClick(object sender, RoutedEventArgs e)
        {
            LegacyBinaryServiceReference.BinaryServiceClient binaryServiceClient =
                new LegacyBinaryServiceReference.BinaryServiceClient();
            binaryServiceClient.EchoCompleted +=
                (_, ee) => LegacyBinaryEchoTextBlock.Text = ee.Error?.Message ?? ee.Result;
            binaryServiceClient.EchoAsync(LegacyBinaryEchoTextBox.Text);
        }

#endregion

#region Modern

#if OPENSILVER
        private async void BasicHttpEchoButton_OnClick(object sender, RoutedEventArgs e)
        {
            BasicHttpServiceReference.BasicHttpServiceClient basicHttpServiceClient = new BasicHttpServiceReference.BasicHttpServiceClient();
            string testString = await basicHttpServiceClient.EchoAsync(BasicHttpEchoTextBox.Text);
#else
        private void BasicHttpEchoButton_OnClick(object sender, RoutedEventArgs e)
        {
            string testString = "Ignore this case: not possible to add modern ServiceReference in Silverlight.";
#endif
            BasicHttpEchoTextBlock.Text = testString;
        }

#if OPENSILVER
        private async void BasicHttpBodyMemberButton_OnClick(object sender, RoutedEventArgs e)
        {
            BasicHttpServiceReference.BasicHttpServiceClient basicHttpServiceClient = new BasicHttpServiceReference.BasicHttpServiceClient();
            BodyMemberResponseMessage responseMessage = await basicHttpServiceClient.BodyMemberAsync(BasicHttpBodyMemberTextBox.Text);
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
            BinaryServiceReference.BinaryServiceClient binaryServiceClient = new BinaryServiceReference.BinaryServiceClient();
            string testString = await binaryServiceClient.EchoAsync(BinaryEchoTextBox.Text);
#else
        private void BinaryEchoButton_OnClick(object sender, RoutedEventArgs e)
        {
            string testString = "Ignore this case: not possible to add modern ServiceReference in Silverlight.";
#endif
            BinaryEchoTextBlock.Text = testString;
        }

#endregion

#region Custom Client

        private void CustomLegacyBasicHttpBodyMemberButton_OnClick(object sender, RoutedEventArgs e)
        {
            CustomLegacyBasicHttpClient customLegacyBasicHttpClient =
                new CustomLegacyBasicHttpClient();
            customLegacyBasicHttpClient.BodyMemberCompleted +=
                (_, ee) => CustomBasicHttpBodyMemberTextBlock.Text = ee.Error?.Message ?? ee.Result;
            customLegacyBasicHttpClient.BodyMemberAsync(CustomLegacyBasicHttpBodyMemberTextBox.Text);
        }

#endregion
    }
}
