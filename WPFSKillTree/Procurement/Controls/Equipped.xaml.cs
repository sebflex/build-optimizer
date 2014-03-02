using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using POEApi.Model;
using Procurement.ViewModel;

namespace Procurement.Controls
{
    public partial class Equipped : UserControl
    {
        private Dictionary<string, Tuple<int, int>> absolutely;
        private EquipedItems equipped;
        private EquipedItems cashedEquipped;
        private bool showAlts = false;
        private Dictionary<GearType, List<Item>> gearPerSlot;
        private Dictionary<string, UIElement> itemViews;

        public EquipedItems EquippedGear { get { return equipped; } }
        public string Character
        {
            get { return (string)GetValue(CharacterProperty); }
            set
            {
                SetValue(CharacterProperty, value);
                render();
            }
        }

        public static readonly DependencyProperty CharacterProperty =
            DependencyProperty.Register("Character", typeof(string), typeof(Equipped), new PropertyMetadata(OnCharacterChanged));

        public Equipped()
        {
            InitializeComponent();
            absolutely = getAbolutePositions();
            GetAllGears();

            this.PreviewKeyDown += new KeyEventHandler(Equipped_PreviewKeyDown);
            this.Loaded += new RoutedEventHandler(Equipped_Loaded);

        }

        public static void OnCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Equipped instance = d as Equipped;
            if (instance.equipped == null)
                return;

            instance.render();
        }

        void Equipped_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.X)
                return;

            showAlts = !showAlts;
            render();
        }

        void Equipped_Loaded(object sender, RoutedEventArgs e)
        {
            render();
            this.Loaded -= Equipped_Loaded;
        }

        private void OnEquippedGearChanged(bool characterChanged = false)
        {
            if (ApplicationState.EquippedGearChanged != null)
                ApplicationState.EquippedGearChanged(this, new GearChangedEventArgs(){CharacterChanged = characterChanged});
        }
        //-----------------------------------------------------------------
        #region Helper Methods
        //-----------------------------------------------------------------
        private void render()
        {
            itemViews = new Dictionary<string, UIElement>();
            var equippedGear = ApplicationState.Inventory[Character].Where(i => i.inventoryId != "MainInventory");
            equipped = new EquipedItems(equippedGear);
            equipped.PropertyChanged += equipped_PropertyChanged;
            //cashedEquipped = new EquipedItems(equippedGear);
            OnEquippedGearChanged(true);
            davinci.Children.Clear();
            Dictionary<string, Item> itemsAtPosition = equipped.GetItems();

            foreach (string key in itemsAtPosition.Keys)
            {
                Grid childGrid = new Grid();
                childGrid.Margin = new Thickness(1);

                Item gearAtLocation = itemsAtPosition[key];
                if (gearAtLocation == null)
                    continue;

                if (key.Contains("Weapon") || key.Contains("Offhand"))
                {
                    bool isAlt = key.StartsWith("Alt");
                    childGrid.Height = 187;
                    childGrid.Width = 93;
                    childGrid.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    childGrid.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

                    if (!showAlts && isAlt)
                        continue;

                    if (showAlts && !isAlt)
                        continue;
                }


                Border border = getBorder();
                childGrid.Children.Add(border);

                UIElement itemView = getImage(gearAtLocation);
                childGrid.Children.Add(itemView);

                if (itemViews.ContainsKey(key))
                    itemViews[key] = childGrid;
                else
                    itemViews.Add(key, childGrid);

                Canvas.SetTop(childGrid, absolutely[key].Item1);
                Canvas.SetLeft(childGrid, absolutely[key].Item2);

                davinci.Children.Add(childGrid);
            }
            this.davinci.Focus();
        }

        void equipped_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            bool matchFound = false;
            string propertyName = e.PropertyName;
            if (equipped.propertyMapping.ContainsKey(propertyName))
                propertyName = equipped.propertyMapping[propertyName]; //need to do some mapping between inventoryID and property names

            for (int i = 0; i < ApplicationState.Inventory[Character].Count; i++)
            {
                if (ApplicationState.Inventory[Character][i].inventoryId == e.PropertyName)
                {
                    //If we found a gear in character inventory which has same slot as new item that we want to put in that slot, we update the inventory for item in that slot
                    Gear newItem = equipped.GetType().GetProperty(propertyName).GetValue(equipped) as Gear; //item that assigned to new slot
                    Gear cloned = ObjectCopier.Clone<Gear>(newItem); //creating clone of the gear so modification to it wont affect original item
                    if (newItem != null) //offhand might be removed if we select two hander for main hand
                    {
                        cloned.inventoryId = e.PropertyName; //updating inventoryId for this item
                        ApplicationState.Inventory[Character][i] = cloned; //overwriting previous item in that slot
                        matchFound = true;
                        break;
                    }
                    else //we remove offhand from inventory
                    {
                        ApplicationState.Inventory[Character].RemoveAt(i);
                        matchFound = true;
                        break;
                    }
                }
            }
            if (!matchFound)
            {
                Gear newItem = equipped.GetType().GetProperty(propertyName).GetValue(equipped) as Gear;
                newItem.inventoryId = e.PropertyName;
                ApplicationState.Inventory[Character].Add(newItem);
            }
            OnEquippedGearChanged();
        }

        private Dictionary<string, Tuple<int, int>> getAbolutePositions()
        {
            Dictionary<string, Tuple<int, int>> ret = new Dictionary<string, Tuple<int, int>>();

            ret.Add("Amulet", new Tuple<int, int>(192, 367));
            ret.Add("Belt", new Tuple<int, int>(357, 250));
            ret.Add("Helm", new Tuple<int, int>(97, 250));
            ret.Add("RingLeft", new Tuple<int, int>(251, 181));
            ret.Add("RingRight", new Tuple<int, int>(251, 367));
            ret.Add("Flask0", new Tuple<int, int>(416, 184));
            ret.Add("Flask1", new Tuple<int, int>(416, 229));
            ret.Add("Flask2", new Tuple<int, int>(416, 277));
            ret.Add("Flask3", new Tuple<int, int>(416, 324));
            ret.Add("Flask4", new Tuple<int, int>(416, 372));
            ret.Add("Weapon", new Tuple<int, int>(109, 63));
            ret.Add("Offhand", new Tuple<int, int>(109, 436));
            ret.Add("AltWeapon", new Tuple<int, int>(109, 63));
            ret.Add("AltOffhand", new Tuple<int, int>(109, 436));
            ret.Add("Boots", new Tuple<int, int>(310, 367));
            ret.Add("Armour", new Tuple<int, int>(204, 250));
            ret.Add("Gloves", new Tuple<int, int>(310, 135));

            return ret;
        }

        private UIElement getImage(Item item)
        {
            return new ItemDisplay() { DataContext = new ItemDisplayViewModel(item) };
        }

        private Border getBorder()
        {
            Border b = new Border();
            b.BorderBrush = Brushes.Transparent;
            b.BorderThickness = new Thickness(1);
            return b;
        }

        private void GetAllGears()
        {
            gearPerSlot = new Dictionary<GearType, List<Item>>();

            foreach (var stashName in ApplicationState.Stash.Keys)
            {
                var gears = ApplicationState.Stash[stashName].Get<Gear>();
                foreach (var gear in gears)
                    if (gear.Identified && gearPerSlot.ContainsKey(gear.GearType))
                        gearPerSlot[gear.GearType].Add(gear);
                    else
                    {
                        if (gear.Identified)
                        {
                            gearPerSlot.Add(gear.GearType, new List<Item>());
                            gearPerSlot[gear.GearType].Add(gear);
                        }
                    }
            }
            foreach (var character in ApplicationState.Inventory.Keys)
            {
                var gears = ApplicationState.Inventory[character].OfType<Gear>().ToList();
                foreach (Gear gear in gears)
                    if (gear.Identified && gearPerSlot.ContainsKey(gear.GearType))
                        gearPerSlot[gear.GearType].Add(gear);
                    else
                    {
                        if (gear.Identified)
                        {
                            gearPerSlot.Add(gear.GearType, new List<Item>());
                            gearPerSlot[gear.GearType].Add(gear);
                        }
                    }
            }
        }
        //-----------------------------------------------------------------
        #endregion
        //-----------------------------------------------------------------
        #region Gear Swap
        //-----------------------------------------------------------------
        bool MHIsBow = false;
        bool MHIsWand = false;

        private void Weapon_Click(object sender, RoutedEventArgs e)
        {
            List<Item> weapons = new List<Item>();
            weapons.AddRange(gearPerSlot[GearType.Sword]);
            weapons.AddRange(gearPerSlot[GearType.Axe]);
            weapons.AddRange(gearPerSlot[GearType.Bow]);
            weapons.AddRange(gearPerSlot[GearType.Claw]);
            weapons.AddRange(gearPerSlot[GearType.Dagger]);
            weapons.AddRange(gearPerSlot[GearType.Mace]);
            weapons.AddRange(gearPerSlot[GearType.Sceptre]);
            weapons.AddRange(gearPerSlot[GearType.Staff]);
            weapons.AddRange(gearPerSlot[GearType.Wand]);
            GearWindow weaponWindow = new GearWindow(weapons);
            weaponWindow.Title = "Select Weapon";
            weaponWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            weaponWindow.Show();
            weaponWindow.GearSelected += weaponWindow_GearSelected;
        }

        void weaponWindow_GearSelected(object sender, EventArgs e)
        {
            Gear selected = (e as GearEventArgs).SelectedGear;

            Grid childGrid = new Grid();
            childGrid.Margin = new Thickness(1);

            childGrid.Height = 187;
            childGrid.Width = 93;
            childGrid.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            childGrid.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

            Border border = getBorder();
            childGrid.Children.Add(border);

            childGrid.Children.Add(getImage(selected));

            Canvas.SetTop(childGrid, absolutely["Weapon"].Item1);
            Canvas.SetLeft(childGrid, absolutely["Weapon"].Item2);

            if (itemViews.ContainsKey("Weapon"))
                davinci.Children.Remove(itemViews["Weapon"]);
            davinci.Children.Add(childGrid);
            itemViews["Weapon"] = childGrid;
            equipped.Weapon = selected;
            CheckWeaponCompatibility(selected);
        }

        private void CheckWeaponCompatibility(Gear selected)
        {
            //Special check for weapons compatibility
            offhandButton.IsEnabled = true;
            MHIsBow = false;
            if (selected.GearType == GearType.Wand)
            {
                MHIsWand = true;
                if (equipped.Offhand != null && (equipped.Offhand as Gear).GearType != GearType.Wand)
                {
                    davinci.Children.Remove(itemViews["Offhand"]);
                    itemViews["Offhand"] = null;
                    equipped.Offhand = null;
                }
            }
            else
            {
                MHIsWand = false;
                if (equipped.Offhand != null && (equipped.Offhand as Gear).GearType == GearType.Wand)
                {
                    davinci.Children.Remove(itemViews["Offhand"]);
                    itemViews["Offhand"] = null;
                    equipped.Offhand = null;
                }
            }
            if (selected.GearType == GearType.Staff || selected.GearType == GearType.Bow || selected.Properties[0].Name.Contains("Two Handed"))
            {
                if (itemViews.ContainsKey("Offhand"))
                    davinci.Children.Remove(itemViews["Offhand"]);
                itemViews["Offhand"] = null;
                equipped.Offhand = null;
                if (selected.GearType != GearType.Bow)
                    offhandButton.IsEnabled = false;
                else
                    MHIsBow = true;
            }
        }

        private void Offhand_Click(object sender, RoutedEventArgs e)
        {
            List<Item> weapons = new List<Item>();
            if (MHIsBow)
                weapons.AddRange(gearPerSlot[GearType.Quiver]);
            else if (MHIsWand)
                weapons.AddRange(gearPerSlot[GearType.Wand]);
            else
            {
                weapons.AddRange(gearPerSlot[GearType.Sword].Where(gear => !gear.Properties[0].Name.Contains("Two Handed")));
                weapons.AddRange(gearPerSlot[GearType.Axe].Where(gear => !gear.Properties[0].Name.Contains("Two Handed")));
                weapons.AddRange(gearPerSlot[GearType.Claw]);
                weapons.AddRange(gearPerSlot[GearType.Dagger]);
                weapons.AddRange(gearPerSlot[GearType.Mace].Where(gear => !gear.Properties[0].Name.Contains("Two Handed")));
                weapons.AddRange(gearPerSlot[GearType.Sceptre]);
                weapons.AddRange(gearPerSlot[GearType.Shield]);
            }
            GearWindow offHandWindow = new GearWindow(weapons);
            offHandWindow.Title = "Select Offhand";
            offHandWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            offHandWindow.Show();
            offHandWindow.GearSelected += offHandWindow_GearSelected;
        }

        void offHandWindow_GearSelected(object sender, EventArgs e)
        {
            Gear selected = (e as GearEventArgs).SelectedGear;

            Grid childGrid = new Grid();
            childGrid.Margin = new Thickness(1);


            childGrid.Height = 187;
            childGrid.Width = 93;
            childGrid.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            childGrid.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

            Border border = getBorder();
            childGrid.Children.Add(border);

            childGrid.Children.Add(getImage(selected));

            Canvas.SetTop(childGrid, absolutely["Offhand"].Item1);
            Canvas.SetLeft(childGrid, absolutely["Offhand"].Item2);

            if (itemViews.ContainsKey("Offhand"))
                davinci.Children.Remove(itemViews["Offhand"]);
            davinci.Children.Add(childGrid);
            itemViews["Offhand"] = childGrid;
            equipped.Offhand = selected;
        }

        private void Helm_Click(object sender, RoutedEventArgs e)
        {
            GearWindow helmetWindow = new GearWindow(gearPerSlot[GearType.Helmet]);
            helmetWindow.Title = "Select Helm";
            helmetWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            helmetWindow.Show();
            helmetWindow.GearSelected += helmetWindow_GearSelected;
        }

        void helmetWindow_GearSelected(object sender, EventArgs e)
        {
            Gear selected = (e as GearEventArgs).SelectedGear;

            Grid childGrid = new Grid();
            childGrid.Margin = new Thickness(1);

            Border border = getBorder();
            childGrid.Children.Add(border);

            childGrid.Children.Add(getImage(selected));

            Canvas.SetTop(childGrid, absolutely["Helm"].Item1);
            Canvas.SetLeft(childGrid, absolutely["Helm"].Item2);

            if (itemViews.ContainsKey("Helm"))
                davinci.Children.Remove(itemViews["Helm"]);
            davinci.Children.Add(childGrid);
            itemViews["Helm"] = childGrid;
            equipped.Helm = selected;
        }

        private void Armour_Click(object sender, RoutedEventArgs e)
        {
            GearWindow armourWindow = new GearWindow(gearPerSlot[GearType.Chest]);
            armourWindow.Title = "Select Armour";
            armourWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            armourWindow.Show();
            armourWindow.GearSelected += armourWindow_GearSelected;
        }

        void armourWindow_GearSelected(object sender, EventArgs e)
        {
            Gear selected = (e as GearEventArgs).SelectedGear;

            Grid childGrid = new Grid();
            childGrid.Margin = new Thickness(1);

            Border border = getBorder();
            childGrid.Children.Add(border);

            childGrid.Children.Add(getImage(selected));

            Canvas.SetTop(childGrid, absolutely["Armour"].Item1);
            Canvas.SetLeft(childGrid, absolutely["Armour"].Item2);

            if (itemViews.ContainsKey("Armour"))
                davinci.Children.Remove(itemViews["Armour"]);
            davinci.Children.Add(childGrid);
            itemViews["Armour"] = childGrid;
            equipped.Armour = selected;
        }

        private void Belt_Click(object sender, RoutedEventArgs e)
        {
            GearWindow beltWindow = new GearWindow(gearPerSlot[GearType.Belt]);
            beltWindow.Title = "Select Belt";
            beltWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            beltWindow.Show();
            beltWindow.GearSelected += beltWindow_GearSelected;
        }

        void beltWindow_GearSelected(object sender, EventArgs e)
        {
            Gear selected = (e as GearEventArgs).SelectedGear;

            Grid childGrid = new Grid();
            childGrid.Margin = new Thickness(1);

            Border border = getBorder();
            childGrid.Children.Add(border);

            childGrid.Children.Add(getImage(selected));

            Canvas.SetTop(childGrid, absolutely["Belt"].Item1);
            Canvas.SetLeft(childGrid, absolutely["Belt"].Item2);

            if (itemViews.ContainsKey("Belt"))
                davinci.Children.Remove(itemViews["Belt"]);
            davinci.Children.Add(childGrid);
            itemViews["Belt"] = childGrid;
            equipped.Belt = selected;
        }

        private void Gloves_Click(object sender, RoutedEventArgs e)
        {
            GearWindow glovesWindow = new GearWindow(gearPerSlot[GearType.Gloves]);
            glovesWindow.Title = "Select Gloves";
            glovesWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            glovesWindow.Show();
            glovesWindow.GearSelected += glovesWindow_GearSelected;
        }

        void glovesWindow_GearSelected(object sender, EventArgs e)
        {
            Gear selected = (e as GearEventArgs).SelectedGear;

            Grid childGrid = new Grid();
            childGrid.Margin = new Thickness(1);

            Border border = getBorder();
            childGrid.Children.Add(border);

            childGrid.Children.Add(getImage(selected));

            Canvas.SetTop(childGrid, absolutely["Gloves"].Item1);
            Canvas.SetLeft(childGrid, absolutely["Gloves"].Item2);

            if (itemViews.ContainsKey("Gloves"))
                davinci.Children.Remove(itemViews["Gloves"]);
            davinci.Children.Add(childGrid);
            itemViews["Gloves"] = childGrid;
            equipped.Gloves = selected;
        }

        private void Boots_Click(object sender, RoutedEventArgs e)
        {
            GearWindow bootsWindow = new GearWindow(gearPerSlot[GearType.Boots]);
            bootsWindow.Title = "Select Boots";
            bootsWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            bootsWindow.Show();
            bootsWindow.GearSelected += bootsWindow_GearSelected;
        }

        void bootsWindow_GearSelected(object sender, EventArgs e)
        {
            Gear selected = (e as GearEventArgs).SelectedGear;

            Grid childGrid = new Grid();
            childGrid.Margin = new Thickness(1);

            Border border = getBorder();
            childGrid.Children.Add(border);

            childGrid.Children.Add(getImage(selected));

            Canvas.SetTop(childGrid, absolutely["Boots"].Item1);
            Canvas.SetLeft(childGrid, absolutely["Boots"].Item2);

            if (itemViews.ContainsKey("Boots"))
                davinci.Children.Remove(itemViews["Boots"]);
            davinci.Children.Add(childGrid);
            itemViews["Boots"] = childGrid;
            equipped.Boots = selected;
        }

        private void RingRight_Click(object sender, RoutedEventArgs e)
        {
            GearWindow ringRightWindow = new GearWindow(gearPerSlot[GearType.Ring]);
            ringRightWindow.Title = "Select Right Ring";
            ringRightWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            ringRightWindow.Show();
            ringRightWindow.GearSelected += ringRightWindow_GearSelected;
        }

        void ringRightWindow_GearSelected(object sender, EventArgs e)
        {
            Gear selected = (e as GearEventArgs).SelectedGear;

            Grid childGrid = new Grid();
            childGrid.Margin = new Thickness(1);

            Border border = getBorder();
            childGrid.Children.Add(border);

            childGrid.Children.Add(getImage(selected));

            Canvas.SetTop(childGrid, absolutely["RingRight"].Item1);
            Canvas.SetLeft(childGrid, absolutely["RingRight"].Item2);

            if (itemViews.ContainsKey("RingRight"))
                davinci.Children.Remove(itemViews["RingRight"]);
            davinci.Children.Add(childGrid);
            itemViews["RingRight"] = childGrid;
            equipped.RingRight = selected;
        }

        private void RingLeft_Click(object sender, RoutedEventArgs e)
        {
            GearWindow ringLeftWindow = new GearWindow(gearPerSlot[GearType.Ring]);
            ringLeftWindow.Title = "Select Left Ring";
            ringLeftWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            ringLeftWindow.Show();
            ringLeftWindow.GearSelected += ringLeftWindow_GearSelected;
        }

        void ringLeftWindow_GearSelected(object sender, EventArgs e)
        {
            Gear selected = (e as GearEventArgs).SelectedGear;

            Grid childGrid = new Grid();
            childGrid.Margin = new Thickness(1);

            Border border = getBorder();
            childGrid.Children.Add(border);

            childGrid.Children.Add(getImage(selected));

            Canvas.SetTop(childGrid, absolutely["RingLeft"].Item1);
            Canvas.SetLeft(childGrid, absolutely["RingLeft"].Item2);

            if (itemViews.ContainsKey("RingLeft"))
                davinci.Children.Remove(itemViews["RingLeft"]);
            davinci.Children.Add(childGrid);
            itemViews["RingLeft"] = childGrid;
            equipped.RingLeft = selected;
        }

        private void Amulet_Click(object sender, RoutedEventArgs e)
        {
            GearWindow amuletWindow = new GearWindow(gearPerSlot[GearType.Amulet]);
            amuletWindow.Title = "Select Amulet";
            amuletWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            amuletWindow.Show();
            amuletWindow.GearSelected += amuletWindow_GearSelected;
        }

        void amuletWindow_GearSelected(object sender, EventArgs e)
        {
            Gear selected = (e as GearEventArgs).SelectedGear;

            Grid childGrid = new Grid();
            childGrid.Margin = new Thickness(1);

            Border border = getBorder();
            childGrid.Children.Add(border);

            childGrid.Children.Add(getImage(selected));

            Canvas.SetTop(childGrid, absolutely["Amulet"].Item1);
            Canvas.SetLeft(childGrid, absolutely["Amulet"].Item2);

            if (itemViews.ContainsKey("Amulet"))
                davinci.Children.Remove(itemViews["Amulet"]);
            davinci.Children.Add(childGrid);
            itemViews["Amulet"] = childGrid;
            equipped.Amulet = selected;
        }
        //-----------------------------------------------------------------
        #endregion
    }
}
