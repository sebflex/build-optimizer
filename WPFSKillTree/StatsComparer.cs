using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POESKillTree
{
    class StatsComparer : CharStats
    {
        VIPStats savedStats;

        private string lifeChange;
        public string LifeChange { get { return lifeChange; } set { SetField(ref lifeChange, value); } }
        private string lifeRegenChange;
        public string LifeRegenChange { get { return lifeRegenChange; } set { SetField(ref lifeRegenChange, value); } }
        private string manaChange;
        public string ManaChange { get { return manaChange; } set { SetField(ref manaChange, value); } }
        private string manaRegenChange;
        public string ManaRegenChange { get { return manaRegenChange; } set { SetField(ref manaRegenChange, value); } }
        private string energyShieldChange;
        public string EnergyShieldChange { get { return energyShieldChange; } set { SetField(ref energyShieldChange, value); } }
        private string physDamageReducChange;
        public string PhysDamageReducChange { get { return physDamageReducChange; } set { SetField(ref physDamageReducChange, value); } }
        private string chanceToEvadeChange;
        public string ChanceToEvadeChange { get { return chanceToEvadeChange; } set { SetField(ref chanceToEvadeChange, value); } }
        private string critChanceMHChange;
        public string CritChanceMHChange { get { return critChanceMHChange; } set { SetField(ref critChanceMHChange, value); } }
        private string critChanceOHChange;
        public string CritChanceOHChange { get { return critChanceOHChange; } set { SetField(ref critChanceOHChange, value); } }
        private string critChanceSpellChange;
        public string CritChanceSpellChange { get { return critChanceSpellChange; } set { SetField(ref critChanceSpellChange, value); } }

        private string attackSkillDPSChange;
        public string AttackSkillDPSChange { get { return attackSkillDPSChange; } set { SetField(ref attackSkillDPSChange, value); } }

        public StatsComparer(SkillTree tree) : base(tree) { }

        public override void UpdateStats(Dictionary<string, List<float>> attribs, int level, CharItemData itemData)
        {
            base.UpdateStats(attribs, level, itemData);
            SaveStats();
        }

        private void SaveStats()
        {
            savedStats = new VIPStats(Life, LifeRegen, Mana, ManaRegen, EnergyShield, PhysDamageReduction, ChanceToEvade, ChanceToBlock, CritChanceMH, CritChanceOH, CritChanceSpell, AttackSkillDPS, SpellSkillDPS);
            CompareStats();
        }

        private void CompareStats()
        {
            LifeChange = Life + " (" + (Life - savedStats.Life).ToString("+#;-#;0") + ")";
            LifeRegenChange = LifeRegen + " (" + (LifeRegen - savedStats.LifeRegen).ToString("+#;-#;0") + ")";
            ManaChange = Mana + " (" + (Mana - savedStats.Mana).ToString("+#;-#;0") + ")";
            ManaRegenChange = ManaRegen + " (" + (ManaRegen - savedStats.ManaRegen).ToString("+#;-#;0") + ")";
            EnergyShieldChange = EnergyShield + " (" + (EnergyShield - savedStats.EnergyShield).ToString("+#;-#;0") + ")";
            PhysDamageReducChange = PhysDamageReduction + "% (" + (PhysDamageReduction - savedStats.PhysDamageReduction).ToString("+#;-#;0") + "%)";
            ChanceToEvadeChange = ChanceToEvade + "% (" + (ChanceToEvade - savedStats.ChanceToEvade).ToString("+#;-#;0") + "%)";
            CritChanceMHChange = CritChanceMH + "% (" + (CritChanceMH - savedStats.CritChanceMH).ToString("+#;-#;0") + "%)";
            CritChanceOHChange = CritChanceOH + "% (" + (CritChanceOH - savedStats.CritChanceOH).ToString("+#;-#;0") + "%)";
            CritChanceSpellChange = CritChanceSpell + "% (" + (CritChanceSpell - savedStats.SpellCritChance).ToString("+#;-#;0") + "%)";
            if (ActiveSkillGem != null)
            {
                if (ActiveSkillGem.GemType == GemType.Attack)
                    AttackSkillDPSChange = AttackSkillDPS + " (" + (AttackSkillDPS - savedStats.AttackSkillDPS).ToString("+#;-#;0") + ")";
                else
                    AttackSkillDPSChange = SpellSkillDPS + " (" + (SpellSkillDPS - savedStats.SpellSkillDPS).ToString("+#;-#;0") + ")";
            }
        }

        internal void ModifyStrength(int newStr)
        {
            Strength += newStr;
            UpdateHP();
            UpdatePhysicalDamage();
            CalcSkillDPS();
            CompareStats();
        }
        internal void ModifyDexterity(int newDex)
        {
            Dexterity += newDex;
            UpdateBaseStats();
            ChanceToHitMH = (int)(CalcChanceToHit(Level, AccuracyRatingMH) + 0.5f);
            ChanceToHitOH = (int)(CalcChanceToHit(Level, AccuracyRatingOH) + 0.5f);
            if (CharWeaponStyle == WeaponStyle.DualWield)
                ChanceToHit = (int)((ChanceToHitMH + ChanceToHitOH) / 2f + 0.5f);
            else
                ChanceToHit = ChanceToHitMH;
            CalcChanceToEvade();
            CalcSkillDPS();
            CompareStats();
        }
        internal void ModifyIntelligence(int newInt)
        {
            Intelligence += newInt;
            UpdateBaseStats();
            UpdateMana(); //Must come After updateES
            UpdateElementalDamage(); //must come after UpdatePhysicalDamage. due to wands
            CalcSkillDPS();
            CompareStats();
        }
        int lastNewArmour;
        internal void ModifyArmour(int newArmour)
        {
            lastNewArmour += newArmour;
            float armourFromItems = 0;
            if (itemData != null)
            {
                foreach (CharItemData.Attribute attrib in itemData.AttribCollectionView)
                {
                    if (attrib.Group != "Global Mods")
                    {
                        if (attrib.TextAttribute.Contains("Armour: "))
                            armourFromItems += attrib.Values[0];
                    }
                    else
                    {
                        if (attrib.TextAttribute.Contains("to Armour") && !attrib.TextAttribute.Contains("to Armour while"))
                            armourFromItems += attrib.Values[0];
                    }
                }
            }
            armourFromItems += lastNewArmour;
            UpdateArmour(armourFromItems, lastNewArmourIncrease);
            CalcDamageReduction();
            CompareStats();
            //Armour = oldValue;
        }
        int lastNewArmourIncrease;
        internal void ModifyIncreasedArmour(int newArmourIncrease)
        {
            lastNewArmourIncrease += newArmourIncrease;
            float armourFromItems = 0;
            if (itemData != null)
            {
                foreach (CharItemData.Attribute attrib in itemData.AttribCollectionView)
                {
                    if (attrib.Group != "Global Mods")
                    {
                        if (attrib.TextAttribute.Contains("Armour: "))
                            armourFromItems += attrib.Values[0];
                    }
                    else
                    {
                        if (attrib.TextAttribute.Contains("to Armour") && !attrib.TextAttribute.Contains("to Armour while"))
                            armourFromItems += attrib.Values[0];
                    }
                }
            }
            armourFromItems += lastNewArmour;
            UpdateArmour(armourFromItems, lastNewArmourIncrease);
            CalcDamageReduction();
            CompareStats();
        }

        int lastToEvasion;
        internal void ModifyEvasionRating(int toEvasion)
        {
            lastToEvasion += toEvasion;
            float evasionFromItems = 0;
            if (itemData != null)
            {
                foreach (CharItemData.Attribute attrib in itemData.AttribCollectionView)
                {
                    if (attrib.Group != "Global Mods")
                    {
                        if (attrib.TextAttribute.Contains("Evasion Rating: "))
                            evasionFromItems += attrib.Values[0];
                    }
                    else
                    {

                        if (attrib.TextAttribute.Contains("to Evasion Rating"))
                            evasionFromItems += attrib.Values[0];
                    }
                }
            }
            evasionFromItems += lastToEvasion;
            UpdateEvasionRating(evasionFromItems, lastIncreasedEvasion);
            CalcChanceToEvade();
            ModifyArmour(0); // force update armour
            CompareStats();
            //EvasionRating = oldValue;
        }
        int lastIncreasedEvasion;
        internal void ModifyIncreasedEvasion(int increasedEvasion)
        {
            lastIncreasedEvasion += increasedEvasion;
            float evasionFromItems = 0;
            if (itemData != null)
            {
                foreach (CharItemData.Attribute attrib in itemData.AttribCollectionView)
                {
                    if (attrib.Group != "Global Mods")
                    {
                        if (attrib.TextAttribute.Contains("Evasion Rating: "))
                            evasionFromItems += attrib.Values[0];
                    }
                    else
                    {

                        if (attrib.TextAttribute.Contains("to Evasion Rating"))
                            evasionFromItems += attrib.Values[0];
                    }
                }
            }
            evasionFromItems += lastToEvasion;
            UpdateEvasionRating(evasionFromItems, lastIncreasedEvasion);
            CalcChanceToEvade();
            CompareStats();
        }
        int lastToMaxLife;
        internal void ModifyToMaximumLife(int toMaxLife)
        {
            lastToMaxLife += toMaxLife;
            UpdateHP(lastToMaxLife, lastIncreaseLife);
            CompareStats();
        }
        int lastIncreaseLife;
        internal void ModifyIncreaseLife(int increaseLife)
        {
            lastIncreaseLife += increaseLife;
            UpdateHP(lastToMaxLife, lastIncreaseLife);
            CompareStats();
        }
        int lastToMaxES;
        internal void ModifyToMaximumES(int toMaxES)
        {
            lastToMaxES += toMaxES;

            float energyFromItems = 0;
            if (itemData != null)
            {
                foreach (CharItemData.Attribute attrib in itemData.AttribCollectionView)
                {
                    if (attrib.Group != "Global Mods")
                    {
                        if (attrib.TextAttribute.Contains("Energy Shield: "))
                            energyFromItems += attrib.Values[0];
                    }
                    else
                    {
                        if (attrib.TextAttribute.Contains("to maximum Energy Shield"))
                            energyFromItems += attrib.Values[0];
                    }
                }
            }
            energyFromItems += lastToMaxES;
            UpdateES(energyFromItems, lastIncreasedES);
            UpdateMana(lastToMaxMana, lastIncreasedMana);
            CompareStats();
        }
        int lastIncreasedES;
        internal void ModifyIncreasedES(int increasedES)
        {
            lastIncreasedES += increasedES;

            float energyFromItems = 0;
            if (itemData != null)
            {
                foreach (CharItemData.Attribute attrib in itemData.AttribCollectionView)
                {
                    if (attrib.Group != "Global Mods")
                    {
                        if (attrib.TextAttribute.Contains("Energy Shield: "))
                            energyFromItems += attrib.Values[0];
                    }
                    else
                    {
                        if (attrib.TextAttribute.Contains("to maximum Energy Shield"))
                            energyFromItems += attrib.Values[0];
                    }
                }
            }
            energyFromItems += lastToMaxES;
            UpdateES(energyFromItems, lastIncreasedES);
            UpdateMana(lastToMaxMana, lastIncreasedMana);
            CompareStats();
        }
        int lastToMaxMana;
        internal void ModifyToMaximumMana(int toMaxMana)
        {
            lastToMaxMana += toMaxMana;
            UpdateMana(lastToMaxMana, lastIncreasedMana);
            CompareStats();
        }
        int lastIncreasedMana;
        internal void ModifyIncreasedMana(int increasedMana)
        {
            lastIncreasedMana += increasedMana;
            UpdateMana(lastToMaxMana, lastIncreasedMana);
            CompareStats();
        }
        int lastToAccuracyRating;
        internal void ModifyToAccuracyRating(int toAccuracyRating)
        {
            lastToAccuracyRating += toAccuracyRating;

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
            accuracyFromItems += lastToAccuracyRating;
            UpdateAccuracy(accuracyFromItems, lastIncreasedAccuracy);

            ChanceToHitMH = (int)(CalcChanceToHit(Level, AccuracyRatingMH) + 0.5f);
            ChanceToHitOH = (int)(CalcChanceToHit(Level, AccuracyRatingOH) + 0.5f);
            if (CharWeaponStyle == WeaponStyle.DualWield)
                ChanceToHit = (int)((ChanceToHitMH + ChanceToHitOH) / 2f + 0.5f);
            else
                ChanceToHit = ChanceToHitMH;
            CalcSkillDPS();
            CompareStats();
        }
        int lastIncreasedAccuracy;
        internal void ModifyIncreasedAccuracy(int increasedAccuracy)
        {
            lastIncreasedAccuracy += increasedAccuracy;

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
            accuracyFromItems += lastToAccuracyRating;
            UpdateAccuracy(accuracyFromItems, lastIncreasedAccuracy);

            ChanceToHitMH = (int)(CalcChanceToHit(Level, AccuracyRatingMH) + 0.5f);
            ChanceToHitOH = (int)(CalcChanceToHit(Level, AccuracyRatingOH) + 0.5f);
            if (CharWeaponStyle == WeaponStyle.DualWield)
                ChanceToHit = (int)((ChanceToHitMH + ChanceToHitOH) / 2f + 0.5f);
            else
                ChanceToHit = ChanceToHitMH;
            UpdatePhysicalDamage();
            UpdateElementalDamage(); //must come after UpdatePhysicalDamage. due to wands
            CalcSkillDPS();
            CompareStats();
        }
        int lastIncreasedAS;
        internal void ModifyIncreasedAS(int increasedAS)
        {
            lastIncreasedAS += increasedAS;
            UpdateAttackSpeed(lastIncreasedAS);
            CalcSkillDPS();
            CompareStats();
        }
        int lastIncreasedCS;
        internal void ModifyIncreasedCS(int increasedCS)
        {
            lastIncreasedCS += increasedCS;
            UpdateCastSpeed(lastIncreasedCS);
            CalcSkillDPS();
            CompareStats();
        }
        //int lastIncreasedCritChance;
        internal void ModifyIncreasedCritChance(int increasedCritChance)
        {
            comparerModifiedIncreasedCrit += increasedCritChance;
            //UpdateCriticalStrike(lastIncreasedCritChance, lastIncreasedCritMultiplier);
            CalcSkillDPS();
            CompareStats();
        }
        //int lastIncreasedCritMultiplier;
        internal void ModifyIncreasedCritMultiplier(int increasedCritMultiplier)
        {
            comparerModifiedIncreasedCritMult += increasedCritMultiplier;
            //UpdateCriticalStrike(lastIncreasedCritChance, lastIncreasedCritMultiplier);
            CalcSkillDPS();
            CompareStats();
        }
        int lastToColdDamage;
        internal void ModifyToColdDamage(int toColdDamage)
        {
            lastToColdDamage += toColdDamage;
            UpdateElementalDamage(lastToColdDamage, lastIncreasedColdDamage, lastToFireDamage, lastIncreasedFireDamage, lastToLightDamage, lastIncreasedLightDamage, lastToChaosDamage, lastIncreasedChaosDamage, lastIncreasedEleDamage);
            CalcSkillDPS();
            CompareStats();
        }
        int lastIncreasedColdDamage;
        internal void ModifyIncreasedColdDamage(int increasedColdDamage)
        {
            lastIncreasedColdDamage += increasedColdDamage;
            UpdateElementalDamage(lastToColdDamage, lastIncreasedColdDamage, lastToFireDamage, lastIncreasedFireDamage, lastToLightDamage, lastIncreasedLightDamage, lastToChaosDamage, lastIncreasedChaosDamage, lastIncreasedEleDamage);
            CalcSkillDPS();
            CompareStats();
        }
        int lastToFireDamage;
        internal void ModifyToFireDamage(int toFireDamage)
        {
            lastToFireDamage += toFireDamage;
            UpdateElementalDamage(lastToColdDamage, lastIncreasedColdDamage, lastToFireDamage, lastIncreasedFireDamage, lastToLightDamage, lastIncreasedLightDamage, lastToChaosDamage, lastIncreasedChaosDamage, lastIncreasedEleDamage);
            CalcSkillDPS();
            CompareStats();
        }
        int lastIncreasedFireDamage;
        internal void ModifyIncreasedFireDamage(int increasedFireDamage)
        {
            lastIncreasedFireDamage += increasedFireDamage;
            UpdateElementalDamage(lastToColdDamage, lastIncreasedColdDamage, lastToFireDamage, lastIncreasedFireDamage, lastToLightDamage, lastIncreasedLightDamage, lastToChaosDamage, lastIncreasedChaosDamage, lastIncreasedEleDamage);
            CalcSkillDPS();
            CompareStats();
        }
        int lastToLightDamage;
        internal void ModifyToLightDamage(int toLightDamage)
        {
            lastToLightDamage += toLightDamage;
            UpdateElementalDamage(lastToColdDamage, lastIncreasedColdDamage, lastToFireDamage, lastIncreasedFireDamage, lastToLightDamage, lastIncreasedLightDamage, lastToChaosDamage, lastIncreasedChaosDamage, lastIncreasedEleDamage);
            CalcSkillDPS();
            CompareStats();
        }
        int lastIncreasedLightDamage;
        internal void ModifyIncreasedLightDamage(int increasedLightDamage)
        {
            lastIncreasedLightDamage += increasedLightDamage;
            UpdateElementalDamage(lastToColdDamage, lastIncreasedColdDamage, lastToFireDamage, lastIncreasedFireDamage, lastToLightDamage, lastIncreasedLightDamage, lastToChaosDamage, lastIncreasedChaosDamage, lastIncreasedEleDamage);
            CalcSkillDPS();
            CompareStats();
        }
        int lastToChaosDamage;
        internal void ModifyToChaosDamage(int toChaosDamage)
        {
            lastToChaosDamage += toChaosDamage;
            UpdateElementalDamage(lastToColdDamage, lastIncreasedColdDamage, lastToFireDamage, lastIncreasedFireDamage, lastToLightDamage, lastIncreasedLightDamage, lastToChaosDamage, lastIncreasedChaosDamage, lastIncreasedEleDamage);
            CalcSkillDPS();
            CompareStats();
        }
        int lastIncreasedChaosDamage;
        internal void ModifyIncreasedChaosDamage(int increasedChaosDamage)
        {
            lastIncreasedChaosDamage += increasedChaosDamage;
            UpdateElementalDamage(lastToColdDamage, lastIncreasedColdDamage, lastToFireDamage, lastIncreasedFireDamage, lastToLightDamage, lastIncreasedLightDamage, lastToChaosDamage, lastIncreasedChaosDamage, lastIncreasedEleDamage);
            CalcSkillDPS();
            CompareStats();
        }
        int lastIncreasedEleDamage;
        internal void ModifyIncreasedEleDamage(int increasedEleDamage)
        {
            lastIncreasedEleDamage += increasedEleDamage;
            UpdateElementalDamage(lastToColdDamage, lastIncreasedColdDamage, lastToFireDamage, lastIncreasedFireDamage, lastToLightDamage, lastIncreasedLightDamage, lastToChaosDamage, lastIncreasedChaosDamage, lastIncreasedEleDamage);
            CalcSkillDPS();
            CompareStats();
        }
        int lastToPhysicalDamage;
        internal void ModifyToPhysicalDamage(int toPhysicalDamage)
        {
            lastToPhysicalDamage += toPhysicalDamage;
            UpdatePhysicalDamage(lastToPhysicalDamage, lastIncreasedPhysicalDamage);
            CalcSkillDPS();
            CompareStats();
        }
        int lastIncreasedPhysicalDamage;
        internal void ModifyIncreasedPhysicalDamage(int increasedPhysicalDamage)
        {
            lastIncreasedPhysicalDamage += increasedPhysicalDamage;
            UpdatePhysicalDamage(lastToPhysicalDamage, lastIncreasedPhysicalDamage);
            CalcSkillDPS();
            CompareStats();
        }
    }
}
