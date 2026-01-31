namespace CavemanChronicles
{
    public class Character
    {
        public string Name { get; set; }
        public Race Race { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public TechnologyEra CurrentEra { get; set; }
        public List<Item> Inventory { get; set; }
        public CharacterStats Stats { get; set; }
    }

    public enum TechnologyEra
    {
        Caveman,      // Level 1-5
        StoneAge,     // Level 6-10
        BronzeAge,    // Level 11-15
        IronAge,      // Level 16-20
        Medieval,     // Level 21-25
        Renaissance,  // Level 26-30
        Industrial,   // Level 31-35
        Modern,       // Level 36-40
        Future        // Level 41+
    }

    public class CharacterStats
    {
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Intelligence { get; set; }
    }

    public class Item
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ItemType ItemType { get; set; }
        public int Value { get; set; }
    }

    public enum ItemType
    {
        Weapon,
        Armor,
        Consumable,
        Quest,
        Misc
    }
}