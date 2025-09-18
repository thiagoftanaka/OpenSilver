using System.Windows;
using System.Windows.Controls;
using TestApplication.BasicHttpServiceReference;
using TestApplication.BinaryServiceReference;

namespace TestApplication.Tests
{
    public partial class ServiceReferenceTest : Page
    {
        public ServiceReferenceTest()
        {
            this.InitializeComponent();
        }

        private void BasicHttpGetTestStringButton_OnClick(object sender, RoutedEventArgs e)
        {
            BasicHttpServiceClient basicHttpServiceClient = new BasicHttpServiceClient();
            basicHttpServiceClient.GetTestStringCompleted += BasicHttpServiceClient_GetTestStringCompleted;
            basicHttpServiceClient.GetTestStringAsync();
        }

        private void BasicHttpServiceClient_GetTestStringCompleted(object sender, BasicHttpServiceReference.GetTestStringCompletedEventArgs e)
        {
            BasicHttpGetTestStringResultTextBlock.Text = e.Error?.Message ?? e.Result;
        }

        private void BinaryGetTestStringButton_OnClick(object sender, RoutedEventArgs e)
        {
            BinaryServiceClient binaryServiceClient = new BinaryServiceClient();
            binaryServiceClient.GetTestStringCompleted += BinaryServiceClient_GetTestStringCompleted;
            binaryServiceClient.GetTestStringAsync();
        }

        private void BinaryServiceClient_GetTestStringCompleted(object sender, BinaryServiceReference.GetTestStringCompletedEventArgs e)
        {
            BinaryGetTestStringResultTextBlock.Text = e.Error?.Message ?? e.Result;
        }
    }
}
