namespace CavemanChronicles
{
    public class Monster
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public MonsterType Type { get; set; }
        public TechnologyEra Era { get; set; }

        // Combat Stats
        public int ChallengeRating { get; set; }
        public int ArmorClass { get; set; }
        public int HitPoints { get; set; }
        public int MaxHitPoints { get; set; }
        public int HitDice { get; set; }
        public int HitDieSize { get; set; }
        public int Speed { get; set; }

        // Ability Scores
        public MonsterStats Stats { get; set; }

        // Attacks and Abilities
        public List<MonsterAttack> Attacks { get; set; }
        public List<MonsterAbility> SpecialAbilities { get; set; }

        // Loot and Rewards
        public int MinGold { get; set; }
        public int MaxGold { get; set; }
        public int ExperienceValue { get; set; }
        public List<string> PossibleLoot { get; set; }

        // Flavor Text
        public List<string> FlavorText { get; set; }
        public string DefeatedText { get; set; }

        // ADDED METHOD
        public string GetRandomFlavorText()
        {
            if (FlavorText == null || FlavorText.Count == 0)
                return $"The {Name} prepares to attack!";

            var random = new Random();
            return FlavorText[random.Next(FlavorText.Count)];
        }
    }

    public class MonsterStats
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

    public class MonsterAttack
    {
        public string Name { get; set; }
        public int AttackBonus { get; set; }
        public int DamageDiceCount { get; set; }
        public int DamageDieSize { get; set; }
        public int DamageBonus { get; set; }
        public DamageType DamageType { get; set; }
        public string Description { get; set; }
    }

    public class MonsterAbility
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public AbilityType Type { get; set; }
    }

    public enum MonsterType
    {
        Beast,
        Humanoid,
        Undead,
        Dragon,
        Construct,
        Elemental,
        Aberration,
        Monstrosity
    }

    public enum DamageType
    {
        Slashing,
        Piercing,
        Bludgeoning,
        Fire,
        Cold,
        Lightning,
        Poison,
        Acid,
        Thunder,
        Force,
        Necrotic,
        Radiant,
        Psychic
    }

    public enum AbilityType
    {
        Passive,
        Action,
        BonusAction,
        Reaction
    }
}