namespace CavemanChronicles
{
    public enum Race
    {
        Human,
        Elf,
        Dwarf,
        HalfOrc,
        Halfling
    }

    public class RaceStats
    {
        public Race Race { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int StrengthBonus { get; set; }
        public int DexterityBonus { get; set; }
        public int IntelligenceBonus { get; set; }
        public int HealthBonus { get; set; }
    }

    public static class RaceData
    {
        public static List<RaceStats> AllRaces = new List<RaceStats>
        {
            new RaceStats
            {
                Race = Race.Human,
                Name = "Human",
                Description = "Versatile and adaptable. Humans are well-rounded survivors.",
                StrengthBonus = 1,
                DexterityBonus = 1,
                IntelligenceBonus = 1,
                HealthBonus = 5
            },
            new RaceStats
            {
                Race = Race.Elf,
                Name = "Elf",
                Description = "Swift and perceptive. Elves excel in agility and awareness.",
                StrengthBonus = 0,
                DexterityBonus = 3,
                IntelligenceBonus = 2,
                HealthBonus = 0
            },
            new RaceStats
            {
                Race = Race.Dwarf,
                Name = "Dwarf",
                Description = "Sturdy and resilient. Dwarves are tough as stone.",
                StrengthBonus = 2,
                DexterityBonus = 0,
                IntelligenceBonus = 1,
                HealthBonus = 15
            },
            new RaceStats
            {
                Race = Race.HalfOrc,
                Name = "Half-Orc",
                Description = "Powerful and fierce. Half-Orcs dominate in raw strength.",
                StrengthBonus = 4,
                DexterityBonus = 0,
                IntelligenceBonus = -1,
                HealthBonus = 10
            },
            new RaceStats
            {
                Race = Race.Halfling,
                Name = "Halfling",
                Description = "Quick and lucky. Halflings are nimble and resourceful.",
                StrengthBonus = -1,
                DexterityBonus = 3,
                IntelligenceBonus = 2,
                HealthBonus = -5
            }
        };

        public static RaceStats GetRaceStats(Race race)
        {
            return AllRaces.First(r => r.Race == race);
        }
    }
}