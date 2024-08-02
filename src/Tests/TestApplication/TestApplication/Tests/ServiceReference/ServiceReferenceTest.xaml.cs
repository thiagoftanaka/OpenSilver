using System;
using System.Windows;
using System.Windows.Controls;
using TestApplication.LegacyBasicHttpServiceReference;
using TestApplication.LegacyBinaryServiceReference;
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
            try
            {
                LegacyBasicHttpServiceReference.BasicHttpServiceClient basicHttpServiceClient =
                    new LegacyBasicHttpServiceReference.BasicHttpServiceClient();
                basicHttpServiceClient.GetTestStringCompleted += BasicHttpServiceClient_GetTestStringCompleted;
                basicHttpServiceClient.GetTestStringAsync();
            }
            catch (Exception exception)
            {
                throw;
            }
        }

        private void BasicHttpServiceClient_GetTestStringCompleted(object sender,
            LegacyBasicHttpServiceReference.GetTestStringCompletedEventArgs e)
        {
            try
            {
                LegacyBasicHttpGetTestStringResultTextBlock.Text = e.Error?.Message ?? e.Result;
            }
            catch (Exception exception)
            {
                throw;
            }
        }

#if OPENSILVER
        private async void BasicHttpGetTestStringButton_OnClick(object sender, RoutedEventArgs e)
#else
        private void BasicHttpGetTestStringButton_OnClick(object sender, RoutedEventArgs e)
#endif
        {
            try
            {
#if OPENSILVER
                BasicHttpServiceReference.BasicHttpServiceClient basicHttpServiceClient = new BasicHttpServiceReference.BasicHttpServiceClient();
                string testString = await basicHttpServiceClient.GetTestStringAsync();
#else
                string testString = "Not possible to add modern ServiceReference in Silverlight.";
#endif
                BasicHttpGetTestStringResultTextBlock.Text = testString;
            }
            catch (Exception exception)
            {
                throw;
            }
        }

        private void LegacyBinaryGetTestStringButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                LegacyBinaryServiceReference.BinaryServiceClient binaryServiceClient =
                    new LegacyBinaryServiceReference.BinaryServiceClient();
                binaryServiceClient.GetTestStringCompleted += BinaryServiceClient_GetTestStringCompleted;
                binaryServiceClient.GetTestStringAsync();
            }
            catch (Exception exception)
            {
                throw;
            }
        }

        private void BinaryServiceClient_GetTestStringCompleted(object sender,
            LegacyBinaryServiceReference.GetTestStringCompletedEventArgs e)
        {
            try
            {
                LegacyBinaryGetTestStringResultTextBlock.Text = e.Error?.Message ?? e.Result;
            }
            catch (Exception exception)
            {
                throw;
            }
        }

#if OPENSILVER
        private async void BinaryGetTestStringButton_OnClick(object sender, RoutedEventArgs e)
#else
        private void BinaryGetTestStringButton_OnClick(object sender, RoutedEventArgs e)
#endif
        {
            try
            {
#if OPENSILVER
                BinaryServiceReference.BinaryServiceClient binaryServiceClient = new BinaryServiceReference.BinaryServiceClient();
                string testString = await binaryServiceClient.GetTestStringAsync();
#else
                string testString = "Not possible to add modern ServiceReference in Silverlight.";
#endif
                BinaryGetTestStringResultTextBlock.Text = testString;
            }
            catch (Exception exception)
            {
                throw;
            }
        }
    }
}
