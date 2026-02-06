namespace CavemanChronicles
{
    public class Item
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ItemType ItemType { get; set; }
        public ItemRarity Rarity { get; set; }
        public TechnologyEra Era { get; set; }

        // Value
        public int Value { get; set; } // Gold cost
        public int Weight { get; set; } // For inventory management

        // Stacking
        public bool IsStackable { get; set; }
        public int MaxStackSize { get; set; }
        public int Quantity { get; set; }

        // Consumable Effects
        public ConsumableEffect? Effect { get; set; }

        // Equipment Stats (for weapons/armor)
        public EquipmentStats? EquipmentStats { get; set; }
        public EquipmentSlot EquipmentSlot { get; set; }

        // Visual
        public string IconEmoji { get; set; }

        public Item()
        {
            Quantity = 1;
            MaxStackSize = 99;
        }
    }

    public class ConsumableEffect
    {
        public EffectType Type { get; set; }
        public int Value { get; set; }
        public int Duration { get; set; } // In turns, 0 = instant
        public string Description { get; set; }
    }

    public class EquipmentStats
    {
        // Offensive
        public int AttackBonus { get; set; }
        public int DamageDiceCount { get; set; }
        public int DamageDieSize { get; set; }
        public int DamageBonus { get; set; }
        public DamageType DamageType { get; set; }

        // Defensive
        public int ArmorClass { get; set; }

        // Ability Score Bonuses
        public int StrengthBonus { get; set; }
        public int DexterityBonus { get; set; }
        public int ConstitutionBonus { get; set; }
        public int IntelligenceBonus { get; set; }
        public int WisdomBonus { get; set; }
        public int CharismaBonus { get; set; }

        // Special Properties
        public List<string> SpecialProperties { get; set; }

        public EquipmentStats()
        {
            SpecialProperties = new List<string>();
            DamageType = DamageType.Bludgeoning;
        }
    }

    public enum ItemType
    {
        Consumable,
        Weapon,
        Armor,
        Shield,
        Accessory,
        QuestItem,
        Material,
        Currency,
        Misc
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum EffectType
    {
        HealHP,
        RestoreMP,
        BuffStrength,
        BuffDexterity,
        BuffConstitution,
        BuffIntelligence,
        BuffWisdom,
        BuffCharisma,
        BuffArmor,
        BuffAttack,
        CurePoison,
        CureDisease,
        ReviveAlly
    }

    public enum EquipmentSlot
    {
        None,
        MainHand,
        OffHand,
        Head,
        Chest,
        Legs,
        Feet,
        Hands,
        Neck,
        Ring,
        Back
    }
}