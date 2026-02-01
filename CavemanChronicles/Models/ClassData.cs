namespace CavemanChronicles
{
    public class ClassData
    {
        public CharacterClass Class { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int HitDie { get; set; }
        public string PrimaryAbility { get; set; }
        public List<string> SavingThrowProficiencies { get; set; }
        public List<string> SkillProficiencies { get; set; } // Available skills
        public int SkillChoiceCount { get; set; } // How many they can pick
        public List<string> StartingEquipment { get; set; }
        public List<string> ClassFeatures { get; set; }

        public static List<ClassData> AllClasses = new List<ClassData>
        {
            new ClassData
            {
                Class = CharacterClass.Fighter,
                Name = "Fighter",
                Description = "Master of weapons and armor. Fighters excel in combat through superior training and martial prowess.",
                HitDie = 10,
                PrimaryAbility = "STR or DEX",
                SavingThrowProficiencies = new List<string> { "Strength", "Constitution" },
                SkillProficiencies = new List<string>
                {
                    "Acrobatics", "Animal Handling", "Athletics",
                    "History", "Insight", "Intimidation", "Perception", "Survival"
                },
                SkillChoiceCount = 2,
                StartingEquipment = new List<string> { "Chain Mail", "Shield", "Weapon" },
                ClassFeatures = new List<string>
                {
                    "Fighting Style", "Second Wind"
                }
            },
            new ClassData
            {
                Class = CharacterClass.Wizard,
                Name = "Wizard",
                Description = "Scholar of arcane magic. Wizards shape reality through careful study and powerful spells.",
                HitDie = 6,
                PrimaryAbility = "INT",
                SavingThrowProficiencies = new List<string> { "Intelligence", "Wisdom" },
                SkillProficiencies = new List<string>
                {
                    "Arcana", "History", "Insight", "Investigation", "Medicine", "Religion"
                },
                SkillChoiceCount = 2,
                StartingEquipment = new List<string> { "Quarterstaff", "Spellbook", "Robes" },
                ClassFeatures = new List<string>
                {
                    "Spellcasting", "Arcane Recovery"
                }
            },
            new ClassData
            {
                Class = CharacterClass.Rogue,
                Name = "Rogue",
                Description = "Expert in stealth and precision. Rogues rely on cunning and agility to overcome obstacles.",
                HitDie = 8,
                PrimaryAbility = "DEX",
                SavingThrowProficiencies = new List<string> { "Dexterity", "Intelligence" },
                SkillProficiencies = new List<string>
                {
                    "Acrobatics", "Athletics", "Deception", "Insight",
                    "Intimidation", "Investigation", "Perception", "Performance",
                    "Persuasion", "Sleight of Hand", "Stealth"
                },
                SkillChoiceCount = 4,
                StartingEquipment = new List<string> { "Leather Armor", "Dagger", "Thieves' Tools" },
                ClassFeatures = new List<string>
                {
                    "Sneak Attack", "Thieves' Cant", "Expertise"
                }
            },
            new ClassData
            {
                Class = CharacterClass.Cleric,
                Name = "Cleric",
                Description = "Divine conduit of faith. Clerics wield sacred magic to heal allies and smite enemies.",
                HitDie = 8,
                PrimaryAbility = "WIS",
                SavingThrowProficiencies = new List<string> { "Wisdom", "Charisma" },
                SkillProficiencies = new List<string>
                {
                    "History", "Insight", "Medicine", "Persuasion", "Religion"
                },
                SkillChoiceCount = 2,
                StartingEquipment = new List<string> { "Chain Mail", "Shield", "Holy Symbol" },
                ClassFeatures = new List<string>
                {
                    "Spellcasting", "Divine Domain", "Channel Divinity"
                }
            },
            new ClassData
            {
                Class = CharacterClass.Ranger,
                Name = "Ranger",
                Description = "Wilderness warrior. Rangers combine martial skill with nature magic to track and hunt their prey.",
                HitDie = 10,
                PrimaryAbility = "DEX or WIS",
                SavingThrowProficiencies = new List<string> { "Strength", "Dexterity" },
                SkillProficiencies = new List<string>
                {
                    "Animal Handling", "Athletics", "Insight", "Investigation",
                    "Nature", "Perception", "Stealth", "Survival"
                },
                SkillChoiceCount = 3,
                StartingEquipment = new List<string> { "Leather Armor", "Longbow", "Arrows" },
                ClassFeatures = new List<string>
                {
                    "Favored Enemy", "Natural Explorer"
                }
            },
            new ClassData
            {
                Class = CharacterClass.Barbarian,
                Name = "Barbarian",
                Description = "Primal warrior. Barbarians tap into raw fury to become unstoppable forces of destruction.",
                HitDie = 12,
                PrimaryAbility = "STR",
                SavingThrowProficiencies = new List<string> { "Strength", "Constitution" },
                SkillProficiencies = new List<string>
                {
                    "Animal Handling", "Athletics", "Intimidation",
                    "Nature", "Perception", "Survival"
                },
                SkillChoiceCount = 2,
                StartingEquipment = new List<string> { "Hide Armor", "Greataxe", "Javelins" },
                ClassFeatures = new List<string>
                {
                    "Rage", "Unarmored Defense"
                }
            }
        };

        public static ClassData GetClassData(CharacterClass characterClass)
        {
            return AllClasses.First(c => c.Class == characterClass);
        }
    }

    public class BackgroundData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> SkillProficiencies { get; set; }
        public List<string> Equipment { get; set; }
        public string Feature { get; set; }

        public static List<BackgroundData> AllBackgrounds = new List<BackgroundData>
        {
            new BackgroundData
            {
                Name = "Soldier",
                Description = "You served in an organized military force, trained in combat and discipline.",
                SkillProficiencies = new List<string> { "Athletics", "Intimidation" },
                Equipment = new List<string> { "Insignia of Rank", "Trophy" },
                Feature = "Military Rank"
            },
            new BackgroundData
            {
                Name = "Criminal",
                Description = "You lived on the wrong side of the law, surviving through stealth and cunning.",
                SkillProficiencies = new List<string> { "Deception", "Stealth" },
                Equipment = new List<string> { "Crowbar", "Dark Clothes" },
                Feature = "Criminal Contact"
            },
            new BackgroundData
            {
                Name = "Folk Hero",
                Description = "You came from humble beginnings but rose to defend your community from danger.",
                SkillProficiencies = new List<string> { "Animal Handling", "Survival" },
                Equipment = new List<string> { "Artisan's Tools", "Shovel" },
                Feature = "Rustic Hospitality"
            },
            new BackgroundData
            {
                Name = "Sage",
                Description = "You spent years learning the lore of the multiverse through study and research.",
                SkillProficiencies = new List<string> { "Arcana", "History" },
                Equipment = new List<string> { "Bottle of Ink", "Books" },
                Feature = "Researcher"
            },
            new BackgroundData
            {
                Name = "Acolyte",
                Description = "You served in a temple, monastery, or religious institution.",
                SkillProficiencies = new List<string> { "Insight", "Religion" },
                Equipment = new List<string> { "Holy Symbol", "Prayer Book" },
                Feature = "Shelter of the Faithful"
            }
        };
    }
}