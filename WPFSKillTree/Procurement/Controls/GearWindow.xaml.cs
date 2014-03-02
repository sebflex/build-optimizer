using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using POEApi.Model;
using Procurement.ViewModel;


namespace Procurement.Controls
{
    /// <summary>
    /// Interaction logic for TextureWindow.xaml
    /// </summary>
    public partial class GearWindow
    {
        List<UIElement> cachedControls;
        List<Item> items;

        public event EventHandler GearSelected;

        public GearWindow(List<Item> items)
        {
            InitializeComponent();
            cachedControls = new List<UIElement>();
            this.items = items;
        }

        private void DisplayImages()
        {
            foreach (var item in items)
                CreateBoxItem(item);
        }

        private void CreateBoxItem(Item item)
        {
            ListBoxItem boxItem = new ListBoxItem();

            boxItem.Content = getImage(item);
            boxItem.Tag = item;
            gearViewBox.Items.Add(boxItem);
            cachedControls.Add(boxItem);
        }

        private UIElement getImage(Item item)
        {
            return new ItemDisplay() { DataContext = new ItemDisplayViewModel(item) };
        }

        private void gearViewBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gearViewBox.SelectedItem != null)
            {
                Gear selected = (gearViewBox.SelectedItem as ListBoxItem).Tag as Gear;
                GearEventArgs ge = new GearEventArgs() { SelectedGear = selected };
                OnGearSelected(ge);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayImages();
        }

        private void SearchTextBox_Search(object sender, RoutedEventArgs e)
        {
            var searchTextBox = sender as UIControls.SearchTextBox;
            gearViewBox.Items.Clear();
            foreach (ListBoxItem item in cachedControls)
            {
                string name = (item.Tag as Gear).Name + " " + (item.Tag as Gear).TypeLine;
                if (name.ToLower().Contains(searchTextBox.Text.ToLower()))
                    gearViewBox.Items.Add(item);
            }
        }

        private void RarityRB_Checked(object sender, RoutedEventArgs e)
        {
            if (gearViewBox != null)
            {
                if ((sender as RadioButton).Name == "normalRarity")
                {
                    gearViewBox.Items.Clear();
                    foreach (ListBoxItem item in cachedControls)
                        if ((item.Tag as Gear).Rarity == Rarity.Normal)
                            gearViewBox.Items.Add(item);
                }
                else if ((sender as RadioButton).Name == "magicRarity")
                {
                    gearViewBox.Items.Clear();
                    foreach (ListBoxItem item in cachedControls)
                        if ((item.Tag as Gear).Rarity == Rarity.Magic)
                            gearViewBox.Items.Add(item);
                }
                else if ((sender as RadioButton).Name == "rareRarity")
                {
                    gearViewBox.Items.Clear();
                    foreach (ListBoxItem item in cachedControls)
                        if ((item.Tag as Gear).Rarity == Rarity.Rare)
                            gearViewBox.Items.Add(item);
                }
                else if ((sender as RadioButton).Name == "uniqueRarity")
                {
                    gearViewBox.Items.Clear();
                    foreach (ListBoxItem item in cachedControls)
                        if ((item.Tag as Gear).Rarity == Rarity.Unique)
                            gearViewBox.Items.Add(item);
                }
                else
                {
                    gearViewBox.Items.Clear();
                    foreach (ListBoxItem item in cachedControls)
                        gearViewBox.Items.Add(item);
                }
            }
        }

        private void OnGearSelected(EventArgs e)
        {
            EventHandler materialSelected = GearSelected;
            if (materialSelected != null)
                materialSelected(this, e);
        }

    }
    public class GearEventArgs : EventArgs
    {
        public Gear SelectedGear;
    }
}
