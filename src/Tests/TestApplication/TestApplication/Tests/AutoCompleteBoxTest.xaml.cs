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
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Markup;
using System.Reflection;

namespace TestApplication.Tests
{
    public partial class AutoCompleteBoxTest : Page
    {
        private ObservableCollection<string> _items = new ObservableCollection<string>();

        public AutoCompleteBoxTest()
        {
            InitializeComponent();

#if OPENSILVER
            AutoCompleteVirtualized.CustomLayout = true;
            AutoCompleteVirtualized.IsAutoHeightOnCustomLayout = true;
#endif
        }

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            CreateNewItemsSource();
        }

        private void CreateNewItemsSource()
        {
            _items = new ObservableCollection<string>(Enumerable.Range(1, 50).Select(i => $"Initial item {i}").ToList());
            AutoCompleteBox1.ItemsSource = _items;
            AutoCompleteBox1.SelectedItem = _items[0];

            AutoCompleteVirtualized.ItemsSource = _items;
            AutoCompleteVirtualized.SelectedItem = _items[0];
        }

        string RandomId()
        {
            return (new Random()).Next(1000).ToString();
        }

        private void ButtonTestAutoCompleteBox1_ItemsAdd_Click(object sender, RoutedEventArgs e)
        {
            _items.Add("Item #" + RandomId());
        }

        private void ButtonTestAutoCompleteBox1_ItemsClear_Click(object sender, RoutedEventArgs e)
        {
            _items.Clear();
        }

        private void ButtonTestAutoCompleteBox1_ItemsRemoveFirst_Click(object sender, RoutedEventArgs e)
        {
            _items.Remove(_items[0]);
        }

        private void ButtonTestAutoCompleteBox1_SetNewItemsSource_Click(object sender, RoutedEventArgs e)
        {
            CreateNewItemsSource();
        }

        private void ButtonTestAutoCompleteBox1_ItemsSourceAdd_Click(object sender, RoutedEventArgs e)
        {
            ((ObservableCollection<string>)AutoCompleteBox1.ItemsSource).Add("Item #" + RandomId());
        }

        private void ButtonTestAutoCompleteBox1_ItemsSourceClear_Click(object sender, RoutedEventArgs e)
        {
            ((ObservableCollection<string>)AutoCompleteBox1.ItemsSource).Clear();
        }

        private void ButtonTestAutoCompleteBox1_ItemsSourceRemove_Click(object sender, RoutedEventArgs e)
        {
            ((ObservableCollection<string>)AutoCompleteBox1.ItemsSource).Remove(((ObservableCollection<string>)AutoCompleteBox1.ItemsSource).FirstOrDefault());
        }

        private void ButtonTestAutoCompleteBox1_SetItemsSourceToNull_Click(object sender, RoutedEventArgs e)
        {
            AutoCompleteBox1.ItemsSource = null;
        }

        private void ButtonTestAutoCompleteBox1_SelectSecondItem_Click(object sender, RoutedEventArgs e)
        {
            AutoCompleteBox1.SelectedItem = _items[1];
        }

        private void ButtonTestAutoCompleteBox1_SelectItemNull_Click(object sender, RoutedEventArgs e)
        {
            AutoCompleteBox1.SelectedItem = null;
        }
    }
}
