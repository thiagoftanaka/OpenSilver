using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Text;
using System.IO.IsolatedStorage;
using System.IO;

namespace TestApplication.Tests
{
    public partial class IsolatedStorageTest : Page
    {
        public IsolatedStorageTest()
        {
            InitializeComponent();
        }

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        #region ApplicationSettings File
        private void ButtonSaveToIsolatedStorage_Click(object sender, RoutedEventArgs e)
        {
            string path = TextBoxWithIsolatedStorageFilePath.Text;
            FileSystemHelpers.WriteTextToFile(path, TextBoxWithNewTextForIsolatedStorage.Text, false);
        }

        private void ButtonLoadFromIsolatedStorage_Click(object sender, RoutedEventArgs e)
        {
            string path = TextBoxWithIsolatedStorageFilePath.Text;
            TextBlockWithApplicationLoadedText.Text = FileSystemHelpers.ReadTextFromFile(path, false);
        }

        private void ButtonDeleteFromIsolatedStorage_Click(object sender, RoutedEventArgs e)
        {
            string path = TextBoxWithIsolatedStorageFilePath.Text;
            FileSystemHelpers.DeleteFile(path, false);
        }
        #endregion

        #region ApplicationSettings IsolatedStorageSettings
        private void ButtonSaveToIsolatedStorageSettings_Click(object sender, RoutedEventArgs e)
        {
            string key = TextBoxWithIsolatedStorageSettingsKey.Text;
            FileSystemHelpers.WriteTextToSettings(key, TextBoxWithIsolatedStorageSettingsValue.Text, false);
        }
        private void ButtonSaveToIsolatedStorageSettingsWithAdd_Click(object sender, RoutedEventArgs e)
        {
            string key = TextBoxWithIsolatedStorageSettingsKey.Text;
            IsolatedStorageSettings.ApplicationSettings.Add(key, TextBoxWithIsolatedStorageSettingsValue.Text);
        }


        private void ButtonRemoveFromIsolatedStorageSettings_Click(object sender, RoutedEventArgs e)
        {
            string key = TextBoxWithIsolatedStorageSettingsKey.Text;
            IsolatedStorageSettings.ApplicationSettings.Remove(key);
        }

        private void ButtonLoadFromIsolatedStorageSettings_Click(object sender, RoutedEventArgs e)
        {
            string key = TextBoxWithIsolatedStorageSettingsKey.Text;
            TextBlockWithIsolatedStorageSettingsLoadedText.Text = FileSystemHelpers.ReadTextFromSettings(key, false);
            TextBlockWithIsolatedStorageSettingsElementsCount.Text = IsolatedStorageSettings.ApplicationSettings.Count.ToString();
            string temp = "";
            foreach (var pair in IsolatedStorageSettings.ApplicationSettings)
            {
                temp += "{" + pair.Key + "," + pair.Value + "},";
            }
            TextBlockWithIsolatedStorageSettingsElements.Text = temp;
        }

        private void ButtonLoadFromIsolatedStorageUsingTryGetValue_Click(object sender, RoutedEventArgs e)
        {
            string key = TextBoxWithIsolatedStorageSettingsKey.Text;
            string value = string.Empty;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue(key, out value);
            TextBlockWithIsolatedStorageSettingsLoadedText.Text = value;
            TextBlockWithIsolatedStorageSettingsElementsCount.Text = IsolatedStorageSettings.ApplicationSettings.Count.ToString();
            string temp = "";
            foreach (var pair in IsolatedStorageSettings.ApplicationSettings)
            {
                temp += "{" + pair.Key + "," + pair.Value + "},";
            }
            TextBlockWithIsolatedStorageSettingsElements.Text = temp;
        }
        #endregion

        #region SiteSettings File
        private void ButtonSaveToSiteIsolatedStorage_Click(object sender, RoutedEventArgs e)
        {
            string path = TextBoxWithSiteIsolatedStorageFilePath.Text;
            FileSystemHelpers.WriteTextToFile(path, TextBoxWithNewTextForSiteIsolatedStorage.Text, true);
        }

        private void ButtonLoadFromSiteIsolatedStorage_Click(object sender, RoutedEventArgs e)
        {
            string path = TextBoxWithSiteIsolatedStorageFilePath.Text;
            TextBlockWithSiteLoadedText.Text = FileSystemHelpers.ReadTextFromFile(path, true);
        }

        private void ButtonDeleteFromSiteIsolatedStorage_Click(object sender, RoutedEventArgs e)
        {
            string path = TextBoxWithSiteIsolatedStorageFilePath.Text;
            FileSystemHelpers.DeleteFile(path, true);
        }
        #endregion

        #region SiteSettings IsolatedStorageSettings
        private void ButtonSaveToSiteIsolatedStorageSettings_Click(object sender, RoutedEventArgs e)
        {
            string key = TextBoxWithSiteIsolatedStorageSettingsKey.Text;
            FileSystemHelpers.WriteTextToSettings(key, TextBoxWithSiteIsolatedStorageSettingsValue.Text, true);
        }
        private void ButtonSaveToSiteIsolatedStorageSettingsWithAdd_Click(object sender, RoutedEventArgs e)
        {
            string key = TextBoxWithSiteIsolatedStorageSettingsKey.Text;
            IsolatedStorageSettings.SiteSettings.Add(key, TextBoxWithSiteIsolatedStorageSettingsValue.Text);
        }

        private void ButtonRemoveFromSiteIsolatedStorageSettings_Click(object sender, RoutedEventArgs e)
        {
            string key = TextBoxWithSiteIsolatedStorageSettingsKey.Text;
            IsolatedStorageSettings.SiteSettings.Remove(key);
        }

        private void ButtonLoadFromSiteIsolatedStorageSettings_Click(object sender, RoutedEventArgs e)
        {
            string key = TextBoxWithSiteIsolatedStorageSettingsKey.Text;
            TextBlockWithSiteIsolatedStorageSettingsLoadedText.Text = FileSystemHelpers.ReadTextFromSettings(key, true);
            TextBlockWithSiteIsolatedStorageSettingsElementsCount.Text = IsolatedStorageSettings.SiteSettings.Count.ToString();
            string temp = "";
            foreach (var pair in IsolatedStorageSettings.SiteSettings)
            {
                temp += "{" + pair.Key + "," + pair.Value + "},";
            }
            TextBlockWithSiteIsolatedStorageSettingsElements.Text = temp;
        }

        private void ButtonLoadFromSiteIsolatedStorageUsingTryGetValue_Click(object sender, RoutedEventArgs e)
        {
            string key = TextBoxWithSiteIsolatedStorageSettingsKey.Text;
            string value = string.Empty;
            IsolatedStorageSettings.SiteSettings.TryGetValue(key, out value);
            TextBlockWithSiteIsolatedStorageSettingsLoadedText.Text = value;
            TextBlockWithSiteIsolatedStorageSettingsElementsCount.Text = IsolatedStorageSettings.SiteSettings.Count.ToString();
            string temp = "";
            foreach (var pair in IsolatedStorageSettings.SiteSettings)
            {
                temp += "{" + pair.Key + "," + pair.Value + "},";
            }
            TextBlockWithSiteIsolatedStorageSettingsElements.Text = temp;
        }
        #endregion

        public static class FileSystemHelpers
        {
            public static void WriteTextToFile(string fileName, string fileContent, bool isForSite)
            {
#if OPENSILVER || SILVERLIGHT
                using (IsolatedStorageFile storage =
                       isForSite ? IsolatedStorageFile.GetUserStoreForSite() : IsolatedStorageFile.GetUserStoreForApplication())
#else
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForAssembly())
#endif
                {
                    IsolatedStorageFileStream fs = null;
                    using (fs = storage.CreateFile(fileName))
                    {
                        if (fs != null)
                        {
                            //using (StreamWriter sw = new StreamWriter(fs))
                            //{
                            //    sw.Write(fileContent);
                            //}
                            Encoding encoding = new UTF8Encoding();
                            byte[] bytes = encoding.GetBytes(fileContent);
                            fs.Write(bytes, 0, bytes.Length);
                            fs.Close();
                        }
                    }
                }
            }

            public static void DeleteFile(string fileName, bool isForSite)
            {
#if OPENSILVER || SILVERLIGHT
                using (IsolatedStorageFile storage =
                       isForSite ? IsolatedStorageFile.GetUserStoreForSite() : IsolatedStorageFile.GetUserStoreForApplication())
#else
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForAssembly())
#endif
                {
                    storage.DeleteFile(fileName);
                }
            }

            public static string ReadTextFromFile(string fileName, bool isForSite)
            {
#if OPENSILVER || SILVERLIGHT
                using (IsolatedStorageFile storage =
                       isForSite ? IsolatedStorageFile.GetUserStoreForSite() : IsolatedStorageFile.GetUserStoreForApplication())
#else
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForAssembly())
#endif
                {
                    if (storage.FileExists(fileName))
                    {
                        using (IsolatedStorageFileStream fs = storage.OpenFile(fileName, System.IO.FileMode.Open))
                        {
                            if (fs != null)
                            {
                                using (StreamReader sr = new StreamReader(fs))
                                {
                                    return sr.ReadToEnd();
                                }

                                //byte[] saveBytes = new byte[4];
                                //int count = fs.Read(saveBytes, 0, 4);
                                //if (count > 0)
                                //{
                                //    number = System.BitConverter.ToInt32(saveBytes, 0);
                                //}
                            }
                        }
                    }
                }
                return null;
            }

            public static void WriteTextToSettings(string key, string value, bool isForSite)
            {
                (isForSite ? IsolatedStorageSettings.SiteSettings : IsolatedStorageSettings.ApplicationSettings)[key] = value;
            }

            public static string ReadTextFromSettings(string key, bool isForSite)
            {
                IsolatedStorageSettings settings = isForSite ? IsolatedStorageSettings.SiteSettings : IsolatedStorageSettings.ApplicationSettings;
                if (settings.Contains(key))
                {
                    object value = settings[key];
                    if (value is string)
                    {
                        return (string)value;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }


        }

        private void ButtonClearIsolatedStorageSettings_Click(object sender, RoutedEventArgs e)
        {
            IsolatedStorageSettings.ApplicationSettings.Clear();
        }

        private void ButtonClearSiteIsolatedStorageSettings_Click(object sender, RoutedEventArgs e)
        {
            IsolatedStorageSettings.SiteSettings.Clear();
        }
    }
}
