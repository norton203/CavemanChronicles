namespace CavemanChronicles
{
    public class Character
    {
        // Basic Info
        public string Name { get; set; }
        public Race Race { get; set; }
        public CharacterClass Class { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }

        // Health & Combat
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int ArmorClass { get; set; }
        public int HitDice { get; set; }
        public int Initiative { get; set; }
        public int Speed { get; set; }

        // Calculated Properties
        public int ProficiencyBonus => 2 + ((Level - 1) / 4); // D&D 5e proficiency bonus

        // Progression
        public TechnologyEra CurrentEra { get; set; }

        // Stats
        public CharacterStats Stats { get; set; }

        // Currency
        public int Gold { get; set; }

        // Inventory System 
        public List<Item> Inventory { get; set; }
        public Dictionary<EquipmentSlot, Item> EquippedItems { get; set; }

        // Character Flavor
        public Background? Background { get; set; }  
        public string Backstory { get; set; }
        public string Alignment { get; set; }

        // Skills & Languages
        public List<Skill> Skills { get; set; }
        public List<string> Languages { get; set; }

        public Character()
        {
            Inventory = new List<Item>();
            EquippedItems = new Dictionary<EquipmentSlot, Item>();
            Stats = new CharacterStats();
            Skills = new List<Skill>();
            Languages = new List<string>();

            Gold = 100; // Starting gold
            Level = 1;
            Experience = 0;
            CurrentEra = TechnologyEra.Caveman;
            ArmorClass = 10; // Base AC
            Speed = 30; // Base speed
            Initiative = 0; // Will be calculated
            Alignment = "Neutral";
            Backstory = "";
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

    public class Skill
    {
        public string Name { get; set; }
        public string Ability { get; set; } // "STR", "DEX", "CON", etc.
        public bool IsProficient { get; set; }
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