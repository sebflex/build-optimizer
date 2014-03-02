using System;
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
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Xceed.Wpf.Toolkit;


namespace POESKillTree
{
    /// <summary>
    /// Interaction logic for StatsWindow.xaml
    /// </summary>
    public partial class StatsWindow : Window
    {
        int maxSkillPoints = 120;
        SkillTree Tree;
        CharItemData ItemAttributes;
        CharStats characterStats;
        public CharStats CharacterStats { get { return characterStats; } }
        StatsComparer statsComparer;
        public CharStats StatsComparer { get { return statsComparer; } }
        ObservableCollection<ListBoxItem> selectedSkillsCollection;
        ObservableCollection<ListBoxItem> allSkillsCollection;
        CharStats.VIPStats maxStats;
        CharStats.VIPStats requiredStats;
        List<HashSet<ushort>> availableBuilds;
        List<HashSet<ushort>> remainedBuilds;
        Dictionary<HashSet<ushort>, CharStats.VIPStats> buildRecords;
        bool buildsLoaded = false;
        bool intitializationFinished = false;
        //--------------------------Constructor-----------------------------
        public StatsWindow(SkillTree tree, CharItemData itemAttributes)
        {
            InitializeComponent();
            this.Tree = tree;
            ItemAttributes = itemAttributes;
            requiredStats = new CharStats.VIPStats(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            characterStats = new CharStats(Tree);
            statsComparer = new StatsComparer(Tree);

            changeStatsGroupBox.DataContext = statsComparer;
            UpdateStatComparer();

            InitListViews();
            intitializationFinished = true;
            buildRecords = new Dictionary<HashSet<ushort>, CharStats.VIPStats>();
        }
        //-------------------------------------------------------------------------
        #region Methods
        //-------------------------------------------------------------------------
        public void CloneCharStats(CharStats srcStats, CharStats destStats)
        {
            if (srcStats.CruelDiff)
                destStats.CruelDiff = true;
            else if (srcStats.MercilessDiff)
                destStats.MercilessDiff = true;
            else
                destStats.NormalDiff = true;

            if (srcStats.BanditsAllResistReward)
                destStats.BanditsAllResistReward = true;
            else if (srcStats.BanditsLifeReward)
                destStats.BanditsLifeReward = true;
            else if (srcStats.BanditsManaReward)
                destStats.BanditsManaReward = true;
            else
                destStats.NormalBanditsKillAll = true;

            if (srcStats.BanditsASReward)
                destStats.BanditsASReward = true;
            else if (srcStats.BanditsCSReward)
                destStats.BanditsCSReward = true;
            else if (srcStats.BanditsPDReward)
                destStats.BanditsPDReward = true;
            else
                destStats.CruelBanditsKillAll = true;

            if (srcStats.BanditsFCReward)
                destStats.BanditsFCReward = true;
            else if (srcStats.BanditsPCReward)
                destStats.BanditsPCReward = true;
            else if (srcStats.BanditsECReward)
                destStats.BanditsECReward = true;
            else
                destStats.MercilessBanditsKillAll = true;

            destStats.ActiveSkillGem = srcStats.ActiveSkillGem;
            destStats.SupportGems = srcStats.SupportGems;
            destStats.CurrentEndurance = srcStats.CurrentEndurance;
            destStats.CurrentFrenzy = srcStats.CurrentFrenzy;
            destStats.CurrentPower = srcStats.CurrentPower;
            destStats.AngerLvl = srcStats.AngerLvl;
            destStats.HatredLvl = srcStats.HatredLvl;
            destStats.WrathLvl = srcStats.WrathLvl;
            destStats.VitalityLvl = srcStats.VitalityLvl;
            destStats.HasteLvl = srcStats.HasteLvl;
            destStats.DisciplineLvl = srcStats.DisciplineLvl;
            destStats.DeterminationLvl = srcStats.DeterminationLvl;
            destStats.GraceLvl = srcStats.GraceLvl;
            destStats.ClarityLvl = srcStats.ClarityLvl;
            destStats.PurityofElementsLvl = srcStats.PurityofElementsLvl;
            destStats.PurityofFireLvl = srcStats.PurityofFireLvl;
            destStats.PurityofIceLvl = srcStats.PurityofIceLvl;
            destStats.PurityofLightningLvl = srcStats.PurityofLightningLvl;

            UpdateStatComparer();
        }

        public void LoadBuilds(List<HashSet<ushort>> buildCollection)
        {
            if (buildCollection.Count > 0)
            {
                buildsLoaded = true;
                filterGroupBox.Visibility = System.Windows.Visibility.Visible;
                totalBuildsTextBlock.Text = "Total Builds = " + buildCollection.Count;
                GetBuildsStats(buildCollection);
                FilterBuildsBySkills();
            }
        }

        private void UpdateStatComparer()
        {
            var passiveAttribs = Tree.SelectedAttributes;
            foreach (CharItemData.Attribute mod in ItemAttributes.NonLocalMods)
            {
                if (passiveAttribs.ContainsKey(mod.TextAttribute))
                {
                    for (int i = 0; i < mod.Values.Count; i++)
                    {
                        passiveAttribs[mod.TextAttribute][i] += mod.Values[i];
                    }
                }
                else
                {
                    passiveAttribs[mod.TextAttribute] = mod.Values;
                }
            }
            statsComparer.UpdateStats(passiveAttribs, Tree.CharLevel, ItemAttributes);
        }

        private void InitListViews()
        {
            selectedSkillsCollection = new ObservableCollection<ListBoxItem>();
            allSkillsCollection = new ObservableCollection<ListBoxItem>();
            ListBoxItem itemToAdd;
            StringBuilder stBuild = new StringBuilder();
            foreach (var nodeID in Tree.Skillnodes.Keys)
            {
                if (Tree.Skillnodes[nodeID].not)
                {
                    //if (Tree.SkilledNodes.Contains(nodeID))
                    //{
                    //    itemToAdd = new ListBoxItem() { Content = Tree.Skillnodes[nodeID].name };
                    //    itemToAdd.Tag = nodeID;
                    //    stBuild.Clear();
                    //    foreach (string tip in Tree.Skillnodes[nodeID].attributes)
                    //        stBuild.AppendLine(tip);
                    //    itemToAdd.ToolTip = stBuild.ToString();
                    //    selectedSkillsCollection.Add(itemToAdd);
                    //}
                    //else
                    //{
                    itemToAdd = new ListBoxItem() { Content = Tree.Skillnodes[nodeID].name };
                    itemToAdd.Tag = nodeID;
                    stBuild.Clear();
                    foreach (string tip in Tree.Skillnodes[nodeID].attributes)
                        stBuild.AppendLine(tip);
                    itemToAdd.ToolTip = stBuild.ToString();
                    allSkillsCollection.Add(itemToAdd);
                    //}
                }
                else if (Tree.Skillnodes[nodeID].ks)
                {
                    if (Tree.SkilledNodes.Contains(nodeID))
                    {
                        itemToAdd = new ListBoxItem() { Content = Tree.Skillnodes[nodeID].name };
                        itemToAdd.Tag = nodeID;
                        stBuild.Clear();
                        foreach (string tip in Tree.Skillnodes[nodeID].attributes)
                            stBuild.AppendLine(tip);
                        itemToAdd.ToolTip = stBuild.ToString();
                        selectedSkillsCollection.Add(itemToAdd);
                    }
                    else
                    {
                        itemToAdd = new ListBoxItem() { Content = Tree.Skillnodes[nodeID].name };
                        itemToAdd.Tag = nodeID;
                        stBuild.Clear();
                        foreach (string tip in Tree.Skillnodes[nodeID].attributes)
                            stBuild.AppendLine(tip);
                        itemToAdd.ToolTip = stBuild.ToString();
                        allSkillsCollection.Add(itemToAdd);
                    }
                }
            }
            ListCollectionView selectedSkillsView = (ListCollectionView)CollectionViewSource.GetDefaultView(selectedSkillsCollection);
            selectedSkillsView.SortDescriptions.Add(new System.ComponentModel.SortDescription("Content", System.ComponentModel.ListSortDirection.Ascending));
            selectedSkillsListBox.ItemsSource = selectedSkillsCollection;
            ListCollectionView allSkillsView = (ListCollectionView)CollectionViewSource.GetDefaultView(allSkillsCollection);
            allSkillsView.SortDescriptions.Add(new System.ComponentModel.SortDescription("Content", System.ComponentModel.ListSortDirection.Ascending));
            allNotableSkillsListBox.ItemsSource = allSkillsCollection;
        }

        private void FilterBuildsBySkills()
        {
            //Dictionary<HashSet<ushort>, CharStats.VIPStats> filteredRecords = new Dictionary<HashSet<ushort>, CharStats.VIPStats>();
            availableBuilds = new List<HashSet<ushort>>();
            var filteredBuilds = from build in buildRecords.Keys
                                 where BuildContainsSelectedSkills(build)
                                 select build;
            foreach (var build in filteredBuilds)
                availableBuilds.Add(build);
            GetMaxStats();
            FilterBuildsByStats();
        }

        private bool BuildContainsSelectedSkills(HashSet<ushort> build)
        {
            if (build.Count > maxSkillPoints + 1) //each build also has its class points, hence we should reduce by 1
                return false;
            foreach (var item in selectedSkillsCollection)
                if (!build.Contains((ushort)item.Tag))
                    return false;
            return true;
        }

        private void FilterBuildsByStats()
        {
            //Dictionary<HashSet<ushort>, CharStats.VIPStats> filteredRecords = new Dictionary<HashSet<ushort>, CharStats.VIPStats>();
            remainedBuilds = new List<HashSet<ushort>>(availableBuilds);
            var filteredBuilds = from build in remainedBuilds
                                 where BuildContainsRequiredStats(build)
                                 select build;
            remainedBuilds = filteredBuilds.ToList();
            ShowResults();
        }

        private bool BuildContainsRequiredStats(HashSet<ushort> build)
        {
            var stats = buildRecords[build];
            if (stats.ChanceToBlock < requiredStats.ChanceToBlock)
                return false;
            if (stats.ChanceToEvade < requiredStats.ChanceToEvade)
                return false;
            if (stats.CritChanceMH < requiredStats.CritChanceMH)
                return false;
            if (stats.CritChanceOH < requiredStats.CritChanceOH)
                return false;
            if (stats.EnergyShield < requiredStats.EnergyShield)
                return false;
            if (stats.Life < requiredStats.Life)
                return false;
            if (stats.Mana < requiredStats.Mana)
                return false;
            if (stats.PhysDamageReduction < requiredStats.PhysDamageReduction)
                return false;
            return true;
        }

        private void GetMaxStats()
        {
            maxStats = new CharStats.VIPStats(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            int maxPoints = 1;
            foreach (var build in availableBuilds)
            {
                CharStats.VIPStats stats = buildRecords[build];

                if (stats.AttackSkillDPS > maxStats.AttackSkillDPS)
                    maxStats.AttackSkillDPS = stats.AttackSkillDPS;
                if (stats.SpellSkillDPS > maxStats.SpellSkillDPS)
                    maxStats.SpellSkillDPS = stats.SpellSkillDPS;
                if (stats.ChanceToBlock > maxStats.ChanceToBlock)
                    maxStats.ChanceToBlock = stats.ChanceToBlock;
                if (stats.ChanceToEvade > maxStats.ChanceToEvade)
                    maxStats.ChanceToEvade = stats.ChanceToEvade;
                if (stats.CritChanceMH > maxStats.CritChanceMH)
                    maxStats.CritChanceMH = stats.CritChanceMH;
                if (stats.CritChanceOH > maxStats.CritChanceOH)
                    maxStats.CritChanceOH = stats.CritChanceOH;
                if (stats.SpellCritChance > maxStats.SpellCritChance)
                    maxStats.SpellCritChance = stats.SpellCritChance;
                if (stats.EnergyShield > maxStats.EnergyShield)
                    maxStats.EnergyShield = stats.EnergyShield;
                if (stats.Life > maxStats.Life)
                    maxStats.Life = stats.Life;
                if (stats.Mana > maxStats.Mana)
                    maxStats.Mana = stats.Mana;
                if (stats.PhysDamageReduction > maxStats.PhysDamageReduction)
                    maxStats.PhysDamageReduction = stats.PhysDamageReduction;
                if (build.Count > maxPoints)
                    maxPoints = build.Count - 1;
            }

            maxCritMHTextBlock.Text = "Critical MainHand (" + maxStats.CritChanceMH.ToString() + "):";
            maxCritOHTextBlock.Text = "Critical OffHand (" + maxStats.CritChanceOH.ToString() + "):";
            maxSpellCritTextBlock.Text = "Critical Spell (" + maxStats.SpellCritChance.ToString() + "):";
            maxEvadeTextBlock.Text = "Evasion Chance (" + maxStats.ChanceToEvade.ToString() + "):";
            maxBlockTextBlock.Text = "Block Chance (" + maxStats.ChanceToBlock.ToString() + "):";
            maxEnergySTextBlock.Text = "Energy Shield (" + maxStats.EnergyShield.ToString() + "):";
            maxLifeTextBlock.Text = "Life (" + maxStats.Life.ToString() + "):";
            maxManaTextBlock.Text = "Mana (" + maxStats.Mana.ToString() + "):";
            maxDmgRedTextBlock.Text = "Physical Reduction (" + maxStats.PhysDamageReduction.ToString() + "):";
            maxSPTextBlock.Text = "Max Used Points (" + maxPoints.ToString() + "):";

        }

        private void GetBuildsStats(List<HashSet<ushort>> buildCollection)
        {
            bool firstTime = true;
            foreach (var build in buildCollection)
            {
                Dictionary<string, List<float>> passiveAttribs = new Dictionary<string, List<float>>();
                foreach (ushort inode in build)
                {
                    var node = Tree.Skillnodes[inode];
                    foreach (var attr in node.Attributes)
                    {
                        if (!passiveAttribs.ContainsKey(attr.Key))
                            passiveAttribs[attr.Key] = new List<float>();
                        for (int i = 0; i < attr.Value.Count; i++)
                        {

                            if (passiveAttribs.ContainsKey(attr.Key) && passiveAttribs[attr.Key].Count > i)
                                passiveAttribs[attr.Key][i] += attr.Value[i];
                            else
                            {
                                passiveAttribs[attr.Key].Add(attr.Value[i]);
                            }
                        }
                    }
                }

                foreach (CharItemData.Attribute mod in ItemAttributes.NonLocalMods)
                {
                    if (passiveAttribs.ContainsKey(mod.TextAttribute))
                    {
                        for (int i = 0; i < mod.Values.Count; i++)
                        {
                            passiveAttribs[mod.TextAttribute][i] += mod.Values[i];
                        }
                    }
                    else
                    {
                        passiveAttribs[mod.TextAttribute] = mod.Values;
                    }
                }
                CharacterStats.UpdateStats(passiveAttribs, Tree.CharLevel, ItemAttributes);
                if (firstTime)
                {
                    CloneCharStats(statsComparer, CharacterStats);
                    firstTime = false;
                }
                CharStats.VIPStats stats = new CharStats.VIPStats(CharacterStats.Life, CharacterStats.LifeRegen, CharacterStats.Mana, CharacterStats.ManaRegen, CharacterStats.EnergyShield, CharacterStats.PhysDamageReduction, CharacterStats.ChanceToEvade, CharacterStats.ChanceToBlock, CharacterStats.CritChanceMH, CharacterStats.CritChanceOH, CharacterStats.CritChanceSpell, CharacterStats.AttackSkillDPS, CharacterStats.SpellSkillDPS);
                buildRecords.Add(build, stats);
            }
            buildCollection.Clear();
            buildCollection = null;
        }

        private void ShowResults()
        {
            float maxDPS = 0;
            List<HashSet<ushort>> optBuilds = new List<HashSet<ushort>>();
            bool isSpell = false;
            if (characterStats.ActiveSkillGem != null)
            {
                if (characterStats.ActiveSkillGem.GemType == GemType.Attack)
                    isSpell = false;
                else
                    isSpell = true;
            }
            foreach (var build in remainedBuilds)
            {
                if (isSpell)
                {
                    if (buildRecords[build].SpellSkillDPS > maxDPS)
                    {
                        maxDPS = buildRecords[build].SpellSkillDPS;
                        optBuilds.Add(build);
                    }
                }
                else
                    if (buildRecords[build].AttackSkillDPS > maxDPS)
                    {
                        maxDPS = buildRecords[build].AttackSkillDPS;
                        optBuilds.Add(build);
                    }
            }
            for (int i = optBuilds.Count - 1; i >= 0; i--)//correction
            {
                if (isSpell)
                {
                    if (buildRecords[optBuilds[i]].SpellSkillDPS < maxDPS)
                        optBuilds.RemoveAt(i);
                }
                else
                    if (buildRecords[optBuilds[i]].AttackSkillDPS < maxDPS)
                        optBuilds.RemoveAt(i);
            }
            if (remainedBuilds.Count > 0)
            {
                if (optBuilds.Count > 1)
                    optBuildTextBox.Text = "More than one possible build with this DPS; Try to apply more stats";
                else if (optBuilds.Count == 1)
                    optBuildTextBox.Text = Tree.SaveToURL(optBuilds[0]);
            }
            else
                optBuildTextBox.Text = "Not any build build found; Try to lower requirements";
            maxDPSTextBlock.Text = characterStats.ActiveSkillGem.Name + " DPS: " + maxDPS.ToString("f2");
            filteredBuildsTextBlock.Text = "Filtered Build Count: " + (remainedBuilds.Count);
        }

        #endregion
        //-------------------------------------------------------------------------
        #region Event Handlers
        //-------------------------------------------------------------------------
        private void addToSelectedsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (allNotableSkillsListBox.SelectedItem != null)
            {
                selectedSkillsCollection.Add(allNotableSkillsListBox.SelectedItem as ListBoxItem);
                allSkillsCollection.Remove(allNotableSkillsListBox.SelectedItem as ListBoxItem);
                if (buildsLoaded)
                    FilterBuildsBySkills();
            }
        }

        private void removefromSelectedsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSkillsListBox.SelectedItem != null)
            {
                allSkillsCollection.Add(selectedSkillsListBox.SelectedItem as ListBoxItem);
                selectedSkillsCollection.Remove(selectedSkillsListBox.SelectedItem as ListBoxItem);
                if (selectedSkillsListBox.Items.Count > 0)
                    selectedSkillsListBox.SelectedIndex = 0;
                if (buildsLoaded)
                    FilterBuildsBySkills();
            }
        }

        private void maxSPTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int number = 120;
                if (int.TryParse(maxSPTextBox.Text, out number))
                {
                    if (number < 1)
                    {
                        number = 1;
                        maxSPTextBox.Text = number.ToString();
                    }
                }
                else
                    minLifeTextBox.Text = "120";
                maxSkillPoints = number;
                FilterBuildsBySkills();
            }
        }

        private void maxSPTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            int number = 120;
            if (int.TryParse(maxSPTextBox.Text, out number))
            {
                if (number < 1)
                {
                    number = 1;
                    maxSPTextBox.Text = number.ToString();
                }
            }
            else
                minLifeTextBox.Text = "120";
            maxSkillPoints = number;
            FilterBuildsBySkills();
        }

        private void minLifeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int number = 0;
                if (int.TryParse(minLifeTextBox.Text, out number))
                {
                    if (number > maxStats.Life)
                    {
                        number = maxStats.Life;
                        minLifeTextBox.Text = maxStats.Life.ToString();
                    }
                    if (number > 0)
                    {

                    }
                }
                else
                    minLifeTextBox.Text = "0";
                requiredStats.Life = number;
                FilterBuildsByStats();

            }
        }

        private void minLifeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            int number = 0;
            if (int.TryParse(minLifeTextBox.Text, out number))
            {
                if (number > maxStats.Life)
                {
                    number = maxStats.Life;
                    minLifeTextBox.Text = maxStats.Life.ToString();
                }
                if (number > 0)
                {

                }
            }
            else
                minLifeTextBox.Text = "0";
            requiredStats.Life = number;
            FilterBuildsByStats();
        }

        private void minManaTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int number = 0;
                if (int.TryParse(minManaTextBox.Text, out number))
                {
                    if (number > maxStats.Mana)
                    {
                        number = maxStats.Mana;
                        minManaTextBox.Text = maxStats.Mana.ToString();
                    }
                    if (number > 0)
                    {

                    }
                }
                else
                    minManaTextBox.Text = "0";
                requiredStats.Mana = number;
                FilterBuildsByStats();

            }
        }

        private void minManaTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            int number = 0;
            if (int.TryParse(minManaTextBox.Text, out number))
            {
                if (number > maxStats.Mana)
                {
                    number = maxStats.Mana;
                    minManaTextBox.Text = maxStats.Mana.ToString();
                }
                if (number > 0)
                {

                }
            }
            else
                minManaTextBox.Text = "0";
            requiredStats.Mana = number;
            FilterBuildsByStats();
        }

        private void minESTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int number = 0;
                if (int.TryParse(minESTextBox.Text, out number))
                {
                    if (number > maxStats.EnergyShield)
                    {
                        number = maxStats.EnergyShield;
                        minESTextBox.Text = maxStats.EnergyShield.ToString();
                    }
                    if (number > 0)
                    {

                    }
                }
                else
                    minESTextBox.Text = "0";
                requiredStats.EnergyShield = number;
                FilterBuildsByStats();
            }
        }

        private void minESTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            int number = 0;
            if (int.TryParse(minESTextBox.Text, out number))
            {
                if (number > maxStats.EnergyShield)
                {
                    number = maxStats.EnergyShield;
                    minESTextBox.Text = maxStats.EnergyShield.ToString();
                }
                if (number > 0)
                {

                }
            }
            else
                minESTextBox.Text = "0";
            requiredStats.EnergyShield = number;
            FilterBuildsByStats();
        }

        private void minDmgRedTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int number = 0;
                if (int.TryParse(minDmgRedTextBox.Text, out number))
                {
                    if (number > maxStats.PhysDamageReduction)
                    {
                        number = maxStats.PhysDamageReduction;
                        minDmgRedTextBox.Text = maxStats.PhysDamageReduction.ToString();
                    }
                    if (number > 0)
                    {

                    }
                }
                else
                    minDmgRedTextBox.Text = "0";
                requiredStats.PhysDamageReduction = number;
                FilterBuildsByStats();
            }
        }

        private void minDmgRedTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            int number = 0;
            if (int.TryParse(minDmgRedTextBox.Text, out number))
            {
                if (number > maxStats.PhysDamageReduction)
                {
                    number = maxStats.PhysDamageReduction;
                    minDmgRedTextBox.Text = maxStats.PhysDamageReduction.ToString();
                }
                if (number > 0)
                {

                }
            }
            else
                minDmgRedTextBox.Text = "0";
            requiredStats.PhysDamageReduction = number;
            FilterBuildsByStats();
        }

        private void minEvasionTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int number = 0;
                if (int.TryParse(minEvasionTextBox.Text, out number))
                {
                    if (number > maxStats.ChanceToEvade)
                    {
                        number = maxStats.ChanceToEvade;
                        minEvasionTextBox.Text = maxStats.ChanceToEvade.ToString();
                    }
                    if (number > 0)
                    {

                    }
                }
                else
                    minEvasionTextBox.Text = "0";
                requiredStats.ChanceToEvade = number;
                FilterBuildsByStats();
            }
        }

        private void minEvasionTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            int number = 0;
            if (int.TryParse(minEvasionTextBox.Text, out number))
            {
                if (number > maxStats.ChanceToEvade)
                {
                    number = maxStats.ChanceToEvade;
                    minEvasionTextBox.Text = maxStats.ChanceToEvade.ToString();
                }
                if (number > 0)
                {

                }
            }
            else
                minEvasionTextBox.Text = "0";
            requiredStats.ChanceToEvade = number;
            FilterBuildsByStats();
        }

        private void minBlockTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int number = 0;
                if (int.TryParse(minBlockTextBox.Text, out number))
                {
                    if (number > maxStats.ChanceToBlock)
                    {
                        number = maxStats.ChanceToBlock;
                        minBlockTextBox.Text = maxStats.ChanceToBlock.ToString();
                    }
                    if (number > 0)
                    {

                    }
                }
                else
                    minBlockTextBox.Text = "0";
                requiredStats.ChanceToBlock = number;
                FilterBuildsByStats();
            }
        }

        private void minBlockTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            int number = 0;
            if (int.TryParse(minBlockTextBox.Text, out number))
            {
                if (number > maxStats.ChanceToBlock)
                {
                    number = maxStats.ChanceToBlock;
                    minBlockTextBox.Text = maxStats.ChanceToBlock.ToString();
                }
                if (number > 0)
                {

                }
            }
            else
                minBlockTextBox.Text = "0";
            requiredStats.ChanceToBlock = number;
            FilterBuildsByStats();
        }

        private void minCritMHTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                float number = 0;
                if (float.TryParse(minCritMHTextBox.Text, out number))
                {
                    if (number > maxStats.CritChanceMH)
                    {
                        number = maxStats.CritChanceMH;
                        minCritMHTextBox.Text = maxStats.CritChanceMH.ToString();
                    }
                    if (number > 0)
                    {

                    }
                }
                else
                    minCritMHTextBox.Text = "0";
                requiredStats.CritChanceMH = number;
                FilterBuildsByStats();
            }
        }

        private void minCritMHTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            float number = 0;
            if (float.TryParse(minCritMHTextBox.Text, out number))
            {
                if (number > maxStats.CritChanceMH)
                {
                    number = maxStats.CritChanceMH;
                    minCritMHTextBox.Text = maxStats.CritChanceMH.ToString();
                }
                if (number > 0)
                {

                }
            }
            else
                minCritMHTextBox.Text = "0";
            requiredStats.CritChanceMH = number;
            FilterBuildsByStats();
        }

        private void minCritOHTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                float number = 0;
                if (float.TryParse(minCritOHTextBox.Text, out number))
                {
                    if (number > maxStats.CritChanceOH)
                    {
                        number = maxStats.CritChanceMH;
                        minCritOHTextBox.Text = maxStats.CritChanceOH.ToString();
                    }
                    if (number > 0)
                    {

                    }
                }
                else
                    minCritOHTextBox.Text = "0";
                requiredStats.CritChanceOH = number;
                FilterBuildsByStats();
            }
        }

        private void minCritOHTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            float number = 0;
            if (float.TryParse(minCritOHTextBox.Text, out number))
            {
                if (number > maxStats.CritChanceOH)
                {
                    number = maxStats.CritChanceMH;
                    minCritOHTextBox.Text = maxStats.CritChanceOH.ToString();
                }

            }
            else
                minCritOHTextBox.Text = "0";
            requiredStats.CritChanceOH = number;
            FilterBuildsByStats();
        }

        private void minSpellCritTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                float number = 0;
                if (float.TryParse(minSpellCritTextBox.Text, out number))
                {
                    if (number > maxStats.SpellCritChance)
                    {
                        number = maxStats.SpellCritChance;
                        minSpellCritTextBox.Text = maxStats.SpellCritChance.ToString();
                    }
                }
                else
                    minSpellCritTextBox.Text = "0";
                requiredStats.SpellCritChance = number;
                FilterBuildsByStats();
            }
        }

        private void minSpellCritTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            float number = 0;
            if (float.TryParse(minSpellCritTextBox.Text, out number))
            {
                if (number > maxStats.SpellCritChance)
                {
                    number = maxStats.SpellCritChance;
                    minSpellCritTextBox.Text = maxStats.SpellCritChance.ToString();
                }
            }
            else
                minSpellCritTextBox.Text = "0";
            requiredStats.SpellCritChance = number;
            FilterBuildsByStats();
        }

        private void strength_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyStrength((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }
        private void dexterity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyDexterity((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }
        private void intelligence_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyIntelligence((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }
        private void Armour_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyArmour((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }
        private void IncreasedArmour_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyIncreasedArmour((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }
        private void EvasionRating_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyEvasionRating((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }

        private void IncreasedEvasion_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyIncreasedEvasion((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }

        private void ToMaximumLife_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyToMaximumLife((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }

        private void IncreaseLife_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyIncreaseLife((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }

        private void ToMaximumES_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyToMaximumES((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }

        private void IncreasedES_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyIncreasedES((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }

        private void ToMaximumMana_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyToMaximumMana((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }

        private void IncreasedMana_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyIncreasedMana((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }

        private void ToAccuracyRating_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyToAccuracyRating((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }

        private void IncreasedAccuracy_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyIncreasedAccuracy((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }

        private void IncreasedAS_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyIncreasedAS((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }

        private void IncreasedCS_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyIncreasedCS((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }

        private void IncreasedCritChance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyIncreasedCritChance((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }

        private void IncreasedCritMultiplier_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyIncreasedCritMultiplier((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }

        private void ToColdDamage_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyToColdDamage((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }

        private void IncreasedColdDamage_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyIncreasedColdDamage((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }

        private void ToFireDamage_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyToFireDamage((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }

        private void IncreasedFireDamage_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyIncreasedFireDamage((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }

        private void ToLightDamage_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyToLightDamage((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }

        private void IncreasedLightDamage_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyIncreasedLightDamage((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }

        private void ToChaosDamage_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyToChaosDamage((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }

        private void IncreasedChaosDamage_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyIncreasedChaosDamage((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }

        private void IncreasedEleDamage_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyIncreasedEleDamage((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }

        private void ToPhysicalDamage_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyToPhysicalDamage((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }

        private void IncreasedPhysicalDamage_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (intitializationFinished)
                statsComparer.ModifyIncreasedPhysicalDamage((e.NewValue == null ? 0 : (int)e.NewValue) - (e.OldValue == null ? 0 : (int)e.OldValue));
        }
        //-------------------------------------------------------------------------
        #endregion
    }

}
