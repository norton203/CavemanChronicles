// Add to existing Monster.cs - make sure properties have setters
namespace CavemanChronicles
{
    public class Monster
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public MonsterType Type { get; set; }
        public TechnologyEra Era { get; set; }
        public int ChallengeRating { get; set; }
        public int ArmorClass { get; set; }
        public int HitPoints { get; set; }
        public int HitDice { get; set; }
        public int HitDieSize { get; set; }
        public int Speed { get; set; }

        public MonsterStats Stats { get; set; } = new MonsterStats();
        public List<MonsterAttack> Attacks { get; set; } = new List<MonsterAttack>();
        public List<MonsterAbility> SpecialAbilities { get; set; } = new List<MonsterAbility>();

        public int MinGold { get; set; }
        public int MaxGold { get; set; }
        public int ExperienceValue { get; set; }
        public List<string> PossibleLoot { get; set; } = new List<string>();

        public List<string> FlavorText { get; set; } = new List<string>();
        public string DefeatedText { get; set; } = "";

        public string GetRandomFlavorText()
        {
            if (FlavorText == null || FlavorText.Count == 0)
                return $"A {Name} appears!";

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

        public int StrengthMod => (Strength - 10) / 2;
        public int DexterityMod => (Dexterity - 10) / 2;
        public int ConstitutionMod => (Constitution - 10) / 2;
        public int IntelligenceMod => (Intelligence - 10) / 2;
        public int WisdomMod => (Wisdom - 10) / 2;
        public int CharismaMod => (Charisma - 10) / 2;
    }

    public class MonsterAttack
    {
        public string Name { get; set; } = "";
        public int AttackBonus { get; set; }
        public int DamageDiceCount { get; set; }
        public int DamageDieSize { get; set; }
        public int DamageBonus { get; set; }
        public DamageType DamageType { get; set; }
        public string Description { get; set; } = "";

        public string GetDamageString()
        {
            return $"{DamageDiceCount}d{DamageDieSize}+{DamageBonus} {DamageType}";
        }
    }

    public class MonsterAbility
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public AbilityType Type { get; set; }
    }

    public enum MonsterType
    {
        Beast,
        Humanoid,
        Undead,
        Construct,
        Dragon,
        Elemental,
        Aberration,
        Monstrosity
    }

    public enum DamageType
    {
        Bludgeoning,
        Piercing,
        Slashing,
        Fire,
        Cold,
        Lightning,
        Poison,
        Acid,
        Psychic,
        Radiant,
        Necrotic,
        Force
    }

    public enum AbilityType
    {
        Passive,
        Action,
        Reaction,
        BonusAction
    }
}