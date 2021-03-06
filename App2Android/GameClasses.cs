using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Xml.Serialization;

namespace App2Android
{


    [Serializable]
    [XmlRoot("GameElements")]
    public class GameElements
    {
        [XmlArray("StartupStats"), XmlArrayItem(typeof(StartupStats), ElementName = "StartupStat")]
        public List<StartupStats> StartupStats;
        [XmlArray("Items"), XmlArrayItem(typeof(ItemStrings), ElementName = "Item")]
        public List<ItemStrings> Items;
        [XmlArray("Skills"), XmlArrayItem(typeof(ItemStrings), ElementName = "Skill")]
        public List<ItemStrings> Skills;
        [XmlArray("Stats"), XmlArrayItem(typeof(ItemStrings), ElementName = "Stat")]
        public List<ItemStrings> Stats;
    }

    public static class Globals
    {
        public static GameElements GameElements;
    }

    public class EpizodeXML
    {
        public int ID;
        public string Text;

        public List<Inventory> Inventories;
        public List<Skill> Skills;
        public List<Stat> Stats;
        public Choices Choices;
        public byte[] image;
    }

    [Serializable]
    [XmlRoot("Epizode")]
    public class Epizode
    {
        [XmlElement("EpizodeNumber")]
        public int EpizodeNumber;
        [XmlElement("EpizodeText")]
        public string EpizodeText;
        [XmlArray("Inventories"), XmlArrayItem(typeof(Inventory), ElementName = "Inventory")]
        public List<Inventory> lstInventories;
        [XmlArray("Skills"), XmlArrayItem(typeof(Skill), ElementName = "Skill")]
        public List<Skill> lstSkills;
        [XmlArray("Stats"), XmlArrayItem(typeof(Stat), ElementName = "Stat")]
        public List<Stat> lstStats;
        public Epizode()
        {
            this.lstInventories = new List<Inventory>();
            this.lstSkills = new List<Skill>();
            this.lstStats = new List<Stat>();
            this.EpizodeNumber = 0;
        }
    }

    public class StartupStats
    {
        public string Name { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
    }

    public class Inventory
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public bool Action { get; set; }
    }

    public class Skill
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public bool Action { get; set; }
    }

    public class Stat
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public bool Action { get; set; }
        public bool Reset { get; set; }
    }

    public class Decision
    {
        public int GoTo;
        public string Text;
    }

    public class InventoryCondition : Decision
    {
        public string Name;
        public int Quantity;
        public bool IsAvailable;
    }

    public class Chance : Decision
    {
        public double Probability;
    }

    public class Battle : Decision
    {
        public int Lose;
        public int EnemyStrength;
        public int EnemyHealth;
    }

    public class Predicate
    {
        public PredicateTypes Type { get; set; }
        public string Name { get; set; }
        public bool IsAvailable { get; set; }
        public int Quantity { get; set; }
    }

    public class Condition : Decision
    {
        public List<Predicate> Predicates;
    }

    public class ChanceRollBack : Condition
    {
    }

    public class ItemStrings
    {
        public string Name { get; set; }
    }

    public enum ConnectionTypes
    {
        eDecision,
        eChance,
        eBattle,
        eCondition,
        eInventoryCondition,
    }

    public enum PredicateTypes
    {
        eInventory,
        eStat,
        eSkill,
    }

    [Serializable]
    [XmlRoot("Connection")]
    public class ConnectionXML
    {
        [XmlElement("Type")]
        public ConnectionTypes Type;
        [XmlElement("Decision")]
        public Decision Decision;
        [XmlElement("Chance")]
        public Chance Chance;
        [XmlElement("Battle")]
        public Battle Battle;
        [XmlElement("Condition")]
        public Condition Condition;
        [XmlElement("InventoryCondition")]
        public InventoryCondition InventoryCondition;
    }

    public class Choices
    {
        public List<Decision> Decisions;
        public List<Chance> Chances;
        public List<Battle> Battles;
        public List<Condition> Conditions;
        public List<InventoryCondition> InventoryConditions;
        public List<ChanceRollBack> ChanceRollBacks;

        public Choices()
        {
            this.Decisions = new List<Decision>();
            this.Chances = new List<Chance>();
            this.Battles = new List<Battle>();
            this.Conditions = new List<Condition>();
            this.InventoryConditions = new List<InventoryCondition>();
        }
    }

    [Serializable]
    [XmlRoot("Game")]
    public class Game
    {
        //[XmlArray("Skills"), XmlArrayItem(typeof(Skills), ElementName = "Skill")]
        //public List<Skills> lstSkills{ get; set; }

        [XmlArray("Stats"), XmlArrayItem(typeof(StartupStats), ElementName = "Stat")]
        public List<StartupStats> lstStats { get; set; }

        [XmlArray("Epizodes"), XmlArrayItem(typeof(EpizodeXML), ElementName = "Epizode")]
        public List<EpizodeXML> lstEpizodes { get; set; }
    }

    public class Skills
    {
        public string Name { get; set; }
    }

    public class PersonStats
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public bool IsGreen { get; set; }
        public bool IsRed { get; set; }
    }

    //[Serializable]
    //[XmlRoot("SaveGameData")]
    //public class SaveGameData
    //{
    //    [XmlElement("CurrentEpizode")]
    //    public int CurrentEpizode { get; set; }

    //    [XmlArray("InventoryList"), XmlArrayItem(typeof(Inventory), ElementName = "Inventory")]
    //    public List<Inventory> lstInventory { get; set; }

    //    [XmlArray("StatsList"), XmlArrayItem(typeof(PersonStats), ElementName = "Stats")]
    //    public List<PersonStats> lstStats { get; set; }
    //}


    public class SaveGameData
    {
        public int CurrentEpizode { get; set; }
        public List<Inventory> lstInventory { get; set; }
        public List<PersonStats> lstStats { get; set; }
    }
}