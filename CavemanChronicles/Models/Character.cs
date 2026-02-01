namespace CavemanChronicles
{
    public class Character
    {
        public string Name { get; set; }
        public Race Race { get; set; }
        public CharacterClass Class { get; set; }
        public string Background { get; set; }
        public string Backstory { get; set; } // Optional
        public string Alignment { get; set; }

        public int Level { get; set; }
        public int Experience { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int HitDice { get; set; }

        public TechnologyEra CurrentEra { get; set; }

        // Core D&D Stats
        public CharacterStats Stats { get; set; }

        // Combat Stats
        public int ArmorClass { get; set; }
        public int Initiative { get; set; }
        public int Speed { get; set; }
        public int ProficiencyBonus { get; set; }

        // Skills
        public List<Skill> Skills { get; set; }

        public List<Item> Inventory { get; set; }
        public List<string> Languages { get; set; }

        // Gold/Currency
        public int Gold { get; set; }
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
        public string Ability { get; set; } // STR, DEX, CON, INT, WIS, CHA
        public bool IsProficient { get; set; }
        public int Bonus { get; set; }
    }

    public class Item
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ItemType ItemType { get; set; }
        public int Value { get; set; }
        public int Damage { get; set; } // For weapons
        public int ArmorBonus { get; set; } // For armor
    }

    public enum ItemType
    {
        Weapon,
        Armor,
        Consumable,
        Quest,
        Misc
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
}