using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Data;
using System.Xml;
using System.Text.RegularExpressions;
using Raven.Json.Linq;

namespace POESKillTree
{
    public class CharItemData
    {
        #region Fields
        //-------------------------------------------------------------------------------
        private List<Attribute> attribList = new List<Attribute>();
        public ListCollectionView AttribCollectionView;
        public List<Attribute> NonLocalMods = new List<Attribute>();
        public List<Item> EquippedItems = new List<Item>();
        public List<Item> NonEquippedItems = new List<Item>();
        Dictionary<string, float> skillCastTimes = new Dictionary<string, float>();
        #endregion
        //-------------------------------------------------------------------------------
        #region Creation & clean Up
        //-------------------------------------------------------------------------------
        public CharItemData(string path)
        {
            ReadSkillCastTimes();
            ReadItemsData(path);
            attribList.Clear();
            NonLocalMods.Clear();
            AttribCollectionView = new ListCollectionView(attribList);
            SetAttributes();

            PropertyGroupDescription pgd = new PropertyGroupDescription("Group");
            AttribCollectionView.GroupDescriptions.Add(pgd);
            AttribCollectionView.CustomSort = new NumberLessStringComparer();

            AttribCollectionView.Refresh();
        }

        public CharItemData(POEApi.Model.EquipedItems equipped, List<POEApi.Model.Item> nonEquipped)
        {
            ReadSkillCastTimes();
            ReadItemsData(equipped);
            ReadItemsData(nonEquipped);
            attribList.Clear();
            NonLocalMods.Clear();
            AttribCollectionView = new ListCollectionView(attribList);
            SetAttributes();

            PropertyGroupDescription pgd = new PropertyGroupDescription("Group");
            AttribCollectionView.GroupDescriptions.Add(pgd);
            AttribCollectionView.CustomSort = new NumberLessStringComparer();

            AttribCollectionView.Refresh();
        }

        #endregion
        //-------------------------------------------------------------------------------
        #region Methods
        private void ReadItemsData(string path)
        {
            RavenJObject jObject = RavenJObject.Parse(File.ReadAllText(path));
            foreach (RavenJObject jobj in (RavenJArray)jObject["items"])
            {
                string id = jobj["inventoryId"].Value<string>();
                Item item;
                switch (id)
                {
                    case "BodyArmour":
                        item = new Item(Item.ItemClass.Armor, jobj, skillCastTimes);
                        EquippedItems.Add(item);
                        break;
                    case "Ring":
                    case "Ring2":
                        item = new Item(Item.ItemClass.Ring, jobj, skillCastTimes);
                        EquippedItems.Add(item);
                        break;
                    case "Gloves":
                        item = new Item(Item.ItemClass.Gloves, jobj, skillCastTimes);
                        EquippedItems.Add(item);
                        break;
                    case "Weapon":
                        item = new Item(Item.ItemClass.MainHand, jobj, skillCastTimes);
                        EquippedItems.Add(item);
                        break;
                    case "Offhand":
                        item = new Item(Item.ItemClass.OffHand, jobj, skillCastTimes);
                        EquippedItems.Add(item);
                        break;
                    case "Helm":
                        item = new Item(Item.ItemClass.Helm, jobj, skillCastTimes);
                        EquippedItems.Add(item);
                        break;
                    case "Boots":
                        item = new Item(Item.ItemClass.Boots, jobj, skillCastTimes);
                        EquippedItems.Add(item);
                        break;
                    case "Amulet":
                        item = new Item(Item.ItemClass.Amulet, jobj, skillCastTimes);
                        EquippedItems.Add(item);
                        break;
                    case "Belt":
                        item = new Item(Item.ItemClass.Belt, jobj, skillCastTimes);
                        EquippedItems.Add(item);
                        break;
                    default:
                        if (jobj.ContainsKey("frameType"))
                            if (jobj["frameType"].Value<int>() == 4)
                            {
                                item = new Item(Item.ItemClass.Gem, jobj, skillCastTimes);
                                item.ProcessGem(item);
                                NonEquippedItems.Add(item);
                            }
                        break;
                }
            }
        }

        private void ReadItemsData(POEApi.Model.EquipedItems equippedGear)
        {
            Item item;
            if (equippedGear.Armour != null)
            {
                item = new Item(Item.ItemClass.Armor, equippedGear.Armour as POEApi.Model.Gear, skillCastTimes);
                EquippedItems.Add(item);
            }
            if (equippedGear.RingLeft != null)
            {
                item = new Item(Item.ItemClass.Ring, equippedGear.RingLeft as POEApi.Model.Gear, skillCastTimes);
                EquippedItems.Add(item);
            }
            if (equippedGear.RingRight != null)
            {
                item = new Item(Item.ItemClass.Ring, equippedGear.RingRight as POEApi.Model.Gear, skillCastTimes);
                EquippedItems.Add(item);
            }
            if (equippedGear.Gloves != null)
            {
                item = new Item(Item.ItemClass.Gloves, equippedGear.Gloves as POEApi.Model.Gear, skillCastTimes);
                EquippedItems.Add(item);
            }
            if (equippedGear.Weapon != null)
            {
                item = new Item(Item.ItemClass.MainHand, equippedGear.Weapon as POEApi.Model.Gear, skillCastTimes);
                EquippedItems.Add(item);
            }
            if (equippedGear.Offhand != null)
            {
                item = new Item(Item.ItemClass.OffHand, equippedGear.Offhand as POEApi.Model.Gear, skillCastTimes);
                EquippedItems.Add(item);
            }
            if (equippedGear.Helm != null)
            {
                item = new Item(Item.ItemClass.Helm, equippedGear.Helm as POEApi.Model.Gear, skillCastTimes);
                EquippedItems.Add(item);
            }
            if (equippedGear.Boots != null)
            {
                item = new Item(Item.ItemClass.Boots, equippedGear.Boots as POEApi.Model.Gear, skillCastTimes);
                EquippedItems.Add(item);
            }
            if (equippedGear.Amulet != null)
            {
                item = new Item(Item.ItemClass.Amulet, equippedGear.Amulet as POEApi.Model.Gear, skillCastTimes);
                EquippedItems.Add(item);
            }
            if (equippedGear.Belt != null)
            {
                item = new Item(Item.ItemClass.Belt, equippedGear.Belt as POEApi.Model.Gear, skillCastTimes);
                EquippedItems.Add(item);
            }
        }

        private void ReadItemsData(List<POEApi.Model.Item> nonEquippedGears)
        {
            Item item;
            foreach (var nonEquipped in nonEquippedGears)
            {
                if (nonEquipped.ItemType == POEApi.Model.ItemType.Gem)
                {
                    item = new Item(Item.ItemClass.Gem, nonEquipped, skillCastTimes);
                    item.ProcessGem(item);
                    NonEquippedItems.Add(item);
                }
                else if (nonEquipped.ItemType == POEApi.Model.ItemType.Gear)
                {
                    item = new Item(Item.ItemClass.Other, nonEquipped, skillCastTimes);
                    NonEquippedItems.Add(item);
                }
            }
            

        }

        private void SetAttributes()
        {
            foreach (Item item in EquippedItems)
            {
                if (item.Properties != null)
                    foreach (KeyValuePair<string, List<float>> attr in item.Properties)
                    {
                        attribList.Add(new Attribute(attr.Key, attr.Value, item.Class.ToString()));
                    }

                foreach (Item.Mod mod in item.Mods)
                {
                    if (!CheckForCompoundMods(mod, attribList))
                    {
                        Attribute existedAttrib = null;
                        existedAttrib = attribList.Find(ad => ad.TextAttribute == mod.Attribute && ad.Group == (mod.isLocal ? item.Class.ToString() : "Global Mods"));
                        if (existedAttrib == null)
                        {
                            attribList.Add(new Attribute(mod.Attribute, mod.Values, (mod.isLocal ? item.Class.ToString() : "Global Mods")));
                        }
                        else
                        {
                            existedAttrib.Add(mod.Attribute, mod.Values);
                        }
                    }
                    if (!mod.isLocal)
                    {
                        if (!CheckForCompoundMods(mod, NonLocalMods))
                        {
                            Attribute existedAttrib = null;
                            existedAttrib = NonLocalMods.Find(ad => ad.TextAttribute == mod.Attribute);
                            if (existedAttrib == null)
                            {
                                NonLocalMods.Add(new Attribute(mod.Attribute, mod.Values, ""));
                            }
                            else
                            {
                                existedAttrib.Add(mod.Attribute, mod.Values);
                            }
                        }
                    }
                }
            }
        }

        private bool CheckForCompoundMods(Item.Mod mod, List<Attribute> modList)
        {
            Attribute existedAttrib = null;
            if (mod.Attribute.Contains("to Dexterity and Intelligence"))
            {
                existedAttrib = modList.Find(ad => ad.TextAttribute == "+# to Dexterity");
                if (existedAttrib == null)
                    modList.Add(new Attribute("+# to Dexterity", mod.Values, "Global Mods"));
                else
                    existedAttrib.Add("+# to Dexterity", mod.Values);
                existedAttrib = modList.Find(ad => ad.TextAttribute == "+# to Intelligence");
                if (existedAttrib == null)
                    modList.Add(new Attribute("+# to Intelligence", mod.Values, "Global Mods"));
                else
                    existedAttrib.Add("+# to Intelligence", mod.Values);
                return true;
            }
            else if (mod.Attribute.Contains("to Strength and Intelligence"))
            {
                existedAttrib = modList.Find(ad => ad.TextAttribute == "+# to Strength");
                if (existedAttrib == null)
                    modList.Add(new Attribute("+# to Strength", mod.Values, "Global Mods"));
                else
                    existedAttrib.Add("+# to Strength", mod.Values);
                existedAttrib = modList.Find(ad => ad.TextAttribute == "+# to Intelligence");
                if (existedAttrib == null)
                    modList.Add(new Attribute("+# to Intelligence", mod.Values, "Global Mods"));
                else
                    existedAttrib.Add("+# to Intelligence", mod.Values);
                return true;
            }
            else if (mod.Attribute.Contains("to Strength and Dexterity"))
            {
                existedAttrib = modList.Find(ad => ad.TextAttribute == "+# to Strength");
                if (existedAttrib == null)
                    modList.Add(new Attribute("+# to Strength", mod.Values, "Global Mods"));
                else
                    existedAttrib.Add("+# to Strength", mod.Values);
                existedAttrib = modList.Find(ad => ad.TextAttribute == "+# to Dexterity");
                if (existedAttrib == null)
                    modList.Add(new Attribute("+# to Dexterity", mod.Values, "Global Mods"));
                else
                    existedAttrib.Add("+# to Dexterity", mod.Values);
                return true;
            }
            else if (mod.Attribute.Contains("to all Attributes"))
            {
                existedAttrib = modList.Find(ad => ad.TextAttribute == "+# to Strength");
                if (existedAttrib == null)
                    modList.Add(new Attribute("+# to Strength", mod.Values, "Global Mods"));
                else
                    existedAttrib.Add("+# to Strength", mod.Values);
                existedAttrib = modList.Find(ad => ad.TextAttribute == "+# to Dexterity");
                if (existedAttrib == null)
                    modList.Add(new Attribute("+# to Dexterity", mod.Values, "Global Mods"));
                else
                    existedAttrib.Add("+# to Dexterity", mod.Values);
                existedAttrib = modList.Find(ad => ad.TextAttribute == "+# to Intelligence");
                if (existedAttrib == null)
                    modList.Add(new Attribute("+# to Intelligence", mod.Values, "Global Mods"));
                else
                    existedAttrib.Add("+# to Intelligence", mod.Values);
                return true;
            }
            else if (mod.Attribute.Contains("to Cold and Lightning Res"))
            {
                existedAttrib = modList.Find(ad => ad.TextAttribute == "+#% to Cold Resistance");
                if (existedAttrib == null)
                    modList.Add(new Attribute("+#% to Cold Resistance", mod.Values, "Global Mods"));
                else
                    existedAttrib.Add("+#% to Cold Resistance", mod.Values);
                existedAttrib = modList.Find(ad => ad.TextAttribute == "+#% to Lightning Resistance");
                if (existedAttrib == null)
                    modList.Add(new Attribute("+#% to Lightning Resistance", mod.Values, "Global Mods"));
                else
                    existedAttrib.Add("+#% to Lightning Resistance", mod.Values);
                return true;
            }
            else if (mod.Attribute.Contains("to Fire and Cold Res"))
            {
                existedAttrib = modList.Find(ad => ad.TextAttribute == "+#% to Cold Resistance");
                if (existedAttrib == null)
                    modList.Add(new Attribute("+#% to Cold Resistance", mod.Values, "Global Mods"));
                else
                    existedAttrib.Add("+#% to Cold Resistance", mod.Values);
                existedAttrib = modList.Find(ad => ad.TextAttribute == "+#% to Fire Resistance");
                if (existedAttrib == null)
                    modList.Add(new Attribute("+#% to Fire Resistance", mod.Values, "Global Mods"));
                else
                    existedAttrib.Add("+#% to Fire Resistance", mod.Values);
                return true;
            }
            else if (mod.Attribute.Contains("to Fire and Lightning Res"))
            {
                existedAttrib = modList.Find(ad => ad.TextAttribute == "+#% to Lightning Resistance");
                if (existedAttrib == null)
                    modList.Add(new Attribute("+#% to Lightning Resistance", mod.Values, "Global Mods"));
                else
                    existedAttrib.Add("+#% to Lightning Resistance", mod.Values);
                existedAttrib = modList.Find(ad => ad.TextAttribute == "+#% to Fire Resistance");
                if (existedAttrib == null)
                    modList.Add(new Attribute("+#% to Fire Resistance", mod.Values, "Global Mods"));
                else
                    existedAttrib.Add("+#% to Fire Resistance", mod.Values);
                return true;
            }
            return false;
        }

        private void ReadSkillCastTimes()
        {
            try
            {
                var reader = new StreamReader(File.OpenRead("Data/ActiveSkills.csv"));
                var line = reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    var values = line.Split(',');
                    float value;
                    if (float.TryParse(values[1], out value))
                    {
                        skillCastTimes.Add(values[0], value);
                    }
                    else
                        System.Windows.Forms.MessageBox.Show("Wrong value in 'ActiveSkills.csv' Calcuated spell dps will be wrong", "Wrong Input", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Error in loading \"Data\\ActiveSkills.csv\"", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                System.Windows.Application.Current.Shutdown();
            }
        }
        #endregion
        //-------------------------------------------------------------------------------
        #region Internal Classes
        //-------------------------------------------------------------------------------
        public class Item
        {
            //-------------------------------------------------------------------------------
            #region Fields
            //-------------------------------------------------------------------------------
            static Regex colorcleaner = new Regex("\\<.+?\\>");
            static Regex numberfilter = new Regex("[0-9]*\\.?[0-9]+");
            public ItemClass Class;
            public Dictionary<string, List<float>> Properties;
            public Dictionary<string, List<float>> Requirements;
            Dictionary<string, float> skillCastTimes;
            public List<Mod> Mods;
            public List<SkillGem> Gems;
            public Dictionary<int, List<SocketColor>> Sockets; //linked grp, colour
            public string Name { get; private set; }
            public string Description { get; private set; } //for gems
            public bool IsSupport { get; private set; } //for gems
            public string TypeLine { get; private set; }
            public string KeyWords { get; private set; } //for gems
            public string IconUrl { get; private set; }
            public string WeaponType { get; private set; } //for weapons
            #endregion
            //-------------------------------------------------------------------------------
            public Item(ItemClass iClass, RavenJObject val, Dictionary<string, float> skillCastTimes)
            {
                this.skillCastTimes = skillCastTimes;
                Mods = new List<Mod>();
                Requirements = new Dictionary<string, List<float>>();
                Properties = new Dictionary<string, List<float>>();
                Class = iClass;
                WeaponType = "";

                Gems = new List<SkillGem>();

                TypeLine = val["typeLine"].Value<string>();
                Name = val["name"].Value<string>();

                if (val.ContainsKey("icon"))
                {
                    IconUrl = val["icon"].Value<string>();
                }

                if (val.ContainsKey("properties"))
                {

                    if (Class == ItemClass.MainHand || Class == ItemClass.OffHand)
                    {
                        var nameObj = (((RavenJArray)val["properties"])[0] as RavenJObject)["name"];
                        WeaponType = nameObj != null ? nameObj.ToString() : "";
                    }
                    else if (Class == ItemClass.Gem)
                    {
                        var nameObj = (((RavenJArray)val["properties"])[0] as RavenJObject)["name"];
                        KeyWords = nameObj != null ? nameObj.ToString() : "";
                    }
                    ReadProperties(val);
                }
                else
                    if (Class == ItemClass.OffHand)
                        WeaponType = "Quiver";

                if (val.ContainsKey("requirements") && Class == ItemClass.Gem)
                {
                    foreach (RavenJObject obj in (RavenJArray)val["requirements"])
                    {
                        string name = obj["name"].Value<string>();
                        List<float> values = new List<float>();
                        foreach (RavenJArray jva in (RavenJArray)obj["values"])
                        {
                            values.Add(jva[0].Value<float>());
                        }
                        Requirements.Add(name, values);
                    }
                }
                //if item has explicit Mods we add it to it's Mods list
                if (val.ContainsKey("explicitMods"))
                    foreach (string s in val["explicitMods"].Values<string>())
                    {
                        Mod modifier = new Mod(s.Replace("Additional ", ""), this.Class, WeaponType == "Quiver");
                        Mods.Add(modifier);
                    }
                //if item has implicit Mods we add it to it's Mods list
                if (val.ContainsKey("implicitMods"))
                    foreach (string s in val["implicitMods"].Values<string>())
                    {
                        Mod modifier = new Mod(s.Replace("Additional ", ""), this.Class, WeaponType == "Quiver");
                        Mods.Add(modifier);
                    }
                if (val.ContainsKey("sockets"))
                {
                    Sockets = new Dictionary<int, List<SocketColor>>();
                    ReadSockets(val);
                }
                if (val.ContainsKey("socketedItems"))
                    ReadGems(val);

                if (val.ContainsKey("secDescrText"))
                    Description = val["secDescrText"].Value<string>();

                if (val.ContainsKey("support"))
                    IsSupport = val["support"].Value<bool>();
                
            }

            public Item(ItemClass iClass, POEApi.Model.Item item, Dictionary<string, float> skillCastTimes)
            {

                this.skillCastTimes = skillCastTimes;
                Mods = new List<Mod>();
                Requirements = new Dictionary<string, List<float>>();
                Properties = new Dictionary<string, List<float>>();
                Class = iClass;
                WeaponType = "";
                Gems = new List<SkillGem>();

                TypeLine = item.TypeLine;
                Name = item.Name;
                IconUrl = item.IconURL;

                if (item.Properties != null && item.Properties.Count > 0)
                {
                    if (Class == ItemClass.MainHand || Class == ItemClass.OffHand)
                    {
                        WeaponType = item.Properties[0].Name;
                    }
                    else if (Class == ItemClass.Gem)
                    {
                        KeyWords = item.Properties[0].Name;
                    }
                    ReadProperties(item.Properties);
                }
                else
                    if (Class == ItemClass.OffHand)
                        WeaponType = "Quiver";

                if (item is POEApi.Model.Gem && (item as POEApi.Model.Gem).Requirements != null)
                {
                    foreach (var req in (item as POEApi.Model.Gem).Requirements)
                    {
                        string name = req.Name;
                        List<float> values = new List<float>();
                        float value = float.Parse(req.Value);
                        values.Add(value);
                        Requirements.Add(name, values);
                    }
                }
                //if item has explicit Mods we add it to it's Mods list
                if (item.Explicitmods != null)
                    foreach (string s in item.Explicitmods)
                    {
                        Mod modifier = new Mod(s.Replace("Additional ", ""), this.Class, WeaponType == "Quiver");
                        Mods.Add(modifier);
                    }
                //if item has implicit Mods we add it to it's Mods list
                if (item is POEApi.Model.Gear && (item as POEApi.Model.Gear).Implicitmods != null)
                    foreach (string s in (item as POEApi.Model.Gear).Implicitmods)
                    {
                        Mod modifier = new Mod(s.Replace("Additional ", ""), this.Class, WeaponType == "Quiver");
                        Mods.Add(modifier);
                    }
                if (item is POEApi.Model.Gear && (item as POEApi.Model.Gear).Sockets != null)
                {
                    Sockets = new Dictionary<int, List<SocketColor>>();
                    ReadSockets((item as POEApi.Model.Gear).Sockets);
                }

                if (item is POEApi.Model.Gear && (item as POEApi.Model.Gear).SocketedItems != null)
                {
                    ReadGems((item as POEApi.Model.Gear).SocketedItems);
                }
                if (item.SecDescrText != null)
                    Description = item.SecDescrText;

                if (item is POEApi.Model.Gem)
                    IsSupport = (item as POEApi.Model.Gem).IsSupport;
            }

            private void ReadSockets(RavenJObject val)
            {
                foreach (RavenJObject obj in (RavenJArray)val["sockets"])
                {
                    int group = obj["group"].Value<int>();
                    string attr = obj["attr"].Value<string>();
                    switch (attr)
                    {
                        case "D":
                            if (Sockets.ContainsKey(group))
                                Sockets[group].Add(SocketColor.Green);
                            else
                            {
                                Sockets.Add(group, new List<SocketColor>());
                                Sockets[group].Add(SocketColor.Green);
                            }
                            break;
                        case "S":
                            if (Sockets.ContainsKey(group))
                                Sockets[group].Add(SocketColor.Red);
                            else
                            {
                                Sockets.Add(group, new List<SocketColor>());
                                Sockets[group].Add(SocketColor.Red);
                            }
                            break;
                        case "I":
                            if (Sockets.ContainsKey(group))
                                Sockets[group].Add(SocketColor.Green);
                            else
                            {
                                Sockets.Add(group, new List<SocketColor>());
                                Sockets[group].Add(SocketColor.Green);
                            }
                            break;
                        default:
                            if (Sockets.ContainsKey(group))
                                Sockets[group].Add(SocketColor.White);
                            else
                            {
                                Sockets.Add(group, new List<SocketColor>());
                                Sockets[group].Add(SocketColor.White);
                            }
                            break;
                    }
                }
            }

            private void ReadSockets(List<POEApi.Model.Socket> sockets)
            {
                foreach (var socket in sockets)
                {
                    int group = socket.Group;
                    string attr = socket.Attribute;
                    switch (attr)
                    {
                        case "D":
                            if (Sockets.ContainsKey(group))
                                Sockets[group].Add(SocketColor.Green);
                            else
                            {
                                Sockets.Add(group, new List<SocketColor>());
                                Sockets[group].Add(SocketColor.Green);
                            }
                            break;
                        case "S":
                            if (Sockets.ContainsKey(group))
                                Sockets[group].Add(SocketColor.Red);
                            else
                            {
                                Sockets.Add(group, new List<SocketColor>());
                                Sockets[group].Add(SocketColor.Red);
                            }
                            break;
                        case "I":
                            if (Sockets.ContainsKey(group))
                                Sockets[group].Add(SocketColor.Green);
                            else
                            {
                                Sockets.Add(group, new List<SocketColor>());
                                Sockets[group].Add(SocketColor.Green);
                            }
                            break;
                        default:
                            if (Sockets.ContainsKey(group))
                                Sockets[group].Add(SocketColor.White);
                            else
                            {
                                Sockets.Add(group, new List<SocketColor>());
                                Sockets[group].Add(SocketColor.White);
                            }
                            break;
                    }
                }
            }

            private void ReadGems(RavenJObject val)
            {
                foreach (RavenJObject obj in (RavenJArray)val["socketedItems"])
                {
                    Item gem = new Item(ItemClass.Gem, obj, skillCastTimes);
                    ProcessGem(gem);
                }
            }

            private void ReadGems(List<POEApi.Model.Gem> gems)
            {
                foreach (var socketedGem in gems)
                {
                    Item gem = new Item(ItemClass.Gem, socketedGem, skillCastTimes);
                    ProcessGem(gem);
                }
            }

            public void ProcessGem(Item gem)
            {
                SkillGem skillGem;
                GemType gemType;
                if (gem.IsSupport)
                {
                    gemType = GemType.Support;
                    skillGem = new SupportGem();
                    if (gem.Properties.ContainsKey("Mana Multiplier:  #%"))
                        (skillGem as SupportGem).ManaMultiplier = (int)gem.Properties["Mana Multiplier:  #%"][0];
                    else
                        (skillGem as SupportGem).ManaMultiplier = 1;
                }
                else if (gem.KeyWords.Contains("Aura"))
                {
                    gemType = GemType.Aura;
                    skillGem = new AuraGem();
                    if (gem.Properties.ContainsKey("Mana Reserved:  #%"))
                        (skillGem as AuraGem).ReservedMana = (int)gem.Properties["Mana Reserved:  #%"][0];
                }
                else
                {
                    skillGem = new ActiveGem();
                    if (gem.Properties.ContainsKey("Mana Cost:  #"))
                        (skillGem as ActiveGem).ManaCost = (int)gem.Properties["Mana Cost:  #"][0];
                    if (gem.KeyWords.Contains("Attack"))
                        gemType = GemType.Attack;
                    else if (gem.KeyWords.Contains("Totem"))
                        gemType = GemType.Totem;
                    else if (gem.KeyWords.Contains("Mine"))
                        gemType = GemType.Mine;
                    else if (gem.KeyWords.Contains("Cast"))
                        gemType = GemType.Cast;
                    else if (gem.KeyWords.Contains("Trap"))
                        gemType = GemType.Trap;
                    else if (gem.KeyWords.Contains("Curse"))
                        gemType = GemType.Curse;
                    else if (gem.KeyWords.Contains("Minion"))
                        gemType = GemType.Minion;
                    else
                        gemType = GemType.Spell;
                    if (skillCastTimes.ContainsKey(gem.TypeLine))
                        skillGem.CastTime = skillCastTimes[gem.TypeLine] / 1000f;
                    else
                        skillGem.CastTime = 0;
                }
                skillGem.GemType = gemType;
                skillGem.Description = gem.Description;
                string[] keywords = gem.KeyWords.Split(',');
                for (int i = 0; i < keywords.Length; i++)
                    keywords[i] = keywords[i].TrimStart(' ');
                skillGem.Keywords = new List<string>(keywords);
                if (gem.Properties.ContainsKey("Level:  #"))
                    skillGem.Level = (int)gem.Properties["Level:  #"][0];
                else if (gem.Properties.ContainsKey("Level:  # (Max)"))
                    skillGem.Level = (int)gem.Properties["Level:  # (Max)"][0];
                else
                    skillGem.Level = 1;
                skillGem.Name = gem.TypeLine;
                skillGem.Properties = new Dictionary<string, List<float>>(gem.Properties);
                skillGem.Mods = new Dictionary<string, List<float>>();
                foreach (Mod mod in gem.Mods)
                    skillGem.Mods.Add(mod.Attribute, mod.Values);
                skillGem.Requirements = new int[4];
                skillGem.Requirements[0] = (int)gem.Requirements["Level"][0];
                skillGem.Requirements[1] = gem.Requirements.ContainsKey("Str") ? (int)gem.Requirements["Str"][0] : 0;
                skillGem.Requirements[2] = gem.Requirements.ContainsKey("Dex") ? (int)gem.Requirements["Dex"][0] : 0;
                skillGem.Requirements[3] = gem.Requirements.ContainsKey("Int") ? (int)gem.Requirements["Int"][0] : 0;
                this.Gems.Add(skillGem);
            }

            private void ReadProperties(RavenJObject val)
            {
                foreach (RavenJObject obj in (RavenJArray)val["properties"])
                {
                    List<float> values = new List<float>();
                    string s = "";

                    foreach (RavenJArray jva in (RavenJArray)obj["values"])
                    {
                        s += " " + jva[0].Value<string>();
                    }

                    if (s == "") continue;

                    foreach (Match m in numberfilter.Matches(s))
                    {
                        if (m.Value == "") values.Add(float.NaN);
                        else values.Add(float.Parse(m.Value, System.Globalization.CultureInfo.InvariantCulture));
                    }
                    string cs = obj["name"].Value<string>() + ": " + (numberfilter.Replace(s, "#"));


                    Properties.Add(cs, values);
                }
            }

            private void ReadProperties(List<POEApi.Model.Property> properties)
            {
                foreach (var property in properties)
                {
                    List<float> values = new List<float>();
                    string s = "";

                    foreach (var value in property.Values)
                    {
                        s += " " + value.Item1;
                    }

                    if (s == "") continue;

                    foreach (Match m in numberfilter.Matches(s))
                    {
                        if (m.Value == "") values.Add(float.NaN);
                        else values.Add(float.Parse(m.Value, System.Globalization.CultureInfo.InvariantCulture));
                    }
                    string cs = property.Name + ": " + (numberfilter.Replace(s, "#"));

                    Properties.Add(cs, values);
                }
            }

            public Item XmlRead(XmlReader xml)
            {
                while (xml.Read())
                {
                    if (xml.HasAttributes)
                    {
                        for (int i = 0; i < xml.AttributeCount; i++)
                        {
                            string s = xml.GetAttribute(i);
                            if (s == "socketPopups")
                                return this;
                            if (s.Contains("itemName"))
                            {
                                var xs = xml.ReadSubtree();
                                xs.ReadToDescendant("span");
                                for (int j = 0; xs.Read(); )
                                {
                                    if (xs.NodeType == XmlNodeType.Text)
                                    {
                                        if (j == 0) Name = xs.Value.Replace("Additional ", "");
                                        if (j == 1) TypeLine = xs.Value;
                                        j++;
                                    }
                                }
                            }
                            if (s.Contains("displayProperty"))
                            {
                                List<float> attrval = new List<float>();
                                string[] span = new string[2] { "", "" };
                                var xs = xml.ReadSubtree();
                                xs.ReadToDescendant("span");
                                for (int j = 0; xs.Read(); )
                                {
                                    if (xs.NodeType == XmlNodeType.Text)
                                    {
                                        span[j] = xs.Value.Replace("Additional ", ""); ;
                                        j++;
                                    }
                                }
                                var matches = numberfilter.Matches(span[1]);
                                if (matches != null && matches.Count != 0)
                                {
                                    foreach (Match match in matches)
                                    {
                                        attrval.Add(float.Parse(match.Value, System.Globalization.CultureInfo.InvariantCulture));
                                    }
                                    Properties.Add(span[0] + "#", attrval);
                                }
                            }
                            if (s == "implicitMod" || s == "explicitMod")
                            {
                                string span = "";
                                var xs = xml.ReadSubtree();
                                xs.ReadToDescendant("span");
                                while (xs.Read())
                                {
                                    if (xs.NodeType == XmlNodeType.Text)
                                    {
                                        Mod modifier = new Mod(xs.Value.Replace("Additional ", ""), this.Class, WeaponType == "Quiver");
                                        Mods.Add(modifier);
                                    }
                                }

                            }

                        }
                    }
                }
                return this;

            }

            //-------------------------------------------------------------------------------
            #region Internal Types
            //-------------------------------------------------------------------------------
            public enum SocketColor
            {
                Red,
                Green,
                Blue,
                White
            }
            public enum ItemClass
            {
                Armor,
                MainHand,
                OffHand,
                Ring,
                Amulet,
                Helm,
                Gloves,
                Boots,
                Gem,
                Belt,
                Other
            }
            public class Mod
            {
                enum ValueType //Not used atm.
                {
                    Flat, Percentage, FlatMinMax
                }
                public string Attribute { get; private set; }
                public List<float> Values;
                public bool isLocal { get; private set; }
                private bool isQuiver = false;
                public Mod(string attribute, ItemClass ic, bool isQuiver)
                {
                    List<float> values = new List<float>(); //Can have two values etc. Adds 17-45 physical damage 
                    foreach (Match match in numberfilter.Matches(attribute))
                    {
                        values.Add(float.Parse(match.Value, System.Globalization.CultureInfo.InvariantCulture));
                    }
                    string at = numberfilter.Replace(attribute, "#");
                    this.Values = values;
                    this.Attribute = at;
                    this.isQuiver = isQuiver;
                    this.isLocal = IsLocalAttrib(ic);
                }

                private bool IsLocalAttrib(Item.ItemClass itemclass)
                {
                    return (itemclass != Item.ItemClass.Amulet && itemclass != Item.ItemClass.Ring && itemclass != Item.ItemClass.Belt && !isQuiver) &&
                            !Attribute.Contains("increased Physical Damage with Weapons per Red") &&
                            !Attribute.Contains("Global") &&
                            (Attribute.Contains("increased Physical Damage") ||
                            Attribute.Contains("Armour") ||
                            Attribute.Contains("Evasion") ||
                            Attribute.Contains("Energy Shield") ||
                            Attribute.Contains("Weapon Class") ||
                            Attribute.Contains("Critical Strike Chance with this Weapon") ||
                            Attribute.Contains("Critical Strike Damage Multiplier with this Weapon") ||
                           ((itemclass == Item.ItemClass.MainHand || itemclass == Item.ItemClass.OffHand) && (Attribute.Contains("increased Attack Speed") || Attribute == "#% additional Block Chance" || Attribute == "Adds #-# Physical Damage" || Attribute == "Adds #-# Cold Damage" || Attribute == "Adds #-# Cold Damage in Off Hand" || Attribute == "Adds #-# Fire Damage" || Attribute == "Adds #-# Fire Damage in Main Hand" || Attribute == "Adds #-# Lightning Damage" || Attribute == "Adds #-# Chaos Damage" || Attribute.Contains("increased Accuracy Rating") || Attribute.Contains("to Accuracy Rating"))));
                }
            }
            #endregion
        }

        public class Attribute : INotifyPropertyChanged
        {
            #region Fields & Props
            //-------------------------------------------------------------------------------
            private string attribute;
            private List<float> value;
            private string group;
            private Regex backreplace = new Regex("#");

            public event PropertyChangedEventHandler PropertyChanged;
            public List<float> Values { get { return value; } }
            public string TextAttribute
            {
                get { return attribute; }
            }
            public string ValuedAttribute
            {
                get { return InsertNumbersInAttributes(attribute, value); }
            }
            public string Group { get { return group; } }

            #endregion
            //-------------------------------------------------------------------------------
            public Attribute(string attribute, List<float> val, string grp)
            {
                this.attribute = attribute;
                value = new List<float>(val);
                group = grp;
            }

            #region Methods
            //-------------------------------------------------------------------------------
            private string InsertNumbersInAttributes(string attrib, List<float> attribValues)
            {
                foreach (var number in attribValues)
                {
                    attrib = backreplace.Replace(attrib, number + "", 1);
                }
                return attrib;
            }

            public bool Add(string attrib, List<float> values) //needs rework, return value not used atm
            {
                if (attribute != attrib) return false;
                if (value.Count != values.Count) return false;
                for (int i = 0; i < values.Count; i++)
                {
                    value[i] += values[i];
                }
                OnPropertyChanged("ValuedAttribute");
                return true;
            }

            private void OnPropertyChanged(string info) //not used atm
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(info));
                }
            }

            /*    public override string ToString()
                {
                    return ValuedAttribute;
                }*/
            #endregion
            //-------------------------------------------------------------------------------
        }

        public class NumberLessStringComparer : System.Collections.IComparer
        {
            static Regex numberfilter = new Regex("[0-9]*\\.?[0-9]+");
            static Dictionary<string, int> grpValues = new Dictionary<string, int>() { { "MainHand", -4 }, { "OffHand", -3 }, { "Helm", -2 }, { "Armor", -1 }, { "Gloves", 0 }, { "Boots", 1 }, { "Amulet", 2 }, { "Ring", 3 }, { "Belt", 4 }, { "Global Mods", 5 } };
            public int Compare(string x, string y)
            {
                return numberfilter.Replace(x, "").CompareTo(numberfilter.Replace(y, ""));
            }

            public int Compare(object x, object y)
            {
                if (x is Attribute && y is Attribute)
                {
                    if (grpValues[((Attribute)x).Group] > grpValues[((Attribute)y).Group])
                        return +1;
                    else if (grpValues[((Attribute)x).Group] < grpValues[((Attribute)y).Group])
                        return -1;
                    return string.Compare(((Attribute)x).TextAttribute, ((Attribute)y).TextAttribute, true);
                }
                return 0;
            }
        }
        #endregion
    }
}
