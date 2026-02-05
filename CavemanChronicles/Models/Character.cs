namespace CavemanChronicles
{
    public class Character
    {
        public string Name { get; set; }
        public Race Race { get; set; }
        public CharacterClass Class { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int ArmorClass { get; set; }
        public int ProficiencyBonus => 2 + ((Level - 1) / 4); // D&D 5e proficiency bonus
        public TechnologyEra CurrentEra { get; set; }
        public CharacterStats Stats { get; set; }
        public int Gold { get; set; }

        // Inventory System
        public List<Item> Inventory { get; set; }
        public Dictionary<EquipmentSlot, Item> EquippedItems { get; set; }

        // Optional Background
        public Background? Background { get; set; }

        public Character()
        {
            Inventory = new List<Item>();
            EquippedItems = new Dictionary<EquipmentSlot, Item>();
            Stats = new CharacterStats();
            Gold = 100; // Starting gold
            Level = 1;
            Experience = 0;
            CurrentEra = TechnologyEra.Caveman;
            ArmorClass = 10; // Base AC
        }
    }

    public class CharacterStats
    {
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Constitution { get; set; }
        public int Intelligence { get; set; }
        public int Wisdom { get; set; }
        public int Charisma { get; set; }

        // Calculated modifiers
        public int StrengthMod => CalculateModifier(Strength);
        public int DexterityMod => CalculateModifier(Dexterity);
        public int ConstitutionMod => CalculateModifier(Constitution);
        public int IntelligenceMod => CalculateModifier(Intelligence);
        public int WisdomMod => CalculateModifier(Wisdom);
        public int CharismaMod => CalculateModifier(Charisma);

        private int CalculateModifier(int score)
        {
            return (score - 10) / 2;
        }
    }

    public enum CharacterClass
    {
        Fighter,
        Wizard,
        Rogue,
        Cleric,
        Ranger,
        Barbarian
    }

   

    public enum TechnologyEra
    {
        Caveman,
        StoneAge,
        BronzeAge,
        IronAge,
        Medieval,
        Renaissance,
        Industrial,
        Modern,
        Future
    }

    public enum Background
    {
        None,
        Soldier,
        Scholar,
        Criminal,
        Noble,
        FolkHero,
        Hermit
    }
}