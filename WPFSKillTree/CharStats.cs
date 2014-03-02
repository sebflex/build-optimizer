using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using INI;

namespace POESKillTree
{
    public class CharStats : INotifyPropertyChanged
    {
        #region Constants
        public const int BaseMaxFrenzyCharge = 3;
        public const int BaseMaxEnduranceCharge = 3;
        public const int BaseMaxPowerCharge = 3;
        public const int BaseMaxResistance = 75;
        public const float ResPerEndurance = 4f;
        public const float PhysRedPerEndurance = 4f;
        public const float ASPerFrenzy = 5f;
        public const float CSPerFrenzy = 5f;
        public const float CritChancePerPower = 50f;

        public const float BaseLife = 42f;
        public const float BaseMana = 36f;
        public const float BaseEvasion = 50f;
        public const float LifePerLevel = 8f;
        public const float EvasPerLevel = 3f;
        public const float ManaPerLevel = 4f;
        public const float AccPerLevel = 2f;
        public const float ManaPerInt = 0.5f;
        public const float ESPerInt = 0.2f; //%
        public const float LifePerStr = 0.5f;
        public const float MDmgPerStr = 0.2f; //%
        public const float AccPerDex = 2f;
        public const float EvasPerDex = 0.2f; //%
        public const float CruelAllResistPenalty = -20;//%
        public const float MercilessAllResistPenalty = -60;//%
        public const float DuelWieldBlockChance = 15;//%
        public const float DuelWieldAttackSpeed = 10;//%
        public const float PrismaticEclipseAs = 12;//%
        public const float PrismaticEclipsePD = 25;//%
        public Dictionary<string, float> BaseAttributes = new Dictionary<string, float>()
                                                              {
                                                                  {"+# to maximum Mana",36},
                                                                  {"+# to maximum Life",42},
                                                                  {"Evasion Rating: #",50},
                                                                  {"+# Maximum Endurance Charge",3},
                                                                  {"+# Maximum Frenzy Charge",3},
                                                                  {"+# Maximum Power Charge",3},
                                                                  {"#% Additional Elemental Resistance per Endurance Charge",4},
                                                                  {"#% Physical Damage Reduction per Endurance Charge",4},
                                                                  {"#% Attack Speed Increase per Frenzy Charge",5},
                                                                  {"#% Cast Speed Increase per Frenzy Charge",5},
                                                                  {"#% Critical Strike Chance Increase per Power Charge",50},
                                                              };
        #endregion
        //-------------------------------------------------------------------------------
        #region Fields
        //-------------------------------------------------------------------------------
        SkillTree skillTree;
        protected CharItemData itemData;
        protected Dictionary<string, List<float>> attribs;
        protected bool normalDiff = false;
        protected bool cruelDiff = false;
        protected bool mercilessDiff = true;
        protected bool banditsLifeReward = false;
        protected bool banditsManaReward = false;
        protected bool banditsAllResistReward = false;
        protected bool banditsASReward = false;
        protected bool banditsCSReward = false;
        protected bool banditsPDReward = false;
        protected bool banditsFCReward = false;
        protected bool banditsPCReward = false;
        protected bool banditsECReward = false;
        protected int[] monsterEvasion;
        protected int[] monsterAccuracy;
        protected int[] monsterDamage;
        protected float increasedAuraEff;
        protected float increasedBuffEff;
        protected float[] vitalityAura;
        protected float[] hatredAura;
        protected float[] determinationAura;
        protected float[] graceAura;
        protected float[] disciplineAura;
        protected float[] clarityAura;
        protected float[] purityofEleAura;
        protected float[][] hasteAura;
        protected float[][] angerAura;
        protected float[][] wrathAura;
        protected float[][] purityofFireAura;
        protected float[][] purityofIceAura;
        protected float[][] purityofLightAura;
        protected int manaFromES;
        protected int armourFromEvasion;
        protected float mainHandMinDamage = 0;
        protected float mainHandMaxDamage = 0;
        protected float offHandMinDamage;
        protected float offHandMaxDamage;
        protected float increasedMeleePdMH = 0;
        protected float increasedMeleePdOH = 0;
        protected float increasedProjDMH = 0;
        protected float increasedProjDOH = 0;

        protected float increasedSpellDmg;
        protected float increasedWeaponED;
        protected float increasedWeaponColdD;
        protected float increasedWeaponFireD;
        protected float increasedWeaponLightningD;
        protected float increasedWeaponChaosD;
        protected float increasedEdMainHand = 0;
        protected float increasedEdOffHand = 0;

        private float mainHandMinDamage_Org;
        private float mainHandMaxDamage_Org;
        private float mainHandAs_Org;
        private float CritChanceMH_Org;
        private float MainHandMinColdD_Org;
        private float MainHandMaxColdD_Org;
        private float MainHandMinFireD_Org;
        private float MainHandMaxFireD_Org;
        private float MainHandMinLightD_Org;
        private float MainHandMaxLightD_Org;
        private float MainHandMinChaosD_Org;
        private float MainHandMaxChaosD_Org;
        private float offHandMinDamage_Org;
        private float offHandMaxDamage_Org;
        private float CritChanceOH_Org;
        private float offHandAs_Org;
        private float OffHandMinColdD_Org;
        private float OffHandMaxColdD_Org;
        private float OffHandMinFireD_Org;
        private float OffHandMaxFireD_Org;
        private float OffHandMinLightD_Org;
        private float OffHandMaxLightD_Org;
        private float OffHandMinChaosD_Org;
        private float OffHandMaxChaosD_Org;

        private float MainHandMinColdD_Tot;
        private float MainHandMaxColdD_Tot;
        private float MainHandMinFireD_Tot;
        private float MainHandMaxFireD_Tot;
        private float MainHandMinLightD_Tot;
        private float MainHandMaxLightD_Tot;
        private float MainHandMinChaosD_Tot;
        private float MainHandMaxChaosD_Tot;
        private float OffHandMinColdD_Tot;
        private float OffHandMaxColdD_Tot;
        private float OffHandMinFireD_Tot;
        private float OffHandMaxFireD_Tot;
        private float OffHandMinLightD_Tot;
        private float OffHandMaxLightD_Tot;
        private float OffHandMinChaosD_Tot;
        private float OffHandMaxChaosD_Tot;
        float increasedAsMH;
        float increasedAsOH;
        float increasedCritSpell;
        protected float comparerModifiedIncreasedCrit;
        protected float comparerModifiedIncreasedCritMult;
        #endregion
        //-------------------------------------------------------------------------------
        #region Properties and Events
        //-------------------------------------------------------------------------------
        public int Level { get; private set; }

        public SkillGem ActiveSkillGem { get; set; }
        public List<SkillGem> SupportGems { get; set; }

        public bool CruelDiff { get { return cruelDiff; } set { cruelDiff = value; mercilessDiff = !value; UpdateBaseAttribs(); } }
        public bool MercilessDiff { get { return mercilessDiff; } set { mercilessDiff = value; cruelDiff = !value; UpdateBaseAttribs(); } }
        public bool NormalDiff { set { cruelDiff = false; mercilessDiff = false; UpdateBaseAttribs(); } }
        public bool BanditsAllResistReward { get { return banditsAllResistReward; } set { banditsAllResistReward = value; banditsManaReward = !value; banditsLifeReward = !value; UpdateBaseAttribs(); UpdateBaseStats(); UpdateMana(); UpdateHP(); } }
        public bool BanditsManaReward { get { return banditsManaReward; } set { banditsManaReward = value; banditsAllResistReward = !value; banditsLifeReward = !value; UpdateBaseAttribs(); UpdateBaseStats(); UpdateMana(); UpdateHP(); } }
        public bool BanditsLifeReward { get { return banditsLifeReward; } set { banditsLifeReward = value; banditsManaReward = !value; banditsAllResistReward = !value; UpdateBaseAttribs(); UpdateBaseStats(); UpdateHP(); UpdateMana(); } }
        public bool BanditsASReward { get { return banditsASReward; } set { banditsASReward = value; banditsCSReward = !value; banditsPDReward = !value; UpdateAttackSpeed(); UpdateCastSpeed(); UpdatePhysicalDamage(); CalcSkillDPS(); } }
        public bool BanditsCSReward { get { return banditsCSReward; } set { banditsCSReward = value; banditsASReward = !value; banditsPDReward = !value; UpdateCastSpeed(); UpdateAttackSpeed(); UpdatePhysicalDamage(); CalcSkillDPS(); } }
        public bool BanditsPDReward { get { return banditsPDReward; } set { banditsPDReward = value; banditsASReward = !value; banditsCSReward = !value; UpdatePhysicalDamage(); UpdateCastSpeed(); UpdateAttackSpeed(); CalcSkillDPS(); } }
        public bool BanditsFCReward { get { return banditsFCReward; } set { banditsFCReward = value; banditsPCReward = !value; banditsECReward = !value; UpdateCharges(); } }
        public bool BanditsPCReward { get { return banditsPCReward; } set { banditsPCReward = value; banditsFCReward = !value; banditsECReward = !value; UpdateCharges(); } }
        public bool BanditsECReward { get { return banditsECReward; } set { banditsECReward = value; banditsFCReward = !value; banditsPCReward = !value; UpdateCharges(); } }
        public bool NormalBanditsKillAll { set { banditsManaReward = false; banditsLifeReward = false; banditsAllResistReward = false; UpdateBaseAttribs(); UpdateBaseStats(); UpdateHP(); UpdateMana(); } }
        public bool CruelBanditsKillAll { set { banditsASReward = false; banditsCSReward = false; banditsPDReward = false; UpdateAttackSpeed(); UpdateCastSpeed(); UpdatePhysicalDamage(); CalcSkillDPS(); } }
        public bool MercilessBanditsKillAll { set { banditsFCReward = false; banditsPCReward = false; banditsECReward = false; UpdateCharges(); } }
        //Charges
        private int currentFrenzy = 0;
        public int CurrentFrenzy { get { return currentFrenzy; } set { currentFrenzy = value; UpdateAttackSpeed(); UpdateCastSpeed(); UpdateChanceToDodge(); UpdateHP(); UpdateBaseStats(); CalcSkillDPS(); } }
        private int currentEndurance = 0;
        public int CurrentEndurance { get { return currentEndurance; } set { currentEndurance = value; UpdateBaseAttribs(); CalcDamageReduction(); } }
        private int currentPower = 0;
        public int CurrentPower { get { return currentPower; } set { currentPower = value; UpdateCriticalStrike(); UpdatePhysicalDamage(); CalcSkillDPS(); } }
        private int maxFrenzy = BaseMaxFrenzyCharge;
        public int MaxFrenzy { get { return maxFrenzy; } set { SetField(ref maxFrenzy, value); CurrentFrenzy = CurrentFrenzy > maxFrenzy ? maxFrenzy : CurrentFrenzy; } }
        private int maxEndurance = BaseMaxEnduranceCharge;
        public int MaxEndurance { get { return maxEndurance; } set { SetField(ref maxEndurance, value); CurrentEndurance = CurrentEndurance > maxEndurance ? maxEndurance : CurrentEndurance; } }
        private int maxPower = BaseMaxPowerCharge;
        public int MaxPower { get { return maxPower; } set { SetField(ref maxPower, value); CurrentPower = CurrentPower > maxPower ? maxPower : CurrentPower; } }
        //Auras
        private int angerLvl;
        public int AngerLvl { get { return angerLvl; } set { angerLvl = value; UpdateElementalDamage(); CalcSkillDPS(); } }
        public float[] AngerFireD
        {
            get
            {
                if (AngerLvl > 0)
                    return AngerLvl <= angerAura.Length ? angerAura[AngerLvl - 1] : angerAura[angerAura.Length - 1];
                else
                    return new float[] { 0, 0 };
            }
        }
        private int wrathLvl;
        public int WrathLvl { get { return wrathLvl; } set { wrathLvl = value; UpdateElementalDamage(); CalcSkillDPS(); } }
        public float[] WrathLightD
        {
            get
            {
                if (WrathLvl > 0)
                    return WrathLvl <= wrathAura.Length ? wrathAura[WrathLvl - 1] : wrathAura[wrathAura.Length - 1];
                else
                    return new float[] { 0, 0 };
            }
        }
        private int hatredLvl;
        public int HatredLvl { get { return hatredLvl; } set { hatredLvl = value; CalcSkillDPS(); } }
        public float HatredColdD
        {
            get
            {
                if (HatredLvl > 0)
                    return HatredLvl <= hatredAura.Length ? hatredAura[HatredLvl - 1] : hatredAura[hatredAura.Length - 1];
                else
                    return 0;
            }
        }
        private int determinationLvl;
        public int DeterminationLvl { get { return determinationLvl; } set { determinationLvl = value; UpdateBaseStats(); CalcDamageReduction(); } }
        public float DeterminationArmour
        {
            get
            {
                if (DeterminationLvl > 0)
                    return DeterminationLvl <= determinationAura.Length ? determinationAura[DeterminationLvl - 1] : determinationAura[determinationAura.Length - 1];
                else
                    return 0;
            }
        }
        private int graceLvl;
        public int GraceLvl { get { return graceLvl; } set { graceLvl = value; UpdateBaseStats(); CalcChanceToEvade(); } }
        public float graceEvasion
        {
            get
            {
                if (GraceLvl > 0)
                    return GraceLvl <= graceAura.Length ? graceAura[GraceLvl - 1] : graceAura[graceAura.Length - 1];
                else
                    return 0;
            }
        }
        private int disciplineLvl;
        public int DisciplineLvl { get { return disciplineLvl; } set { disciplineLvl = value; UpdateBaseStats(); UpdateMana(); } }
        public float disciplineES
        {
            get
            {
                if (DisciplineLvl > 0)
                    return DisciplineLvl <= disciplineAura.Length ? disciplineAura[DisciplineLvl - 1] : disciplineAura[disciplineAura.Length - 1];
                else
                    return 0;
            }
        }
        private int vitalityLvl;
        public int VitalityLvl { get { return vitalityLvl; } set { vitalityLvl = value; UpdateHP(); } }
        public float VitalityLifeRegen
        {
            get
            {
                if (VitalityLvl > 0)
                    return VitalityLvl <= vitalityAura.Length ? vitalityAura[VitalityLvl - 1] : vitalityAura[vitalityAura.Length - 1];
                else
                    return 0;
            }
        }
        private int hasteLvl;
        public int HasteLvl { get { return hasteLvl; } set { hasteLvl = value; UpdateAttackSpeed(); UpdateCastSpeed(); CalcSkillDPS(); } }
        public float[] HasteSpeed
        {
            get
            {
                if (HasteLvl > 0)
                    return HasteLvl <= hasteAura.Length ? hasteAura[HasteLvl - 1] : hasteAura[hasteAura.Length - 1];
                else
                    return new float[] { 0, 0 };
            }
        }
        private int clarityLvl;
        public int ClarityLvl { get { return clarityLvl; } set { clarityLvl = value; UpdateMana(); } }
        public float ClarityManaRegen
        {
            get
            {
                if (ClarityLvl > 0)
                    return ClarityLvl <= clarityAura.Length ? clarityAura[ClarityLvl - 1] : clarityAura[clarityAura.Length - 1];
                else
                    return 0;
            }
        }
        private int purityofFireLvl;
        public int PurityofFireLvl { get { return purityofFireLvl; } set { purityofFireLvl = value; UpdateBaseAttribs(); } }
        public float[] PurityofFireRes
        {
            get
            {
                if (PurityofFireLvl > 0)
                    return PurityofFireLvl <= purityofFireAura.Length ? purityofFireAura[PurityofFireLvl - 1] : purityofFireAura[purityofFireAura.Length - 1];
                else
                    return new float[] { 0, 0 };
            }
        }
        private int purityofIceLvl;
        public int PurityofIceLvl { get { return purityofIceLvl; } set { purityofIceLvl = value; UpdateBaseAttribs(); } }
        public float[] PurityofIceRes
        {
            get
            {
                if (PurityofIceLvl > 0)
                    return PurityofIceLvl <= purityofIceAura.Length ? purityofIceAura[PurityofIceLvl - 1] : purityofIceAura[purityofIceAura.Length - 1];
                else
                    return new float[] { 0, 0 };
            }
        }
        private int purityofLightLvl;
        public int PurityofLightningLvl { get { return purityofLightLvl; } set { purityofLightLvl = value; UpdateBaseAttribs(); } }
        public float[] PurityofLightRes
        {
            get
            {
                if (PurityofLightningLvl > 0)
                    return PurityofLightningLvl <= purityofLightAura.Length ? purityofLightAura[PurityofLightningLvl - 1] : purityofLightAura[purityofLightAura.Length - 1];
                else
                    return new float[] { 0, 0 };
            }
        }
        private int purityofElementsLvl;
        public int PurityofElementsLvl { get { return purityofElementsLvl; } set { purityofElementsLvl = value; UpdateBaseAttribs(); } }
        public float PurityofEleRes
        {
            get
            {
                if (PurityofElementsLvl > 0)
                    return PurityofElementsLvl <= purityofEleAura.Length ? purityofEleAura[PurityofElementsLvl - 1] : purityofEleAura[purityofEleAura.Length - 1];
                else
                    return 0;
            }
        }
        //---------------------------------------------------
        private System.Windows.Visibility offHandVisibility;
        public System.Windows.Visibility OffHandVisibility { get { return offHandVisibility; } set { SetField(ref offHandVisibility, value); } }
        private System.Windows.Visibility mainHandVisibility;
        public System.Windows.Visibility MainHandVisibility { get { return mainHandVisibility; } set { SetField(ref mainHandVisibility, value); } }
        private System.Windows.Visibility spellVisibility;
        public System.Windows.Visibility SpellVisibility { get { return spellVisibility; } set { SetField(ref spellVisibility, value); } }
        private System.Windows.Visibility dotVisibility;
        public System.Windows.Visibility DotVisibility { get { return dotVisibility; } set { SetField(ref dotVisibility, value); } }
        public WeaponStyle CharWeaponStyle { get; private set; }
        public WeaponType WeaponTypeMH { get; private set; }
        public WeaponType WeaponTypeOH { get; private set; }
        private int strength;
        public int Strength { get { return strength; } set { SetField(ref strength, value); } }
        private int dexterity;
        public int Dexterity { get { return dexterity; } set { SetField(ref dexterity, value); } }
        private int intelligence;
        public int Intelligence { get { return intelligence; } set { SetField(ref intelligence, value); } }
        private int fireResist;
        public int FireResist { get { return fireResist; } set { SetField(ref fireResist, value); } }
        private int coldResist;
        public int ColdResist { get { return coldResist; } set { SetField(ref coldResist, value); } }
        private int lightningResist;
        public int LightningResist { get { return lightningResist; } set { SetField(ref lightningResist, value); } }
        private int chaosResist;
        public int ChaosResist { get { return chaosResist; } set { SetField(ref chaosResist, value); } }
        public int KraitynAllResist
        {
            get
            {
                if (BanditsAllResistReward) return 8;
                else return 0;
            }
        }
        public int KraitynAttackSpeed
        {
            get
            {
                if (BanditsASReward) return 8;
                else return 0;
            }
        }
        public int KraitynFrenzyCharge
        {
            get
            {
                if (BanditsFCReward) return 1;
                else return 0;
            }
        }
        //Measured Stats
        private int life;
        public int Life { get { return life; } set { SetField(ref life, value); } }
        private float lifeRegen;
        public float LifeRegen { get { return lifeRegen; } set { SetField(ref lifeRegen, value); } }
        public int OakLife
        {
            get
            {
                if (BanditsLifeReward) return 40;
                else return 0;
            }
        }
        public int OakPhysicalDamage
        {
            get
            {
                if (BanditsPDReward) return 18;
                else return 0;
            }
        }
        public int OakEnduranceCharge
        {
            get
            {
                if (BanditsECReward) return 1;
                else return 0;
            }
        }
        public int AliraMana
        {
            get
            {
                if (BanditsManaReward) return 40;
                else return 0;
            }
        }
        public int AliraCastSpeed
        {
            get
            {
                if (BanditsCSReward) return 4;
                else return 0;
            }
        }
        public int AliraPowerCharge
        {
            get
            {
                if (BanditsPCReward) return 1;
                else return 0;
            }
        }
        private int mana;
        public int Mana { get { return mana; } set { SetField(ref mana, value); } }
        private float manaRegen;
        public float ManaRegen { get { return manaRegen; } set { SetField(ref manaRegen, value); } }

        private int energyShield;
        public int EnergyShield { get { return energyShield; } set { SetField(ref energyShield, value); } }

        private int accuracyRatingMH;
        public int AccuracyRatingMH { get { return accuracyRatingMH; } set { SetField(ref accuracyRatingMH, value); } }
        private int accuracyRatingOH;
        public int AccuracyRatingOH { get { return accuracyRatingOH; } set { SetField(ref accuracyRatingOH, value); } }
        private int chanceToHitMH;
        public int ChanceToHitMH { get { return chanceToHitMH; } set { SetField(ref chanceToHitMH, value); } }
        private int chanceToHitOH;
        public int ChanceToHitOH { get { return chanceToHitOH; } set { SetField(ref chanceToHitOH, value); } }
        private int chanceToHit;
        public int ChanceToHit
        {
            set { SetField(ref chanceToHit, value); }
            get
            {
                return chanceToHit;
            }
        }

        private float increasedCastSpeed;
        public float IncreasedCastSpeed { get { return increasedCastSpeed; } set { SetField(ref increasedCastSpeed, value); } }
        private float increasedCurseCastSpeed;
        public float IncreasedCurseCastSpeed { get { return increasedCurseCastSpeed; } set { SetField(ref increasedCurseCastSpeed, value); } }
        //
        public int IncreasedElementalDamageSpells { get; private set; }
        public int IncreasedColdDamage { get; private set; }
        public int IncreasedFireDamage { get; private set; }
        public int IncreasedLightningDamage { get; private set; }
        public int IncreasedChaosDamage { get; private set; }

        public float PhysicalDamageMH { get; private set; }
        public float PhysicalDamageOH { get; private set; }
        private float mainHandMinPD;
        public float MainHandMinPD { get { return (float)Math.Round(mainHandMinPD, 2); } set { SetField(ref mainHandMinPD, value); } }
        private float mainHandMaxPD;
        public float MainHandMaxPD { get { return (float)Math.Round(mainHandMaxPD, 2); } set { SetField(ref mainHandMaxPD, value); } }
        private float offHandMinPD;
        public float OffHandMinPD { get { return (float)Math.Round(offHandMinPD, 2); } set { SetField(ref offHandMinPD, value); } }
        private float offHandMaxPD;
        public float OffHandMaxPD { get { return (float)Math.Round(offHandMaxPD, 2); } set { SetField(ref offHandMaxPD, value); } }

        public float ElementalDamageMH { get; private set; }
        public float ElementalDamageOH { get; private set; }
        private float mainHandMinFireD;
        public float MainHandMinFireD { get { return (float)Math.Round(mainHandMinFireD, 2); } set { SetField(ref mainHandMinFireD, value); } }
        private float mainHandMaxFireD;
        public float MainHandMaxFireD { get { return (float)Math.Round(mainHandMaxFireD, 2); } set { SetField(ref mainHandMaxFireD, value); } }
        private float mainHandMinColdD;
        public float MainHandMinColdD { get { return (float)Math.Round(mainHandMinColdD, 2); } set { SetField(ref mainHandMinColdD, value); } }
        private float mainHandMaxColdD;
        public float MainHandMaxColdD { get { return (float)Math.Round(mainHandMaxColdD, 2); } set { SetField(ref mainHandMaxColdD, value); } }
        private float mainHandMinLightD;
        public float MainHandMinLightD { get { return (float)Math.Round(mainHandMinLightD, 2); } set { SetField(ref mainHandMinLightD, value); } }
        private float mainHandMaxLightD;
        public float MainHandMaxLightD { get { return (float)Math.Round(mainHandMaxLightD, 2); } set { SetField(ref mainHandMaxLightD, value); } }
        private float mainHandMinChaosD;
        public float MainHandMinChaosD { get { return (float)Math.Round(mainHandMinChaosD, 2); } set { SetField(ref mainHandMinChaosD, value); } }
        private float mainHandMaxChaosD;
        public float MainHandMaxChaosD { get { return (float)Math.Round(mainHandMaxChaosD, 2); } set { SetField(ref mainHandMaxChaosD, value); } }
        private float offHandMinFireD;
        public float OffHandMinFireD { get { return (float)Math.Round(offHandMinFireD, 2); } set { SetField(ref offHandMinFireD, value); } }
        private float offHandMaxFireD;
        public float OffHandMaxFireD { get { return (float)Math.Round(offHandMaxFireD, 2); } set { SetField(ref offHandMaxFireD, value); } }
        private float offHandMinColdD;
        public float OffHandMinColdD { get { return (float)Math.Round(offHandMinColdD, 2); } set { SetField(ref offHandMinColdD, value); } }
        private float offHandMaxColdD;
        public float OffHandMaxColdD { get { return (float)Math.Round(offHandMaxColdD, 2); } set { SetField(ref offHandMaxColdD, value); } }
        private float offHandMinLightD;
        public float OffHandMinLightD { get { return (float)Math.Round(offHandMinLightD, 2); } set { SetField(ref offHandMinLightD, value); } }
        private float offHandMaxLightD;
        public float OffHandMaxLightD { get { return (float)Math.Round(offHandMaxLightD, 2); } set { SetField(ref offHandMaxLightD, value); } }
        private float offHandMinChaosD;
        public float OffHandMinChaosD { get { return (float)Math.Round(offHandMinChaosD, 2); } set { SetField(ref offHandMinChaosD, value); } }
        private float offHandMaxChaosD;
        public float OffHandMaxChaosD { get { return (float)Math.Round(offHandMaxChaosD, 2); } set { SetField(ref offHandMaxChaosD, value); } }
        private float attackSkillDPS;
        public float AttackSkillDPS { get { return (float)Math.Round(attackSkillDPS, 2); } set { SetField(ref attackSkillDPS, value); } }

        private float spellSkillDoT;
        public float SpellSkillDoT { get { return (float)Math.Round(spellSkillDoT, 2); } set { SetField(ref spellSkillDoT, value); } }
        private float spellSkillDPS;
        public float SpellSkillDPS { get { return (float)Math.Round(spellSkillDPS, 2); } set { SetField(ref spellSkillDPS, value); } }
        private float spellMinPD;
        public float SpellMinPD { get { return (float)Math.Round(spellMinPD, 2); } set { SetField(ref spellMinPD, value); } }
        private float spellMaxPD;
        public float SpellMaxPD { get { return (float)Math.Round(spellMaxPD, 2); } set { SetField(ref spellMaxPD, value); } }
        private float spellMinFireD;
        public float SpellMinFireD { get { return (float)Math.Round(spellMinFireD, 2); } set { SetField(ref spellMinFireD, value); } }
        private float spellMaxFireD;
        public float SpellMaxFireD { get { return (float)Math.Round(spellMaxFireD, 2); } set { SetField(ref spellMaxFireD, value); } }
        private float spellMinColdD;
        public float SpellMinColdD { get { return (float)Math.Round(spellMinColdD, 2); } set { SetField(ref spellMinColdD, value); } }
        private float spellMaxColdD;
        public float SpellMaxColdD { get { return (float)Math.Round(spellMaxColdD, 2); } set { SetField(ref spellMaxColdD, value); } }
        private float spellMinLightD;
        public float SpellMinLightD { get { return (float)Math.Round(spellMinLightD, 2); } set { SetField(ref spellMinLightD, value); } }
        private float spellMaxLightD;
        public float SpellMaxLightD { get { return (float)Math.Round(spellMaxLightD, 2); } set { SetField(ref spellMaxLightD, value); } }
        private float spellMinChaosD;
        public float SpellMinChaosD { get { return (float)Math.Round(spellMinChaosD, 2); } set { SetField(ref spellMinChaosD, value); } }
        private float spellMaxChaosD;
        public float SpellMaxChaosD { get { return (float)Math.Round(spellMaxChaosD, 2); } set { SetField(ref spellMaxChaosD, value); } }

        private float castTime;
        public float CastTime { get { return (float)Math.Round(castTime, 2); } set { SetField(ref castTime, value); } }
        private float castPerSec;
        public float CastsPerSecond { get { return (float)Math.Round(castPerSec, 2); } set { SetField(ref castPerSec, value); } }

        private float critChanceMH;
        public float CritChanceMH { get { return critChanceMH; } set { SetField(ref critChanceMH, value); } }
        private float critChanceOH;
        public float CritChanceOH { get { return critChanceOH; } set { SetField(ref critChanceOH, value); } }
        private float critMultiplierMH;
        public float CritMultiplierMH { get { return critMultiplierMH; } set { SetField(ref critMultiplierMH, value); } }
        private float critMultiplierOH;
        public float CritMultiplierOH { get { return critMultiplierOH; } set { SetField(ref critMultiplierOH, value); } }
        private float critChanceSpell;
        public float CritChanceSpell { get { return critChanceSpell; } set { SetField(ref critChanceSpell, value); } }
        private float critMultiplierSpell;
        public float CritMultiplierSpell { get { return critMultiplierSpell; } set { SetField(ref critMultiplierSpell, value); } }

        private float attackSpeedMH;
        public float AttackSpeedMH { get { return (float)Math.Round(attackSpeedMH, 3); } set { SetField(ref attackSpeedMH, value); } }
        private float attackSpeedOH;
        public float AttackSpeedOH { get { return (float)Math.Round(attackSpeedOH, 3); } set { SetField(ref attackSpeedOH, value); } }
        private float attacksPerSecond;
        public float AttacksPerSecond
        {
            set { SetField(ref attacksPerSecond, value); }
            get
            {
                return attacksPerSecond;
            }
        }

        private int evasionRating;
        public int EvasionRating { get { return evasionRating; } set { SetField(ref evasionRating, value); } }
        private int armour;
        public int Armour { get { return armour; } set { SetField(ref armour, value); } }
        private int physDamageReduction;
        public int PhysDamageReduction { get { return physDamageReduction; } set { SetField(ref physDamageReduction, value); } }
        private int chanceToDodge;
        public int ChanceToDodge { get { return chanceToDodge; } set { SetField(ref chanceToDodge, value); } }
        private int chanceToDodgeSpell;
        public int ChanceToDodgeSpell { get { return chanceToDodgeSpell; } set { SetField(ref chanceToDodgeSpell, value); } }
        private int chanceToBlock;
        public int ChanceToBlock { get { return chanceToBlock; } set { SetField(ref chanceToBlock, value); } }
        private int chanceToBlockSpell;
        public int ChanceToBlockSpell { get { return chanceToBlockSpell; } set { SetField(ref chanceToBlockSpell, value); } }
        private int chanceToEvade;
        public int ChanceToEvade { get { return chanceToEvade; } set { SetField(ref chanceToEvade, value); } }
        private int chanceToEvadeProjectileAttacks;
        public int ChanceToEvadeProjectileAttacks { get { return chanceToEvadeProjectileAttacks; } set { SetField(ref chanceToEvadeProjectileAttacks, value); } }
        #endregion
        //-------------------------------------------------------------------------------
        #region Creation & clean Up
        //-------------------------------------------------------------------------------
        public CharStats(SkillTree tree)
        {
            this.skillTree = tree;
            monsterEvasion = new int[100];
            monsterAccuracy = new int[100];
            monsterDamage = new int[100];
            ReadMonsterStats();
            ReadAuras();
        }

        private void ReadMonsterStats()
        {
            try
            {
                var reader = new StreamReader(File.OpenRead("Data/DefaultMonsterStats.csv"));
                var line = reader.ReadLine();
                int lineNumber = 0;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    var values = line.Split(',');
                    monsterEvasion[lineNumber] = int.Parse(values[2]);
                    monsterAccuracy[lineNumber] = int.Parse(values[3]);
                    monsterDamage[lineNumber] = values[8] != "" ? int.Parse(values[8]) : (int)(3E-05 * Math.Pow(lineNumber + 1, 4) - 0.0018 * Math.Pow(lineNumber + 1, 3) + 0.0957 * Math.Pow(lineNumber + 1, 2) - 0.1938 * (lineNumber + 1) + 8);
                    lineNumber++;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Error in loading \"Data\\DefaultMonsterStats.csv\"", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void ReadAuras()
        {
            try
            {
                IniFile ini = new IniFile("Data/PoE Auras.ini");
                string vitality = ini.IniReadValue("Vitality", "Level");
                string[] values = vitality.Split(',');
                vitalityAura = new float[values.Length];
                float number;
                float number2;
                for (int i = 0; i < values.Length; i++)
                {
                    if (float.TryParse(values[i], out number))
                        vitalityAura[i] = number;
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Wrong input detected in vitality Aura numbers", "Wrong Number", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                        vitalityAura[i] = 0;
                    }
                }

                string hatred = ini.IniReadValue("Hatred", "Level");
                string[] hatredValues = hatred.Split(',');
                hatredAura = new float[hatredValues.Length];
                for (int i = 0; i < hatredAura.Length; i++)
                {
                    if (float.TryParse(hatredValues[i], out number))
                        hatredAura[i] = number;
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Wrong input detected in vitality Aura numbers", "Wrong Number", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                        hatredAura[i] = 0;
                    }
                }

                string determination = ini.IniReadValue("Determination", "Level");
                string[] determinationValues = determination.Split(',');
                determinationAura = new float[determinationValues.Length];
                for (int i = 0; i < determinationAura.Length; i++)
                {
                    if (float.TryParse(determinationValues[i], out number))
                        determinationAura[i] = number;
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Wrong input detected in vitality Aura numbers", "Wrong Number", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                        determinationAura[i] = 0;
                    }
                }

                string grace = ini.IniReadValue("Grace", "Level");
                string[] graceValues = grace.Split(',');
                graceAura = new float[graceValues.Length];
                for (int i = 0; i < graceAura.Length; i++)
                {
                    if (float.TryParse(graceValues[i], out number))
                        graceAura[i] = number;
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Wrong input detected in vitality Aura numbers", "Wrong Number", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                        graceAura[i] = 0;
                    }
                }

                string discipline = ini.IniReadValue("Discipline", "Level");
                string[] disciplineValues = discipline.Split(',');
                disciplineAura = new float[disciplineValues.Length];
                for (int i = 0; i < disciplineAura.Length; i++)
                {
                    if (float.TryParse(disciplineValues[i], out number))
                        disciplineAura[i] = number;
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Wrong input detected in vitality Aura numbers", "Wrong Number", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                        disciplineAura[i] = 0;
                    }
                }

                string clarity = ini.IniReadValue("Clarity", "Level");
                string[] clarityValues = clarity.Split(',');
                clarityAura = new float[clarityValues.Length];
                for (int i = 0; i < clarityAura.Length; i++)
                {
                    if (float.TryParse(clarityValues[i], out number))
                        clarityAura[i] = number;
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Wrong input detected in vitality Aura numbers", "Wrong Number", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                        clarityAura[i] = 0;
                    }
                }

                string purityOfEle = ini.IniReadValue("PurityofElements", "Level");
                string[] purityOfEleValues = purityOfEle.Split(',');
                purityofEleAura = new float[purityOfEleValues.Length];
                for (int i = 0; i < purityofEleAura.Length; i++)
                {
                    if (float.TryParse(purityOfEleValues[i], out number))
                        purityofEleAura[i] = number;
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Wrong input detected in vitality Aura numbers", "Wrong Number", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                        purityofEleAura[i] = 0;
                    }
                }

                string purityofFireRes = ini.IniReadValue("PurityofFire", "Level");
                string[] purityofFireResValues = purityofFireRes.Split(',');
                string purityofFireMax = ini.IniReadValue("PurityofFire", "Level2");
                string[] purityofFireMaxValues = purityofFireMax.Split(',');
                purityofFireAura = new float[purityofFireResValues.Length][];
                for (int i = 0; i < purityofFireResValues.Length; i++)
                {
                    if (float.TryParse(purityofFireResValues[i], out number) && float.TryParse(purityofFireMaxValues[i], out number2))
                    {
                        purityofFireAura[i] = new float[2];
                        purityofFireAura[i][0] = number;
                        purityofFireAura[i][1] = number2;
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Wrong input detected in vitality Aura numbers", "Wrong Number", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                        purityofFireAura[i] = new float[] { 0, 0 };
                    }
                }

                string purityofIceRes = ini.IniReadValue("PurityofIce", "Level");
                string[] purityofIceResValues = purityofIceRes.Split(',');
                string purityofIceMax = ini.IniReadValue("PurityofIce", "Level2");
                string[] purityofIceMaxValues = purityofIceMax.Split(',');
                purityofIceAura = new float[purityofIceResValues.Length][];
                for (int i = 0; i < purityofIceResValues.Length; i++)
                {
                    if (float.TryParse(purityofIceResValues[i], out number) && float.TryParse(purityofIceMaxValues[i], out number2))
                    {
                        purityofIceAura[i] = new float[2];
                        purityofIceAura[i][0] = number;
                        purityofIceAura[i][1] = number2;
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Wrong input detected in vitality Aura numbers", "Wrong Number", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                        purityofIceAura[i] = new float[] { 0, 0 };
                    }
                }

                string purityofLightRes = ini.IniReadValue("PurityofLight", "Level");
                string[] purityofLightResValues = purityofLightRes.Split(',');
                string purityofLightMax = ini.IniReadValue("PurityofLight", "Level2");
                string[] purityofLightMaxValues = purityofLightMax.Split(',');
                purityofLightAura = new float[purityofLightResValues.Length][];
                for (int i = 0; i < purityofLightResValues.Length; i++)
                {
                    if (float.TryParse(purityofLightResValues[i], out number) && float.TryParse(purityofLightMaxValues[i], out number2))
                    {
                        purityofLightAura[i] = new float[2];
                        purityofLightAura[i][0] = number;
                        purityofLightAura[i][1] = number2;
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Wrong input detected in vitality Aura numbers", "Wrong Number", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                        purityofLightAura[i] = new float[] { 0, 0 };
                    }
                }

                string hasteAS = ini.IniReadValue("Haste", "Level");
                string[] hasteASValues = hasteAS.Split(',');
                string hasteCS = ini.IniReadValue("Haste", "Level2");
                string[] hasteCSValues = hasteCS.Split(',');
                hasteAura = new float[hasteASValues.Length][];
                for (int i = 0; i < hasteASValues.Length; i++)
                {
                    if (float.TryParse(hasteASValues[i], out number) && float.TryParse(hasteCSValues[i], out number2))
                    {
                        hasteAura[i] = new float[2];
                        hasteAura[i][0] = number;
                        hasteAura[i][1] = number2;
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Wrong input detected in vitality Aura numbers", "Wrong Number", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                        hasteAura[i] = new float[] { 0, 0 };
                    }
                }

                string angerMin = ini.IniReadValue("Anger", "Level");
                string[] angerMinValues = angerMin.Split(',');
                string angerMax = ini.IniReadValue("Anger", "Level2");
                string[] angerMaxValues = angerMax.Split(',');
                angerAura = new float[angerMinValues.Length][];
                for (int i = 0; i < angerMinValues.Length; i++)
                {
                    if (float.TryParse(angerMinValues[i], out number) && float.TryParse(angerMaxValues[i], out number2))
                    {
                        angerAura[i] = new float[2];
                        angerAura[i][0] = number;
                        angerAura[i][1] = number2;
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Wrong input detected in vitality Aura numbers", "Wrong Number", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                        angerAura[i] = new float[] { 0, 0 };
                    }
                }

                string wrathMin = ini.IniReadValue("Wrath", "Level");
                string[] wrathMinValues = wrathMin.Split(',');
                string wrathMax = ini.IniReadValue("Wrath", "Level2");
                string[] wrathMaxValues = wrathMax.Split(',');
                wrathAura = new float[wrathMinValues.Length][];
                for (int i = 0; i < wrathMinValues.Length; i++)
                {
                    if (float.TryParse(wrathMinValues[i], out number) && float.TryParse(wrathMaxValues[i], out number2))
                    {
                        wrathAura[i] = new float[2];
                        wrathAura[i][0] = number;
                        wrathAura[i][1] = number2;
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Wrong input detected in vitality Aura numbers", "Wrong Number", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                        wrathAura[i] = new float[] { 0, 0 };
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Error in loading \"DataDefaultMonsterStats.csv\"", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                System.Windows.Application.Current.Shutdown();
            }
        }
        #endregion
        //-------------------------------------------------------------------------------
        #region Methods
        //-------------------------------------------------------------------------------
        public virtual void UpdateStats(Dictionary<string, List<float>> attribs, int level, CharItemData itemData)
        {
            this.Level = level;
            this.attribs = attribs;
            this.itemData = itemData;
            GetWeaponsInfo();
            increasedAuraEff = attribs.ContainsKey("#% increased effect of Auras you Cast") ? attribs["#% increased effect of Auras you Cast"][0] : 0f;
            increasedBuffEff = attribs.ContainsKey("#% increased Effect of Buffs on You") ? attribs["#% increased Effect of Buffs on You"][0] : 0f;
            increasedAuraEff += increasedBuffEff;
            UpdateBaseAttribs();
            UpdateCharges();
            UpdateHP();
            UpdateBaseStats();
            UpdateMana(); //Must come After updateES  "#% increased effect of Auras you Cast"
            UpdateChanceToDodge();
            UpdateAttackSpeed();
            UpdateCastSpeed();
            CalcDamageReduction();
            ChanceToHitMH = (int)(CalcChanceToHit(Level, AccuracyRatingMH) + 0.5f);
            ChanceToHitOH = (int)(CalcChanceToHit(Level, AccuracyRatingOH) + 0.5f);
            if (CharWeaponStyle == WeaponStyle.DualWield)
                ChanceToHit = (int)((ChanceToHitMH + ChanceToHitOH) / 2f + 0.5f);
            else
                ChanceToHit = ChanceToHitMH;
            CalcChanceToEvade();
            ChanceToEvadeProjectileAttacks = ChanceToEvade;
            if (attribs.ContainsKey("Doubles chance to Evade Projectile Attacks"))
                ChanceToEvadeProjectileAttacks = 2 * ChanceToEvade;
            UpdatePhysicalDamage();
            UpdateElementalDamage(); //must come after UpdatePhysicalDamage. due to wands
            UpdateCriticalStrike();
            CalcSkillDPS();
        }

        public void CalcSkillDPS()
        {
            if (ActiveSkillGem != null)
            {
                if (ActiveSkillGem.GemType == GemType.Attack)
                {
                    SpellVisibility = System.Windows.Visibility.Collapsed;
                    CalcAttackSkillDPS(ActiveSkillGem as ActiveGem);
                }
                else if (ActiveSkillGem.GemType == GemType.Spell || ActiveSkillGem.GemType == GemType.Totem || ActiveSkillGem.GemType == GemType.Trap)
                {
                    CalcSpellSkillDPS(ActiveSkillGem as ActiveGem);
                    SpellVisibility = System.Windows.Visibility.Visible;
                }
            }
        }
        #endregion
        //-------------------------------------------------------------------------------
        #region Helper Methods
        //-------------------------------------------------------------------------------
        protected void GetWeaponsInfo()
        {
            CharItemData.Item MHWeapon = null;
            CharItemData.Item OHWeapon = null;
            CharWeaponStyle = WeaponStyle.Onehand;
            WeaponTypeMH = WeaponType.Other;
            WeaponTypeOH = WeaponType.Other;
            mainHandMinDamage_Org = 2; // is it minimum unarmed?;
            mainHandMaxDamage_Org = 7;// is it max unarmed?;
            mainHandAs_Org = 0;
            CritChanceMH_Org = 0;
            MainHandMinColdD_Org = 0;
            MainHandMaxColdD_Org = 0;
            MainHandMinFireD_Org = 0;
            MainHandMaxFireD_Org = 0;
            MainHandMinLightD_Org = 0;
            MainHandMaxLightD_Org = 0;
            MainHandMinChaosD_Org = 0;
            MainHandMaxChaosD_Org = 0;
            offHandMinDamage_Org = 0;
            offHandMaxDamage_Org = 0;
            CritChanceOH_Org = 0;
            offHandAs_Org = 0;
            OffHandMinColdD_Org = 0;
            OffHandMaxColdD_Org = 0;
            OffHandMinFireD_Org = 0;
            OffHandMaxFireD_Org = 0;
            OffHandMinLightD_Org = 0;
            OffHandMaxLightD_Org = 0;
            OffHandMinChaosD_Org = 0;
            OffHandMaxChaosD_Org = 0;

            foreach (var item in itemData.EquippedItems)
            {
                if (item.Class == CharItemData.Item.ItemClass.MainHand)
                {
                    MHWeapon = item;
                }
                else if (item.Class == CharItemData.Item.ItemClass.OffHand)
                    OHWeapon = item;
            }
            if (MHWeapon != null)
            {
                foreach (var mod in MHWeapon.Properties.Keys)
                {
                    if (mod.Contains("Physical Damage:"))
                    {
                        mainHandMinDamage_Org = MHWeapon.Properties[mod][0];
                        mainHandMaxDamage_Org = MHWeapon.Properties[mod][1];
                    }
                    if (mod.Contains("Attacks per Second:"))
                    {
                        mainHandAs_Org = MHWeapon.Properties[mod][0];
                    }
                    if (mod.Contains("Critical Strike Chance:"))
                    {
                        CritChanceMH_Org = MHWeapon.Properties[mod][0];
                    }
                }
                foreach (var mod in MHWeapon.Mods)
                {
                    if (mod.Attribute == "Adds #-# Cold Damage")
                    {
                        MainHandMinColdD_Org = mod.Values[0];
                        MainHandMaxColdD_Org = mod.Values[1];
                    }
                    else if (mod.Attribute == "Adds #-# Fire Damage" || mod.Attribute == "Adds #-# Fire Damage in Main Hand")
                    {
                        MainHandMinFireD_Org = mod.Values[0];
                        MainHandMaxFireD_Org = mod.Values[1];
                    }
                    else if (mod.Attribute == "Adds #-# Lightning Damage")
                    {
                        MainHandMinLightD_Org = mod.Values[0];
                        MainHandMaxLightD_Org = mod.Values[1];
                    }
                    else if (mod.Attribute == "Adds #-# Chaos Damage")
                    {
                        MainHandMinChaosD_Org = mod.Values[0];
                        MainHandMaxChaosD_Org = mod.Values[1];
                    }
                }
                switch (MHWeapon.WeaponType)
                {
                    case "One Handed Sword":
                        WeaponTypeMH = WeaponType.OneHandedSword;
                        break;
                    case "Two Handed Sword":
                        WeaponTypeMH = WeaponType.TwoHandedSword;
                        CharWeaponStyle = WeaponStyle.TwoHand;
                        break;
                    case "One Handed Mace":
                        WeaponTypeMH = WeaponType.OneHandedMace;
                        break;
                    case "Two Handed Mace":
                        WeaponTypeMH = WeaponType.TwoHandedMace;
                        CharWeaponStyle = WeaponStyle.TwoHand;
                        break;
                    case "One Handed Axe":
                        WeaponTypeMH = WeaponType.OneHandedAxe;
                        break;
                    case "Two Handed Axe":
                        WeaponTypeMH = WeaponType.TwoHandedAxe;
                        if (MHWeapon.Name == "Wings of Entropy")
                            CharWeaponStyle = WeaponStyle.DualWield2H;
                        break;
                    case "Bow":
                        WeaponTypeMH = WeaponType.Bow;
                        CharWeaponStyle = WeaponStyle.Ranged;
                        break;
                    case "Staff":
                        WeaponTypeMH = WeaponType.Staff;
                        CharWeaponStyle = WeaponStyle.TwoHand;
                        break;
                    case "Dagger":
                        WeaponTypeMH = WeaponType.Dagger;
                        break;
                    case "Claw":
                        WeaponTypeMH = WeaponType.Claw;
                        break;
                    case "Wand":
                        WeaponTypeMH = WeaponType.Wand;
                        break;
                    default:
                        WeaponTypeMH = WeaponType.Other;
                        CharWeaponStyle = WeaponStyle.UnArmed;
                        break;
                }
            }
            else
            {
                WeaponTypeMH = WeaponType.Other;
            }
            if (OHWeapon != null)
            {
                foreach (var mod in OHWeapon.Properties.Keys)
                {
                    if (mod.Contains("Physical Damage:"))
                    {
                        offHandMinDamage_Org = OHWeapon.Properties[mod][0];
                        offHandMaxDamage_Org = OHWeapon.Properties[mod][1];
                    }
                    if (mod.Contains("Critical Strike Chance:"))
                    {
                        CritChanceOH_Org = OHWeapon.Properties[mod][0];
                    }
                    if (mod.Contains("Attacks per Second:"))
                    {
                        offHandAs_Org = OHWeapon.Properties[mod][0];
                    }
                }

                foreach (var mod in OHWeapon.Mods)
                {
                    if (mod.Attribute == "Adds #-# Cold Damage" || mod.Attribute == "Adds #-# Cold Damage in Off Hand")
                    {
                        OffHandMinColdD_Org = mod.Values[0];
                        OffHandMaxColdD_Org = mod.Values[1];
                    }
                    else if (mod.Attribute == "Adds #-# Fire Damage")
                    {
                        OffHandMinFireD_Org = mod.Values[0];
                        OffHandMaxFireD_Org = mod.Values[1];
                    }
                    else if (mod.Attribute == "Adds #-# Lightning Damage")
                    {
                        OffHandMinLightD_Org = mod.Values[0];
                        OffHandMaxLightD_Org = mod.Values[1];
                    }
                    else if (mod.Attribute == "Adds #-# Chaos Damage")
                    {
                        OffHandMinChaosD_Org = mod.Values[0];
                        OffHandMaxChaosD_Org = mod.Values[1];
                    }
                }
                switch (OHWeapon.WeaponType)
                {
                    case "One Handed Sword":
                        WeaponTypeOH = WeaponType.OneHandedSword;
                        CharWeaponStyle = WeaponStyle.DualWield;
                        break;
                    case "One Handed Mace":
                        WeaponTypeOH = WeaponType.OneHandedMace;
                        CharWeaponStyle = WeaponStyle.DualWield;
                        break;
                    case "One Handed Axe":
                        WeaponTypeOH = WeaponType.OneHandedAxe;
                        CharWeaponStyle = WeaponStyle.DualWield;
                        break;
                    case "Dagger":
                        WeaponTypeOH = WeaponType.Dagger;
                        CharWeaponStyle = WeaponStyle.DualWield;
                        break;
                    case "Claw":
                        WeaponTypeOH = WeaponType.Claw;
                        CharWeaponStyle = WeaponStyle.DualWield;
                        break;
                    case "Wand":
                        WeaponTypeOH = WeaponType.Wand;
                        CharWeaponStyle = WeaponStyle.DualWield;
                        break;
                    default:
                        WeaponTypeOH = WeaponType.Other;
                        if (MHWeapon != null)
                            if (WeaponTypeMH != WeaponType.Bow)
                            {
                                if (WeaponTypeMH == WeaponType.Other)
                                    CharWeaponStyle = WeaponStyle.UnArmedShield;
                                else
                                    CharWeaponStyle = WeaponStyle.OnehandShield;
                            }
                        break;
                }
            }
            else
            {
                WeaponTypeOH = WeaponType.Other; //We dont have offhand
                if (WeaponTypeMH == WeaponType.Other)
                    CharWeaponStyle = WeaponStyle.UnArmed;//we dont have main hand either 
                else
                    CharWeaponStyle = WeaponStyle.Onehand;//we have main hand but not shield
            }
            if (CharWeaponStyle == WeaponStyle.DualWield)
                OffHandVisibility = System.Windows.Visibility.Visible;
            else
                OffHandVisibility = System.Windows.Visibility.Collapsed;
        }

        protected void UpdateBaseAttribs()
        {
            int maxElementalRes = (int)(attribs.ContainsKey("+#% to maximum Cold Resistance") ? attribs["+#% to maximum Cold Resistance"][0] : 0f) + BaseMaxResistance;
            Strength = (int)((attribs.ContainsKey("+# to Strength") ? attribs["+# to Strength"][0] : 0f) + skillTree.CharBaseStats[skillTree.Chartype]["Strength"]);
            Dexterity = (int)((attribs.ContainsKey("+# to Dexterity") ? attribs["+# to Dexterity"][0] : 0f) + skillTree.CharBaseStats[skillTree.Chartype]["Dexterity"]);
            Intelligence = (int)((attribs.ContainsKey("+# to Intelligence") ? attribs["+# to Intelligence"][0] : 0f) + skillTree.CharBaseStats[skillTree.Chartype]["Intelligence"]);
            float resistPenalty = 0;
            if (CruelDiff) resistPenalty = CruelAllResistPenalty;
            else if (MercilessDiff) resistPenalty = MercilessAllResistPenalty;
            float allResist = attribs.ContainsKey("+#% to all Elemental Resistances") ? attribs["+#% to all Elemental Resistances"][0] : 0f;
            allResist += CurrentEndurance * ResPerEndurance + PurityofEleRes * (1 + increasedAuraEff / 100f);
            int fireResist = (int)((attribs.ContainsKey("+#% to Fire Resistance") ? attribs["+#% to Fire Resistance"][0] : 0f) + allResist + KraitynAllResist + PurityofFireRes[0] * (1 + increasedAuraEff / 100f) + resistPenalty);
            FireResist = fireResist > maxElementalRes + PurityofFireRes[1] * (1 + increasedAuraEff / 100f) ? maxElementalRes + (int)(PurityofFireRes[1] * (1 + increasedAuraEff / 100f)) : fireResist;
            int coldResist = (int)((attribs.ContainsKey("+#% to Cold Resistance") ? attribs["+#% to Cold Resistance"][0] : 0f) + allResist + KraitynAllResist + PurityofIceRes[0] * (1 + increasedAuraEff / 100f) + resistPenalty);
            ColdResist = coldResist > maxElementalRes + PurityofIceRes[1] * (1 + increasedAuraEff / 100f) ? maxElementalRes + (int)(PurityofIceRes[1] * (1 + increasedAuraEff / 100f)) : coldResist;
            int lightningResist = (int)((attribs.ContainsKey("+#% to Lightning Resistance") ? attribs["+#% to Lightning Resistance"][0] : 0f) + allResist + KraitynAllResist + PurityofLightRes[0] * (1 + increasedAuraEff / 100f) + resistPenalty);
            LightningResist = lightningResist > maxElementalRes + PurityofLightRes[1] * (1 + increasedAuraEff / 100f) ? maxElementalRes + (int)(PurityofLightRes[1] * (1 + increasedAuraEff / 100f)) : lightningResist;
            ChaosResist = (int)((attribs.ContainsKey("+#% to Chaos Resistance") ? attribs["+#% to Chaos Resistance"][0] : 0f) + resistPenalty);
            if (attribs.ContainsKey("Maximum Life becomes #, Immune to Chaos Damage"))
                ChaosResist = 100;
        }
        //needs more testing
        protected void UpdateBaseStats()
        {
            float armourFromItems = 0;
            float evasionFromItems = 0;
            float energyFromItems = 0;
            float blockFromShield = 0;
            float accuracyFromItems = 0;
            if (itemData != null)
            {
                foreach (CharItemData.Attribute attrib in itemData.AttribCollectionView)
                {
                    if (attrib.Group != "Global Mods")
                    {
                        if (attrib.TextAttribute.Contains("Armour: "))
                            armourFromItems += attrib.Values[0];
                        else if (attrib.TextAttribute.Contains("Evasion Rating: "))
                            evasionFromItems += attrib.Values[0];
                        else if (attrib.TextAttribute.Contains("Energy Shield: "))
                            energyFromItems += attrib.Values[0];
                        else if (attrib.TextAttribute.ToLower().Contains("chance to block"))//needs testing 
                            blockFromShield += attrib.Values[0];
                    }
                    else
                    {
                        if (attrib.TextAttribute.Contains("to Armour") && !attrib.TextAttribute.Contains("to Armour while"))
                            armourFromItems += attrib.Values[0];
                        else if (attrib.TextAttribute.Contains("to Evasion Rating"))
                            evasionFromItems += attrib.Values[0];
                        else if (attrib.TextAttribute.Contains("to maximum Energy Shield"))
                            energyFromItems += attrib.Values[0];
                        else if (attrib.TextAttribute.Contains("to Accuracy Rating"))
                            accuracyFromItems += attrib.Values[0];
                    }
                }
            }
            UpdateEvasionRating(evasionFromItems); //must come before armour
            UpdateArmour(armourFromItems);
            UpdateES(energyFromItems);
            UpdateAccuracy(accuracyFromItems);
            UpdateBlock(blockFromShield);
        }

        protected void UpdateCharges()
        {
            float toEndurance = attribs.ContainsKey("+# Maximum Endurance Charge") ? attribs["+# Maximum Endurance Charge"][0] : 0f;
            MaxEndurance = (int)(BaseMaxEnduranceCharge + toEndurance + OakEnduranceCharge);
            float toFrenzy = attribs.ContainsKey("+# Maximum Frenzy Charge") ? attribs["+# Maximum Frenzy Charge"][0] : 0f;
            MaxFrenzy = (int)(BaseMaxFrenzyCharge + toFrenzy + KraitynFrenzyCharge);
            float toPower = attribs.ContainsKey("+# Maximum Power Charge") ? attribs["+# Maximum Power Charge"][0] : 0f;
            MaxPower = (int)(BaseMaxPowerCharge + toPower + AliraPowerCharge);
        }

        //tested and is correct
        protected void UpdateAccuracy(float accuracyFromItems, float modifiedIncreasedAcc = 0)
        {
            float increasedAcc = attribs.ContainsKey("#% increased Accuracy Rating") ? attribs["#% increased Accuracy Rating"][0] : 0f;
            increasedAcc += modifiedIncreasedAcc;
            increasedAcc += attribs.ContainsKey("#% increased Accuracy Rating per Frenzy Charge") ? attribs["#% increased Accuracy Rating per Frenzy Charge"][0] * CurrentFrenzy : 0f;
            if (CharWeaponStyle == WeaponStyle.DualWield)
            {
                increasedAcc += attribs.ContainsKey("#% increased Accuracy Rating while Dual Wielding") ? attribs["#% increased Accuracy Rating while Dual Wielding"][0] : 0f;
                increasedAcc += attribs.ContainsKey("#% increased Accuracy Rating with One Handed Melee Weapons") ? attribs["#% increased Accuracy Rating with One Handed Melee Weapons"][0] : 0f;
                float increasedAccMH = 0;
                float plusAccMH = 0;
                float increasedAccOH = 0;
                float plusAccOH = 0;
                var MainHand = itemData.EquippedItems.Where(item => item.Class == CharItemData.Item.ItemClass.MainHand).First();
                foreach (var mod in MainHand.Mods)
                {
                    if (mod.Attribute == "#% increased Accuracy Rating")
                        increasedAccMH += mod.Values[0];
                    if (mod.Attribute == "+# to Accuracy Rating")
                        plusAccMH += mod.Values[0];
                }

                if (WeaponTypeMH == WeaponType.Claw)
                    increasedAccMH += attribs.ContainsKey("#% increased Accuracy Rating with Claws") ? attribs["#% increased Accuracy Rating with Claws"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.Dagger)
                    increasedAccMH += attribs.ContainsKey("#% increased Accuracy Rating with Daggers") ? attribs["#% increased Accuracy Rating with Daggers"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.OneHandedAxe)
                    increasedAccMH += attribs.ContainsKey("#% increased Accuracy Rating with Axes") ? attribs["#% increased Accuracy Rating with Axes"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.OneHandedMace)
                    increasedAccMH += attribs.ContainsKey("#% increased Accuracy Rating with Maces") ? attribs["#% increased Accuracy Rating with Maces"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.OneHandedSword)
                    increasedAccMH += attribs.ContainsKey("#% increased Accuracy Rating with Swords") ? attribs["#% increased Accuracy Rating with Swords"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.Wand)//Wand is not melee weapn and we correct it here
                {
                    increasedAccMH += attribs.ContainsKey("#% increased Accuracy Rating with Wands") ? attribs["#% increased Accuracy Rating with Wands"][0] : 0f;
                    increasedAcc -= attribs.ContainsKey("#% increased Accuracy Rating with One Handed Melee Weapons") ? attribs["#% increased Accuracy Rating with One Handed Melee Weapons"][0] : 0f;
                }
                AccuracyRatingMH = (int)((Dexterity * AccPerDex + accuracyFromItems + (Level - 1) * AccPerLevel + plusAccMH) * (1 + increasedAcc / 100f) * (1 + increasedAccMH / 100f) + 0.5f); //Interesting! must multiply wep acc to increased acc

                var OffHand = itemData.EquippedItems.Where(item => item.Class == CharItemData.Item.ItemClass.OffHand).First();
                foreach (var mod in OffHand.Mods)
                {
                    if (mod.Attribute == "#% increased Accuracy Rating")
                        increasedAccOH += mod.Values[0];
                    if (mod.Attribute == "+# to Accuracy Rating")
                        plusAccOH += mod.Values[0];
                }
                if (WeaponTypeOH == WeaponType.Claw)
                    increasedAccOH += attribs.ContainsKey("#% increased Accuracy Rating with Claws") ? attribs["#% increased Accuracy Rating with Claws"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.Dagger)
                    increasedAccOH += attribs.ContainsKey("#% increased Accuracy Rating with Daggers") ? attribs["#% increased Accuracy Rating with Daggers"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.OneHandedAxe)
                    increasedAccOH += attribs.ContainsKey("#% increased Accuracy Rating with Axes") ? attribs["#% increased Accuracy Rating with Axes"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.OneHandedMace)
                    increasedAccOH += attribs.ContainsKey("#% increased Accuracy Rating with Maces") ? attribs["#% increased Accuracy Rating with Maces"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.OneHandedSword)
                    increasedAccOH += attribs.ContainsKey("#% increased Accuracy Rating with Swords") ? attribs["#% increased Accuracy Rating with Swords"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.Wand)
                {
                    increasedAccOH += attribs.ContainsKey("#% increased Accuracy Rating with Wands") ? attribs["#% increased Accuracy Rating with Wands"][0] : 0f;
                    increasedAcc -= attribs.ContainsKey("#% increased Accuracy Rating with One Handed Melee Weapons") ? attribs["#% increased Accuracy Rating with One Handed Melee Weapons"][0] : 0f;
                }
                AccuracyRatingOH = (int)((Dexterity * AccPerDex + accuracyFromItems + (Level - 1) * AccPerLevel + plusAccOH) * (1 + increasedAcc / 100f) * (1 + increasedAccOH / 100f) + 0.5f);
            }
            else if (CharWeaponStyle == WeaponStyle.OnehandShield || CharWeaponStyle == WeaponStyle.Onehand)
            {
                increasedAcc += attribs.ContainsKey("#% increased Accuracy Rating with One Handed Melee Weapons") ? attribs["#% increased Accuracy Rating with One Handed Melee Weapons"][0] : 0f;
                float increasedAccMH = 0;
                float plusAccMH = 0;
                var MainHand = itemData.EquippedItems.Where(item => item.Class == CharItemData.Item.ItemClass.MainHand).First();
                foreach (var mod in MainHand.Mods)
                {
                    if (mod.Attribute == "#% increased Accuracy Rating")
                        increasedAccMH += mod.Values[0];
                    if (mod.Attribute == "+# to Accuracy Rating")
                        plusAccMH += mod.Values[0];
                }

                if (WeaponTypeMH == WeaponType.Claw)
                    increasedAccMH += attribs.ContainsKey("#% increased Accuracy Rating with Claws") ? attribs["#% increased Accuracy Rating with Claws"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.Dagger)
                    increasedAccMH += attribs.ContainsKey("#% increased Accuracy Rating with Daggers") ? attribs["#% increased Accuracy Rating with Daggers"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.OneHandedAxe)
                    increasedAccMH += attribs.ContainsKey("#% increased Accuracy Rating with Axes") ? attribs["#% increased Accuracy Rating with Axes"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.OneHandedMace)
                    increasedAccMH += attribs.ContainsKey("#% increased Accuracy Rating with Maces") ? attribs["#% increased Accuracy Rating with Maces"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.OneHandedSword)
                    increasedAccMH += attribs.ContainsKey("#% increased Accuracy Rating with Swords") ? attribs["#% increased Accuracy Rating with Swords"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.Wand)
                {
                    increasedAccMH += attribs.ContainsKey("#% increased Accuracy Rating with Wands") ? attribs["#% increased Accuracy Rating with Wands"][0] : 0f;
                    increasedAcc -= attribs.ContainsKey("#% increased Accuracy Rating with One Handed Melee Weapons") ? attribs["#% increased Accuracy Rating with One Handed Melee Weapons"][0] : 0f;
                }

                AccuracyRatingMH = (int)((Dexterity * AccPerDex + accuracyFromItems + (Level - 1) * AccPerLevel + plusAccMH) * (1 + increasedAcc / 100f) * (1 + increasedAccMH / 100f) + 0.5f); //Interesting! must multiply wep acc to increased acc
            }
            else if (CharWeaponStyle == WeaponStyle.TwoHand)
            {
                increasedAcc += attribs.ContainsKey("#% increased Accuracy Rating with Two Handed Melee Weapons") ? attribs["#% increased Accuracy Rating with Two Handed Melee Weapons"][0] : 0f;
                float increasedAccMH = 0;
                float plusAccMH = 0;
                var MainHand = itemData.EquippedItems.Where(item => item.Class == CharItemData.Item.ItemClass.MainHand).First();
                foreach (var mod in MainHand.Mods)
                {
                    if (mod.Attribute == "#% increased Accuracy Rating")
                        increasedAccMH += mod.Values[0];
                    if (mod.Attribute == "+# to Accuracy Rating")
                        plusAccMH += mod.Values[0];
                }

                if (WeaponTypeMH == WeaponType.Staff)
                    increasedAccMH += attribs.ContainsKey("#% increased Accuracy Rating with Staves") ? attribs["#% increased Accuracy Rating with Staves"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.TwoHandedAxe)
                    increasedAccMH += attribs.ContainsKey("#% increased Accuracy Rating with Axes") ? attribs["#% increased Accuracy Rating with Axes"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.TwoHandedMace)
                    increasedAccMH += attribs.ContainsKey("#% increased Accuracy Rating with Maces") ? attribs["#% increased Accuracy Rating with Maces"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.TwoHandedSword)
                    increasedAccMH += attribs.ContainsKey("#% increased Accuracy Rating with Swords") ? attribs["#% increased Accuracy Rating with Swords"][0] : 0f;
                AccuracyRatingMH = (int)((Dexterity * AccPerDex + accuracyFromItems + (Level - 1) * AccPerLevel + plusAccMH) * (1 + increasedAcc / 100f) * (1 + increasedAccMH / 100f) + 0.5f); //Interesting! must multiply wep acc to increased acc
            }
            else if (CharWeaponStyle == WeaponStyle.Ranged)
            {
                increasedAcc += attribs.ContainsKey("#% increased Accuracy Rating with Bows") ? attribs["#% increased Accuracy Rating with Bows"][0] : 0f;
                float increasedAccMH = 0;
                float plusAccMH = 0;
                var MainHand = itemData.EquippedItems.Where(item => item.Class == CharItemData.Item.ItemClass.MainHand).First();
                foreach (var mod in MainHand.Mods)
                {
                    if (mod.Attribute == "#% increased Accuracy Rating")
                        increasedAccMH += mod.Values[0];
                    if (mod.Attribute == "+# to Accuracy Rating")
                        plusAccMH += mod.Values[0];
                }
                AccuracyRatingMH = (int)((Dexterity * AccPerDex + accuracyFromItems + (Level - 1) * AccPerLevel + plusAccMH) * (1 + increasedAcc / 100f) * (1 + increasedAccMH / 100f) + 0.5f); //Interesting! must multiply wep acc to increased acc
            }
            else if (CharWeaponStyle == WeaponStyle.DualWield2H)//wings of entropy
            {
                increasedAcc += attribs.ContainsKey("#% increased Accuracy Rating while Dual Wielding") ? attribs["#% increased Accuracy Rating while Dual Wielding"][0] : 0f;
                increasedAcc += attribs.ContainsKey("#% increased Accuracy Rating with Two Handed Melee Weapons") ? attribs["#% increased Accuracy Rating with Two Handed Melee Weapons"][0] : 0f;
                float increasedAccMH = 0;
                float plusAccMH = 0;
                var MainHand = itemData.EquippedItems.Where(item => item.Class == CharItemData.Item.ItemClass.MainHand).First();
                foreach (var mod in MainHand.Mods)
                {
                    if (mod.Attribute == "#% increased Accuracy Rating")
                        increasedAccMH += mod.Values[0];
                    if (mod.Attribute == "+# to Accuracy Rating")
                        plusAccMH += mod.Values[0];
                }

                if (WeaponTypeMH == WeaponType.Staff)
                    increasedAccMH += attribs.ContainsKey("#% increased Accuracy Rating with Staves") ? attribs["#% increased Accuracy Rating with Staves"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.TwoHandedAxe)
                    increasedAccMH += attribs.ContainsKey("#% increased Accuracy Rating with Axes") ? attribs["#% increased Accuracy Rating with Axes"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.TwoHandedMace)
                    increasedAccMH += attribs.ContainsKey("#% increased Accuracy Rating with Maces") ? attribs["#% increased Accuracy Rating with Maces"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.TwoHandedSword)
                    increasedAccMH += attribs.ContainsKey("#% increased Accuracy Rating with Swords") ? attribs["#% increased Accuracy Rating with Swords"][0] : 0f;
                AccuracyRatingMH = (int)((Dexterity * AccPerDex + accuracyFromItems + (Level - 1) * AccPerLevel + plusAccMH) * (1 + increasedAcc / 100f) * (1 + increasedAccMH / 100f) + 0.5f); //Interesting! must multiply wep acc to increased acc
            }
        }
        //tested and is correct
        protected void UpdateHP(float modifiedToMaxLife = 0, float modifiedIncreasedLife = 0)
        {
            float toMaxLife = attribs.ContainsKey("+# to maximum Life") ? attribs["+# to maximum Life"][0] : 0f;
            toMaxLife += modifiedToMaxLife;
            float increasedLife = attribs.ContainsKey("#% increased maximum Life") ? attribs["#% increased maximum Life"][0] : 0f;
            increasedLife += modifiedIncreasedLife;
            float reducedLife = attribs.ContainsKey("#% reduced Maximum Life") ? attribs["#% reduced Maximum Life"][0] : 0f;
            Life = (int)(((Level * LifePerLevel) + (Strength * LifePerStr) + toMaxLife + OakLife + BaseLife) * (1 + (increasedLife - reducedLife) / 100f) + 0.51f /*for rounding*/);
            LifeRegen = Life * ((attribs.ContainsKey("#% of Life Regenerated per Second") ? attribs["#% of Life Regenerated per Second"][0] / 100f : 0f) + VitalityLifeRegen * (1 + increasedAuraEff / 100f) / 100f);
            if (attribs.ContainsKey("#% of Life Regenerated per Second per Frenzy Charge"))
                LifeRegen += Life * attribs["#% of Life Regenerated per Second per Frenzy Charge"][0] / 100f;
            LifeRegen += attribs.ContainsKey("# Life Regenerated per second") ? attribs["# Life Regenerated per second"][0] : 0f;
            if (attribs.ContainsKey("Maximum Life becomes #, Immune to Chaos Damage"))
                Life = (int)attribs["Maximum Life becomes #, Immune to Chaos Damage"][0];
        }
        //tested and is correct
        protected void UpdateMana(float modifiedToMaxMana = 0, float modifiedIncreasedMana = 0)
        {
            float toMaxMana = attribs.ContainsKey("+# to maximum Mana") ? attribs["+# to maximum Mana"][0] : 0f;
            toMaxMana += modifiedToMaxMana;
            float increasedMana = attribs.ContainsKey("#% increased maximum Mana") ? attribs["#% increased maximum Mana"][0] : 0f;
            increasedMana += modifiedIncreasedMana;
            float reducedMana = attribs.ContainsKey("#% reduced maximum Mana") ? attribs["#% reduced maximum Mana"][0] : 0f;
            if (attribs.ContainsKey("Removes all mana. Spend Life instead of Mana for Skills"))
                Mana = 0;
            else
                Mana = (int)(((Level * ManaPerLevel) + (Intelligence * ManaPerInt) + toMaxMana + AliraMana + BaseMana) * (1 + (increasedMana - reducedMana) / 100f) + 0.51f /*for rounding*/);
            Mana += manaFromES;
            float increasedManaRegen = attribs.ContainsKey("#% increased Mana Regeneration Rate") ? attribs["#% increased Mana Regeneration Rate"][0] : 0f;
            ManaRegen = (Mana * 1.75f / 100f + ClarityManaRegen * (1 + increasedAuraEff / 100f)) * (1 + increasedManaRegen / 100f);

        }
        //tested and needs correction . needs test for shields
        protected void UpdateEvasionRating(float evasionFromItems, float modifiedIncreasedEvasion = 0f)
        {
            float increasedEvasion = attribs.ContainsKey("#% increased Evasion Rating") ? attribs["#% increased Evasion Rating"][0] : 0f + (attribs.ContainsKey("#% increased Evasion Rating and Armour") ? attribs["#% increased Evasion Rating and Armour"][0] : 0f);
            increasedEvasion += modifiedIncreasedEvasion;
            evasionFromItems += graceEvasion * (1 + increasedAuraEff / 100f);
            float shiedEvasion = 0;
            if (CharWeaponStyle == WeaponStyle.OnehandShield || CharWeaponStyle == WeaponStyle.UnArmedShield)
            {
                var offHand = itemData.EquippedItems.Where(item => item.Class == CharItemData.Item.ItemClass.OffHand).First();
                if (offHand.Properties.ContainsKey("Evasion Rating:  #"))
                    shiedEvasion = offHand.Properties["Evasion Rating:  #"][0];
            }
            evasionFromItems -= shiedEvasion;
            if (attribs.ContainsKey("#% increased Defences from equipped Shield"))
            {
                shiedEvasion += (shiedEvasion * attribs["#% increased Defences from equipped Shield"][0] / 100f); //modify evasion
            }
            if (!attribs.ContainsKey("All bonuses from an equipped Shield apply to your Minions instead of you"))
                evasionFromItems += shiedEvasion;
            EvasionRating = (int)((BaseEvasion + Level * EvasPerLevel + evasionFromItems) * (1 + (increasedEvasion + EvasPerDex * Dexterity) / 100f));
            if (attribs.ContainsKey("Converts all Evasion Rating to Armour. Dexterity provides no bonus to Evasion Rating"))
            {
                armourFromEvasion = EvasionRating;
                EvasionRating = 0;
            }
            else
                armourFromEvasion = 0;
        }
        //tested and is correct. needs test for shields
        protected void UpdateArmour(float armourFromItems, float modifiedIncreasedArmour = 0f)
        {
            float increasedArmour = attribs.ContainsKey("#% increased Armour") ? attribs["#% increased Armour"][0] : 0f + (attribs.ContainsKey("#% increased Evasion Rating and Armour") ? attribs["#% increased Evasion Rating and Armour"][0] : 0f);
            increasedArmour += modifiedIncreasedArmour;
            float lessArmour = attribs.ContainsKey("#% Chance to Dodge Attacks. #% less Armour and Energy Shield") ? attribs["#% Chance to Dodge Attacks. #% less Armour and Energy Shield"][1] : 100f; //Acrobatics
            float shiedArmour = 0;
            if (CharWeaponStyle == WeaponStyle.OnehandShield || CharWeaponStyle == WeaponStyle.UnArmedShield)
            {
                var offHand = itemData.EquippedItems.Where(item => item.Class == CharItemData.Item.ItemClass.OffHand).First();
                if (offHand != null && offHand.Properties.ContainsKey("Armour:  #"))
                    shiedArmour = offHand.Properties["Armour:  #"][0];
                armourFromItems -= shiedArmour;
            }
            if (attribs.ContainsKey("#% increased Defences from equipped Shield"))
            {
                shiedArmour += (shiedArmour * attribs["#% increased Defences from equipped Shield"][0] / 100f); //modify ES
            }
            if (!attribs.ContainsKey("All bonuses from an equipped Shield apply to your Minions instead of you"))
                armourFromItems += shiedArmour; //Add modified Shieldarmour to the total armour
            Armour = (int)((armourFromItems + armourFromEvasion) * (1 + increasedArmour / 100f) * (lessArmour / 100) * (1 + DeterminationArmour * (1 + increasedAuraEff / 100f) / 100f) + 0.5f);
        }
        //not tested also increased Defences from equipped Shield must be considered
        protected void UpdateES(float energyFromItems, float modifiedincreasedES = 0)
        {
            manaFromES = 0;
            float increasedES = attribs.ContainsKey("#% increased maximum Energy Shield") ? attribs["#% increased maximum Energy Shield"][0] : 0f;
            float reducedES = attribs.ContainsKey("#% reduced maximum Energy Shield") ? attribs["#% reduced maximum Energy Shield"][0] : 0f;
            float moreES = 1 + (attribs.ContainsKey("#% more Maximum Energy Shield") ? attribs["#% more Maximum Energy Shield"][0] / 100f : 0f);
            float lessES = 1 - (attribs.ContainsKey("#% Chance to Dodge Attacks. #% less Armour and Energy Shield") ? attribs["#% Chance to Dodge Attacks. #% less Armour and Energy Shield"][1] / 100f : 0); //Acrobatics
            float shiedES = 0;
            energyFromItems += disciplineES * (1 + increasedAuraEff / 100f);
            if (CharWeaponStyle == WeaponStyle.OnehandShield || CharWeaponStyle == WeaponStyle.UnArmedShield)
            {
                var offHand = itemData.EquippedItems.Where(item => item.Class == CharItemData.Item.ItemClass.OffHand).First();
                if (offHand != null && offHand.Properties.ContainsKey("Energy Shield:  #"))
                    shiedES = offHand.Properties["Energy Shield:  #"][0];
            }
            energyFromItems -= shiedES;//first we deduct shieldES from total ES from items
            if (attribs.ContainsKey("#% increased Energy Shield from equipped Shield"))
            {
                shiedES += (shiedES * attribs["#% increased Energy Shield from equipped Shield"][0] / 100f);
            }
            if (attribs.ContainsKey("#% increased Defences from equipped Shield"))
            {
                shiedES += (shiedES * attribs["#% increased Defences from equipped Shield"][0] / 100f); //modify ES
            }
            if (!attribs.ContainsKey("All bonuses from an equipped Shield apply to your Minions instead of you"))
                energyFromItems += shiedES; //Add modified ShieldES to the total ES
            EnergyShield = (int)((energyFromItems) * (1 + (increasedES + Intelligence * ESPerInt - reducedES) / 100f) * lessES * moreES);
            if (attribs.ContainsKey("Converts all Energy Shield to Mana"))
            {
                float increasedMana = attribs.ContainsKey("#% increased maximum Mana") ? attribs["#% increased maximum Mana"][0] : 0f;
                float reducedMana = attribs.ContainsKey("#% reduced maximum Mana") ? attribs["#% reduced maximum Mana"][0] : 0f;
                manaFromES = (int)((energyFromItems) * (1 + (increasedES + Intelligence * ESPerInt - reducedES + increasedMana - reducedMana) / 100f) * lessES * moreES);
                EnergyShield = 0;
            }
        }
        //needs more testing works for dual wielding
        protected void UpdateBlock(float blockFromShieldAndStaff)
        {
            float blockChance = attribs.ContainsKey("#% Block Chance") ? attribs["#% Block Chance"][0] : 0f;
            float spellBlockChance = attribs.ContainsKey("#% of Block Chance applied to Spells") ? attribs["#% of Block Chance applied to Spells"][0] : 0f;
            if (CharWeaponStyle == WeaponStyle.DualWield || CharWeaponStyle == WeaponStyle.DualWield2H)
            {
                float dualWieldBlock = DuelWieldBlockChance;

                dualWieldBlock += attribs.ContainsKey("#% additional Chance to Block while Dual Wielding") ? attribs["#% additional Chance to Block while Dual Wielding"][0] : 0f;
                dualWieldBlock += attribs.ContainsKey("#% additional Block Chance while Dual Wielding") ? attribs["#% additional Block Chance while Dual Wielding"][0] : 0f;
                dualWieldBlock += attribs.ContainsKey("#% additional Block Chance while Dual Wielding or holding a Shield") ? attribs["#% additional Block Chance while Dual Wielding or holding a Shield"][0] : 0f;
                dualWieldBlock += attribs.ContainsKey("#% additional Chance to Block while Dual Wielding or holding a Shield") ? attribs["#% additional Chance to Block while Dual Wielding or holding a Shield"][0] : 0f;
                if (WeaponTypeMH == WeaponType.Claw && WeaponTypeOH == WeaponType.Claw)
                    dualWieldBlock += attribs.ContainsKey("#% additional Block Chance while Dual Wielding Claws") ? attribs["#% additional Block Chance while Dual Wielding Claws"][0] : 0f;
                ChanceToBlock = (int)(blockChance + dualWieldBlock);
                ChanceToBlockSpell = (int)(ChanceToBlock * (spellBlockChance / 100f));
            }
            else if (CharWeaponStyle == WeaponStyle.TwoHand)
            {
                if (WeaponTypeMH == WeaponType.Staff)
                {
                    float staffBlock = blockFromShieldAndStaff;
                    staffBlock += attribs.ContainsKey("#% additional Block Chance with Staves") ? attribs["#% additional Block Chance with Staves"][0] : 0f;
                    ChanceToBlock = (int)(blockChance + staffBlock);
                    ChanceToBlockSpell = (int)(ChanceToBlock * (spellBlockChance / 100f));
                }
            }
            else if (CharWeaponStyle == WeaponStyle.OnehandShield || CharWeaponStyle == WeaponStyle.UnArmedShield)
            {
                if (blockFromShieldAndStaff != 0)
                {
                    float shieldBlock = blockFromShieldAndStaff;
                    shieldBlock += attribs.ContainsKey("#% additional Shield Block Chance") ? attribs["#% additional Shield Block Chance"][0] : 0f;
                    shieldBlock += attribs.ContainsKey("#% additional Block Chance while Dual Wielding or holding a Shield") ? attribs["#% additional Block Chance while Dual Wielding or holding a Shield"][0] : 0f;
                    shieldBlock += attribs.ContainsKey("#% additional Chance to Block while Dual Wielding or holding a Shield") ? attribs["#% additional Chance to Block while Dual Wielding or holding a Shield"][0] : 0f;
                    ChanceToBlock = (int)(blockChance + shieldBlock);
                    ChanceToBlockSpell = (int)(ChanceToBlock * (spellBlockChance / 100f));
                }
            }
        }
        //tested and is ok
        protected void UpdateChanceToDodge()
        {
            float dodgeAttacks = 0;
            if (attribs.ContainsKey("#% Chance to Dodge Attacks. #% less Armour and Energy Shield") || attribs.ContainsKey("Acrobatics"))
                dodgeAttacks = 30;
            dodgeAttacks += attribs.ContainsKey("#% additional chance to Dodge Attacks") ? attribs["#% additional chance to Dodge Attacks"][0] : 0f;
            if (attribs.ContainsKey("#% chance to Dodge Attacks per Frenzy Charge"))
                dodgeAttacks += CurrentFrenzy * attribs["#% chance to Dodge Attacks per Frenzy Charge"][0];
            ChanceToDodge = (int)dodgeAttacks;
            ChanceToDodgeSpell = (int)(attribs.ContainsKey("#% Chance to Dodge Spell Damage") ? attribs["#% Chance to Dodge Spell Damage"][0] : 0f); //Phase Acrobatics
        }
        //tested with my setup and works 
        protected void UpdatePhysicalDamage(int toPD = 0, int incPD = 0)
        {
            increasedMeleePdMH = 0;
            increasedMeleePdOH = 0;
            increasedProjDMH = 0;
            increasedProjDOH = 0;
            mainHandMinDamage = mainHandMinDamage_Org + toPD;
            mainHandMaxDamage = mainHandMaxDamage_Org + toPD;
            offHandMinDamage = offHandMinDamage_Org + toPD;
            offHandMaxDamage = offHandMaxDamage_Org + toPD;
            increasedMeleePdMH += incPD;
            increasedMeleePdOH += incPD;
            increasedProjDMH += incPD;
            increasedProjDOH += incPD;

            float increasedPD = attribs.ContainsKey("#% increased Physical Damage") ? attribs["#% increased Physical Damage"][0] : 0f; //affects both melee and projectiles
            increasedPD += incPD;
            increasedPD -= attribs.ContainsKey("#% reduced Physical Damage") ? attribs["#% reduced Physical Damage"][0] : 0f; //for unique shits
            float increasedMeleePD = attribs.ContainsKey("#% increased Melee Physical Damage") ? attribs["#% increased Melee Physical Damage"][0] : 0f;

            if (attribs.ContainsKey("#% increased Physical Damage with Weapons per Red Socket"))
            {
                int redSockets = 0;
                var MainHand = itemData.EquippedItems.Where(item => item.Class == CharItemData.Item.ItemClass.MainHand).First();
                if (MainHand.Name.ToLower() == "prismatic eclipse")
                {
                    foreach (int grp in MainHand.Sockets.Keys)
                    {
                        foreach (var socketColor in MainHand.Sockets[grp])
                            if (socketColor == CharItemData.Item.SocketColor.Red)
                                redSockets++;
                    }
                }
                if (CharWeaponStyle == WeaponStyle.DualWield)
                {
                    var OffHand = itemData.EquippedItems.Where(item => item.Class == CharItemData.Item.ItemClass.OffHand).First();
                    if (OffHand.Name.ToLower() == "prismatic eclipse")
                    {
                        foreach (int grp in OffHand.Sockets.Keys)
                        {
                            foreach (var socketColor in OffHand.Sockets[grp])
                                if (socketColor == CharItemData.Item.SocketColor.Red)
                                    redSockets++;
                        }
                    }
                }
                increasedPD += redSockets * PrismaticEclipsePD;
                increasedPD += redSockets * PrismaticEclipsePD;
            }
            increasedPD += OakPhysicalDamage;

            if (CharWeaponStyle == WeaponStyle.DualWield)
            {
                increasedPD += attribs.ContainsKey("#% increased Physical Weapon Damage while Dual Wielding") ? attribs["#% increased Physical Weapon Damage while Dual Wielding"][0] : 0f;
                increasedPD += attribs.ContainsKey("#% increased Physical Damage with One Handed Melee Weapons") ? attribs["#% increased Physical Damage with One Handed Melee Weapons"][0] : 0f;
                increasedMeleePD += Strength * MDmgPerStr; //for melees only
                if (attribs.ContainsKey("The increase to Physical Damage from Strength applies to Projectile Attacks as well as Melee Attacks"))
                {
                    increasedProjDMH += Strength * MDmgPerStr;
                    increasedProjDOH += Strength * MDmgPerStr;
                }
                increasedProjDMH += attribs.ContainsKey("#% increased Projectile Damage") ? attribs["#% increased Projectile Damage"][0] : 0f;
                increasedProjDOH += attribs.ContainsKey("#% increased Projectile Damage") ? attribs["#% increased Projectile Damage"][0] : 0f;
                increasedProjDMH += attribs.ContainsKey("#% increased Projectile Damage per Power Charge") ? attribs["#% increased Projectile Damage per Power Charge"][0] * CurrentPower : 0f;
                increasedProjDOH += attribs.ContainsKey("#% increased Projectile Damage per Power Charge") ? attribs["#% increased Projectile Damage per Power Charge"][0] * CurrentPower : 0f;

                if (attribs.ContainsKey("Adds #-# Physical Damage"))
                {
                    mainHandMinDamage += attribs["Adds #-# Physical Damage"][0];
                    offHandMinDamage += attribs["Adds #-# Physical Damage"][0];
                    mainHandMaxDamage += attribs["Adds #-# Physical Damage"][1];
                    offHandMaxDamage += attribs["Adds #-# Physical Damage"][1];
                }
                if (WeaponTypeMH == WeaponType.Claw)
                    increasedPD += attribs.ContainsKey("#% increased Physical Damage with Claws") ? attribs["#% increased Physical Damage with Claws"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.Dagger)
                    increasedPD += attribs.ContainsKey("#% increased Physical Damage with Daggers") ? attribs["#% increased Physical Damage with Daggers"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.OneHandedAxe)
                    increasedPD += attribs.ContainsKey("#% increased Physical Damage with Axes") ? attribs["#% increased Physical Damage with Axes"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.OneHandedMace)
                    increasedPD += attribs.ContainsKey("#% increased Physical Damage with Maces") ? attribs["#% increased Physical Damage with Maces"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.OneHandedSword)
                    increasedPD += attribs.ContainsKey("#% increased Physical Damage with Swords") ? attribs["#% increased Physical Damage with Swords"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.Wand)
                {
                    increasedProjDMH += attribs.ContainsKey("#% increased Physical Damage with Wands") ? attribs["#% increased Physical Damage with Wands"][0] : 0f;
                    increasedProjDMH -= attribs.ContainsKey("#% increased Physical Damage with One Handed Melee Weapons") ? attribs["#% increased Physical Damage with One Handed Melee Weapons"][0] : 0f;
                }
                increasedMeleePdMH = increasedMeleePD + increasedPD;
                increasedProjDMH += increasedPD;

                if (WeaponTypeOH == WeaponType.Claw)
                    increasedPD += attribs.ContainsKey("#% increased Physical Damage with Claws") ? attribs["#% increased Physical Damage with Claws"][0] : 0f;
                else if (WeaponTypeOH == WeaponType.Dagger)
                    increasedPD += attribs.ContainsKey("#% increased Physical Damage with Daggers") ? attribs["#% increased Physical Damage with Daggers"][0] : 0f;
                else if (WeaponTypeOH == WeaponType.OneHandedAxe)
                    increasedPD += attribs.ContainsKey("#% increased Physical Damage with Axes") ? attribs["#% increased Physical Damage with Axes"][0] : 0f;
                else if (WeaponTypeOH == WeaponType.OneHandedMace)
                    increasedPD += attribs.ContainsKey("#% increased Physical Damage with Maces") ? attribs["#% increased Physical Damage with Maces"][0] : 0f;
                else if (WeaponTypeOH == WeaponType.OneHandedSword)
                    increasedPD += attribs.ContainsKey("#% increased Physical Damage with Swords") ? attribs["#% increased Physical Damage with Swords"][0] : 0f;
                else if (WeaponTypeOH == WeaponType.Wand)
                {
                    increasedProjDOH += attribs.ContainsKey("#% increased Physical Damage with Wands") ? attribs["#% increased Physical Damage with Wands"][0] : 0f;
                    increasedProjDOH -= attribs.ContainsKey("#% increased Physical Damage with One Handed Melee Weapons") ? attribs["#% increased Physical Damage with One Handed Melee Weapons"][0] : 0f;
                }
                increasedMeleePdOH = increasedMeleePD + increasedPD;
                increasedProjDOH += increasedPD;

            }
            else if (CharWeaponStyle == WeaponStyle.OnehandShield || CharWeaponStyle == WeaponStyle.Onehand)
            {
                increasedPD += attribs.ContainsKey("#% increased Physical Damage with One Handed Melee Weapons") ? attribs["#% increased Physical Damage with One Handed Melee Weapons"][0] : 0f;
                increasedMeleePD += Strength * MDmgPerStr;
                if (attribs.ContainsKey("The increase to Physical Damage from Strength applies to Projectile Attacks as well as Melee Attacks"))
                {
                    increasedProjDMH += Strength * MDmgPerStr;
                }
                increasedProjDMH += attribs.ContainsKey("#% increased Projectile Damage") ? attribs["#% increased Projectile Damage"][0] : 0f;
                increasedProjDMH += attribs.ContainsKey("#% increased Projectile Damage per Power Charge") ? attribs["#% increased Projectile Damage per Power Charge"][0] * CurrentPower : 0f;

                if (attribs.ContainsKey("Adds #-# Physical Damage"))
                {
                    mainHandMinDamage += attribs["Adds #-# Physical Damage"][0];
                    mainHandMaxDamage += attribs["Adds #-# Physical Damage"][1];
                }
                if (WeaponTypeMH == WeaponType.Claw)
                    increasedPD += attribs.ContainsKey("#% increased Physical Damage with Claws") ? attribs["#% increased Physical Damage with Claws"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.Dagger)
                    increasedPD += attribs.ContainsKey("#% increased Physical Damage with Daggers") ? attribs["#% increased Physical Damage with Daggers"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.OneHandedAxe)
                    increasedPD += attribs.ContainsKey("#% increased Physical Damage with Axes") ? attribs["#% increased Physical Damage with Axes"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.OneHandedMace)
                    increasedPD += attribs.ContainsKey("#% increased Physical Damage with Maces") ? attribs["#% increased Physical Damage with Maces"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.OneHandedSword)
                    increasedPD += attribs.ContainsKey("#% increased Physical Damage with Swords") ? attribs["#% increased Physical Damage with Swords"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.Wand)
                {
                    increasedProjDMH += attribs.ContainsKey("#% increased Physical Damage with Wands") ? attribs["#% increased Physical Damage with Wands"][0] : 0f;
                    increasedProjDMH -= attribs.ContainsKey("#% increased Physical Damage with One Handed Melee Weapons") ? attribs["#% increased Physical Damage with One Handed Melee Weapons"][0] : 0f;
                }
                increasedMeleePdMH = increasedMeleePD + increasedPD;
                increasedProjDMH += increasedPD;
            }
            else if (CharWeaponStyle == WeaponStyle.TwoHand)
            {
                increasedPD += attribs.ContainsKey("#% increased Physical Damage with Two Handed Melee Weapons") ? attribs["#% increased Physical Damage with Two Handed Melee Weapons"][0] : 0f;
                increasedMeleePD += Strength * MDmgPerStr;
                if (attribs.ContainsKey("The increase to Physical Damage from Strength applies to Projectile Attacks as well as Melee Attacks"))
                {
                    increasedProjDMH += Strength * MDmgPerStr;
                }
                increasedProjDMH += attribs.ContainsKey("#% increased Projectile Damage") ? attribs["#% increased Projectile Damage"][0] : 0f;
                increasedProjDMH += attribs.ContainsKey("#% increased Projectile Damage per Power Charge") ? attribs["#% increased Projectile Damage per Power Charge"][0] * CurrentPower : 0f;

                if (attribs.ContainsKey("Adds #-# Physical Damage"))
                {
                    mainHandMinDamage += attribs["Adds #-# Physical Damage"][0];
                    mainHandMaxDamage += attribs["Adds #-# Physical Damage"][1];
                }
                if (WeaponTypeMH == WeaponType.Staff)
                    increasedPD += attribs.ContainsKey("#% increased Physical Damage with Staves") ? attribs["#% increased Physical Damage with Staves"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.TwoHandedAxe)
                    increasedPD += attribs.ContainsKey("#% increased Physical Damage with Axes") ? attribs["#% increased Physical Damage with Axes"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.TwoHandedMace)
                    increasedPD += attribs.ContainsKey("#% increased Physical Damage with Maces") ? attribs["#% increased Physical Damage with Maces"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.TwoHandedSword)
                    increasedPD += attribs.ContainsKey("#% increased Physical Damage with Swords") ? attribs["#% increased Physical Damage with Swords"][0] : 0f;
                increasedMeleePdMH = increasedMeleePD + increasedPD;
                increasedProjDMH += increasedPD;
            }
            else if (CharWeaponStyle == WeaponStyle.Ranged)
            {
                if (attribs.ContainsKey("Adds #-# Physical Damage"))
                {
                    mainHandMinDamage += attribs["Adds #-# Physical Damage"][0];
                    mainHandMaxDamage += attribs["Adds #-# Physical Damage"][1];
                }
                increasedProjDMH += attribs.ContainsKey("#% increased Physical Damage with Bows") ? attribs["#% increased Physical Damage with Bows"][0] : 0f;
                increasedProjDMH += attribs.ContainsKey("#% increased Projectile Damage") ? attribs["#% increased Projectile Damage"][0] : 0f;
                increasedProjDMH += attribs.ContainsKey("#% increased Projectile Damage per Power Charge") ? attribs["#% increased Projectile Damage per Power Charge"][0] * CurrentPower : 0f;
                increasedProjDMH += increasedPD;
                if (attribs.ContainsKey("The increase to Physical Damage from Strength applies to Projectile Attacks as well as Melee Attacks"))
                    increasedProjDMH += Strength * MDmgPerStr;
            }
            else if (CharWeaponStyle == WeaponStyle.DualWield2H)//wings of entropy. This need more testing.
            {
                increasedPD += attribs.ContainsKey("#% increased Physical Weapon Damage while Dual Wielding") ? attribs["#% increased Physical Weapon Damage while Dual Wielding"][0] : 0f;
                increasedPD += attribs.ContainsKey("#% increased Physical Damage with Two Handed Melee Weapons") ? attribs["#% increased Physical Damage with Two Handed Melee Weapons"][0] : 0f;
                increasedMeleePD += Strength * MDmgPerStr;
                if (attribs.ContainsKey("The increase to Physical Damage from Strength applies to Projectile Attacks as well as Melee Attacks"))
                {
                    increasedProjDMH += Strength * MDmgPerStr;
                }
                increasedProjDMH += attribs.ContainsKey("#% increased Projectile Damage") ? attribs["#% increased Projectile Damage"][0] : 0f;
                increasedProjDMH += attribs.ContainsKey("#% increased Projectile Damage per Power Charge") ? attribs["#% increased Projectile Damage per Power Charge"][0] * CurrentPower : 0f;

                if (attribs.ContainsKey("Adds #-# Physical Damage"))
                {
                    mainHandMinDamage += attribs["Adds #-# Physical Damage"][0];
                    mainHandMaxDamage += attribs["Adds #-# Physical Damage"][1];
                }
                offHandMinDamage = mainHandMinDamage;
                offHandMinDamage = mainHandMinDamage;
                if (WeaponTypeMH == WeaponType.Staff)
                    increasedPD += attribs.ContainsKey("#% increased Physical Damage with Staves") ? attribs["#% increased Physical Damage with Staves"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.TwoHandedAxe)
                    increasedPD += attribs.ContainsKey("#% increased Physical Damage with Axes") ? attribs["#% increased Physical Damage with Axes"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.TwoHandedMace)
                    increasedPD += attribs.ContainsKey("#% increased Physical Damage with Maces") ? attribs["#% increased Physical Damage with Maces"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.TwoHandedSword)
                    increasedPD += attribs.ContainsKey("#% increased Physical Damage with Swords") ? attribs["#% increased Physical Damage with Swords"][0] : 0f;

                increasedMeleePdMH = increasedMeleePD + increasedPD;
                increasedProjDMH += increasedPD;
                increasedProjDOH = increasedProjDMH;
            }
            else if (CharWeaponStyle == WeaponStyle.UnArmed || CharWeaponStyle == WeaponStyle.UnArmedShield)//Unarmed. This need more testing.
            {
                increasedMeleePD += Strength * MDmgPerStr;

                if (attribs.ContainsKey("Adds #-# Physical Damage"))
                {
                    mainHandMinDamage += attribs["Adds #-# Physical Damage"][0];
                    mainHandMaxDamage += attribs["Adds #-# Physical Damage"][1];
                }

                increasedMeleePdMH = increasedMeleePD + increasedPD;
            }
        }
        //tested, needs more testing with elelemtal damage on weps
        protected void UpdateElementalDamage(int toCold = 0, int incCold = 0, int toFire = 0, int incFire = 0, int toLight = 0, int incLight = 0, int toChaos = 0, int incChaos = 0, int incEle = 0)
        {
            increasedWeaponED = 0;
            increasedWeaponColdD = 0;
            increasedWeaponFireD = 0;
            increasedWeaponLightningD = 0;
            increasedWeaponChaosD = 0;
            increasedEdMainHand = 0;
            increasedEdOffHand = 0;

            IncreasedElementalDamageSpells = (int)(attribs.ContainsKey("#% increased Elemental Damage with Spells") ? attribs["#% increased Elemental Damage with Spells"][0] : 0f);

            int increasedElementalDamage = (int)(attribs.ContainsKey("#% increased Elemental Damage") ? attribs["#% increased Elemental Damage"][0] : 0f);
            increasedElementalDamage += incEle;
            IncreasedColdDamage = (int)(attribs.ContainsKey("#% increased Cold Damage") ? attribs["#% increased Cold Damage"][0] : 0f);
            IncreasedColdDamage += incCold;
            IncreasedFireDamage = (int)(attribs.ContainsKey("#% increased Fire Damage") ? attribs["#% increased Fire Damage"][0] : 0f);
            IncreasedFireDamage += incFire;
            IncreasedLightningDamage = (int)(attribs.ContainsKey("#% increased Lightning Damage") ? attribs["#% increased Lightning Damage"][0] : 0f);
            IncreasedLightningDamage += incLight;
            IncreasedChaosDamage = (int)(attribs.ContainsKey("#% increased Chaos Damage") ? attribs["#% increased Chaos Damage"][0] : 0f);
            IncreasedChaosDamage += incChaos;
            IncreasedColdDamage += increasedElementalDamage;
            IncreasedFireDamage += increasedElementalDamage;
            IncreasedLightningDamage += increasedElementalDamage;

            increasedWeaponED = attribs.ContainsKey("#% increased Elemental Damage with Weapons") ? attribs["#% increased Elemental Damage with Weapons"][0] : 0f;
            increasedWeaponColdD = attribs.ContainsKey("#% increased Cold Damage with Weapons") ? attribs["#% increased Cold Damage with Weapons"][0] : 0f;
            increasedWeaponFireD = attribs.ContainsKey("#% increased Fire Damage with Weapons") ? attribs["#% increased Fire Damage with Weapons"][0] : 0f;
            increasedWeaponLightningD = attribs.ContainsKey("#% increased Lightning Damage with Weapons") ? attribs["#% increased Lightning Damage with Weapons"][0] : 0f;
            increasedWeaponChaosD = attribs.ContainsKey("#% increased Chaos Damage with Weapons") ? attribs["#% increased Chaos Damage with Weapons"][0] : 0f;

            increasedWeaponColdD += (increasedWeaponED + IncreasedColdDamage);
            increasedWeaponFireD += (increasedWeaponED + IncreasedFireDamage);
            increasedWeaponLightningD += (increasedWeaponED + IncreasedLightningDamage);

            if (WeaponTypeMH == WeaponType.OneHandedMace || WeaponTypeMH == WeaponType.TwoHandedMace)
                increasedEdMainHand = attribs.ContainsKey("#% increased Elemental Damage with Maces") ? attribs["#% increased Elemental Damage with Maces"][0] : 0f;
            else if (WeaponTypeMH == WeaponType.Wand)
                increasedEdMainHand = attribs.ContainsKey("#% increased Elemental Damage with Wands") ? attribs["#% increased Elemental Damage with Wands"][0] : 0f;
            if (WeaponTypeOH == WeaponType.OneHandedMace || WeaponTypeOH == WeaponType.TwoHandedMace)
                increasedEdOffHand = attribs.ContainsKey("#% increased Elemental Damage with Maces") ? attribs["#% increased Elemental Damage with Maces"][0] : 0f;
            else if (WeaponTypeOH == WeaponType.Wand)
                increasedEdOffHand = attribs.ContainsKey("#% increased Elemental Damage with Wands") ? attribs["#% increased Elemental Damage with Wands"][0] : 0f;
            if (WeaponTypeMH != WeaponType.Other)
            {
                MainHandMinFireD_Tot = MainHandMinFireD_Org + toFire + AngerFireD[0] * (1 + increasedAuraEff / 100f);
                MainHandMaxFireD_Tot = MainHandMaxFireD_Org + toFire + AngerFireD[1] * (1 + increasedAuraEff / 100f);
                MainHandMinColdD_Tot = MainHandMinColdD_Org + toCold;
                MainHandMaxColdD_Tot = MainHandMaxColdD_Org + toCold;
                MainHandMinLightD_Tot = MainHandMinLightD_Org + toLight + WrathLightD[0] * (1 + increasedAuraEff / 100f);
                MainHandMaxLightD_Tot = MainHandMaxLightD_Org + toLight + WrathLightD[1] * (1 + increasedAuraEff / 100f);
                MainHandMinChaosD_Tot = MainHandMinChaosD_Org + toChaos;
                MainHandMaxChaosD_Tot = MainHandMaxChaosD_Org + toChaos;

                if (attribs.ContainsKey("Adds #-# Cold Damage"))
                {
                    MainHandMinColdD_Tot += attribs["Adds #-# Cold Damage"][0];
                    MainHandMaxColdD_Tot += attribs["Adds #-# Cold Damage"][1];
                }
                if (attribs.ContainsKey("Adds #-# Fire Damage"))
                {
                    MainHandMinFireD_Tot += attribs["Adds #-# Fire Damage"][0];
                    MainHandMaxFireD_Tot += attribs["Adds #-# Fire Damage"][1];
                }
                if (attribs.ContainsKey("Adds #-# Lightning Damage"))
                {
                    MainHandMinLightD_Tot += attribs["Adds #-# Lightning Damage"][0];
                    MainHandMaxLightD_Tot += attribs["Adds #-# Lightning Damage"][1];
                }
                if (attribs.ContainsKey("Adds #-# Chaos Damage"))
                {
                    MainHandMinChaosD_Tot += attribs["Adds #-# Chaos Damage"][0];
                    MainHandMaxChaosD_Tot += attribs["Adds #-# Chaos Damage"][1];
                }

            }
            if (CharWeaponStyle == WeaponStyle.DualWield)
            {
                OffHandMinFireD_Tot = OffHandMinFireD_Org + toFire + AngerFireD[0] * (1 + increasedAuraEff / 100f);
                OffHandMaxFireD_Tot = OffHandMaxFireD_Org + toFire + AngerFireD[1] * (1 + increasedAuraEff / 100f);
                OffHandMinColdD_Tot = OffHandMinColdD_Org + toCold;
                OffHandMaxColdD_Tot = OffHandMaxColdD_Org + toCold;
                OffHandMinLightD_Tot = OffHandMinLightD_Org + toLight + WrathLightD[0] * (1 + increasedAuraEff / 100f);
                OffHandMaxLightD_Tot = OffHandMaxLightD_Org + toLight + WrathLightD[1] * (1 + increasedAuraEff / 100f);
                OffHandMinChaosD_Tot = OffHandMinChaosD_Org + toChaos;
                OffHandMaxChaosD_Tot = OffHandMaxChaosD_Org + toChaos;

                if (attribs.ContainsKey("Adds #-# Cold Damage"))
                {
                    OffHandMinColdD_Tot += attribs["Adds #-# Cold Damage"][0];
                    OffHandMaxColdD_Tot += attribs["Adds #-# Cold Damage"][1];
                }
                if (attribs.ContainsKey("Adds #-# Fire Damage"))
                {
                    OffHandMinFireD_Tot += attribs["Adds #-# Fire Damage"][0];
                    OffHandMaxFireD_Tot += attribs["Adds #-# Fire Damage"][1];
                }
                if (attribs.ContainsKey("Adds #-# Lightning Damage"))
                {
                    OffHandMinLightD_Tot += attribs["Adds #-# Lightning Damage"][0];
                    OffHandMaxLightD_Tot += attribs["Adds #-# Lightning Damage"][1];
                }
                if (attribs.ContainsKey("Adds #-# Chaos Damage"))
                {
                    OffHandMinChaosD_Tot += attribs["Adds #-# Chaos Damage"][0];
                    OffHandMaxChaosD_Tot += attribs["Adds #-# Chaos Damage"][1];
                }
            }
        }

        protected void UpdateAttackSpeed(float modifiedIncreasedAS = 0)
        {
            increasedAsMH = 0;
            increasedAsOH = 0;
            float increasedAS = attribs.ContainsKey("#% increased Attack Speed") ? attribs["#% increased Attack Speed"][0] : 0f;
            increasedAS += CurrentFrenzy * ASPerFrenzy + HasteSpeed[0] * (1 + increasedAuraEff / 100f);
            if (attribs.ContainsKey("#% reduced Attack and Cast Speed per Frenzy Charge"))
                increasedAS -= CurrentFrenzy * attribs["#% reduced Attack and Cast Speed per Frenzy Charge"][0]; ;
            increasedAS += modifiedIncreasedAS;
            increasedAS -= attribs.ContainsKey("#% reduced Attack Speed") ? attribs["#% reduced Attack Speed"][0] : 0f;
            if (attribs.ContainsKey("#% increased Global Attack Speed per Green Socket"))
            {
                int greenSockets = 0;
                var MainHand = itemData.EquippedItems.Where(item => item.Class == CharItemData.Item.ItemClass.MainHand).First();
                foreach (int grp in MainHand.Sockets.Keys)
                {
                    foreach (var socketColor in MainHand.Sockets[grp])
                        if (socketColor == CharItemData.Item.SocketColor.Green)
                            greenSockets++;
                }
                if (CharWeaponStyle == WeaponStyle.DualWield)
                {
                    var OffHand = itemData.EquippedItems.Where(item => item.Class == CharItemData.Item.ItemClass.OffHand).First();
                    foreach (int grp in OffHand.Sockets.Keys)
                    {
                        foreach (var socketColor in OffHand.Sockets[grp])
                            if (socketColor == CharItemData.Item.SocketColor.Green)
                                greenSockets++;
                    }
                }
                increasedAS += greenSockets * PrismaticEclipseAs;
            }
            increasedAS += KraitynAttackSpeed;//Must check to see if it adds additively to this value;
            if (CharWeaponStyle == WeaponStyle.DualWield)
            {
                increasedAS += attribs.ContainsKey("#% increased Attack Speed while Dual Wielding") ? attribs["#% increased Attack Speed while Dual Wielding"][0] : 0f;
                increasedAS += attribs.ContainsKey("#% increased Attack Speed with One Handed Melee Weapons") ? attribs["#% increased Attack Speed with One Handed Melee Weapons"][0] : 0f;

                if (WeaponTypeMH == WeaponType.Claw)
                    increasedAsMH += attribs.ContainsKey("#% increased Attack Speed with Claws") ? attribs["#% increased Attack Speed with Claws"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.Dagger)
                    increasedAsMH += attribs.ContainsKey("#% increased Attack Speed with Daggers") ? attribs["#% increased Attack Speed with Daggers"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.OneHandedAxe)
                    increasedAsMH += attribs.ContainsKey("#% increased Attack Speed with Axes") ? attribs["#% increased Attack Speed with Axes"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.OneHandedMace)
                    increasedAsMH += attribs.ContainsKey("#% increased Attack Speed with Maces") ? attribs["#% increased Attack Speed with Maces"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.OneHandedSword)
                    increasedAsMH += attribs.ContainsKey("#% increased Attack Speed with Swords") ? attribs["#% increased Attack Speed with Swords"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.Wand)
                {
                    increasedAsMH += attribs.ContainsKey("#% increased Attack Speed with Wands") ? attribs["#% increased Attack Speed with Wands"][0] : 0f;
                    increasedAS -= attribs.ContainsKey("#% increased Attack Speed with One Handed Melee Weapons") ? attribs["#% increased Attack Speed with One Handed Melee Weapons"][0] : 0f;
                }
                increasedAsMH = increasedAS + increasedAsMH;

                if (WeaponTypeOH == WeaponType.Claw)
                    increasedAsOH += attribs.ContainsKey("#% increased Attack Speed with Claws") ? attribs["#% increased Attack Speed with Claws"][0] : 0f;
                else if (WeaponTypeOH == WeaponType.Dagger)
                    increasedAsOH += attribs.ContainsKey("#% increased Attack Speed with Daggers") ? attribs["#% increased Attack Speed with Daggers"][0] : 0f;
                else if (WeaponTypeOH == WeaponType.OneHandedAxe)
                    increasedAsOH += attribs.ContainsKey("#% increased Attack Speed with Axes") ? attribs["#% increased Attack Speed with Axes"][0] : 0f;
                else if (WeaponTypeOH == WeaponType.OneHandedMace)
                    increasedAsOH += attribs.ContainsKey("#% increased Attack Speed with Maces") ? attribs["#% increased Attack Speed with Maces"][0] : 0f;
                else if (WeaponTypeOH == WeaponType.OneHandedSword)
                    increasedAsOH += attribs.ContainsKey("#% increased Attack Speed with Swords") ? attribs["#% increased Attack Speed with Swords"][0] : 0f;
                else if (WeaponTypeOH == WeaponType.Wand)
                {
                    increasedAsOH += attribs.ContainsKey("#% increased Attack Speed with Wands") ? attribs["#% increased Attack Speed with Wands"][0] : 0f;
                    increasedAS -= attribs.ContainsKey("#% increased Attack Speed with One Handed Melee Weapons") ? attribs["#% increased Attack Speed with One Handed Melee Weapons"][0] : 0f;
                }
                increasedAsOH = increasedAsOH + increasedAS;
            }
            else if (CharWeaponStyle == WeaponStyle.OnehandShield)
            {
                increasedAS += attribs.ContainsKey("#% increased Attack Speed with One Handed Melee Weapons") ? attribs["#% increased Attack Speed with One Handed Melee Weapons"][0] : 0f;

                if (WeaponTypeMH == WeaponType.Claw)
                    increasedAsMH += attribs.ContainsKey("#% increased Attack Speed with Claws") ? attribs["#% increased Attack Speed with Claws"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.Dagger)
                    increasedAsMH += attribs.ContainsKey("#% increased Attack Speed with Daggers") ? attribs["#% increased Attack Speed with Daggers"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.OneHandedAxe)
                    increasedAsMH += attribs.ContainsKey("#% increased Attack Speed with Axes") ? attribs["#% increased Attack Speed with Axes"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.OneHandedMace)
                    increasedAsMH += attribs.ContainsKey("#% increased Attack Speed with Maces") ? attribs["#% increased Attack Speed with Maces"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.OneHandedSword)
                    increasedAsMH += attribs.ContainsKey("#% increased Attack Speed with Swords") ? attribs["#% increased Attack Speed with Swords"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.Wand)
                {
                    increasedAsMH += attribs.ContainsKey("#% increased Attack Speed with Wands") ? attribs["#% increased Attack Speed with Wands"][0] : 0f;
                    increasedAS -= attribs.ContainsKey("#% increased Attack Speed with One Handed Melee Weapons") ? attribs["#% increased Attack Speed with One Handed Melee Weapons"][0] : 0f;
                }
                increasedAsMH = increasedAS + increasedAsMH;
            }
            else if (CharWeaponStyle == WeaponStyle.TwoHand)
            {
                increasedAS += attribs.ContainsKey("#% increased Attack Speed with Two Handed Melee Weapons") ? attribs["#% increased Attack Speed with Two Handed Melee Weapons"][0] : 0f;

                if (WeaponTypeMH == WeaponType.Staff)
                    increasedAsMH += attribs.ContainsKey("#% increased Attack Speed with Staves") ? attribs["#% increased Attack Speed with Staves"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.TwoHandedAxe)
                    increasedAsMH += attribs.ContainsKey("#% increased Attack Speed with Axes") ? attribs["#% increased Attack Speed with Axes"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.TwoHandedMace)
                    increasedAsMH += attribs.ContainsKey("#% increased Attack Speed with Maces") ? attribs["#% increased Attack Speed with Maces"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.TwoHandedSword)
                    increasedAsMH += attribs.ContainsKey("#% increased Attack Speed with Swords") ? attribs["#% increased Attack Speed with Swords"][0] : 0f;

                increasedAsMH = increasedAS + increasedAsMH;
            }
            else if (CharWeaponStyle == WeaponStyle.Ranged)
            {
                increasedAsMH += attribs.ContainsKey("#% increased Attack Speed with Bows") ? attribs["#% increased Attack Speed with Bows"][0] : 0f;
                increasedAsMH = increasedAsMH + increasedAS;
            }
            else if (CharWeaponStyle == WeaponStyle.DualWield2H)//wings of entropy
            {
                increasedAS += attribs.ContainsKey("#% increased Attack Speed while Dual Wielding") ? attribs["#% increased Attack Speed while Dual Wielding"][0] : 0f;
                increasedAS += attribs.ContainsKey("#% increased Attack Speed with Two Handed Melee Weapons") ? attribs["#% increased Attack Speed with Two Handed Melee Weapons"][0] : 0f;

                if (WeaponTypeMH == WeaponType.Staff)
                    increasedAsMH += attribs.ContainsKey("#% increased Attack Speed with Staves") ? attribs["#% increased Attack Speed with Staves"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.TwoHandedAxe)
                    increasedAsMH += attribs.ContainsKey("#% increased Attack Speed with Axes") ? attribs["#% increased Attack Speed with Axes"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.TwoHandedMace)
                    increasedAsMH += attribs.ContainsKey("#% increased Attack Speed with Maces") ? attribs["#% increased Attack Speed with Maces"][0] : 0f;
                else if (WeaponTypeMH == WeaponType.TwoHandedSword)
                    increasedAsMH += attribs.ContainsKey("#% increased Attack Speed with Swords") ? attribs["#% increased Attack Speed with Swords"][0] : 0f;
                increasedAsMH = increasedAS + increasedAsMH;
            }
        }

        protected void UpdateCastSpeed(float modifiedIncreasedCS = 0)
        {
            IncreasedCastSpeed = 0;
            IncreasedCurseCastSpeed = 0;
            float increasedCS = attribs.ContainsKey("#% increased Cast Speed") ? attribs["#% increased Cast Speed"][0] : 0f;
            increasedCS += CurrentFrenzy * ASPerFrenzy + HasteSpeed[1] * (1 + increasedAuraEff / 100f) + AliraCastSpeed;
            if (attribs.ContainsKey("#% reduced Attack and Cast Speed per Frenzy Charge"))
                increasedCS -= CurrentFrenzy * attribs["#% reduced Attack and Cast Speed per Frenzy Charge"][0];
            increasedCS += modifiedIncreasedCS;
            increasedCS -= attribs.ContainsKey("#% reduced Cast Speed") ? attribs["#% reduced Cast Speed"][0] : 0f;
            IncreasedCastSpeed = increasedCS;
            IncreasedCurseCastSpeed = IncreasedCastSpeed;
            IncreasedCurseCastSpeed += attribs.ContainsKey("#% increased Cast Speed for Curses") ? attribs["#% increased Cast Speed for Curses"][0] : 0f;
        }

        protected void UpdateCriticalStrike(float modifiedIncreasedCrit = 0, float modifiedIncreasedCritMult = 0)
        {
            CritMultiplierMH = 150;
            CritMultiplierOH = 150;//
            CritMultiplierSpell = 150;
            float lessCrit = attribs.ContainsKey("#% less Critical Strike Chance") ? attribs["#% less Critical Strike Chance"][0] / 100f : 0f;
            float increasedCrit = attribs.ContainsKey("#% increased Critical Strike Chance") ? attribs["#% increased Critical Strike Chance"][0] : 0f;
            increasedCrit += attribs.ContainsKey("#% increased Global Critical Strike Chance") ? attribs["#% increased Global Critical Strike Chance"][0] : 0f;
            increasedCrit += CurrentPower * CritChancePerPower;
            increasedCrit += (comparerModifiedIncreasedCrit + modifiedIncreasedCrit);
            float increasedCritMult = attribs.ContainsKey("#% increased Critical Strike Multiplier") ? attribs["#% increased Critical Strike Multiplier"][0] : 0f;
            increasedCritMult += attribs.ContainsKey("#% increased Global Critical Strike Multiplier") ? attribs["#% increased Global Critical Strike Multiplier"][0] : 0f;
            increasedCritMult += (comparerModifiedIncreasedCritMult + modifiedIncreasedCritMult);
            increasedCritSpell = attribs.ContainsKey("#% increased Critical Strike Chance for Spells") ? attribs["#% increased Critical Strike Chance for Spells"][0] : 0f;
            increasedCritSpell += increasedCrit;
            float increasedCritMultSpell = attribs.ContainsKey("#% increased Critical Strike Multiplier for Spells") ? attribs["#% increased Critical Strike Multiplier for Spells"][0] : 0f;
            increasedCritMultSpell += increasedCritMult;
            CritMultiplierSpell = CritMultiplierSpell * (1 + (increasedCritMultSpell) / 100f);

            if (attribs.ContainsKey("Never deal Critical Strikes"))
            {
                CritChanceMH = 0;
                CritChanceOH = 0;
                return;
            }
            if (CharWeaponStyle == WeaponStyle.DualWield)
            {
                increasedCrit += attribs.ContainsKey("#% increased Weapon Critical Strike Chance while Dual Wielding") ? attribs["#% increased Weapon Critical Strike Chance while Dual Wielding"][0] : 0f;
                increasedCrit += attribs.ContainsKey("#% increased Critical Strike Chance with One Handed Melee Weapons") ? attribs["#% increased Critical Strike Chance with One Handed Melee Weapons"][0] : 0f;
                increasedCritMult += attribs.ContainsKey("#% increased Critical Strike Multiplier with One Handed Melee Weapons") ? attribs["#% increased Critical Strike Multiplier with One Handed Melee Weapons"][0] : 0f;

                float increasedCritMH = 0;
                float increasedCritMultiMH = 0;
                float increasedCritOH = 0;
                float increasedCritMultiOH = 0;

                if (WeaponTypeMH == WeaponType.Claw)
                {
                    increasedCritMH += attribs.ContainsKey("#% increased Critical Strike Chance with Claws") ? attribs["#% increased Critical Strike Chance with Claws"][0] : 0f;
                    increasedCritMultiMH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Claws") ? attribs["#% increased Critical Strike Multiplier with Claws"][0] : 0f;
                }
                else if (WeaponTypeMH == WeaponType.Dagger)
                {
                    increasedCritMH += attribs.ContainsKey("#% increased Critical Strike Chance with Daggers") ? attribs["#% increased Critical Strike Chance with Daggers"][0] : 0f;
                    increasedCritMultiMH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Daggers") ? attribs["#% increased Critical Strike Multiplier with Daggers"][0] : 0f;
                }
                else if (WeaponTypeMH == WeaponType.OneHandedAxe)
                {
                    increasedCritMH += attribs.ContainsKey("#% increased Critical Strike Chance with Axes") ? attribs["#% increased Critical Strike Chance with Axes"][0] : 0f;
                    increasedCritMultiMH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Axes") ? attribs["#% increased Critical Strike Multiplier with Axes"][0] : 0f;
                }
                else if (WeaponTypeMH == WeaponType.OneHandedMace)
                {
                    increasedCritMH += attribs.ContainsKey("#% increased Critical Strike Chance with Maces") ? attribs["#% increased Critical Strike Chance with Maces"][0] : 0f;
                    increasedCritMultiMH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Maces") ? attribs["#% increased Critical Strike Multiplier with Maces"][0] : 0f;
                }
                else if (WeaponTypeMH == WeaponType.OneHandedSword)
                {
                    increasedCritMH += attribs.ContainsKey("#% increased Critical Strike Chance with Swords") ? attribs["#% increased Critical Strike Chance with Swords"][0] : 0f;
                    increasedCritMultiMH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Swords") ? attribs["#% increased Critical Strike Multiplier with Swords"][0] : 0f;
                }
                else if (WeaponTypeMH == WeaponType.Wand)
                {
                    increasedCritMH += attribs.ContainsKey("#% increased Critical Strike Chance with Wands") ? attribs["#% increased Attack Speed with Wands"][0] : 0f;
                    increasedCritMH -= attribs.ContainsKey("#% increased Critical Strike Chance with One Handed Melee Weapons") ? attribs["#% increased Critical Strike Chance with One Handed Melee Weapons"][0] : 0f;
                    increasedCritMultiMH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Wands") ? attribs["#% increased Critical Strike Multiplier with Wands"][0] : 0f;
                    increasedCritMultiMH -= attribs.ContainsKey("#% increased Critical Strike Multiplier with One Handed Melee Weapons") ? attribs["#% increased Critical Strike Multiplier with One Handed Melee Weapons"][0] : 0f;
                }
                CritChanceMH = CritChanceMH_Org * (1 + (increasedCrit + increasedCritMH) / 100f) * (1 - lessCrit);
                CritMultiplierMH = CritMultiplierMH * (1 + (increasedCritMult + increasedCritMultiMH) / 100f);

                if (WeaponTypeOH == WeaponType.Claw)
                {
                    increasedCritOH += attribs.ContainsKey("#% increased Critical Strike Chance with Claws") ? attribs["#% increased Critical Strike Chance with Claws"][0] : 0f;
                    increasedCritMultiOH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Claws") ? attribs["#% increased Critical Strike Multiplier with Claws"][0] : 0f;
                }
                else if (WeaponTypeOH == WeaponType.Dagger)
                {
                    increasedCritOH += attribs.ContainsKey("#% increased Critical Strike Chance with Daggers") ? attribs["#% increased Critical Strike Chance with Daggers"][0] : 0f;
                    increasedCritMultiOH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Daggers") ? attribs["#% increased Critical Strike Multiplier with Daggers"][0] : 0f;
                }
                else if (WeaponTypeOH == WeaponType.OneHandedAxe)
                {
                    increasedCritOH += attribs.ContainsKey("#% increased Critical Strike Chance with Axes") ? attribs["#% increased Critical Strike Chance with Axes"][0] : 0f;
                    increasedCritMultiOH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Axes") ? attribs["#% increased Critical Strike Multiplier with Axes"][0] : 0f;
                }
                else if (WeaponTypeOH == WeaponType.OneHandedMace)
                {
                    increasedCritOH += attribs.ContainsKey("#% increased Critical Strike Chance with Maces") ? attribs["#% increased Critical Strike Chance with Maces"][0] : 0f;
                    increasedCritMultiOH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Maces") ? attribs["#% increased Critical Strike Multiplier with Maces"][0] : 0f;
                }
                else if (WeaponTypeOH == WeaponType.OneHandedSword)
                {
                    increasedCritOH += attribs.ContainsKey("#% increased Critical Strike Chance with Swords") ? attribs["#% increased Critical Strike Chance with Swords"][0] : 0f;
                    increasedCritMultiOH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Swords") ? attribs["#% increased Critical Strike Multiplier with Swords"][0] : 0f;
                }
                else if (WeaponTypeOH == WeaponType.Wand)
                {
                    increasedCritOH += attribs.ContainsKey("#% increased Critical Strike Chance with Wands") ? attribs["#% increased Attack Speed with Wands"][0] : 0f;
                    increasedCritOH -= attribs.ContainsKey("#% increased Critical Strike Chance with One Handed Melee Weapons") ? attribs["#% increased Critical Strike Chance with One Handed Melee Weapons"][0] : 0f;
                    increasedCritMultiOH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Wands") ? attribs["#% increased Critical Strike Multiplier with Wands"][0] : 0f;
                    increasedCritMultiOH -= attribs.ContainsKey("#% increased Critical Strike Multiplier with One Handed Melee Weapons") ? attribs["#% increased Critical Strike Multiplier with One Handed Melee Weapons"][0] : 0f;
                }
                CritChanceOH = CritChanceOH_Org * (1 + (increasedCrit + increasedCritOH) / 100f) * (1 - lessCrit);
                CritMultiplierOH = CritMultiplierOH * (1 + (increasedCritMult + increasedCritMultiOH) / 100f);
            }
            else if (CharWeaponStyle == WeaponStyle.OnehandShield)
            {
                increasedCrit += attribs.ContainsKey("#% increased Critical Strike Chance with One Handed Melee Weapons") ? attribs["#% increased Critical Strike Chance with One Handed Melee Weapons"][0] : 0f;
                increasedCritMult += attribs.ContainsKey("#% increased Critical Strike Multiplier with One Handed Melee Weapons") ? attribs["#% increased Critical Strike Multiplier with One Handed Melee Weapons"][0] : 0f;

                float increasedCritMH = 0;
                float increasedCritMultiMH = 0;

                if (WeaponTypeMH == WeaponType.Claw)
                {
                    increasedCritMH += attribs.ContainsKey("#% increased Critical Strike Chance with Claws") ? attribs["#% increased Critical Strike Chance with Claws"][0] : 0f;
                    increasedCritMultiMH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Claws") ? attribs["#% increased Critical Strike Multiplier with Claws"][0] : 0f;
                }
                else if (WeaponTypeMH == WeaponType.Dagger)
                {
                    increasedCritMH += attribs.ContainsKey("#% increased Critical Strike Chance with Daggers") ? attribs["#% increased Critical Strike Chance with Daggers"][0] : 0f;
                    increasedCritMultiMH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Daggers") ? attribs["#% increased Critical Strike Multiplier with Daggers"][0] : 0f;
                }
                else if (WeaponTypeMH == WeaponType.OneHandedAxe)
                {
                    increasedCritMH += attribs.ContainsKey("#% increased Critical Strike Chance with Axes") ? attribs["#% increased Critical Strike Chance with Axes"][0] : 0f;
                    increasedCritMultiMH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Axes") ? attribs["#% increased Critical Strike Multiplier with Axes"][0] : 0f;
                }
                else if (WeaponTypeMH == WeaponType.OneHandedMace)
                {
                    increasedCritMH += attribs.ContainsKey("#% increased Critical Strike Chance with Maces") ? attribs["#% increased Critical Strike Chance with Maces"][0] : 0f;
                    increasedCritMultiMH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Maces") ? attribs["#% increased Critical Strike Multiplier with Maces"][0] : 0f;
                }
                else if (WeaponTypeMH == WeaponType.OneHandedSword)
                {
                    increasedCritMH += attribs.ContainsKey("#% increased Critical Strike Chance with Swords") ? attribs["#% increased Critical Strike Chance with Swords"][0] : 0f;
                    increasedCritMultiMH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Swords") ? attribs["#% increased Critical Strike Multiplier with Swords"][0] : 0f;
                }
                else if (WeaponTypeMH == WeaponType.Wand)
                {
                    increasedCritMH += attribs.ContainsKey("#% increased Critical Strike Chance with Wands") ? attribs["#% increased Attack Speed with Wands"][0] : 0f;
                    increasedCritMH -= attribs.ContainsKey("#% increased Critical Strike Chance with One Handed Melee Weapons") ? attribs["#% increased Critical Strike Chance with One Handed Melee Weapons"][0] : 0f;
                    increasedCritMultiMH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Wands") ? attribs["#% increased Critical Strike Multiplier with Wands"][0] : 0f;
                    increasedCritMultiMH -= attribs.ContainsKey("#% increased Critical Strike Multiplier with One Handed Melee Weapons") ? attribs["#% increased Critical Strike Multiplier with One Handed Melee Weapons"][0] : 0f;
                }
                CritChanceMH = CritChanceMH_Org * (1 + (increasedCrit + increasedCritMH) / 100f) * (1 - lessCrit);
                CritMultiplierMH = CritMultiplierMH * (1 + (increasedCritMult + increasedCritMultiMH) / 100f);
            }
            else if (CharWeaponStyle == WeaponStyle.TwoHand)
            {
                increasedCrit += attribs.ContainsKey("#% increased Critical Strike Chance with Two Handed Melee Weapons") ? attribs["#% increased Critical Strike Chance with Two Handed Melee Weapons"][0] : 0f;
                increasedCritMult += attribs.ContainsKey("#% increased Critical Strike Multiplier with Two Handed Melee Weapons") ? attribs["#% increased Critical Strike Multiplier with Two Handed Melee Weapons"][0] : 0f; //does not exists atm
                float increasedCritMH = 0;
                float increasedCritMultiMH = 0;

                if (WeaponTypeMH == WeaponType.Staff)
                {
                    increasedCritMH += attribs.ContainsKey("#% increased Critical Strike Chance with Staves") ? attribs["#% increased Critical Strike Chance with Staves"][0] : 0f;
                    increasedCritMultiMH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Staves") ? attribs["#% increased Critical Strike Multiplier with Staves"][0] : 0f;
                }
                else if (WeaponTypeMH == WeaponType.TwoHandedAxe)
                {
                    increasedCritMH += attribs.ContainsKey("#% increased Critical Strike Chance with Axes") ? attribs["#% increased Critical Strike Chance with Axes"][0] : 0f;
                    increasedCritMultiMH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Axes") ? attribs["#% increased Critical Strike Multiplier with Axes"][0] : 0f;
                }
                else if (WeaponTypeMH == WeaponType.TwoHandedMace)
                {
                    increasedCritMH += attribs.ContainsKey("#% increased Critical Strike Chance with Maces") ? attribs["#% increased Critical Strike Chance with Maces"][0] : 0f;
                    increasedCritMultiMH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Maces") ? attribs["#% increased Critical Strike Multiplier with Maces"][0] : 0f;
                }
                else if (WeaponTypeMH == WeaponType.TwoHandedSword)
                {
                    increasedCritMH += attribs.ContainsKey("#% increased Critical Strike Chance with Swords") ? attribs["#% increased Critical Strike Chance with Swords"][0] : 0f;
                    increasedCritMultiMH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Swords") ? attribs["#% increased Critical Strike Multiplier with Swords"][0] : 0f;
                }
                CritChanceMH = CritChanceMH_Org * (1 + (increasedCrit + increasedCritMH) / 100f) * (1 - lessCrit);
                CritMultiplierMH = CritMultiplierMH * (1 + (increasedCritMult + increasedCritMultiMH) / 100f);
            }
            else if (CharWeaponStyle == WeaponStyle.Ranged)
            {
                float increasedCritMH = 0;
                float increasedCritMultiMH = 0;

                increasedCritMH += attribs.ContainsKey("#% increased Critical Strike Chance with Bows") ? attribs["#% increased Critical Strike Chance with Bows"][0] : 0f;
                increasedCritMultiMH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Bows") ? attribs["#% increased Critical Strike Multiplier with Bows"][0] : 0f;
                CritChanceMH = CritChanceMH_Org * (1 + (increasedCrit + increasedCritMH) / 100f) * (1 - lessCrit);
                CritMultiplierMH = CritMultiplierMH * (1 + (increasedCritMult + increasedCritMultiMH) / 100f);
            }
            else if (CharWeaponStyle == WeaponStyle.DualWield2H)//wings of entropy
            {
                increasedCrit += attribs.ContainsKey("#% increased Weapon Critical Strike Chance while Dual Wielding") ? attribs["#% increased Weapon Critical Strike Chance while Dual Wielding"][0] : 0f;
                increasedCrit += attribs.ContainsKey("#% increased Critical Strike Chance with Two Handed Melee Weapons") ? attribs["#% increased Critical Strike Chance with Two Handed Melee Weapons"][0] : 0f;

                float increasedCritMH = 0;
                float increasedCritMultiMH = 0;

                if (WeaponTypeMH == WeaponType.Staff)//maybe one day there will be such a weapon
                {
                    increasedCritMH += attribs.ContainsKey("#% increased Critical Strike Chance with Staves") ? attribs["#% increased Critical Strike Chance with Staves"][0] : 0f;
                    increasedCritMultiMH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Staves") ? attribs["#% increased Critical Strike Multiplier with Staves"][0] : 0f;
                }
                else if (WeaponTypeMH == WeaponType.TwoHandedAxe)
                {
                    increasedCritMH += attribs.ContainsKey("#% increased Critical Strike Chance with Axes") ? attribs["#% increased Critical Strike Chance with Axes"][0] : 0f;
                    increasedCritMultiMH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Axes") ? attribs["#% increased Critical Strike Multiplier with Axes"][0] : 0f;
                }
                else if (WeaponTypeMH == WeaponType.TwoHandedMace)
                {
                    increasedCritMH += attribs.ContainsKey("#% increased Critical Strike Chance with Maces") ? attribs["#% increased Critical Strike Chance with Maces"][0] : 0f;
                    increasedCritMultiMH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Maces") ? attribs["#% increased Critical Strike Multiplier with Maces"][0] : 0f;
                }
                else if (WeaponTypeMH == WeaponType.TwoHandedSword)
                {
                    increasedCritMH += attribs.ContainsKey("#% increased Critical Strike Chance with Swords") ? attribs["#% increased Critical Strike Chance with Swords"][0] : 0f;
                    increasedCritMultiMH += attribs.ContainsKey("#% increased Critical Strike Multiplier with Swords") ? attribs["#% increased Critical Strike Multiplier with Swords"][0] : 0f;
                }
                CritChanceMH = CritChanceMH_Org * (1 + (increasedCrit + increasedCritMH) / 100f) * (1 - lessCrit);
                CritMultiplierMH = CritMultiplierMH * (1 + (increasedCritMult + increasedCritMultiMH) / 100f);
            }
        }

        protected void CalcDamageReduction()
        {
            double reduction = 0;
            reduction = 100 * Armour / (Armour + (12f * monsterDamage[Level - 1]));
            reduction += CurrentEndurance * PhysRedPerEndurance;
            if (reduction > 90) reduction = 90;//capped at 90
            PhysDamageReduction = (int)(reduction + 0.5f);
        }

        protected double CalcChanceToHit(int level, int accuracy)
        {
            if (attribs.ContainsKey("Your hits can't be Evaded"))
                return 100;
            double hitChance = 5; //Min hit chance
            hitChance = 100 * accuracy / (accuracy + Math.Pow((monsterEvasion[level - 1] / 4), 0.8));
            return hitChance > 95 ? 95 : hitChance;
        }

        protected void CalcChanceToEvade()
        {
            if (attribs.ContainsKey("Cannot Evade enemy Attacks"))
                ChanceToEvade = 0;
            double evadeChance = 0; //Min hit chance
            evadeChance = 1 - monsterAccuracy[Level - 1] / (monsterAccuracy[Level - 1] + Math.Pow((EvasionRating / 4), 0.8));
            ChanceToEvade = (int)(evadeChance * 100 + 0.5f);
        }

        protected void CalcSpellSkillDPS(ActiveGem spellSkill)
        {
            #region Additive attribs
            //Damage
            float reducedDamage = attribs.ContainsKey("#% reduced Damage") ? attribs["#% reduced Damage"][0] / 2f : 0f; //Leer Cast unique
            reducedDamage -= attribs.ContainsKey("#% increased Damage") ? attribs["#% increased Damage"][0] : 0f; //Le Heup of All
            float skillIncreasedDmg = spellSkill.Mods.ContainsKey("#% increased Projectile Damage") ? spellSkill.Mods["#% increased Projectile Damage"][0] : 0f;
            if (spellSkill.Keywords.Contains("Projectile"))
                skillIncreasedDmg += attribs.ContainsKey("#% increased Projectile Damage") ? attribs["#% increased Projectile Damage"][0] : 0f;
            if (spellSkill.GemType == GemType.Totem)
                skillIncreasedDmg += attribs.ContainsKey("#% increased Totem Damage") ? attribs["#% increased Totem Damage"][0] : 0f;
            if (spellSkill.GemType == GemType.Trap)
                skillIncreasedDmg += attribs.ContainsKey("#% increased Trap Damage") ? attribs["#% increased Trap Damage"][0] : 0f;
            float skillIncreasedBurningDmg = attribs.ContainsKey("#% increased Burning Damage") ? attribs["#% increased Burning Damage"][0] : 0f;
            skillIncreasedBurningDmg += attribs.ContainsKey("#% increased Damage over Time") ? attribs["#% increased Damage over Time"][0] : 0f;
            skillIncreasedBurningDmg += spellSkill.Mods.ContainsKey("#% increased Burning Damage") ? spellSkill.Mods["#% increased Burning Damage"][0] : 0f;

            //Cast Speed
            float skillReducedCS = spellSkill.Mods.ContainsKey("#% reduced Cast Speed") ? spellSkill.Mods["#% reduced Cast Speed"][0] : 0f;
            float skillIncreasedCS = spellSkill.Mods.ContainsKey("#% increased Cast Speed") ? spellSkill.Mods["#% increased Cast Speed"][0] : 0f;
            skillIncreasedCS += spellSkill.Mods.ContainsKey("#% increased Cast Speed per Frenzy Charge") ? spellSkill.Mods["#% increased Cast Speed per Frenzy Charge"][0] * CurrentFrenzy : 0f;
            if (spellSkill.GemType == GemType.Totem)
                skillIncreasedCS += attribs.ContainsKey("Spells Cast by Totems have #% increased Cast Speed") ? attribs["Spells Cast by Totems have #% increased Cast Speed"][0] : 0f;
            //Crit
            float skillIncreasedCrit = spellSkill.Mods.ContainsKey("#% increased Critical Strike Chance") ? spellSkill.Mods["#% increased Critical Strike Chance"][0] : 0;
            #endregion

            #region Multiplicative attribs
            //Damage
            float skillDmgEffectiveness = 1f;
            if (spellSkill.Mods.ContainsKey("Deals #% of Base Damage"))
                skillDmgEffectiveness = spellSkill.Mods["Deals #% of Base Damage"][0] / 100f;
            else if (spellSkill.Properties.ContainsKey("Damage Effectiveness:  #%"))
                skillDmgEffectiveness = spellSkill.Properties["Damage Effectiveness:  #%"][0] / 100f;
            float coldEffectiveness = 1;
            float lightEffectiveness = 1;
            float fireEffectiveness = 1;
            if (spellSkill.Keywords.Contains("Cold"))
            {
                fireEffectiveness = skillDmgEffectiveness;
                lightEffectiveness = skillDmgEffectiveness;
            }
            else if (spellSkill.Keywords.Contains("Fire"))
            {
                coldEffectiveness = skillDmgEffectiveness;
                lightEffectiveness = skillDmgEffectiveness;
            }
            else if (spellSkill.Keywords.Contains("Lightning"))
            {
                fireEffectiveness = skillDmgEffectiveness;
                coldEffectiveness = skillDmgEffectiveness;
            }
            //Cast Speed
            float skillLessCS = 1 - (spellSkill.Mods.ContainsKey("#% less Cast Speed") ? spellSkill.Mods["#% less Cast Speed"][0] / 100f : 0f);
            float skillMoreCS = 1 + (spellSkill.Mods.ContainsKey("#% more Cast Speed") ? spellSkill.Mods["#% more Cast Speed"][0] / 100f : 0f);
            //Crit
            float lessCrit = 1 - (attribs.ContainsKey("#% less Critical Strike Chance") ? attribs["#% less Critical Strike Chance"][0] / 100f : 0f); //because I did not consider it for spell crit chance in updatecritical()
            #endregion

            //fire conversion
            float coldToFire = attribs.ContainsKey("#% of Cold Damage Converted to Fire Damage") ? attribs["#% of Cold Damage Converted to Fire Damage"][0] : 0f; //Avatar of Fire
            float lightToFire = attribs.ContainsKey("#% of Lightning Damage Converted to Fire Damage") ? attribs["#% of Lightning Damage Converted to Fire Damage"][0] : 0f; //Avatar of Fire
            float physicalToFire = attribs.ContainsKey("#% of Physical Damage Converted to Fire Damage") ? attribs["#% of Physical Damage Converted to Fire Damage"][0] : 0f; //Avatar of Fire
            //cold conversion
            float physicalToCold = attribs.ContainsKey("#% of Physical Damage Converted to Cold Damage") ? attribs["#% of Physical Damage Converted to Cold Damage"][0] : 0f;
            //light conversion
            float physicalToLight = attribs.ContainsKey("#% of Physical Damage Converted to Lightning Damage") ? attribs["#% of Physical Damage Converted to Lightning Damage"][0] : 0f;
            //chaos conversion
            float physicalToChaos = attribs.ContainsKey("#% of Physical Damage Converted to Chaos Damage") ? attribs["#% of Physical Damage Converted to Damage Damage"][0] : 0f;
            float lightToChaos = attribs.ContainsKey("#% of Lightning Damage Converted to Chaos Damage") ? attribs["#% of Lightning Damage Converted to Chaos Damage"][0] : 0f;
            float fireToChaos = attribs.ContainsKey("#% of Fire Damage Converted to Chaos Damage") ? attribs["#% of Fire Damage Converted to Chaos Damage"][0] : 0f;
            float extraChaosDmg = attribs.ContainsKey("Gain #% of Physical Damage as Extra Chaos Damage") ? attribs["Gain #% of Physical Damage as Extra Chaos Damage"][0] : 0f; //Unique ring

            float extraFireDmg = 0;
            float skillIncreasedFireDmg = 0;
            float skillIncreasedColdDmg = 0;
            float skillIncreasedChaosDmg = 0;
            float skillMinColdDmg = spellSkill.Mods.ContainsKey("Deals #-# Cold Damage") ? spellSkill.Mods["Deals #-# Cold Damage"][0] : 0f;
            float skillMaxColdDmg = spellSkill.Mods.ContainsKey("Deals #-# Cold Damage") ? spellSkill.Mods["Deals #-# Cold Damage"][1] : 0f;
            float skillMinChaosDmg = spellSkill.Mods.ContainsKey("Deals #-# Chaos Damage") ? spellSkill.Mods["Deals #-# Chaos Damage"][0] : 0f;
            float skillMaxChaosDmg = spellSkill.Mods.ContainsKey("Deals #-# Chaos Damage") ? spellSkill.Mods["Deals #-# Chaos Damage"][1] : 0f;
            float skillMinLightDmg = spellSkill.Mods.ContainsKey("Deals #-# Lightning Damage") ? spellSkill.Mods["Deals #-# Lightning Damage"][0] : 0f;
            float skillMaxLightDmg = spellSkill.Mods.ContainsKey("Deals #-# Lightning Damage") ? spellSkill.Mods["Deals #-# Lightning Damage"][1] : 0f;
            float skillMinFireDmg = spellSkill.Mods.ContainsKey("Deals #-# Fire Damage") ? spellSkill.Mods["Deals #-# Fire Damage"][0] : 0f;
            float skillMaxFireDmg = spellSkill.Mods.ContainsKey("Deals #-# Fire Damage") ? spellSkill.Mods["Deals #-# Fire Damage"][1] : 0f;
            float skillMinPhysDmg = spellSkill.Mods.ContainsKey("Deals #-# Physical Damage") ? spellSkill.Mods["Deals #-# Physical Damage"][0] : 0f;
            float skillMaxPhysDmg = spellSkill.Mods.ContainsKey("Deals #-# Physical Damage") ? spellSkill.Mods["Deals #-# Physical Damage"][1] : 0f;
            float skillFireDoT = spellSkill.Mods.ContainsKey("Deals # Base Fire Damage per second") ? spellSkill.Mods["Deals # Base Fire Damage per second"][0] : 0f;
            float skillIncCritMult = 0;
            float lessSpellDmg = 1;
            float lessDamage = 1;
            float skillIncreasedSpellDmg = 0;

            //Soul Mantle
            foreach (var item in itemData.EquippedItems)
            {
                if (item.Name == "Soul Mantle" && item.Gems.Contains(spellSkill))//if skill gem socketed in soul mantle
                {
                    lessDamage *= 0.5f;
                    skillReducedCS += 30;//Spells Cast by Totems have 4% increased Cast Speed
                    skillIncreasedCS += attribs.ContainsKey("Spells Cast by Totems have #% increased Cast Speed") ? attribs["Spells Cast by Totems have #% increased Cast Speed"][0] : 0f;
                    skillIncreasedDmg += attribs.ContainsKey("#% increased Totem Damage") ? attribs["#% increased Totem Damage"][0] : 0f;
                    break;
                }
            }

            #region SupportGems
            foreach (SupportGem supportGem in SupportGems)
            {
                if (supportGem.Selected)//be careful of that two similar supports wont stack
                {
                    //extera fire
                    extraFireDmg += supportGem.Mods.ContainsKey("Gain #% of Physical Damage as Extra Fire Damage") ? supportGem.Mods["Gain #% of Physical Damage as Extra Fire Damage"][0] : 0f;
                    skillIncreasedFireDmg += supportGem.Mods.ContainsKey("#% increased Fire Damage") ? supportGem.Mods["#% increased Fire Damage"][0] : 0f;
                    //cold to fire
                    skillIncreasedColdDmg += supportGem.Mods.ContainsKey("#% increased Cold Damage") ? supportGem.Mods["#% increased Cold Damage"][0] : 0f;
                    coldToFire += supportGem.Mods.ContainsKey("#% of Cold Damage Converted to Fire Damage") ? supportGem.Mods["#% of Cold Damage Converted to Fire Damage"][0] : 0f;
                    //Iron Will
                    skillReducedCS += supportGem.Mods.ContainsKey("#% reduced Cast Speed") ? supportGem.Mods["#% reduced Cast Speed"][0] : 0f;
                    if (supportGem.Mods.ContainsKey("Strength's damage bonus applies to Spell Damage as well for Supported Skills"))
                        skillIncreasedSpellDmg = Strength * MDmgPerStr;

                    lessDamage *= (1 - (supportGem.Mods.ContainsKey("#% less Damage to main target") ? supportGem.Mods["#% less Damage to main target"][0] / 100f : 0f));
                    //Multistrike x% more Attack Speed
                    lessSpellDmg *= (1 - (supportGem.Mods.ContainsKey("#% less Spell Damage") ? supportGem.Mods["#% less Spell Damage"][0] / 100f : 0f));
                    skillMoreCS *= (1 + (supportGem.Mods.ContainsKey("#% more Cast Speed") ? supportGem.Mods["#% more Cast Speed"][0] / 100f : 0f));
                    //Increased burning damage
                    skillIncreasedBurningDmg += supportGem.Mods.ContainsKey("#% increased Burning Damage") ? supportGem.Mods["#% increased Burning Damage"][0] : 0f;
                    //Added Cold Damage
                    skillMinColdDmg += supportGem.Mods.ContainsKey("Adds #-# Cold Damage") ? supportGem.Mods["Adds #-# Cold Damage"][0] : 0f;
                    skillMaxColdDmg += supportGem.Mods.ContainsKey("Adds #-# Cold Damage") ? supportGem.Mods["Adds #-# Cold Damage"][1] : 0f;
                    //Added Chaos Damage
                    skillMinChaosDmg += supportGem.Mods.ContainsKey("Adds #-# Chaos Damage") ? supportGem.Mods["Adds #-# Chaos Damage"][0] : 0f;
                    skillMaxChaosDmg += supportGem.Mods.ContainsKey("Adds #-# Chaos Damage") ? supportGem.Mods["Adds #-# Chaos Damage"][1] : 0f;
                    skillIncreasedChaosDmg += supportGem.Mods.ContainsKey("#% increased Chaos Damage") ? supportGem.Mods["#% increased Chaos Damage"][0] : 0f;
                    //Added Lightning Damage
                    skillMinLightDmg += supportGem.Mods.ContainsKey("Adds #-# Lightning Damage") ? supportGem.Mods["Adds #-# Lightning Damage"][0] : 0f;
                    skillMaxLightDmg += supportGem.Mods.ContainsKey("Adds #-# Lightning Damage") ? supportGem.Mods["Adds #-# Lightning Damage"][1] : 0f;
                    //increased Critical Strike Chance
                    if (supportGem.Mods.ContainsKey("#% increased Critical Strike Chance"))
                        skillIncreasedCrit += supportGem.Mods["#% increased Critical Strike Chance"][0];
                    //Chain 
                    lessDamage *= (1 - (supportGem.Mods.ContainsKey("#% less Damage") ? supportGem.Mods["#% less Damage"][0] / 100f : 0f));
                    skillIncreasedDmg += supportGem.Mods.ContainsKey("#% increased Damage") ? supportGem.Mods["#% increased Damage"][0] : 0f;
                    //cast when damage taken
                    lessDamage *= (1 + (supportGem.Mods.ContainsKey("#% more Damage") ? supportGem.Mods["#% more Damage"][0] / 100f : 0f));
                    //Culling Strike
                    skillIncreasedCS += supportGem.Mods.ContainsKey("#% increased Cast Speed") ? supportGem.Mods["#% increased Cast Speed"][0] : 0f;
                    //Spell Totem
                    if (spellSkill.GemType == GemType.Totem)
                    {
                        lessDamage *= (1 - (supportGem.Mods.ContainsKey("Totem deals #% less Damage") ? supportGem.Mods["Totem deals #% less Damage"][0] / 100f : 0f));
                    }
                    //Faster  projectiles 
                    if (spellSkill.Keywords.Contains("Projectile")) // it also applies to damages added by supports
                        skillIncreasedDmg += supportGem.Mods.ContainsKey("#% increased Projectile Damage") ? supportGem.Mods["#% increased Projectile Damage"][0] : 0f;
                    //Fork, Lesser Multiple Projectiles
                    if (spellSkill.Keywords.Contains("Projectile"))
                        lessDamage *= (1 - (supportGem.Mods.ContainsKey("#% less Projectile Damage") ? supportGem.Mods["#% less Projectile Damage"][0] / 100f : 0f));
                    //Concentrated Effect
                    if (spellSkill.Keywords.Contains("AoE") && !spellSkill.Keywords.Contains("Projectile"))
                        lessDamage *= (1 + (supportGem.Mods.ContainsKey("#% more Area Damage") ? supportGem.Mods["#% more Area Damage"][0] / 100f : 0f));
                    //Increased Critical Damage 
                    if (supportGem.Mods.ContainsKey("#% increased Critical Strike Multiplier"))
                        skillIncCritMult = supportGem.Mods["#% increased Critical Strike Multiplier"][0];
                }
            }
            UpdateCriticalStrike(skillIncreasedCrit, skillIncCritMult);
            #endregion
            float increasedPD = attribs.ContainsKey("#% increased Physical Damage") ? attribs["#% increased Physical Damage"][0] : 0f;
            increasedPD += OakPhysicalDamage;
            float increasedSpellDmg = attribs.ContainsKey("#% increased Spell Damage") ? attribs["#% increased Spell Damage"][0] : 0f;
            increasedSpellDmg += attribs.ContainsKey("#% increased Spell Damage per #% Block Chance") ? attribs["#% increased Spell Damage per #% Block Chance"][0] * (ChanceToBlock / attribs["#% increased Spell Damage per #% Block Chance"][1]) : 0f;
            increasedSpellDmg += attribs.ContainsKey("#% increased Spell Damage per Power Charge") ? attribs["#% increased Spell Damage per Power Charge"][0] * CurrentPower : 0f;
            increasedSpellDmg -= attribs.ContainsKey("#% reduced Spell Damage") ? attribs["#% reduced Spell Damage"][0] : 0f;
            increasedSpellDmg += skillIncreasedSpellDmg;
            skillIncreasedChaosDmg += increasedSpellDmg;
            increasedPD += increasedSpellDmg;
            if (attribs.ContainsKey("#% increased Area Damage") && spellSkill.Keywords.Contains("AoE") && !spellSkill.Keywords.Contains("Projectile"))
                increasedSpellDmg += attribs["#% increased Area Damage"][0];
            increasedSpellDmg += IncreasedElementalDamageSpells;
            
            #region Elemental Damages
            //cold                                                                                                                                                                        
            SpellMinColdD = (skillMinColdDmg) * (1 + (increasedSpellDmg + IncreasedColdDamage + skillIncreasedColdDmg + skillIncreasedDmg - reducedDamage) / 100f);
            SpellMaxColdD = (skillMaxColdDmg) * (1 + (increasedSpellDmg + IncreasedColdDamage + skillIncreasedColdDmg + skillIncreasedDmg - reducedDamage) / 100f);
            SpellMinColdD += skillMinPhysDmg * (1 + (increasedPD + skillIncreasedDmg + skillIncreasedColdDmg + IncreasedColdDamage - reducedDamage) / 100f) * (HatredColdD * (1 + increasedAuraEff / 100f) + physicalToCold) / 100f;
            SpellMaxColdD += skillMaxPhysDmg * (1 + (increasedPD + skillIncreasedDmg + skillIncreasedColdDmg + IncreasedColdDamage - reducedDamage) / 100f) * (HatredColdD * (1 + increasedAuraEff / 100f) + physicalToCold) / 100f;
            SpellMinColdD = SpellMinColdD * lessDamage * lessSpellDmg * coldEffectiveness;
            SpellMaxColdD = SpellMaxColdD * lessDamage * lessSpellDmg * coldEffectiveness;
            float coldToFireMin = SpellMinColdD * coldToFire / 100f;
            float coldToFireMax = SpellMaxColdD * coldToFire / 100f;
            SpellMinColdD -= coldToFireMin;
            SpellMaxColdD -= coldToFireMax;
            //light
            SpellMinLightD = (skillMinLightDmg) * (1 + (increasedSpellDmg + IncreasedLightningDamage + skillIncreasedDmg - reducedDamage) / 100f);
            SpellMaxLightD = (skillMaxLightDmg) * (1 + (increasedSpellDmg + IncreasedLightningDamage + skillIncreasedDmg - reducedDamage) / 100f);
            SpellMinLightD += skillMinPhysDmg * (1 + (increasedPD + skillIncreasedDmg + IncreasedLightningDamage - reducedDamage) / 100f) * (physicalToLight) / 100f;
            SpellMaxLightD += skillMaxPhysDmg * (1 + (increasedPD + skillIncreasedDmg + IncreasedLightningDamage - reducedDamage) / 100f) * (physicalToLight) / 100f;
            SpellMinLightD = SpellMinLightD * lessDamage * lessSpellDmg * lightEffectiveness;
            SpellMaxLightD = SpellMaxLightD * lessDamage * lessSpellDmg * lightEffectiveness;
            float lightToFireMin = SpellMinLightD * lightToFire / 100f;
            float lightToFireMax = SpellMaxLightD * lightToFire / 100f;
            float lightToChaosMin = SpellMinLightD * (lightToChaos - lightToFire) / 100f; //which one takes preceedence?
            float lightToChaosMax = SpellMaxLightD * (lightToChaos - lightToFire) / 100f;
            SpellMinLightD -= (lightToFireMin + lightToChaosMin);
            SpellMaxLightD -= (lightToFireMax + lightToChaosMax);
            //fire
            SpellMinFireD = (skillMinFireDmg) * (1 + (increasedSpellDmg + IncreasedFireDamage + skillIncreasedFireDmg + skillIncreasedDmg - reducedDamage) / 100f);
            SpellMaxFireD = (skillMaxFireDmg) * (1 + (increasedSpellDmg + IncreasedFireDamage + skillIncreasedFireDmg + skillIncreasedDmg - reducedDamage) / 100f);
            SpellMinFireD += skillMinPhysDmg * (1 + (increasedPD + skillIncreasedDmg + skillIncreasedFireDmg + IncreasedFireDamage - reducedDamage) / 100f) * (physicalToFire + extraFireDmg) / 100f;
            SpellMaxFireD += skillMaxPhysDmg * (1 + (increasedPD + skillIncreasedDmg + skillIncreasedFireDmg + IncreasedFireDamage - reducedDamage) / 100f) * (physicalToFire + extraFireDmg) / 100f;
            SpellMinFireD = SpellMinFireD * lessDamage * lessSpellDmg * fireEffectiveness;
            SpellMaxFireD = SpellMaxFireD * lessDamage * lessSpellDmg * fireEffectiveness;
            float fireToChaosMin = SpellMinFireD * fireToChaos / 100f;
            float fireToChaosMax = SpellMaxFireD * fireToChaos / 100f;
            SpellMinFireD += (coldToFireMin + lightToFireMin - fireToChaosMin); //conversion from others comes last
            SpellMaxFireD += (coldToFireMax + lightToFireMax - fireToChaosMax);
            //fire DoT
            SpellSkillDoT = skillFireDoT * (1 + (skillIncreasedBurningDmg + IncreasedFireDamage + skillIncreasedFireDmg + skillIncreasedDmg - reducedDamage) / 100f);
            SpellSkillDoT = SpellSkillDoT * lessDamage * fireEffectiveness;
            if (SpellSkillDoT > 0)
                DotVisibility = System.Windows.Visibility.Visible;
            else
                DotVisibility = System.Windows.Visibility.Collapsed;
            //chaos
            SpellMinChaosD = (skillMinChaosDmg) * (1 + (IncreasedChaosDamage + skillIncreasedChaosDmg + skillIncreasedDmg - reducedDamage) / 100f);
            SpellMaxChaosD = (skillMaxChaosDmg) * (1 + (IncreasedChaosDamage + skillIncreasedChaosDmg + skillIncreasedDmg - reducedDamage) / 100f);
            SpellMinChaosD += skillMinPhysDmg * (1 + (increasedPD + skillIncreasedDmg + skillIncreasedChaosDmg + IncreasedChaosDamage - reducedDamage) / 100f) * (physicalToChaos + extraChaosDmg) / 100f;
            SpellMaxChaosD += skillMaxPhysDmg * (1 + (increasedPD + skillIncreasedDmg + skillIncreasedChaosDmg + IncreasedChaosDamage - reducedDamage) / 100f) * (physicalToChaos + extraChaosDmg) / 100f;
            SpellMinChaosD = SpellMinChaosD * lessDamage * lessSpellDmg * skillDmgEffectiveness;
            SpellMaxChaosD = SpellMaxChaosD * lessDamage * lessSpellDmg * skillDmgEffectiveness;
            SpellMinChaosD += (lightToChaosMin + fireToChaosMin);
            SpellMaxChaosD += (lightToChaosMax + fireToChaosMax);
            //physical
            SpellMinPD = skillMinPhysDmg * (1 + (increasedPD + skillIncreasedDmg - reducedDamage) / 100f) * skillDmgEffectiveness * lessDamage * (1 - (physicalToFire + physicalToCold + physicalToLight + physicalToChaos) / 100f);
            SpellMaxPD = skillMaxPhysDmg * (1 + (increasedPD + skillIncreasedDmg - reducedDamage) / 100f) * skillDmgEffectiveness * lessDamage * (1 - (physicalToFire + physicalToCold + physicalToLight + physicalToChaos) / 100f);
            float totalSpellDmg = 0;
            if (!attribs.ContainsKey("Deal no Non-Fire Damage"))
                totalSpellDmg = (SpellMinColdD + SpellMinFireD + SpellMinLightD + SpellMinChaosD + SpellMaxColdD + SpellMaxFireD + SpellMaxLightD + SpellMaxChaosD + SpellMinPD + SpellMaxPD) / 2f;
            else
                totalSpellDmg = (SpellMinFireD + SpellMaxFireD) / 2f;
            #endregion
            float spellCritDmg = totalSpellDmg * CritMultiplierSpell / 100f;
            float nonCritDmg = totalSpellDmg * (attribs.ContainsKey("Non-critical strikes deal #% Damage") ? attribs["Non-critical strikes deal #% Damage"][0] / 100f : 1); //for unique
            CritChanceSpell = spellSkill.Properties.ContainsKey("Critical Strike Chance:  #%") ? spellSkill.Properties["Critical Strike Chance:  #%"][0] * (1 + increasedCritSpell / 100f) * lessCrit : 0;
            float spellDamageTot = (spellCritDmg * CritChanceSpell / 100 + nonCritDmg * (1 - CritChanceSpell / 100));
            float castTime = spellSkill.CastTime / ((1 + (IncreasedCastSpeed + skillIncreasedCS - skillReducedCS) / 100f) * skillMoreCS * skillLessCS);
            CastTime = castTime;
            CastsPerSecond = 1f / castTime;
            SpellSkillDPS = spellDamageTot * CastsPerSecond;
        }

        protected void CalcAttackSkillDPS(ActiveGem attackSkill)
        {
            float damageMultiplier = (1 - (attribs.ContainsKey("#% less Weapon Damage") ? attribs["#% less Weapon Damage"][0] / 100f : 0f)); //for unique bow
            float reducedDamage = attribs.ContainsKey("#% reduced Damage") ? attribs["#% reduced Damage"][0] / 2f : 0f; //Leer Cast unique
            reducedDamage -= attribs.ContainsKey("#% increased Damage") ? attribs["#% increased Damage"][0] : 0f; //Le Heup of All

            float skillIncreasedPDmg = attackSkill.Mods.ContainsKey("#% increased Physical Damage") ? attackSkill.Mods["#% increased Physical Damage"][0] : 0f;
            skillIncreasedPDmg += attackSkill.Mods.ContainsKey("#% Increased Physical Damage per Frenzy Charge") ? attackSkill.Mods["#% Increased Physical Damage per Frenzy Charge"][0] * CurrentFrenzy : 0f; //Starts with capital 'I' !!! frenzy skill
            float skillIncreasedProjDmg = attackSkill.Mods.ContainsKey("#% increased Projectile Damage") ? attackSkill.Mods["#% increased Projectile Damage"][0] : 0f;
            float skillPhysDmgEffectiveness = attackSkill.Mods.ContainsKey("Deals #% of Base Physical Damage") ? attackSkill.Mods["Deals #% of Base Physical Damage"][0] / 100f : 1f; //two possible terms
            float skillDmgEffectiveness = attackSkill.Mods.ContainsKey("Deals #% of Base Damage") ? attackSkill.Mods["Deals #% of Base Damage"][0] / 100f : 1f; //two possible terms
            float skillReducedAs = attackSkill.Mods.ContainsKey("#% reduced Attack Speed") ? attackSkill.Mods["#% reduced Attack Speed"][0] : 0f;
            float skillIncreasedAs = attackSkill.Mods.ContainsKey("#% increased Attack Speed") ? attackSkill.Mods["#% increased Attack Speed"][0] : 0f;
            skillIncreasedAs += attackSkill.Mods.ContainsKey("#% increased Attack Speed per Frenzy Charge") ? attackSkill.Mods["#% increased Attack Speed per Frenzy Charge"][0] * CurrentFrenzy : 0f;
            float skillLessAs = 1 - (attackSkill.Mods.ContainsKey("#% less Attack Speed") ? attackSkill.Mods["#% less Attack Speed"][0] / 100f : 0f);
            float skillMoreAs = 1 + (attackSkill.Mods.ContainsKey("#% more Attack Speed") ? attackSkill.Mods["#% more Attack Speed"][0] / 100f : 0f);
            float skillIncreasedCrit = 0;
            if (attackSkill.Mods.ContainsKey("#% increased Critical Strike Chance"))
                skillIncreasedCrit = attackSkill.Mods["#% increased Critical Strike Chance"][0];

            //fire conversion
            float coldToFire = attribs.ContainsKey("#% of Cold Damage Converted to Fire Damage") ? attribs["#% of Cold Damage Converted to Fire Damage"][0] : 0f; //Avatar of Fire
            float lightToFire = attribs.ContainsKey("#% of Lightning Damage Converted to Fire Damage") ? attribs["#% of Lightning Damage Converted to Fire Damage"][0] : 0f; //Avatar of Fire
            float physicalToFire = attribs.ContainsKey("#% of Physical Damage Converted to Fire Damage") ? attribs["#% of Physical Damage Converted to Fire Damage"][0] : 0f; //Avatar of Fire
            physicalToFire += attackSkill.Mods.ContainsKey("#% of Physical Damage Converted to Fire Damage") ? attackSkill.Mods["#% of Physical Damage Converted to Fire Damage"][0] : 0f; //InfernalBlow
            float extraFireDmg = attackSkill.Mods.ContainsKey("Gain #% of Physical Damage as Extra Fire Damage") ? attackSkill.Mods["Gain #% of Physical Damage as Extra Fire Damage"][0] : 0f; //Burning Arrow
            //cold conversion
            float physicalToCold = attribs.ContainsKey("#% of Physical Damage Converted to Cold Damage") ? attribs["#% of Physical Damage Converted to Cold Damage"][0] : 0f;
            physicalToCold += attackSkill.Mods.ContainsKey("#% of Physical Damage Converted to Cold Damage") ? attackSkill.Mods["#% of Physical Damage Converted to Cold Damage"][0] : 0f;//Glacial Hammer
            //light conversion
            float physicalToLight = attribs.ContainsKey("#% of Physical Damage Converted to Lightning Damage") ? attribs["#% of Physical Damage Converted to Lightning Damage"][0] : 0f;
            physicalToLight += attackSkill.Mods.ContainsKey("#% of Physical Damage Converted to Lightning Damage") ? attackSkill.Mods["#% of Physical Damage Converted to Lightning Damage"][0] : 0f;///Lightning Strike
            //chaos conversion
            float physicalToChaos = attribs.ContainsKey("#% of Physical Damage Converted to Chaos Damage") ? attribs["#% of Physical Damage Converted to Damage Damage"][0] : 0f;
            physicalToChaos += attackSkill.Mods.ContainsKey("#% of Physical Damage Converted to Chaos Damage") ? attackSkill.Mods["#% of Physical Damage Converted to Chaos Damage"][0] : 0f;//nothing atm
            float lightToChaos = attribs.ContainsKey("#% of Lightning Damage Converted to Chaos Damage") ? attribs["#% of Lightning Damage Converted to Chaos Damage"][0] : 0f;
            float fireToChaos = attribs.ContainsKey("#% of Fire Damage Converted to Chaos Damage") ? attribs["#% of Fire Damage Converted to Chaos Damage"][0] : 0f;
            float extraChaosDmg = attribs.ContainsKey("Gain #% of Physical Damage as Extra Chaos Damage") ? attribs["Gain #% of Physical Damage as Extra Chaos Damage"][0] : 0f; //Unique ring
            #region SupportGems

            float skillMoreMeleePD = 1;
            float skillMoreProjPD = 1;
            float moreWepEleDmg = 0;
            float skillIncreasedMeleeDmg = 0;
            float skillIncreasedFireDmg = 0;
            float skillIncreasedColdDmg = 0;
            float skillIncreasedLightDmg = 0;
            float skillIncreasedChaosDmg = 0;
            float skillIncreasedEleDmg = 0;
            float skillMinColdDmg = 0;
            float skillMaxColdDmg = 0;
            float skillMinChaosDmg = 0;
            float skillMaxChaosDmg = 0;
            float skillMinLightningDmg = 0;
            float skillMaxLightningDmg = 0;
            float skillToAccuracyRating = 0;
            //float skillMoreAreaDmg = 0;
            float skillIncCritMult = 0;

            if (attackSkill.Name != "Default Attack")
                foreach (SupportGem supportGem in SupportGems)
                {
                    if (supportGem.Selected)//be careful of that two similar supports wont stack
                    {
                        //extera fire
                        extraFireDmg += supportGem.Mods.ContainsKey("Gain #% of Physical Damage as Extra Fire Damage") ? supportGem.Mods["Gain #% of Physical Damage as Extra Fire Damage"][0] : 0f;
                        skillIncreasedFireDmg += supportGem.Mods.ContainsKey("#% increased Fire Damage") ? supportGem.Mods["#% increased Fire Damage"][0] : 0f;
                        //cold to fire
                        skillIncreasedColdDmg += supportGem.Mods.ContainsKey("#% increased Cold Damage") ? supportGem.Mods["#% increased Cold Damage"][0] : 0f;
                        coldToFire += supportGem.Mods.ContainsKey("#% of Cold Damage Converted to Fire Damage") ? supportGem.Mods["#% of Cold Damage Converted to Fire Damage"][0] : 0f;
                        //Iron grip
                        skillReducedAs += supportGem.Mods.ContainsKey("#% reduced Attack Speed") ? supportGem.Mods["#% reduced Attack Speed"][0] : 0f;
                        if (attackSkill.Keywords.Contains("Projectile") && !attackSkill.Name.Contains("Lightning Strike")) //lightning strike projectiles damage are not calculated
                            if (!attribs.ContainsKey("The increase to Physical Damage from Strength applies to Projectile Attacks as well as Melee Attacks") && supportGem.Mods.ContainsKey("Strength's damage bonus applies to Projectile Attacks made with Supported Skills"))
                            {
                                increasedProjDMH += Strength * MDmgPerStr; //this support works exactly as passive skill, therefor it wont increase non physical damages like other increase projectile damage supports.
                                increasedProjDOH += Strength * MDmgPerStr; //Therefore add it to increasedProjDMH not skillincreasedproj
                            }
                        //Melee Damage on Full Life 
                        skillMoreMeleePD *= (1 + (supportGem.Mods.ContainsKey("#% more Melee Physical Damage when on Full Life") ? supportGem.Mods["#% more Melee Physical Damage when on Full Life"][0] / 100f : 0f));
                        //Melee Splash  (0 to 57)% increased Melee Physical Damage  16% less Damage to main target
                        skillIncreasedMeleeDmg += supportGem.Mods.ContainsKey("#% increased Melee Physical Damage") ? supportGem.Mods["#% increased Melee Physical Damage"][0] : 0f;
                        damageMultiplier *= (1 - (supportGem.Mods.ContainsKey("#% less Damage to main target") ? supportGem.Mods["#% less Damage to main target"][0] / 100f : 0f));
                        //Multistrike x% more Attack Speed
                        damageMultiplier *= (1 - (supportGem.Mods.ContainsKey("#% less Attack Damage") ? supportGem.Mods["#% less Attack Damage"][0] / 100f : 0f));
                        if (attackSkill.Keywords.Contains("Melee"))
                            skillMoreAs *= (1 + (supportGem.Mods.ContainsKey("#% more Melee Attack Speed") ? supportGem.Mods["#% more Melee Attack Speed"][0] / 100f : 0f));
                        //Weapon Elemental Damage 
                        moreWepEleDmg += supportGem.Mods.ContainsKey("#% more Weapon Elemental Damage") ? supportGem.Mods["#% more Weapon Elemental Damage"][0] : 0f;
                        skillIncreasedEleDmg += supportGem.Mods.ContainsKey("#% increased Elemental Damage with Weapons") ? supportGem.Mods["#% increased Elemental Damage with Weapons"][0] : 0f;
                        //Melee Physical Damage 
                        skillMoreMeleePD *= (1 + (supportGem.Mods.ContainsKey("#% more Melee Physical Damage") ? supportGem.Mods["#% more Melee Physical Damage"][0] / 100f : 0f));
                        //Added Cold Damage
                        skillMinColdDmg += supportGem.Mods.ContainsKey("Adds #-# Cold Damage") ? supportGem.Mods["Adds #-# Cold Damage"][0] : 0f;
                        skillMaxColdDmg += supportGem.Mods.ContainsKey("Adds #-# Cold Damage") ? supportGem.Mods["Adds #-# Cold Damage"][1] : 0f;
                        //Added Chaos Damage
                        skillMinChaosDmg += supportGem.Mods.ContainsKey("Adds #-# Chaos Damage") ? supportGem.Mods["Adds #-# Chaos Damage"][0] : 0f;
                        skillMaxChaosDmg += supportGem.Mods.ContainsKey("Adds #-# Chaos Damage") ? supportGem.Mods["Adds #-# Chaos Damage"][1] : 0f;
                        skillIncreasedChaosDmg += supportGem.Mods.ContainsKey("#% increased Chaos Damage") ? supportGem.Mods["#% increased Chaos Damage"][0] : 0f;
                        //Added Lightning Damage
                        skillMinLightningDmg += supportGem.Mods.ContainsKey("Adds #-# Lightning Damage") ? supportGem.Mods["Adds #-# Lightning Damage"][0] : 0f;
                        skillMaxLightningDmg += supportGem.Mods.ContainsKey("Adds #-# Lightning Damage") ? supportGem.Mods["Adds #-# Lightning Damage"][1] : 0f;
                        skillIncreasedLightDmg += supportGem.Mods.ContainsKey("#% increased Lightning Damage") ? supportGem.Mods["#% increased Lightning Damage"][0] : 0f;
                        //Additional Accuracy 2% increased Critical Strike Chance
                        if (supportGem.Mods.ContainsKey("+# to Accuracy Rating"))
                            skillToAccuracyRating = supportGem.Mods["+# to Accuracy Rating"][0];
                        if (supportGem.Mods.ContainsKey("#% increased Critical Strike Chance"))
                            skillIncreasedCrit += supportGem.Mods["#% increased Critical Strike Chance"][0];
                        //Chain 
                        damageMultiplier *= (1 - (supportGem.Mods.ContainsKey("#% less Damage") ? supportGem.Mods["#% less Damage"][0] / 100f : 0f));
                        reducedDamage -= supportGem.Mods.ContainsKey("#% increased Damage") ? supportGem.Mods["#% increased Damage"][0] : 0f; //this needs more testing. it might affect elemental damages also
                        //Culling Strike
                        skillIncreasedAs += supportGem.Mods.ContainsKey("#% increased Attack Speed") ? supportGem.Mods["#% increased Attack Speed"][0] : 0f;
                        //Faster  projectiles 
                        if (attackSkill.Keywords.Contains("Projectile") && !attackSkill.Name.Contains("Lightning Strike"))
                        {
                            skillIncreasedProjDmg += supportGem.Mods.ContainsKey("#% increased Projectile Damage") ? supportGem.Mods["#% increased Projectile Damage"][0] : 0f;
                            //Fork, Lesser Multiple Projectiles
                            damageMultiplier *= (1 - (supportGem.Mods.ContainsKey("#% less Projectile Damage") ? supportGem.Mods["#% less Projectile Damage"][0] / 100f : 0f));
                            //Physical Projectile Attack Damage
                            skillLessAs *= (1 - (supportGem.Mods.ContainsKey("#% less Projectile Attack Speed") ? supportGem.Mods["#% less Projectile Attack Speed"][0] / 100f : 0f));
                            skillMoreProjPD *= (1 + (supportGem.Mods.ContainsKey("#% more Physical Projectile Attack Damage") ? supportGem.Mods["#% more Physical Projectile Attack Damage"][0] / 100f : 0f)); //since we can only have melee or projectile, this is o.k.
                        }
                        ////Concentrated Effect
                        if (attackSkill.Keywords.Contains("AoE") && (attackSkill.Name.Contains("Cleave") || attackSkill.Name.Contains("Reave") || attackSkill.Name.Contains("Sweep") || attackSkill.Name.Contains("Rain of Arrows")))
                            damageMultiplier *= (1 + (supportGem.Mods.ContainsKey("#% more Area Damage") ? supportGem.Mods["#% more Area Damage"][0] / 100f : 0f));
                        //Increased Critical Damage 
                        if (supportGem.Mods.ContainsKey("#% increased Critical Strike Multiplier"))
                            skillIncCritMult = supportGem.Mods["#% increased Critical Strike Multiplier"][0];
                    }
                }
            SkillToAccuracyRating(skillToAccuracyRating);
            UpdateCriticalStrike(skillIncreasedCrit, skillIncCritMult);
            #endregion
            float increasedPdMH_Total = 0;
            float increasedPdOH_Total = 0;
            float physDmgMulti_Total = 1;
            if (attackSkill.Keywords.Contains("Projectile") && !attackSkill.Name.Contains("Lightning Strike"))
            {
                increasedPdMH_Total = increasedProjDMH + skillIncreasedPDmg + skillIncreasedProjDmg;
                increasedPdOH_Total = increasedProjDOH + skillIncreasedPDmg + skillIncreasedProjDmg;
                physDmgMulti_Total = skillPhysDmgEffectiveness * skillMoreProjPD;
            }
            else
            {
                increasedPdMH_Total = increasedMeleePdMH + skillIncreasedPDmg + skillIncreasedMeleeDmg;
                increasedPdOH_Total = increasedMeleePdOH + skillIncreasedPDmg + skillIncreasedMeleeDmg;
                physDmgMulti_Total = skillPhysDmgEffectiveness * skillMoreMeleePD;
            }
            #region Elemental Damages
            //Main Hand
            //cold                                                                                                                                                                                         
            MainHandMinColdD = (MainHandMinColdD_Tot + skillMinColdDmg) * (1 + (increasedWeaponColdD + skillIncreasedColdDmg + increasedEdMainHand + skillIncreasedEleDmg + skillIncreasedProjDmg - reducedDamage) / 100f); //increased projectile damage from skill gems affects elemental damages too.
            MainHandMaxColdD = (MainHandMaxColdD_Tot + skillMaxColdDmg) * (1 + (increasedWeaponColdD + skillIncreasedColdDmg + increasedEdMainHand + skillIncreasedEleDmg + skillIncreasedProjDmg - reducedDamage) / 100f);
            MainHandMinColdD += mainHandMinDamage * (1 + (increasedPdMH_Total + skillIncreasedColdDmg + increasedWeaponColdD + increasedEdMainHand + skillIncreasedEleDmg - reducedDamage) / 100f) * physDmgMulti_Total * (HatredColdD * (1 + increasedAuraEff / 100f) + physicalToCold) / 100f;
            MainHandMaxColdD += mainHandMaxDamage * (1 + (increasedPdMH_Total + skillIncreasedColdDmg + increasedWeaponColdD + increasedEdMainHand + skillIncreasedEleDmg - reducedDamage) / 100f) * physDmgMulti_Total * (HatredColdD * (1 + increasedAuraEff / 100f) + physicalToCold) / 100f;
            MainHandMinColdD = MainHandMinColdD * damageMultiplier * skillDmgEffectiveness * (1 + moreWepEleDmg / 100f);
            MainHandMaxColdD = MainHandMaxColdD * damageMultiplier * skillDmgEffectiveness * (1 + moreWepEleDmg / 100f);
            float coldToFireMin = MainHandMinColdD * coldToFire / 100f;
            float coldToFireMax = MainHandMaxColdD * coldToFire / 100f;
            MainHandMinColdD -= coldToFireMin;
            MainHandMaxColdD -= coldToFireMax;
            //light
            MainHandMinLightD = (MainHandMinLightD_Tot + skillMinLightningDmg) * (1 + (increasedWeaponLightningD + skillIncreasedLightDmg + increasedEdMainHand + skillIncreasedEleDmg + skillIncreasedProjDmg - reducedDamage) / 100f);
            MainHandMaxLightD = (MainHandMaxLightD_Tot + skillMaxLightningDmg) * (1 + (increasedWeaponLightningD + skillIncreasedLightDmg + increasedEdMainHand + skillIncreasedEleDmg + skillIncreasedProjDmg - reducedDamage) / 100f);
            MainHandMinLightD += mainHandMinDamage * (1 + (increasedPdMH_Total + increasedWeaponLightningD + skillIncreasedLightDmg + increasedEdMainHand + skillIncreasedEleDmg - reducedDamage) / 100f) * physDmgMulti_Total * (physicalToLight) / 100f;
            MainHandMaxLightD += mainHandMaxDamage * (1 + (increasedPdMH_Total + increasedWeaponLightningD + skillIncreasedLightDmg + increasedEdMainHand + skillIncreasedEleDmg - reducedDamage) / 100f) * physDmgMulti_Total * (physicalToLight) / 100f;
            MainHandMinLightD = MainHandMinLightD * damageMultiplier * skillDmgEffectiveness * (1 + moreWepEleDmg / 100f);
            MainHandMaxLightD = MainHandMaxLightD * damageMultiplier * skillDmgEffectiveness * (1 + moreWepEleDmg / 100f);
            float lightToFireMin = MainHandMinLightD * lightToFire / 100f;
            float lightToFireMax = MainHandMaxLightD * lightToFire / 100f;
            float lightToChaosMin = MainHandMinLightD * (lightToChaos - lightToFire) / 100f; //which one takes preceedence?
            float lightToChaosMax = MainHandMaxLightD * (lightToChaos - lightToFire) / 100f;
            MainHandMinLightD -= (lightToFireMin + lightToChaosMin);
            MainHandMaxLightD -= (lightToFireMax + lightToChaosMax);
            //fire
            MainHandMinFireD = MainHandMinFireD_Tot * (1 + (increasedWeaponFireD + skillIncreasedFireDmg + increasedEdMainHand + skillIncreasedEleDmg + skillIncreasedProjDmg - reducedDamage) / 100f);
            MainHandMaxFireD = MainHandMaxFireD_Tot * (1 + (increasedWeaponFireD + skillIncreasedFireDmg + increasedEdMainHand + skillIncreasedEleDmg + skillIncreasedProjDmg - reducedDamage) / 100f);
            MainHandMinFireD += mainHandMinDamage * (1 + (increasedPdMH_Total + increasedWeaponFireD + skillIncreasedFireDmg + increasedEdMainHand + skillIncreasedEleDmg - reducedDamage) / 100f) * physDmgMulti_Total * (physicalToFire + extraFireDmg) / 100f;
            MainHandMaxFireD += mainHandMaxDamage * (1 + (increasedPdMH_Total + increasedWeaponFireD + skillIncreasedFireDmg + increasedEdMainHand + skillIncreasedEleDmg - reducedDamage) / 100f) * physDmgMulti_Total * (physicalToFire + extraFireDmg) / 100f;
            MainHandMinFireD = MainHandMinFireD * damageMultiplier * skillDmgEffectiveness * (1 + moreWepEleDmg / 100f);
            MainHandMaxFireD = MainHandMaxFireD * damageMultiplier * skillDmgEffectiveness * (1 + moreWepEleDmg / 100f);
            float fireToChaosMin = MainHandMinFireD * fireToChaos / 100f;
            float fireToChaosMax = MainHandMaxFireD * fireToChaos / 100f;
            MainHandMinFireD += (coldToFireMin + lightToFireMin - fireToChaosMin); //conversion from others comes last
            MainHandMaxFireD += (coldToFireMax + lightToFireMax - fireToChaosMax);
            //chaos
            MainHandMinChaosD = (MainHandMinChaosD_Tot + skillMinChaosDmg) * (1 + (increasedWeaponChaosD + skillIncreasedChaosDmg + skillIncreasedProjDmg - reducedDamage) / 100f);
            MainHandMaxChaosD = (MainHandMaxChaosD_Tot + skillMaxChaosDmg) * (1 + (increasedWeaponChaosD + skillIncreasedChaosDmg + skillIncreasedProjDmg - reducedDamage) / 100f);
            MainHandMinChaosD += mainHandMinDamage * (1 + (increasedPdMH_Total + increasedWeaponChaosD + skillIncreasedChaosDmg - reducedDamage) / 100f) * physDmgMulti_Total * (physicalToChaos + extraChaosDmg) / 100f;
            MainHandMaxChaosD += mainHandMaxDamage * (1 + (increasedPdMH_Total + increasedWeaponChaosD + skillIncreasedChaosDmg - reducedDamage) / 100f) * physDmgMulti_Total * (physicalToChaos + extraChaosDmg) / 100f;
            MainHandMinChaosD = MainHandMinChaosD * damageMultiplier * skillDmgEffectiveness * (1 + moreWepEleDmg / 100f);
            MainHandMaxChaosD = MainHandMaxChaosD * damageMultiplier * skillDmgEffectiveness * (1 + moreWepEleDmg / 100f);
            MainHandMinChaosD += (lightToChaosMin + fireToChaosMin);
            MainHandMaxChaosD += (lightToChaosMax + fireToChaosMax);
            if (!attribs.ContainsKey("Deal no Non-Fire Damage"))
                ElementalDamageMH = (MainHandMinColdD + MainHandMinFireD + MainHandMinLightD + MainHandMinChaosD + MainHandMaxColdD + MainHandMaxFireD + MainHandMaxLightD + MainHandMaxChaosD) / 2f;
            else
                ElementalDamageMH = (MainHandMinFireD + MainHandMaxFireD) / 2f;
            //Offhand
            //cold
            OffHandMinColdD = (OffHandMinColdD_Tot + skillMinColdDmg) * (1 + (increasedWeaponColdD + skillIncreasedColdDmg + increasedEdOffHand + skillIncreasedEleDmg + skillIncreasedProjDmg - reducedDamage) / 100f);
            OffHandMaxColdD = (OffHandMaxColdD_Tot + skillMaxColdDmg) * (1 + (increasedWeaponColdD + skillIncreasedColdDmg + increasedEdOffHand + skillIncreasedEleDmg + skillIncreasedProjDmg - reducedDamage) / 100f);
            OffHandMinColdD += offHandMinDamage * (1 + (increasedPdOH_Total + increasedWeaponColdD + skillIncreasedColdDmg + increasedEdOffHand + skillIncreasedEleDmg - reducedDamage) / 100f) * physDmgMulti_Total * (HatredColdD * (1 + increasedAuraEff / 100f) + physicalToCold) / 100f;
            OffHandMaxColdD += offHandMaxDamage * (1 + (increasedPdOH_Total + increasedWeaponColdD + skillIncreasedColdDmg + increasedEdOffHand + skillIncreasedEleDmg - reducedDamage) / 100f) * physDmgMulti_Total * (HatredColdD * (1 + increasedAuraEff / 100f) + physicalToCold) / 100f;
            OffHandMinColdD = OffHandMinColdD * damageMultiplier * skillDmgEffectiveness * (1 + moreWepEleDmg / 100f);
            OffHandMaxColdD = OffHandMaxColdD * damageMultiplier * skillDmgEffectiveness * (1 + moreWepEleDmg / 100f);
            coldToFireMin = OffHandMinColdD * coldToFire / 100f;
            coldToFireMax = OffHandMaxColdD * coldToFire / 100f;
            OffHandMinColdD -= coldToFireMin;
            OffHandMaxColdD -= coldToFireMax;
            //light
            OffHandMinLightD = (OffHandMinLightD_Tot + skillMinLightningDmg) * (1 + (increasedWeaponLightningD + skillIncreasedLightDmg + increasedEdOffHand + skillIncreasedEleDmg + skillIncreasedProjDmg - reducedDamage) / 100f);
            OffHandMaxLightD = (OffHandMaxLightD_Tot + skillMaxLightningDmg) * (1 + (increasedWeaponLightningD + skillIncreasedLightDmg + increasedEdOffHand + skillIncreasedEleDmg + skillIncreasedProjDmg - reducedDamage) / 100f);
            OffHandMinLightD += offHandMinDamage * (1 + (increasedPdOH_Total + increasedWeaponLightningD + skillIncreasedLightDmg + increasedEdOffHand + skillIncreasedEleDmg - reducedDamage) / 100f) * physDmgMulti_Total * (physicalToLight) / 100f;
            OffHandMaxLightD += offHandMaxDamage * (1 + (increasedPdOH_Total + increasedWeaponLightningD + skillIncreasedLightDmg + increasedEdOffHand + skillIncreasedEleDmg - reducedDamage) / 100f) * physDmgMulti_Total * (physicalToLight) / 100f;
            OffHandMinLightD = OffHandMinLightD * damageMultiplier * skillDmgEffectiveness * (1 + moreWepEleDmg / 100f);
            OffHandMaxLightD = OffHandMaxLightD * damageMultiplier * skillDmgEffectiveness * (1 + moreWepEleDmg / 100f);
            lightToFireMin = OffHandMinLightD * lightToFire / 100f;
            lightToFireMax = OffHandMaxLightD * lightToFire / 100f;
            lightToChaosMin = OffHandMinLightD * (lightToChaos - lightToFire) / 100f; //which one takes preceedence?
            lightToChaosMax = OffHandMaxLightD * (lightToChaos - lightToFire) / 100f;
            OffHandMinLightD -= (lightToFireMin + lightToChaosMin);
            OffHandMaxLightD -= (lightToFireMax + lightToChaosMax);
            //fire
            OffHandMinFireD = OffHandMinFireD_Tot * (1 + (increasedWeaponFireD + skillIncreasedFireDmg + increasedEdOffHand + skillIncreasedEleDmg + skillIncreasedProjDmg) / 100f);
            OffHandMaxFireD = OffHandMaxFireD_Tot * (1 + (increasedWeaponFireD + skillIncreasedFireDmg + increasedEdOffHand + skillIncreasedEleDmg + skillIncreasedProjDmg) / 100f);
            OffHandMinFireD += offHandMinDamage * (1 + (increasedPdOH_Total + increasedWeaponFireD + skillIncreasedFireDmg + increasedEdOffHand + skillIncreasedEleDmg - reducedDamage) / 100f) * physDmgMulti_Total * (physicalToFire + extraFireDmg) / 100f;
            OffHandMaxFireD += offHandMaxDamage * (1 + (increasedPdOH_Total + increasedWeaponFireD + skillIncreasedFireDmg + increasedEdOffHand + skillIncreasedEleDmg - reducedDamage) / 100f) * physDmgMulti_Total * (physicalToFire + extraFireDmg) / 100f;
            OffHandMinFireD = OffHandMinFireD * damageMultiplier * skillDmgEffectiveness * (1 + moreWepEleDmg / 100f);
            OffHandMaxFireD = OffHandMaxFireD * damageMultiplier * skillDmgEffectiveness * (1 + moreWepEleDmg / 100f);
            OffHandMinFireD += (coldToFireMin + lightToFireMin);
            OffHandMaxFireD += (coldToFireMax + lightToFireMax);
            //chaos
            OffHandMinChaosD = (OffHandMinChaosD_Tot + skillMinChaosDmg) * (1 + (increasedWeaponChaosD + skillIncreasedChaosDmg + skillIncreasedProjDmg) / 100f);
            OffHandMaxChaosD = (OffHandMaxChaosD_Tot + skillMaxChaosDmg) * (1 + (increasedWeaponChaosD + skillIncreasedChaosDmg + skillIncreasedProjDmg) / 100f);
            OffHandMinChaosD += offHandMinDamage * (1 + (increasedPdOH_Total + increasedWeaponChaosD + skillIncreasedChaosDmg - reducedDamage) / 100f) * physDmgMulti_Total * (physicalToChaos + extraChaosDmg) / 100f;
            OffHandMaxChaosD += offHandMaxDamage * (1 + (increasedPdOH_Total + increasedWeaponChaosD + skillIncreasedChaosDmg - reducedDamage) / 100f) * physDmgMulti_Total * (physicalToChaos + extraChaosDmg) / 100f;
            OffHandMinChaosD = OffHandMinChaosD * damageMultiplier * skillDmgEffectiveness * (1 + moreWepEleDmg / 100f);
            OffHandMaxChaosD = OffHandMaxChaosD * damageMultiplier * skillDmgEffectiveness * (1 + moreWepEleDmg / 100f);
            OffHandMinChaosD += lightToChaosMin;
            OffHandMaxChaosD += lightToChaosMax;
            if (!attribs.ContainsKey("Deal no Non-Fire Damage"))
                ElementalDamageOH = (OffHandMinColdD + OffHandMinFireD + OffHandMinLightD + OffHandMinChaosD + OffHandMaxColdD + OffHandMaxFireD + OffHandMaxLightD + OffHandMaxChaosD) / 2f;
            else
                ElementalDamageOH = (OffHandMinFireD + OffHandMaxFireD) / 2f;
            #endregion

            if (!attribs.ContainsKey("Deal no Non-Fire Damage"))
            {

                MainHandMinPD = mainHandMinDamage * (1 + (increasedPdMH_Total - reducedDamage) / 100f) * skillDmgEffectiveness * damageMultiplier * physDmgMulti_Total * (1 - (physicalToFire + physicalToCold + physicalToLight + physicalToChaos) / 100f);
                MainHandMaxPD = mainHandMaxDamage * (1 + (increasedPdMH_Total - reducedDamage) / 100f) * skillDmgEffectiveness * damageMultiplier * physDmgMulti_Total * (1 - (physicalToFire + physicalToCold + physicalToLight + physicalToChaos) / 100f);
                OffHandMinPD = offHandMinDamage * (1 + (increasedPdOH_Total - reducedDamage) / 100f) * skillDmgEffectiveness * damageMultiplier * physDmgMulti_Total * (1 - (physicalToFire + physicalToCold + physicalToLight + physicalToChaos) / 100f);
                OffHandMaxPD = offHandMaxDamage * (1 + (increasedPdOH_Total - reducedDamage) / 100f) * skillDmgEffectiveness * damageMultiplier * physDmgMulti_Total * (1 - (physicalToFire + physicalToCold + physicalToLight + physicalToChaos) / 100f);
            }
            else
            {
                MainHandMinPD = MainHandMaxPD = OffHandMinPD = OffHandMaxPD = 0;
            }
            PhysicalDamageOH = (OffHandMinPD + OffHandMaxPD) / 2;
            PhysicalDamageMH = (MainHandMinPD + MainHandMaxPD) / 2;

            if (CharWeaponStyle == WeaponStyle.DualWield)
            {
                AttackSpeedMH = mainHandAs_Org * (1 + (increasedAsMH + skillIncreasedAs - skillReducedAs) / 100f) * (1 + DuelWieldAttackSpeed / 100f) * skillMoreAs * skillLessAs; //Interesting! must add dualwield as multiplicative
                AttackSpeedOH = offHandAs_Org * (1 + (increasedAsOH + skillIncreasedAs - skillReducedAs) / 100f) * (1 + DuelWieldAttackSpeed / 100f) * skillMoreAs * skillLessAs; //Interesting! must add dualwield as multiplicative
                AttacksPerSecond = (float)Math.Round((AttackSpeedMH + AttackSpeedOH) / 2, 2);
            }
            else
            {
                AttackSpeedMH = mainHandAs_Org * (1 + (increasedAsMH + skillIncreasedAs - skillReducedAs) / 100f) * skillMoreAs * skillLessAs; //Interesting! must add dualwield as multiplicative
                AttacksPerSecond = (float)Math.Round(AttackSpeedMH, 2);
            }
            float totalDPS = 0;
            float mainHandDPS = 0;
            float offHandDPS = 0;
            bool[] isWeaponMatchSkill = WeaponMatchSkill(attackSkill);
            if (isWeaponMatchSkill[0])
            {
                MainHandVisibility = System.Windows.Visibility.Visible;
                //Skil can be used by main hand weapon 
                float mainHandDmg = PhysicalDamageMH + ElementalDamageMH;
                float mainHandNonCritDmg = mainHandDmg * (attribs.ContainsKey("Non-critical strikes deal #% Damage") ? attribs["Non-critical strikes deal #% Damage"][0] / 100f : 1); //for unique
                float mainHandCritDmg = mainHandDmg * CritMultiplierMH / 100f;
                if (isWeaponMatchSkill[1])
                    mainHandDPS = (mainHandCritDmg * CritChanceMH / 100 + mainHandNonCritDmg * (1 - CritChanceMH / 100)) * AttacksPerSecond * ChanceToHitMH / 100f;//exact formula
                else
                    mainHandDPS = (mainHandCritDmg * CritChanceMH / 100 + mainHandNonCritDmg * (1 - CritChanceMH / 100)) * AttackSpeedMH * ChanceToHitMH / 100f;//if offhand cant be used we only use main hand attack speed
                //mainHandDPS = (mainHandCritDmg * CritChanceMH + mainHandDmg * (1 - CritChanceMH)) * AttacksPerSecond * ChanceToHit;//average formula
            }
            else
                MainHandVisibility = System.Windows.Visibility.Collapsed;
            if (CharWeaponStyle == WeaponStyle.DualWield && isWeaponMatchSkill[1])
            {
                OffHandVisibility = System.Windows.Visibility.Visible;
                //Skil can be used by main hand weapon 
                float offHandDmg = (PhysicalDamageOH + ElementalDamageOH);
                float offHandNonCritDmg = offHandDmg * (attribs.ContainsKey("Non-critical strikes deal #% Damage") ? attribs["Non-critical strikes deal #% Damage"][0] / 100f : 1); //for unique
                float offHandCritDmg = offHandDmg * CritMultiplierOH / 100f;
                if (isWeaponMatchSkill[0])
                    offHandDPS = (offHandCritDmg * CritChanceOH / 100f + offHandNonCritDmg * (1 - CritChanceOH / 100f)) * AttacksPerSecond * ChanceToHitOH / 100f;//exact formula
                else
                    offHandDPS = (offHandCritDmg * CritChanceOH / 100f + offHandNonCritDmg * (1 - CritChanceOH / 100f)) * AttackSpeedOH * ChanceToHitOH / 100f;//in this case we only use offhand aps
                //offHandDPS = (offHandCritDmg * CritChanceOH + offHandDmg * (1 - CritChanceOH)) * AttacksPerSecond * ChanceToHit;//average formula
                if (attackSkill.Name.Contains("Dual Strike"))
                    totalDPS = isWeaponMatchSkill[0] ? mainHandDPS + offHandDPS : offHandDPS;
                else if (attackSkill.Name.Contains("Cleave"))
                    totalDPS = isWeaponMatchSkill[0] ? 0.6f * mainHandDPS + 0.6f * offHandDPS : offHandDPS;
                else
                    totalDPS = isWeaponMatchSkill[0] ? (mainHandDPS + offHandDPS) / 2f : offHandDPS;
                if (attackSkill.Name.Contains("Cyclone"))//special case for cyclone cause it hits two times per attack
                    totalDPS *= 2;
                AttackSkillDPS = totalDPS;
            }
            else
            {
                if (attackSkill.Name.Contains("Cyclone"))
                    mainHandDPS *= 2;
                AttackSkillDPS = mainHandDPS;
                OffHandVisibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void SkillToAccuracyRating(float toAccuracyRating)
        {
            float accuracyFromItems = 0;
            if (itemData != null)
            {
                foreach (CharItemData.Attribute attrib in itemData.AttribCollectionView)
                {
                    if (attrib.Group == "Global Mods")
                    {
                        if (attrib.TextAttribute.Contains("to Accuracy Rating"))
                            accuracyFromItems += attrib.Values[0];
                    }
                }
            }
            accuracyFromItems += toAccuracyRating;
            UpdateAccuracy(accuracyFromItems);

            ChanceToHitMH = (int)(CalcChanceToHit(Level, AccuracyRatingMH) + 0.5f);
            ChanceToHitOH = (int)(CalcChanceToHit(Level, AccuracyRatingOH) + 0.5f);
            if (CharWeaponStyle == WeaponStyle.DualWield)
                ChanceToHit = (int)((ChanceToHitMH + ChanceToHitOH) / 2f + 0.5f);
            else
                ChanceToHit = ChanceToHitMH;
        }

        private bool[] WeaponMatchSkill(ActiveGem attackSkill)
        {
            bool[] match = new bool[2];
            if (attackSkill.Keywords.Contains("Melee"))
                switch (attackSkill.Name)
                {
                    case "Cyclone":
                        if (WeaponTypeMH != WeaponType.Other && WeaponTypeMH != WeaponType.Wand)
                            match[0] = true;
                        else
                            match[0] = false;
                        if (WeaponTypeMH != WeaponType.Other && WeaponTypeMH != WeaponType.Wand)
                            match[1] = true;
                        else
                            match[1] = false;
                        break;
                    case "Frenzy":
                        if (WeaponTypeMH != WeaponType.Other && WeaponTypeMH != WeaponType.Wand)
                            match[0] = true;
                        else
                            match[0] = false;
                        if (WeaponTypeMH != WeaponType.Other && WeaponTypeMH != WeaponType.Wand)
                            match[1] = true;
                        else
                            match[1] = false;
                        break;
                    case "Double Strike":
                        if (WeaponTypeMH != WeaponType.Other && WeaponTypeMH != WeaponType.Wand)
                            match[0] = true;
                        else
                            match[0] = false; //unarmed not supported atm
                        match[1] = false; //Does not use off hand
                        break;
                    case "Lightning Strike":
                        if (WeaponTypeMH != WeaponType.Other && WeaponTypeMH != WeaponType.Bow && WeaponTypeMH != WeaponType.Wand)
                            match[0] = true;
                        else
                            match[0] = false; //unarmed not supported atm
                        if (WeaponTypeOH != WeaponType.Other && WeaponTypeMH != WeaponType.Bow && WeaponTypeMH != WeaponType.Wand)
                            match[1] = true;
                        else
                            match[1] = false;
                        break;
                    case "Flicker Strike":
                        if (WeaponTypeMH != WeaponType.Other && WeaponTypeMH != WeaponType.Bow && WeaponTypeMH != WeaponType.Wand)
                            match[0] = true;
                        else
                            match[0] = false; //unarmed not supported atm
                        if (WeaponTypeOH != WeaponType.Other)
                            match[1] = true;
                        else
                            match[1] = false;
                        break;
                    case "Infernal Blow":
                        if (WeaponTypeMH != WeaponType.Other && WeaponTypeMH != WeaponType.Dagger && WeaponTypeMH != WeaponType.Claw && WeaponTypeMH != WeaponType.Bow && WeaponTypeMH != WeaponType.Wand)
                            match[0] = true;
                        else
                            match[0] = false;
                        if (WeaponTypeOH != WeaponType.Other && WeaponTypeOH != WeaponType.Dagger && WeaponTypeOH != WeaponType.Claw && WeaponTypeOH != WeaponType.Bow && WeaponTypeOH != WeaponType.Wand)
                            match[1] = true;
                        else
                            match[1] = false;
                        break;
                    case "Heavy Strike":
                        if (WeaponTypeMH != WeaponType.Other && WeaponTypeMH != WeaponType.Dagger && WeaponTypeMH != WeaponType.Claw && WeaponTypeMH != WeaponType.Bow && WeaponTypeMH != WeaponType.Wand)
                            match[0] = true;
                        else
                            match[0] = false;
                        if (WeaponTypeOH != WeaponType.Other && WeaponTypeOH != WeaponType.Dagger && WeaponTypeOH != WeaponType.Claw && WeaponTypeOH != WeaponType.Bow && WeaponTypeOH != WeaponType.Wand)
                            match[1] = true;
                        else
                            match[1] = false;
                        break;
                    case "Leap Slam":
                        if (WeaponTypeMH != WeaponType.Other && WeaponTypeMH != WeaponType.Dagger && WeaponTypeMH != WeaponType.Claw && WeaponTypeMH != WeaponType.Bow && WeaponTypeMH != WeaponType.Wand)
                            match[0] = true;
                        else
                            match[0] = false;
                        match[1] = false; //Does not use off hand
                        break;
                    case "Glacial Hammer":
                        if (WeaponTypeMH == WeaponType.TwoHandedMace || WeaponTypeMH == WeaponType.OneHandedMace || WeaponTypeMH == WeaponType.Staff)
                            match[0] = true;
                        else
                            match[0] = false;
                        if (WeaponTypeOH == WeaponType.TwoHandedMace || WeaponTypeOH == WeaponType.OneHandedMace || WeaponTypeOH == WeaponType.Staff)
                            match[1] = true;
                        else
                            match[1] = false;
                        break;
                    case "Ground Slam":
                        if (WeaponTypeMH == WeaponType.TwoHandedMace || WeaponTypeMH == WeaponType.OneHandedMace || WeaponTypeMH == WeaponType.Staff)
                            match[0] = true;
                        else
                            match[0] = false;
                        match[1] = false; //Does not use off hand
                        break;
                    case "Reave":
                        if (WeaponTypeMH == WeaponType.OneHandedSword || WeaponTypeMH == WeaponType.Claw || WeaponTypeMH == WeaponType.Dagger)
                            match[0] = true;
                        else
                            match[0] = false;
                        if (WeaponTypeOH == WeaponType.OneHandedSword || WeaponTypeOH == WeaponType.Claw || WeaponTypeOH == WeaponType.Dagger)
                            match[1] = true;
                        else
                            match[1] = false;
                        break;
                    case "Sweep":
                        if (WeaponTypeMH == WeaponType.TwoHandedMace || WeaponTypeMH == WeaponType.TwoHandedAxe || WeaponTypeMH == WeaponType.Staff)
                            match[0] = true;
                        else
                            match[0] = false;
                        match[1] = false; //Does not use off hand
                        break;
                    case "Cleave":
                        if (WeaponTypeMH == WeaponType.OneHandedSword || WeaponTypeMH == WeaponType.OneHandedAxe || WeaponTypeMH == WeaponType.TwoHandedSword || WeaponTypeMH == WeaponType.TwoHandedAxe)
                            match[0] = true;
                        else
                            match[0] = false;
                        if (WeaponTypeOH == WeaponType.OneHandedSword || WeaponTypeOH == WeaponType.OneHandedAxe || WeaponTypeOH == WeaponType.TwoHandedSword || WeaponTypeOH == WeaponType.TwoHandedAxe)
                            match[1] = true;
                        else
                            match[1] = false;
                        break;
                    case "Shield Charge":
                        if (CharWeaponStyle == WeaponStyle.OnehandShield)
                            match[0] = true;
                        else
                            match[0] = false;
                        match[1] = false; //Does not use off hand
                        break;
                    case "Dual Strike":
                        if (WeaponTypeMH == WeaponType.OneHandedSword || WeaponTypeMH == WeaponType.OneHandedAxe || WeaponTypeMH == WeaponType.OneHandedMace || WeaponTypeMH == WeaponType.Dagger || WeaponTypeMH == WeaponType.Claw)
                            match[0] = true;
                        else
                            match[0] = false;
                        if (WeaponTypeOH == WeaponType.OneHandedSword || WeaponTypeOH == WeaponType.OneHandedAxe || WeaponTypeOH == WeaponType.OneHandedMace || WeaponTypeOH == WeaponType.Dagger || WeaponTypeOH == WeaponType.Claw)
                            match[1] = true;
                        else
                            match[1] = false;
                        break;
                    case "Puncture":
                        if (WeaponTypeMH == WeaponType.OneHandedSword || WeaponTypeMH == WeaponType.Dagger || WeaponTypeMH == WeaponType.Claw || WeaponTypeMH == WeaponType.TwoHandedSword || WeaponTypeMH == WeaponType.Bow)
                            match[0] = true;
                        else
                            match[0] = false;
                        if (WeaponTypeOH == WeaponType.OneHandedSword || WeaponTypeOH == WeaponType.Dagger || WeaponTypeOH == WeaponType.Claw || WeaponTypeOH == WeaponType.TwoHandedSword)
                            match[1] = true;
                        else
                            match[1] = false;
                        break;
                    case "Viper Strike":
                        if (WeaponTypeMH == WeaponType.OneHandedSword || WeaponTypeMH == WeaponType.Dagger || WeaponTypeMH == WeaponType.Claw || WeaponTypeMH == WeaponType.TwoHandedSword)
                            match[0] = true;
                        else
                            match[0] = false;
                        if (WeaponTypeOH == WeaponType.OneHandedSword || WeaponTypeOH == WeaponType.Dagger || WeaponTypeOH == WeaponType.Claw || WeaponTypeOH == WeaponType.TwoHandedSword)
                            match[1] = true;
                        else
                            match[1] = false;
                        break;
                    case "Whirling Blades":
                        if (WeaponTypeMH == WeaponType.OneHandedSword || WeaponTypeMH == WeaponType.Dagger || WeaponTypeMH == WeaponType.Claw)
                            match[0] = true;
                        else
                            match[0] = false;
                        if (WeaponTypeOH == WeaponType.OneHandedSword || WeaponTypeOH == WeaponType.Dagger || WeaponTypeOH == WeaponType.Claw)
                            match[1] = true;
                        else
                            match[1] = false;
                        break;
                }
            else if (attackSkill.Name == "Default Attack")
            {
                match[0] = true;
                match[1] = true;
            }
            else if (attackSkill.Name == "Spectral Throw")
            {
                match[0] = true;
                match[1] = true;
            }
            else //projectile skills  
            {
                match[0] = true;
                match[1] = false;
            }
            return match;
        }
        #endregion
        //-------------------------------------------------------------------------------
        #region Internal Types
        //-------------------------------------------------------------------------------
        public enum WeaponStyle
        {
            DualWield,
            OnehandShield,
            TwoHand,
            DualWield2H,
            Ranged,
            UnArmed,
            UnArmedShield,
            Onehand,
        }

        public enum WeaponType
        {
            OneHandedSword,
            TwoHandedSword,
            OneHandedMace,
            TwoHandedMace,
            OneHandedAxe,
            TwoHandedAxe,
            Bow,
            Claw,
            Dagger,
            Staff,
            Wand,
            Other,
        }

        public class VIPStats
        {
            public int Life;
            public float LifeRegen;
            public int Mana;
            public float ManaRegen;
            public int EnergyShield;
            public int PhysDamageReduction;
            public int ChanceToEvade;
            public int ChanceToBlock;
            public float CritChanceOH;
            public float CritChanceMH;
            public float AttackSkillDPS;
            public float SpellSkillDPS;
            public float SpellCritChance;

            public VIPStats(int life, float lifeRegen, int mana, float manaRegen, int energyShield, int physDmgReduct, int evade, int block, float critMH, float CritOH, float spellCritChance, float attackDPS, float spellDPS)
            {
                Life = life;
                LifeRegen = lifeRegen;
                Mana = mana;
                ManaRegen = manaRegen;
                EnergyShield = energyShield;
                PhysDamageReduction = physDmgReduct;
                ChanceToBlock = block;
                ChanceToEvade = evade;
                CritChanceMH = critMH;
                CritChanceOH = CritOH;
                SpellCritChance = spellCritChance;
                AttackSkillDPS = attackDPS;
                SpellSkillDPS = spellDPS;
            }
        }
        #endregion
        //-------------------------------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
