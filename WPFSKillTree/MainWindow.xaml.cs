using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Reflection;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using POESKillTree;
using WPFSKillTree;
using System.Web;
using MessageBox = System.Windows.MessageBox;
using Xceed.Wpf.Toolkit;
using Abot.Crawler;
using Abot.Poco;
using POEApi.Model;

namespace POESKillTree
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields
        //-------------------------------------------------------------------------------
        SkillTree Tree;
        CharStats CharacterStats;
        List<PoEBuild> savedBuilds = new List<PoEBuild>();
        //Dictionary<SkillGem, List<SkillGem>> skillSupports;
        List<SkillGem> activeGems;
        private string lastTooltip;
        private string loadedItemDataFile;
        private CharItemData ItemsData = null;

        ToolTip sToolTip = new ToolTip();

        private Vector2D multransform = new Vector2D();
        private Vector2D addtransform = new Vector2D();
        private LoadingWindow loadingWindow;

        private List<ushort> prePath;
        private HashSet<ushort> toRemove;
        private bool SkipTreeReset = false;
        private bool skillListCreated = false;

        Regex backreplace = new Regex("#");

        private ObservableCollection<string> passiveAttributeList = new ObservableCollection<string>();
        private ObservableCollection<string> allAttributesList = new ObservableCollection<string>();

        const string TreeAddress = "http://www.pathofexile.com/passive-skill-tree/";
        private RenderTargetBitmap ClipboardBmp;
        static Action emptyDelegate = delegate
        {
        };

        #endregion
        //-------------------------------------------------------------------------------
        #region Creation & clean Up
        //-------------------------------------------------------------------------------
        public MainWindow()
        {
            Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            //AppDomain.CurrentDomain.AssemblyResolve += ( sender, args ) =>
            //{

            //    String resourceName = "POESKillTree." +

            //       new AssemblyName( args.Name ).Name + ".dll";

            //    using ( var stream = Assembly.GetExecutingAssembly( ).GetManifestResourceStream( resourceName ) )
            //    {

            //        Byte[] assemblyData = new Byte[ stream.Length ];

            //        stream.Read( assemblyData, 0, assemblyData.Length );

            //        return Assembly.Load( assemblyData );

            //    }

            //};
            InitializeComponent();
            DisableAllUserSettings();

        }

        #endregion
        //-------------------------------------------------------------------------------
        #region Methods
        private void startLoadingWindow()
        {
            loadingWindow = new LoadingWindow();
            loadingWindow.Show();
        }
        private void updatetLoadingWindow(double c, double max)
        {
            loadingWindow.progressBar1.Maximum = max;
            loadingWindow.progressBar1.Value = c;
            loadingWindow.Dispatcher.Invoke(DispatcherPriority.Render, emptyDelegate);
        }
        private void closeLoadingWindow()
        {

            loadingWindow.Close();
        }

        private void DisableAllUserSettings()
        {
            foreach (UIElement element in difficultyPanel.Children)
                element.IsEnabled = false;
            foreach (RadioButton rb in FindVisualChildren<RadioButton>(banditsGrid))
                rb.IsEnabled = false;
            foreach (IntegerUpDown intUpDown in FindVisualChildren<IntegerUpDown>(chargesGrid))
                intUpDown.IsEnabled = false;
            foreach (IntegerUpDown intUpDown in FindVisualChildren<IntegerUpDown>(auraSettingsGrid))
                intUpDown.IsEnabled = false;
        }

        private void EnableAllUserSettings()
        {
            foreach (UIElement element in difficultyPanel.Children)
                element.IsEnabled = true;
            foreach (RadioButton rb in FindVisualChildren<RadioButton>(banditsGrid))
                rb.IsEnabled = true;
            foreach (IntegerUpDown intUpDown in FindVisualChildren<IntegerUpDown>(chargesGrid))
                intUpDown.IsEnabled = true;
            foreach (IntegerUpDown intUpDown in FindVisualChildren<IntegerUpDown>(auraSettingsGrid))
                intUpDown.IsEnabled = true;
        }

        private static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public void UpdatePassiveAttributeList()
        {
            passiveAttributeList.Clear();
            foreach (var item in (Tree.SelectedAttributes.Select(InsertNumbersInAttributes)))
            {
                passiveAttributeList.Add(item);
            }
            //passiveAttibuteCollection.Refresh(); not needed with observableCollection
            tbUsedPoints.Text = "" + (Tree.SkilledNodes.Count - 1);
        }

        public void UpdateAllAttributeList()
        {
            UpdatePassiveAttributeList();
            if (ItemsData != null)
            {
                var passiveAttribs = Tree.SelectedAttributes;
                foreach (CharItemData.Attribute mod in ItemsData.NonLocalMods)
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
                CharacterStats.UpdateStats(passiveAttribs, Tree.CharLevel, ItemsData);
                allAttributesList.Clear();
                foreach (var item in (passiveAttribs.Select(InsertNumbersInAttributes)))
                {
                    allAttributesList.Add(item);
                }
            }
        }

        private string InsertNumbersInAttributes(KeyValuePair<string, List<float>> attrib)
        {
            string s = attrib.Key;
            foreach (var f in attrib.Value)
            {
                s = backreplace.Replace(s, f + "", 1);
            }
            return s;
        }

        private void CreateSkillList()
        {
            //skillSupports = new Dictionary<SkillGem,List<SkillGem>>();
            List<SkillGem> supportGems = new List<SkillGem>();
            activeGems = new List<SkillGem>();
            ActiveGem normalAttack = new ActiveGem() { Name = "Default Attack", Mods = new Dictionary<string, List<float>>(), Keywords = new List<string>() };
            activeGems.Add(normalAttack);
            foreach (var item in ItemsData.EquippedItems)
            {
                foreach (var gem in item.Gems)
                {
                    if (gem.GemType == GemType.Support)
                        supportGems.Add(gem);
                    else if (gem.GemType == GemType.Attack || gem.GemType == GemType.Spell || gem.GemType == GemType.Totem || gem.GemType == GemType.Trap)
                        activeGems.Add(gem);
                }
            }
            foreach (var item in ItemsData.NonEquippedItems)
            {
                foreach (var gem in item.Gems)
                {
                    if (gem.GemType == GemType.Support)
                        supportGems.Add(gem);
                    else if (gem.GemType == GemType.Attack || gem.GemType == GemType.Spell || gem.GemType == GemType.Totem || gem.GemType == GemType.Trap)
                        activeGems.Add(gem);
                }
            }
            CharacterStats.SupportGems = supportGems;
            skillsComboBox.Items.Clear();
            skillsComboBox.Items.Add(new ComboBoxItem() { Content = normalAttack.Name });
            foreach (var gem in activeGems)
            {
                if (gem.Name != "Default Attack")
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var mod in gem.Mods)
                        sb.AppendLine(InsertNumbersInAttributes(mod));
                    skillsComboBox.Items.Add(new ComboBoxItem() { Content = gem.Name, ToolTip = "Level: " + gem.Level + "\n" + sb.ToString() + gem.Description });
                }
            }
            skillsComboBox.SelectedIndex = 0;
        }

        private void SaveUserSettings()
        {
            UserSettings userSets = new UserSettings();
            userSets.CharLevel = Tree.CharLevel;
            userSets.ItemDataFile = loadedItemDataFile;
            userSets.BuildLink = tbSkillURL.Text;

            if (cruelDiffRB.IsChecked.Value)
                userSets.Difficulty = "Cruel";
            else if (mercilessDiffRB.IsChecked.Value)
                userSets.Difficulty = "Merciless";
            else
                userSets.Difficulty = "Normal";

            if (normalKraitynRB.IsChecked.Value)
                userSets.BanditsNormal = "Kraityn";
            else if (normalAliraRB.IsChecked.Value)
                userSets.BanditsNormal = "Alira";
            else if (normalOakRB.IsChecked.Value)
                userSets.BanditsNormal = "Oak";
            else
                userSets.BanditsNormal = "Kill All";

            if (cruelKraitynRB.IsChecked.Value)
                userSets.BanditsCruel = "Kraityn";
            else if (cruelAliraRB.IsChecked.Value)
                userSets.BanditsCruel = "Alira";
            else if (cruelOakRB.IsChecked.Value)
                userSets.BanditsCruel = "Oak";
            else
                userSets.BanditsCruel = "Kill All";

            if (mercilessKraitynRB.IsChecked.Value)
                userSets.BanditsMerciless = "Kraityn";
            else if (mercilessAliraRB.IsChecked.Value)
                userSets.BanditsMerciless = "Alira";
            else if (mercilessOakRB.IsChecked.Value)
                userSets.BanditsMerciless = "Oak";
            else
                userSets.BanditsMerciless = "Kill All";

            try
            {
                System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(UserSettings));
                if (!Directory.Exists("Data"))
                    Directory.CreateDirectory("Data");
                using (Stream s = File.Create("Data/UserSettings.xml"))
                    xs.Serialize(s, userSets);

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Error in saving User Settings", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                //System.Windows.Application.Current.Shutdown();
            }
        }

        private void LoadUserSettings()
        {
            try
            {
                UserSettings userSets;
                if (File.Exists("Data/UserSettings.xml"))
                {
                    System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(UserSettings));
                    using (Stream s = File.OpenRead("Data/UserSettings.xml"))
                        userSets = (UserSettings)xs.Deserialize(s);

                    levelNumericUpDown.Value = userSets.CharLevel;
                    //levelNumericUpDown_ValueChanged(this, null);

                    tbSkillURL.Text = userSets.BuildLink;
                    btnLoadBuild_Click(this, new RoutedEventArgs());

                    if (userSets.ItemDataFile != null && userSets.ItemDataFile.Length > 0)
                    {
                        loadedItemDataFile = userSets.ItemDataFile;
                        ItemsData = new CharItemData(loadedItemDataFile);
                        lbItemAttr.ItemsSource = ItemsData.AttribCollectionView;
                        UpdateAllAttributeList();
                        EnableAllUserSettings();
                        CreateSkillList(); //needs to come after UpdateAllAttributeList
                        optimizerButton.IsEnabled = true;
                    }

                    if (userSets.Difficulty == "Cruel")
                        cruelDiffRB.IsChecked = true;
                    else if (userSets.Difficulty == "Merciless")
                        mercilessDiffRB.IsChecked = true;

                    if (userSets.BanditsNormal == "Kraityn")
                        normalKraitynRB.IsChecked = true;
                    else if (userSets.BanditsNormal == "Alira")
                        normalAliraRB.IsChecked = true;
                    else if (userSets.BanditsNormal == "Oak")
                        normalOakRB.IsChecked = true;

                    if (userSets.BanditsCruel == "Kraityn")
                        cruelKraitynRB.IsChecked = true;
                    else if (userSets.BanditsCruel == "Alira")
                        cruelAliraRB.IsChecked = true;
                    else if (userSets.BanditsCruel == "Oak")
                        cruelOakRB.IsChecked = true;

                    if (userSets.BanditsMerciless == "Kraityn")
                        mercilessKraitynRB.IsChecked = true;
                    else if (userSets.BanditsMerciless == "Alira")
                        mercilessAliraRB.IsChecked = true;
                    else if (userSets.BanditsMerciless == "Oak")
                        mercilessOakRB.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Error in loading User Settings", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                //System.Windows.Application.Current.Shutdown();
            }
        }

        private void UpdateBuildCollection()
        {
            lvBuildCollection.Items.Clear();
            foreach (PoEBuild poeBuild in buildCollection)
            {
                var build = Tree.LoadFromURL(new Uri(poeBuild.url));
                if (build.Item1 == Tree.Chartype)
                {
                    ListViewItem lvi = new ListViewItem
                    {
                        Content = poeBuild
                    };
                    lvi.MouseDoubleClick += lvi_MouseDoubleClick;
                    lvBuildCollection.Items.Add(lvi);
                }
            }
        }

        private void LoadSavedBuilds()
        {
            // loading saved build list
            try
            {
                if (File.Exists("Data/savedBuilds"))
                {
                    string[] builds = File.ReadAllText("Data/savedBuilds").Split('\n');
                    foreach (string b in builds)
                    {
                        savedBuilds.Add(new PoEBuild(b.Split(';')[0].Split('|')[0], b.Split(';')[0].Split('|')[1], b.Split(';')[1]));
                    }

                    lvSavedBuilds.Items.Clear();
                    foreach (PoEBuild build in savedBuilds)
                    {
                        ListViewItem lvi = new ListViewItem
                        {
                            Content = build
                        };
                        lvi.MouseDoubleClick += lvi_MouseDoubleClick;
                        lvSavedBuilds.Items.Add(lvi);
                    }
                }
                pathIds = new HashSet<long>();
                if (File.Exists("Data/BuildCollection.xml"))
                {
                    buildCollection = LoadBuilds();
                    foreach (PoEBuild poeBuild in buildCollection)
                    {
                        var build = Tree.LoadFromURL(new Uri(poeBuild.url));
                        long pathId = 0;
                        foreach (var id in build.Item2)
                            pathId += id;
                        if (!pathIds.Contains(pathId))
                            pathIds.Add(pathId);
                        if (build.Item1 == cbCharType.SelectedIndex)
                        {
                            ListViewItem lvi = new ListViewItem
                            {
                                Content = poeBuild
                            };
                            lvi.MouseDoubleClick += lvi_MouseDoubleClick;
                            lvBuildCollection.Items.Add(lvi);
                        }
                    }
                }
                else
                    buildCollection = new List<PoEBuild>();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load the saved builds.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //-------------------------------------------------------------------------------
        #endregion
        //-------------------------------------------------------------------------------
        #region Event Handlers
        //-------------------------------------------------------------------------------
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveUserSettings();

            if (lvSavedBuilds.Items.Count > 0)
            {
                StringBuilder rawBuilds = new StringBuilder();
                foreach (ListViewItem lvi in lvSavedBuilds.Items)
                {
                    PoEBuild build = (PoEBuild)lvi.Content;
                    rawBuilds.Append(build.name + '|' + build.description + ';' + build.url + '\n');
                }
                File.WriteAllText("Data/savedBuilds", rawBuilds.ToString().Trim());
            }
            else
            {
                if (File.Exists("Data/savedBuilds"))
                {
                    File.Delete("Data/savedBuilds");
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Get the default view 
            ListCollectionView passiveAttibuteCollection = (ListCollectionView)CollectionViewSource.GetDefaultView(passiveAttributeList);
            // Do the grouping 
            passiveAttibuteCollection.GroupDescriptions.Clear();
            passiveAttibuteCollection.GroupDescriptions.Add(new PropertyGroupDescription("", new StringToGroupConverter()));
            passiveAttibuteCollection.GroupDescriptions.Add(new PropertyGroupDescription("", new StringToSubGroupConverter()));
            passiveAttibuteCollection.CustomSort = new GroupComparer();
            lbPassiveAttr.ItemsSource = passiveAttibuteCollection;

            ListCollectionView allAttributeCollection = (ListCollectionView)CollectionViewSource.GetDefaultView(allAttributesList);
            allAttributeCollection.GroupDescriptions.Clear();
            allAttributeCollection.GroupDescriptions.Add(new PropertyGroupDescription("", new StringToGroupConverter()));
            allAttributeCollection.GroupDescriptions.Add(new PropertyGroupDescription("", new StringToSubGroupConverter()));
            allAttributeCollection.CustomSort = new GroupComparer();
            lbAllAttr.ItemsSource = allAttributeCollection;

            Tree = SkillTree.CreateSkillTree(startLoadingWindow, updatetLoadingWindow, closeLoadingWindow);
            treeBackgroundImage.Fill = new VisualBrush(Tree.SkillTreeVisual);


            Tree.Chartype = cbCharType.SelectedIndex;
            Tree.Reset();

            levelNumericUpDown.Value = Tree.CharLevel; //it updates UpdateAllAttributeList();
            CharacterStats = new CharStats(Tree);
            statsListView.DataContext = CharacterStats;
            chargesGrid.DataContext = CharacterStats;

            multransform = Tree.TRect.Size / treeBackgroundImage.RenderSize.Height;
            addtransform = Tree.TRect.TopLeft;

            LoadSavedBuilds();
            // loading User settings
            LoadUserSettings(); 
        }

        private void DifficultyRB_Checked(object sender, RoutedEventArgs e)
        {
            if (CharacterStats != null)
            {
                if ((sender as RadioButton).Name == "cruelDiffRB")
                    CharacterStats.CruelDiff = true;
                else if ((sender as RadioButton).Name == "mercilessDiffRB")
                    CharacterStats.MercilessDiff = true;
            }
        }

        private void BanditQuestNormRB_Checked(object sender, RoutedEventArgs e)
        {
            if (CharacterStats != null)
            {
                if ((sender as RadioButton).Name == "normalKraitynRB")
                    CharacterStats.BanditsAllResistReward = true;
                else if ((sender as RadioButton).Name == "normalAliraRB")
                    CharacterStats.BanditsManaReward = true;
                else if ((sender as RadioButton).Name == "normalOakRB")
                    CharacterStats.BanditsLifeReward = true;
                else
                    CharacterStats.NormalBanditsKillAll = true;
            }
        }

        private void BanditQuestCruelRB_Checked(object sender, RoutedEventArgs e)
        {
            if (CharacterStats != null)
            {
                if ((sender as RadioButton).Name == "cruelKraitynRB")
                    CharacterStats.BanditsASReward = true;
                else if ((sender as RadioButton).Name == "cruelAliraRB")
                    CharacterStats.BanditsCSReward = true;
                else if ((sender as RadioButton).Name == "cruelOakRB")
                    CharacterStats.BanditsPDReward = true;
                else
                    CharacterStats.CruelBanditsKillAll = true;
            }
        }

        private void BanditQuestMercRB_Checked(object sender, RoutedEventArgs e)
        {
            if (CharacterStats != null)
            {
                if ((sender as RadioButton).Name == "mercilessKraitynRB")
                    CharacterStats.BanditsFCReward = true;
                else if ((sender as RadioButton).Name == "mercilessAliraRB")
                    CharacterStats.BanditsPCReward = true;
                else if ((sender as RadioButton).Name == "mercilessOakRB")
                    CharacterStats.BanditsECReward = true;
                else
                    CharacterStats.MercilessBanditsKillAll = true;
            }
        }

        private void AngerAura_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (CharacterStats != null)
                CharacterStats.AngerLvl = (int)e.NewValue;
        }
        private void VitalityAura_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (CharacterStats != null)
                CharacterStats.VitalityLvl = (int)e.NewValue;
        }
        private void DeterminationAura_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (CharacterStats != null)
                CharacterStats.DeterminationLvl = (int)e.NewValue;
        }
        private void PurityofFireAura_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (CharacterStats != null)
                CharacterStats.PurityofFireLvl = (int)e.NewValue;
        }
        private void HatredAura_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (CharacterStats != null)
                CharacterStats.HatredLvl = (int)e.NewValue;
        }
        private void HasteAura_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (CharacterStats != null)
                CharacterStats.HasteLvl = (int)e.NewValue;
        }
        private void GraceAura_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (CharacterStats != null)
                CharacterStats.GraceLvl = (int)e.NewValue;
        }
        private void PurityofIceAura_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (CharacterStats != null)
                CharacterStats.PurityofIceLvl = (int)e.NewValue;
        }
        private void WrathAura_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (CharacterStats != null)
                CharacterStats.WrathLvl = (int)e.NewValue;
        }
        private void DisciplineAura_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (CharacterStats != null)
                CharacterStats.DisciplineLvl = (int)e.NewValue;
        }
        private void ClarityAura_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (CharacterStats != null)
                CharacterStats.ClarityLvl = (int)e.NewValue;
        }
        private void PofLightning_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (CharacterStats != null)
                CharacterStats.PurityofLightningLvl = (int)e.NewValue;
        }
        private void PofElements_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (CharacterStats != null)
                CharacterStats.PurityofElementsLvl = (int)e.NewValue;
        }

        private void EnduranceCharge_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (CharacterStats != null)
                CharacterStats.CurrentEndurance = (int)e.NewValue;
        }
        private void FrenzyCharge_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (CharacterStats != null)
                CharacterStats.CurrentFrenzy = (int)e.NewValue;
        }
        private void PowerCharge_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (CharacterStats != null)
                CharacterStats.CurrentPower = (int)e.NewValue;
        }



        private void skillsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (skillsComboBox.SelectedItem != null)
            {
                skillsComboBox.ToolTip = (skillsComboBox.SelectedItem as ComboBoxItem).ToolTip;
                SkillGem activeGem = activeGems.First(gem => gem.Name == (skillsComboBox.SelectedItem as ComboBoxItem).Content.ToString());
                CharacterStats.ActiveSkillGem = activeGem;
                supportSkillPanel.Children.Clear();
                if (activeGem.Name != "Default Attack")
                {
                    foreach (var supportGem in CharacterStats.SupportGems)
                    {
                        if (SupportMatchSkill(activeGem, supportGem))
                        {
                            StringBuilder sb = new StringBuilder();
                            foreach (var mod in supportGem.Mods)
                                sb.AppendLine(InsertNumbersInAttributes(mod));
                            CheckBox chkBox = new CheckBox() { Content = new TextBlock() { Text = supportGem.Name, ToolTip = "Level: " + supportGem.Level + "\n" + sb.ToString() }, Margin = new Thickness(5, 0, 0, 0), IsChecked = (supportGem as SupportGem).Selected, Tag = supportGem.Level };
                            chkBox.Checked += supportGemchkBox_Checked;
                            chkBox.Unchecked += supportGemchkBox_Unchecked;
                            supportSkillPanel.Children.Add(chkBox);
                        }
                        else
                            (supportGem as SupportGem).Selected = false;
                    }
                }
                CharacterStats.CalcSkillDPS();
            }
        }

        private bool SupportMatchSkill(SkillGem activeGem, SkillGem supportGem)
        {
            if (supportGem.Keywords.Contains("AoE") && !activeGem.Keywords.Contains("AoE"))
                return false;
            if (supportGem.Keywords.Contains("Attack") && !activeGem.Keywords.Contains("Attack"))
                return false;
            if (supportGem.Keywords.Contains("Curse") && !activeGem.Keywords.Contains("Curse"))
                return false;
            if (supportGem.Keywords.Contains("Duration") && !activeGem.Keywords.Contains("Duration"))
                return false;
            if (supportGem.Keywords.Contains("Melee") && !activeGem.Keywords.Contains("Melee"))
                return false;
            if (supportGem.Keywords.Contains("Minion") && !activeGem.Keywords.Contains("Minion"))
                return false;
            if (supportGem.Keywords.Contains("Movement") && !activeGem.Keywords.Contains("Movement"))
                return false;
            if (supportGem.Keywords.Contains("Projectile") && (!activeGem.Keywords.Contains("Projectile") && !activeGem.Keywords.Contains("Bow")))
                return false;
            if (supportGem.Keywords.Contains("Spell") && !activeGem.Keywords.Contains("Spell"))
                return false;
            if (supportGem.Keywords.Contains("Trap") && activeGem.Keywords.Contains("Trap")) //trap can not be used for traps
                return false;
            return true;
        }
        void supportGemchkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SkillGem supportGem = CharacterStats.SupportGems.First(gem => gem.Name == ((sender as CheckBox).Content as TextBlock).Text && ((sender as CheckBox).Tag != null && (int)(sender as CheckBox).Tag == gem.Level));
            (supportGem as SupportGem).Selected = false;
            CharacterStats.CalcSkillDPS();
        }

        void supportGemchkBox_Checked(object sender, RoutedEventArgs e)
        {
            SkillGem supportGem = CharacterStats.SupportGems.First(gem => gem.Name == ((sender as CheckBox).Content as TextBlock).Text && ((sender as CheckBox).Tag != null && (int)(sender as CheckBox).Tag == gem.Level));
            (supportGem as SupportGem).Selected = true;
            CharacterStats.CalcSkillDPS();
        }

        private void treeBackground_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(treeBackground.Child);
            Vector2D v = new Vector2D(p.X, p.Y);
            v = v * multransform + addtransform;
            textBox1.Content = "" + v.X;
            textBox2.Content = "" + v.Y;
            SkillTree.SkillNode node = null;

            var nodes = Tree.Skillnodes.Where(n => ((n.Value.Position - v).Length < 50));
            if (nodes != null && nodes.Count() != 0)
                node = nodes.First().Value;

            if (node != null && node.Attributes.Count != 0)
            {
                textBox1.Content = "" + node.id;
                string tooltip = node.name + "\n" + node.attributes.Aggregate((s1, s2) => s1 + "\n" + s2);
                if (!(sToolTip.IsOpen == true && lastTooltip == tooltip))
                {
                    sToolTip.Content = tooltip;
                    sToolTip.IsOpen = true;
                    lastTooltip = tooltip;
                }
                if (Tree.SkilledNodes.Contains(node.id))
                {
                    toRemove = Tree.ForceRefundNodePreview(node.id);
                    if (toRemove != null)
                        Tree.DrawRefundPreview(toRemove);
                }
                else
                {
                    prePath = Tree.GetShortestPathTo(node.id);
                    Tree.DrawPath(prePath);
                }

            }
            else
            {
                //sToolTip.Tag = false;
                sToolTip.IsOpen = false;
                prePath = null;
                toRemove = null;
                if (Tree != null)
                {
                    Tree.ClearPath();
                }

            }

        }

        private void treeBackground_Click(object sender, RoutedEventArgs e)
        {

            Point p = ((MouseEventArgs)e.OriginalSource).GetPosition(treeBackground.Child);
            Vector2D v = new Vector2D(p.X, p.Y);

            v = v * multransform + addtransform;
            SkillTree.SkillNode node = null;

            var nodes = Tree.Skillnodes.Where(n => ((n.Value.Position - v).Length < 50));
            if (nodes != null && nodes.Count() != 0)
            {
                node = nodes.First().Value;

                if (node.spc == null)
                {
                    if (Tree.SkilledNodes.Contains(node.id))
                    {
                        Tree.ForceRefundNode(node.id);
                        UpdateAllAttributeList();

                        prePath = Tree.GetShortestPathTo(node.id);
                        Tree.DrawPath(prePath);
                    }
                    else if (prePath != null)
                    {
                        foreach (ushort i in prePath)
                        {
                            Tree.SkilledNodes.Add(i);
                        }
                        UpdateAllAttributeList();
                        Tree.UpdateAvailNodes();

                        toRemove = Tree.ForceRefundNodePreview(node.id);
                        if (toRemove != null)
                            Tree.DrawRefundPreview(toRemove);
                    }
                }
            }
            tbSkillURL.Text = Tree.SaveToURL();
        }

        private void classComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Tree == null)
                return;

            Tree.Chartype = cbCharType.SelectedIndex;
            UpdateBuildCollection();
            if (SkipTreeReset)
            {             
                SkipTreeReset = false;
                return;
            }

            Tree.Reset();
            UpdateAllAttributeList();
        }

        private void levelNumericUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (levelNumericUpDown.Value != null)
            {
                int lvl = levelNumericUpDown.Value.Value;
                if (lvl >= 1)
                {
                    Tree.CharLevel = lvl;
                    UpdateAllAttributeList();
                }
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            if (Tree == null)
                return;
            Tree.Reset();

            UpdateAllAttributeList();
        }

        private void btnLoadBuild_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (tbSkillURL.Text.Contains("poezone.ru"))
                {
                    SkillTreeImporter.LoadBuildFromPoezone(Tree, tbSkillURL.Text);
                    tbSkillURL.Text = Tree.SaveToURL();
                }
                else
                    Tree.LoadFromURL(tbSkillURL.Text);

                if (cbCharType.SelectedIndex != Tree.Chartype)
                {
                    SkipTreeReset = true;
                    cbCharType.SelectedIndex = Tree.Chartype;
                }
                UpdateAllAttributeList();
            }
            catch (Exception)
            {
                MessageBox.Show("The Build you tried to load, is invalid");
            }
        }

        private void tbSkillURL_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            tbSkillURL.SelectAll();
        }

        private void btnImportItems_Click(object sender, RoutedEventArgs e)
        {
            popup1.IsOpen = true;
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            Tree.HighlightNodes(tbSearch.Text, checkBox1.IsChecked.Value);
        }
        //private void skillAllHighligted_Click(object sender, RoutedEventArgs e)
        //{
        //    Tree.SkillAllHighligtedNodes();
        //    UpdateAllAttributeList();
        //}
        private void btnScreenShot_Click(object sender, RoutedEventArgs e)
        {
            int maxsize = 3000;
            Geometry geometry = Tree.picActiveLinks.Clip;
            Rect2D contentBounds = Tree.picActiveLinks.ContentBounds;
            contentBounds *= 1.2;

            double aspect = contentBounds.Width / contentBounds.Height;
            double xmax = contentBounds.Width;
            double ymax = contentBounds.Height;
            if (aspect > 1 && xmax > maxsize)
            {
                xmax = maxsize;
                ymax = xmax / aspect;
            }
            if (aspect < 1 & ymax > maxsize)
            {
                ymax = maxsize;
                xmax = ymax * aspect;
            }

            ClipboardBmp = new RenderTargetBitmap((int)xmax, (int)ymax, 96, 96, PixelFormats.Pbgra32);
            VisualBrush db = new VisualBrush(Tree.SkillTreeVisual);
            db.ViewboxUnits = BrushMappingMode.Absolute;
            db.Viewbox = contentBounds;
            DrawingVisual dw = new DrawingVisual();

            using (DrawingContext dc = dw.RenderOpen())
            {
                dc.DrawRectangle(db, null, new Rect(0, 0, xmax, ymax));
            }
            ClipboardBmp.Render(dw);
            ClipboardBmp.Freeze();

            Clipboard.SetImage(ClipboardBmp);

            treeBackgroundImage.Fill = new VisualBrush(Tree.SkillTreeVisual);

        }
        private void btnCopyStats_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            if (passiveTabItem.IsSelected)
                foreach (var attrib in lbPassiveAttr.ItemsSource)
                {
                    sb.AppendLine(attrib.ToString());
                }
            else if (itemTabItem.IsSelected)
                foreach (CharItemData.Attribute attrib in lbItemAttr.ItemsSource)
                {
                    sb.AppendLine(attrib.Group + ":  " + attrib.ValuedAttribute);
                }
            else if (totalTabItem.IsSelected)
                foreach (var attrib in lbAllAttr.ItemsSource)
                {
                    sb.AppendLine(attrib.ToString());
                }
            Clipboard.SetText(sb.ToString(), TextDataFormat.Text);

        }

        private void optimizerButton_Click(object sender, RoutedEventArgs e)
        {
            List<HashSet<ushort>> classBuilds = new List<HashSet<ushort>>();
            foreach (var poeBuild in buildCollection)
            {
                var build = Tree.LoadFromURL(new Uri(poeBuild.url));
                if (build.Item1 == cbCharType.SelectedIndex)
                    classBuilds.Add(build.Item2);
            }
            StatsWindow statsWindow = new StatsWindow(Tree, ItemsData);
            //statsWindow.CloneCharStats(CharacterStats, statsWindow.CharacterStats);
            statsWindow.CloneCharStats(CharacterStats, statsWindow.StatsComparer);
            statsWindow.LoadBuilds(classBuilds);
            statsWindow.Dispatcher.Thread.IsBackground = true;
            statsWindow.Owner = this;
            statsWindow.Show();
        }
        //-------------------------------------------------------------------------------
        #region PopUp Window
        private void btnPopupOk_Click(object sender, RoutedEventArgs e)
        {
            popup1.IsOpen = false;

        }

        private void btnDownloadItemData_Click(object sender, RoutedEventArgs e)
        {
            popup1.IsOpen = false;
            System.Diagnostics.Process.Start("http://www.pathofexile.com/character-window/get-items?character=" + tbCharName.Text);

        }

        private void btnLoadItemDataFromFile_Click(object sender, RoutedEventArgs e)
        {
            popup1.IsOpen = false;
            var fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            bool? ftoload = fileDialog.ShowDialog(this);
            if (ftoload.Value)
            {
                try
                {
                    loadedItemDataFile = fileDialog.FileName;
                    ItemsData = new CharItemData(loadedItemDataFile);
                    lbItemAttr.ItemsSource = ItemsData.AttribCollectionView;
                    UpdateAllAttributeList();
                    EnableAllUserSettings();
                    CreateSkillList();
                    optimizerButton.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message, "Error in loading Item's Data", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
        }

        private void btnLoadAllGears_Click(object sender, RoutedEventArgs e)
        {
            popup1.IsOpen = false;
            Window window = new Window();
            window.Owner = this;
            window.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            //window.Topmost = true;
            window.Background = new SolidColorBrush(Color.FromRgb(25, 19, 17));
            window.ResizeMode = System.Windows.ResizeMode.CanMinimize;
            window.Width = 1000;
            window.Height = 675;
            Grid inventoryGrid = new Grid();
            window.Content = inventoryGrid;
            window.Show();
            Procurement.ViewModel.ScreenController.Create(inventoryGrid);
            Procurement.ApplicationState.EquippedGearChanged = ApplicationState_EquippedGearChanged;

        }

        void ApplicationState_EquippedGearChanged(object sender, EventArgs e)
        {
            var equippedGear = Procurement.ApplicationState.Inventory[Procurement.ApplicationState.CurrentCharacter.Name].Where(i => i.inventoryId != "MainInventory");
            EquipedItems equipped = new EquipedItems(equippedGear);
            List<Item> nonEquippedGear = Procurement.ApplicationState.Inventory[Procurement.ApplicationState.CurrentCharacter.Name].Where(i => i.inventoryId == "MainInventory").ToList();
            ItemsData = new CharItemData(equipped, nonEquippedGear);
            lbItemAttr.ItemsSource = ItemsData.AttribCollectionView;
            optimizerButton.IsEnabled = true;

            bool characterChanged = (e as Procurement.GearChangedEventArgs).CharacterChanged;
            if (!skillListCreated || characterChanged)
            {
                string email = POEApi.Model.Settings.UserSettings["AccountLogin"];
                loadedItemDataFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Data", email);
                loadedItemDataFile = Path.Combine(loadedItemDataFile, Procurement.ApplicationState.CurrentCharacter.Name + ".bin");
                var passives = Procurement.ApplicationState.CurrentCharacter.PassiveSkills;
                if (cbCharType.SelectedIndex != Procurement.ApplicationState.CurrentCharacter.ClassId)
                {
                    SkipTreeReset = true;
                    cbCharType.SelectedIndex = Procurement.ApplicationState.CurrentCharacter.ClassId;
                }
                else
                {
                    Tree.Chartype = cbCharType.SelectedIndex; //just to make sure it is correct
                    UpdateBuildCollection();
                }
                Tree.Reset();
                foreach (var passive in passives)
                    Tree.SkilledNodes.Add(passive);
                Tree.UpdateAvailNodes();

                tbSkillURL.Text = Tree.SaveToURL();

                if (levelNumericUpDown.Value != Procurement.ApplicationState.CurrentCharacter.Level)
                    levelNumericUpDown.Value = Procurement.ApplicationState.CurrentCharacter.Level;
                else
                    UpdateAllAttributeList();
                EnableAllUserSettings();
                CreateSkillList();
                skillListCreated = true;
            }
            else
                UpdateAllAttributeList();
        }

        private void tbCharName_TextChanged(object sender, TextChangedEventArgs e)
        {
            tbCharLink.Text = "http://www.pathofexile.com/character-window/get-items?character=" + tbCharName.Text;
        }
        #endregion
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        #region Builds Window
        private void lvi_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem lvi = (ListViewItem)sender;
            tbSkillURL.Text = ((PoEBuild)lvi.Content).url;
            btnLoadBuild_Click(this, null); // loading the build
        }

        private void btnSaveNewBuild_Click(object sender, RoutedEventArgs e)
        {
            FormBuildName formBuildName = new FormBuildName();
            if ((bool)formBuildName.ShowDialog())
            {
                ListViewItem lvi = new ListViewItem
                {
                    Content = new PoEBuild(formBuildName.getBuildName(), cbCharType.Text + ", " + tbUsedPoints.Text + " points used", tbSkillURL.Text)
                };
                lvi.MouseDoubleClick += lvi_MouseDoubleClick;
                lvSavedBuilds.Items.Add(lvi);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lvSavedBuilds.SelectedItems.Count > 0)
            {
                lvSavedBuilds.Items.Remove(lvSavedBuilds.SelectedItem);
            }
        }

        private void btnOverwriteBuild_Click(object sender, RoutedEventArgs e)
        {
            if (lvSavedBuilds.SelectedItems.Count > 0)
            {
                ((ListViewItem)lvSavedBuilds.SelectedItem).Content = new PoEBuild(((ListViewItem)lvSavedBuilds.SelectedItem).Content.ToString().Split('\n')[0], cbCharType.Text + ", " + tbUsedPoints.Text + " points used", tbSkillURL.Text);
            }
            else
            {
                MessageBox.Show("Please select an existing build first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void buildsTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as TabControl).SelectedIndex == 0)
            {
                btnAddBuild.Visibility = System.Windows.Visibility.Collapsed;
                btnRemoveBuild.Visibility = System.Windows.Visibility.Collapsed;
                btnUpdateBuilds.Visibility = System.Windows.Visibility.Collapsed;
                collectedBuildsTB.Visibility = System.Windows.Visibility.Collapsed;
                btnSaveNewBuild.Visibility = System.Windows.Visibility.Visible;
                btnOverwriteBuild.Visibility = System.Windows.Visibility.Visible;
                btnDelete.Visibility = System.Windows.Visibility.Visible;
                savedBuildsTB.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                btnAddBuild.Visibility = System.Windows.Visibility.Visible;
                btnRemoveBuild.Visibility = System.Windows.Visibility.Visible;
                btnUpdateBuilds.Visibility = System.Windows.Visibility.Visible;
                collectedBuildsTB.Visibility = System.Windows.Visibility.Visible;
                btnSaveNewBuild.Visibility = System.Windows.Visibility.Collapsed;
                btnOverwriteBuild.Visibility = System.Windows.Visibility.Collapsed;
                btnDelete.Visibility = System.Windows.Visibility.Collapsed;
                savedBuildsTB.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void btnAddBuild_Click(object sender, RoutedEventArgs e)
        {
            FormBuildName formBuildName = new FormBuildName();
            if ((bool)formBuildName.ShowDialog())
            {
                PoEBuild newBuild = new PoEBuild(formBuildName.getBuildName(), cbCharType.Text + ", " + tbUsedPoints.Text + " points used", tbSkillURL.Text);
                ListViewItem lvi = new ListViewItem
                {
                    Content = newBuild
                };
                lvi.MouseDoubleClick += lvi_MouseDoubleClick;
                lvBuildCollection.Items.Add(lvi);
                buildCollection.Add(newBuild);
                SaveBuilds(buildCollection);
            }
        }

        private void btnRemoveBuild_Click(object sender, RoutedEventArgs e)
        {
            if (lvBuildCollection.SelectedItems.Count > 0)
            {
                PoEBuild build = (lvBuildCollection.SelectedItem as ListViewItem).Content as PoEBuild;
                buildCollection.Remove(build);
                lvBuildCollection.Items.Remove(lvBuildCollection.SelectedItem);
                SaveBuilds(buildCollection);
            }
        }
        #endregion
        //-------------------------------------------------------------------------------
        #endregion
        //-------------------------------------------------------------------------------
        #region Internal Classes
        //-------------------------------------------------------------------------------
        [ValueConversion(typeof(string), typeof(string))]
        public class StringToSubGroupConverter : IValueConverter
        {
            static StringToSubGroupConverter()
            {
                if (!File.Exists("groups.txt"))
                    return;
                Groups.Clear();
                foreach (string s in File.ReadAllLines("groups.txt"))
                {
                    string[] sa = s.Split(',');
                    Groups.Add(sa);
                }
            }
            public static List<string[]> Groups = new List<string[]>()
                                                                 { 
                                                                     //priority
                                                                     new []{"maximum life","Health"}, //for CI
                                                                     new []{"dodge","Avoidance"}, //
                                                                     new []{"evasion rating when on low life","Avoidance"}, //
                                                                     new []{"when hit","Mana"}, //for mind over matter
                                                                     new []{"evade projec","Avoidance"}, //for ondar's guile
                                                                     new []{"evasion rating to arm","Mitigation"}, //for iron reflexes
                                                                     new []{"applies to proj","Projectile Damage"}, //for iron grip
                                                                     new []{"removes all mana","Mana"}, //for blood magic
                                                                     new []{"converted to fire","Elemental Damage"}, //for avatar of fire
                                                                     new []{"leeched as life","Health"},
                                                                     new []{"leeched as mana","Mana"},
                                                                     new []{"melee weapon range per","Misc"},
                                                                     new []{"enemy chance to block","Accuracy"},
                                                                     new []{"increased stun dura","Misc"},
                                                                     new []{"movement penal","Speed"}, 
                                                                     //offensive stats
                                                                     new []{"physical","Physical"},
                                                                     new []{"projectile Damage","Projectile Damage"},
                                                                     new []{"projectile attacks","Projectile Damage"}, //for Point Blank
                                                                     new []{"speed","Speed"},
                                                                     new []{"accur","Accuracy"},
                                                                     new []{"hit","Accuracy"},
                                                                     new []{"critical","Critical Strike"},
                                                                     new []{"elemental dam","Elemental Damage"},
                                                                     new []{"cold dam","Elemental Damage"},
                                                                     new []{"fire dam","Elemental Damage"},
                                                                     new []{"lightning dam","Elemental Damage"},
                                                                     new []{"chaos dam","Elemental Damage"},
                                                                     new []{"spell damage","Spell"},
                                                                     
                                                                     //Defensive Stats
                                                                     new []{"life","Health"},
                                                                     new []{"mana","Mana"},
                                                                     new []{"block","Avoidance"},
                                                                     
                                                                     new []{"stun","Avoidance"},
                                                                     new []{"avoid","Avoidance"},
                                                                     new []{"evasion","Avoidance"},
                                                                     new []{"evade","Avoidance"},
                                                                     new []{"armor","Mitigation"},
                                                                     new []{"armour","Mitigation"},
                                                                     new []{"resista","Mitigation"},
                                                                     new []{"energy shield","Health"},
                                                                     //Base stats
                                                                     new []{"intell","Base Stats"},
                                                                     new []{"stren","Base Stats"},
                                                                     new []{"dext","Base Stats"},
                                                                     //Charge stats
                                                                     new []{"charge","Charge"},
                                                                     //Aura stats
                                                                     new []{"aura","Aura"},
                                                                     //Curse stats
                                                                     new []{"curse","Curse"},
                                                                     //Summon stats
                                                                     new []{"totem damage","Damage"},
                                                                     new []{"mine damage","Damage"},
                                                                     new []{"trap damage","Damage"},
                                                                 };
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                string s = (string)value;
                return GetAttributeSubGroup(s);
            }

            public static object GetAttributeSubGroup(string s)
            {
                foreach (var gp in Groups)
                {
                    if (s.ToLower().Contains(gp[0].ToLower()))
                    {
                        return gp[1];
                    }
                }
                return "Misc";
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public class StringToGroupConverter : IValueConverter
        {
            static StringToGroupConverter()
            {
                if (!File.Exists("groups.txt"))
                    return;
                Groups.Clear();
                foreach (string s in File.ReadAllLines("groups.txt"))
                {
                    string[] sa = s.Split(',');
                    Groups.Add(sa);
                }
            }
            public static List<string[]> Groups = new List<string[]>()
                                                                 { 
                                                                     //Priority
                                                                     new []{"your hits","Offence"},
                                                                     new []{"leeched as life","Defence"},
                                                                     new []{"leech life from","Defence"},
                                                                     new []{"enemy chance to block","Offence"}, 
                                                                     new []{"increased stun dura","Offence"}, 
                                                                     new []{"movement penal","Other"}, 
                                                                     new []{"melee weapon range per","Other"},
                                                                     //Summoning
                                                                     new []{"trap","Summon"},
                                                                     new []{"totem","Summon"},
                                                                     new []{"mine","Summon"},
                                                                     new []{"minion","Summon"},
                                                                     new []{"zombie","Summon"},
                                                                     new []{"skeleton","Summon"},
                                                                     new []{"spectre","Summon"},
                                                                     //Defence
                                                                     new []{"maximum life","Defence"},
                                                                     new []{"life reg","Defence"},
                                                                     new []{"when hit","Defence"}, //for mind over matter
                                                                     new []{"shield","Defence"},
                                                                     new []{"armor","Defence"},
                                                                     new []{"armour","Defence"},
                                                                     new []{"resistance","Defence"},
                                                                     new []{"stun","Defence"},
                                                                     new []{"block","Defence"},
                                                                     new []{"avoid","Defence"},
                                                                     new []{"dodge","Defence"},
                                                                     new []{"evasi","Defence"},
                                                                     new []{"evade","Defence"},
                                                                     new []{"mana","Defence"},
                                                                     //Offence
                                                                     new []{"damage","Offence"},
                                                                     new []{"projectile","Offence"},
                                                                     new []{"attack speed","Offence"},
                                                                     new []{"cast speed","Offence"},
                                                                     new []{"critical","Offence"},
                                                                     new []{"accura","Offence"},
                                                                     new []{"hit","Offence"},
                                                                     new []{"ignite","Offence"},
                                                                     new []{"freeze","Offence"},
                                                                     new []{"shock","Offence"},
                                                                 };
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                string s = (string)value;
                return GetAttributeGroup(s);
            }

            public static object GetAttributeGroup(string s)
            {
                foreach (var gp in Groups)
                {
                    if (s.ToLower().Contains(gp[0].ToLower()))
                    {
                        return gp[1];
                    }
                }
                return "Other";
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public class GroupComparer : System.Collections.IComparer
        {
            //static Regex numberfilter = new Regex(@"[0-9\\.]+");
            static Regex numberfilter = new Regex("[0-9]*\\.?[0-9]+");
            static Dictionary<string, int> grpValues = new Dictionary<string, int>() { { "Offence", -4 }, { "Defence", -3 }, { "Summon", -2 }, { "Other", -1 }, { "Physical", 0 }, { "Projectile Damage", 1 }, { "Elemental Damage", 2 }, { "Spell", 3 }, { "Accuracy", 4 }, { "Speed", 5 }, { "Critical Strike", 6 }, { "Health", 7 }, { "Mana", 8 }, { "Mitigation", 9 }, { "Avoidance", 10 }, { "Base Stats", 11 }, { "Aura", 12 }, { "Curse", 13 }, { "Charge", 14 }, { "Damage", 15 }, { "Misc", 16 } };
            public int Compare(object x, object y)
            {
                string group1 = (string)StringToGroupConverter.GetAttributeGroup((string)x);
                string group2 = (string)StringToGroupConverter.GetAttributeGroup((string)y);

                //return numberfilter.Replace(x, "").CompareTo(numberfilter.Replace(y, ""));
                if (grpValues[group1] > grpValues[group2])
                {
                    return 1;
                }
                else if (grpValues[group1] < grpValues[group2])
                {
                    return -1;
                }
                string subGroup1 = (string)StringToSubGroupConverter.GetAttributeSubGroup((string)x);
                string subGroup2 = (string)StringToSubGroupConverter.GetAttributeSubGroup((string)y);

                if (grpValues[subGroup1] > grpValues[subGroup2])
                    return 1;
                else if (grpValues[subGroup1] < grpValues[subGroup2])
                    return -1;
                return numberfilter.Replace((string)x, "").CompareTo(numberfilter.Replace((string)y, ""));
            }
        }
        #endregion
        //-------------------------------------------------------------------------
        #region WebCrawler
        //-------------------------------------------------------------------------
        List<PoEBuild> buildCollection;
        HashSet<long> pathIds;
        int count;
        byte charClass;
        private void GetNewBuilds_Click(object sender, RoutedEventArgs e)
        {
            CreateProgressWindow();
            while (progressWindow == null) System.Threading.Thread.Sleep(10);

            LetsGetSomeBuilds();

            progressWindow.ShutDown();
            progressWindow = null;
            System.Windows.Forms.MessageBox.Show(count + " New builds found.");
        }

        private void LetsGetSomeBuilds()
        {
            //Will use app.config for confguration
            count = 0;
            charClass = (byte)cbCharType.SelectedIndex;
            PoliteWebCrawler crawler = new PoliteWebCrawler();

            //crawler.PageCrawlStartingAsync += crawler_ProcessPageCrawlStarting;
            crawler.PageCrawlCompletedAsync += crawler_ProcessPageCrawlCompleted;
            //crawler.PageCrawlDisallowedAsync += crawler_PageCrawlDisallowed;
            //crawler.PageLinksCrawlDisallowedAsync += crawler_PageLinksCrawlDisallowed;

            crawler.ShouldCrawlPage((pageToCrawl, crawlContext) =>
            {
                CrawlDecision decision = new CrawlDecision();
                if (pageToCrawl.Uri.AbsoluteUri.Contains("pathofpoe.com/builds/") || pageToCrawl.Uri.AbsoluteUri.Contains("pathofpoe.com/builds/view/"))
                    return new CrawlDecision { Allow = true };
                else
                    decision.Allow = false;
                return decision;
            });

            //crawler.ShouldDownloadPageContent((crawledPage, crawlContext) =>
            //{
            //    CrawlDecision decision = new CrawlDecision();
            //    if (!crawledPage.Uri.AbsoluteUri.Contains("pathofpoe.com/builds"))
            //        return new CrawlDecision { Allow = false, Reason = "Only download raw page content for .com tlds" };
            //    else
            //        decision.Allow = true;
            //    return decision;
            //});

            //crawler.ShouldCrawlPageLinks((crawledPage, crawlContext) =>
            //{
            //    CrawlDecision decision = new CrawlDecision();
            //    if (crawledPage.Uri.AbsoluteUri.Contains("pathofpoe.com/builds/view"))
            //        return new CrawlDecision { Allow = false, Reason = "We dont want to go any further" };
            //    else
            //        decision.Allow = true;
            //    return decision;
            //});
            CrawlResult result = crawler.Crawl(new Uri("http://pathofpoe.com/builds/"));

            if (result.ErrorOccurred)
            {
                //WriteLine("Crawl of {0} completed with error: {1}", result.RootUri.AbsoluteUri, result.ErrorException.Message);
                System.Windows.Forms.MessageBox.Show("Crawl of " + result.RootUri.AbsoluteUri + " completed with error: " + result.ErrorException.Message);
            }
            else
            {
                //Console.WriteLine("Crawl of {0} completed without error.", result.RootUri.AbsoluteUri);
                SaveBuilds(buildCollection);
            }
        }

        void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
            PageToCrawl pageToCrawl = e.PageToCrawl;
            Console.WriteLine("About to crawl link {0} which was found on page {1}", pageToCrawl.Uri.AbsoluteUri, pageToCrawl.ParentUri.AbsoluteUri);
        }

        void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;
            if (crawledPage.ParsedLinks != null)
                foreach (var link in crawledPage.ParsedLinks)
                    if (link.AbsoluteUri.Contains("http://www.pathofexile.com/passive-skill-tree/AAAAA"))
                    {
                        int startIndex = crawledPage.Content.Text.IndexOf("<title>");
                        startIndex += 7;
                        int endIndex = crawledPage.Content.Text.IndexOf("</title>");
                        endIndex -= 14;
                        string buildName = crawledPage.Content.Text.Substring(startIndex, endIndex - startIndex);
                        var build = Tree.LoadFromURL(link);
                        long pathId = 0;
                        foreach (var id in build.Item2)
                            pathId += id;

                        if (!pathIds.Contains(pathId) && build.Item2.Count > 1)
                        {
                            pathIds.Add(pathId);
                            string className = ((SkillTree.PoEClass)build.Item1).ToString();
                            var poeBuild = new PoEBuild(buildName, className + ", " + (build.Item2.Count - 1).ToString() + " points used", link.AbsoluteUri);
                            buildCollection.Add(poeBuild);
                            if (build.Item1 == charClass)
                            {
                                ListViewItem lvi = new ListViewItem
                                {
                                    Content = poeBuild
                                };
                                lvi.MouseDoubleClick += lvi_MouseDoubleClick;
                                lvBuildCollection.Items.Add(lvi);
                            }
                            count++;
                        }


                    }
            //if (crawledPage.WebException != null || crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
            //    Console.WriteLine("Crawl of page failed {0}", crawledPage.Uri.AbsoluteUri);
            //else
            //    Console.WriteLine("Crawl of page succeeded {0}", crawledPage.Uri.AbsoluteUri);

            //if (string.IsNullOrEmpty(crawledPage.Content.Text))
            //    Console.WriteLine("Page had no content {0}", crawledPage.Uri.AbsoluteUri);
        }

        void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;

            Console.WriteLine("Did not crawl the links on page {0} due to {1}", crawledPage.Uri.AbsoluteUri, e.DisallowedReason);
        }

        void crawler_PageCrawlDisallowed(object sender, PageCrawlDisallowedArgs e)
        {
            PageToCrawl pageToCrawl = e.PageToCrawl;
            Console.WriteLine("Did not crawl page {0} due to {1}", pageToCrawl.Uri.AbsoluteUri, e.DisallowedReason);
        }

        LoadingWindow progressWindow;

        private void CreateProgressWindow()
        {
            System.Threading.Thread newWindowThread = new System.Threading.Thread(new System.Threading.ThreadStart(ThreadStartingPoint));
            newWindowThread.SetApartmentState(System.Threading.ApartmentState.STA);
            newWindowThread.IsBackground = true;
            newWindowThread.Start();
        }
        private void ThreadStartingPoint()
        {
            progressWindow = new LoadingWindow() { IsIndeterminate = true };
            progressWindow.Show();
            System.Windows.Threading.Dispatcher.Run();
        }

        private void SaveBuilds(List<PoEBuild> buildCollection)
        {
            try
            {
                System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(List<PoEBuild>));
                if (!Directory.Exists("Data"))
                    Directory.CreateDirectory("Data");
                using (Stream s = File.Create("Data/BuildCollection.xml"))
                    xs.Serialize(s, buildCollection);

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Error in saving builds", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
            }
        }

        private List<PoEBuild> LoadBuilds()
        {
            List<PoEBuild> buildCollection;
            try
            {
                if (File.Exists("Data/BuildCollection.xml"))
                {
                    System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(List<PoEBuild>));
                    using (Stream s = File.OpenRead("Data/BuildCollection.xml"))
                        buildCollection = (List<PoEBuild>)xs.Deserialize(s);
                }
                else
                    buildCollection = new List<PoEBuild>();
                return buildCollection;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Error in loading builds", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
            }
            return new List<PoEBuild>();
        }
        #endregion


    }

    public class PoEBuild
    {
        public string name, description, url;
        public PoEBuild() { }
        public PoEBuild(string n, string d, string u)
        {
            this.name = n;
            this.description = d;
            this.url = u;
        }
        public override string ToString()
        {
            return name + '\n' + description;
        }
    }
}

