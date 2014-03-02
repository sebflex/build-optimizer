using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POESKillTree
{
    public abstract class SkillGem
    {
        #region Properties
        //------------------------------------------------------------------------
        public string Name {get; set;}
        public int Level { get; set; }
        public string Description { get; set; }
        public int[] Requirements; //Level, Str, Dex, Int
        public float CastTime { get; set; }
        public List<string> Keywords;
        public Dictionary<string, List<float>> Properties;
        public Dictionary<string, List<float>> Mods;
        public GemType GemType;
        #endregion
        //------------------------------------------------------------------------
    }

    public class ActiveGem : SkillGem
    {

        //50% of Physical Damage Converted to Cold Damage
        //50% of Physical Damage Converted to Lightning Damage
        //50% of Physical Damage Converted to Fire Damage

        //4% increased Critical Strike Chance
        public int ManaCost { get; set; }
        
    }

    //class AttackGem : ActiveGem
    //{
        
    //}
    public enum GemType 
    {
        Attack,
        Spell,
        Mine,
        Totem,
        Trap,
        Cast,
        Aura,
        Support,
        Curse,
        Minion,
        Duration
    }

    public class SupportGem : SkillGem
    {
        public int ManaMultiplier { get; set; }
        public bool Selected { get; set; }
    }

    public class AuraGem: SkillGem 
    {
        public int ReservedMana { get; set; }
    }
}
