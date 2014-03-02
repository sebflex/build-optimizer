using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace POEApi.Model
{
    public class EquipedItems
    {
        private Dictionary<string, PropertyInfo> properties;
        public Dictionary<string, string> propertyMapping;
        public event PropertyChangedEventHandler PropertyChanged;

        private Item amulet;
        public Item Amulet { get { return amulet; } set { SetField(ref amulet, value); } }
        private Item belt;
        public Item Belt { get { return belt; } set { SetField(ref belt, value); } }
        private Item helm;
        public Item Helm { get { return helm; } set { SetField(ref helm, value); } }
        private Item ringLeft;
        public Item RingLeft { get { return ringLeft; } set { SetField(ref ringLeft, value); } }
        private Item ringRight;
        public Item RingRight { get { return ringRight; } set { SetField(ref ringRight, value); } }
        public Item Flask0 { get; set; }
        public Item Flask1 { get; set; }
        public Item Flask2 { get; set; }
        public Item Flask3 { get; set; }
        public Item Flask4 { get; set; }
        private Item weapon;
        public Item Weapon { get { return weapon; } set { SetField(ref weapon, value); } }
        private Item offhand;
        public Item Offhand { get { return offhand; } set { SetField(ref offhand, value); } }
        private Item altWeapon;
        public Item AltWeapon { get { return altWeapon; } set { SetField(ref altWeapon, value); } }
        private Item altOffhand;
        public Item AltOffhand { get { return altOffhand; } set { SetField(ref altOffhand, value); } }
        private Item boots;
        public Item Boots { get { return boots; } set { SetField(ref boots, value); } }
        private Item armour;
        public Item Armour { get { return armour; } set { SetField(ref armour, value); } }
        private Item gloves;
        public Item Gloves { get { return gloves; } set { SetField(ref gloves, value); } }

        private Dictionary<string, Item> mapping;

        public EquipedItems(IEnumerable<Item> items)
        {
            propertyMapping = new Dictionary<string, string>();
            properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name);
            propertyMapping.Add("Ring", "RingLeft");
            propertyMapping.Add("Ring2", "RingRight");
            propertyMapping.Add("Weapon2", "AltWeapon");
            propertyMapping.Add("Offhand2", "AltOffhand");
            propertyMapping.Add("BodyArmour", "Armour");

            foreach (var item in items)
                setProperty(item);
        }

        private void setProperty(Item item)
        {
            string target = item.inventoryId;

            if (propertyMapping.ContainsKey(item.inventoryId))
                target = propertyMapping[item.inventoryId];

            if (item.inventoryId == "Flask")
                target = item.inventoryId + item.X;

            properties[target].SetValue(this, item, null);
        }

        public Dictionary<string, Item> GetItems()
        {
            return properties.Keys.ToDictionary(prop => prop, prop => (Item)properties[prop].GetValue(this, null));
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            foreach (string name in propertyMapping.Keys)
            {
                if (propertyMapping[name] == propertyName)
                {
                    propertyName = name;
                    break;
                }
            }
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
