using System.Text.Json;

namespace CavemanChronicles
{
    public class MonsterLoaderService
    {
        private Dictionary<TechnologyEra, List<Monster>> _monsterCache;
        private bool _isLoaded = false;

        public MonsterLoaderService()
        {
            _monsterCache = new Dictionary<TechnologyEra, List<Monster>>();
        }

        public async Task LoadAllMonsters()
        {
            if (_isLoaded)
                return;

            try
            {
                // Load monsters for each era
                await LoadMonstersForEra(TechnologyEra.Caveman, "GameData\\monsters_caveman.json");
                await LoadMonstersForEra(TechnologyEra.StoneAge, "GameData\\monsters_stoneage.json");
                await LoadMonstersForEra(TechnologyEra.BronzeAge, "GameData\\monsters_bronzeage.json");
                await LoadMonstersForEra(TechnologyEra.IronAge, "GameData\\monsters_ironage.json");
                await LoadMonstersForEra(TechnologyEra.Medieval, "GameData\\monsters_medieval.json");
                await LoadMonstersForEra(TechnologyEra.Renaissance, "GameData\\monsters_renaissance.json");
                await LoadMonstersForEra(TechnologyEra.Industrial, "GameData\\monsters_industrial.json");
                await LoadMonstersForEra(TechnologyEra.Modern, "GameData\\monsters_modern.json");
                await LoadMonstersForEra(TechnologyEra.Future, "GameData\\monsters_future.json");

                _isLoaded = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading monsters: {ex.Message}");
            }
        }

        private async Task LoadMonstersForEra(TechnologyEra era, string fileName)
        {
            try
            {
                // Try to load from embedded resource
                using var stream = await FileSystem.OpenAppPackageFileAsync(fileName).ConfigureAwait(false);  // ✅ Added
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync().ConfigureAwait(false);  // ✅ Added

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                };

                var monsterData = JsonSerializer.Deserialize<MonsterDataFile>(json, options);

                if (monsterData?.Monsters != null)
                {
                    // Set the Era property for each monster
                    foreach (var monster in monsterData.Monsters)
                    {
                        monster.Era = era;
                        monster.MaxHitPoints = monster.HitPoints; // Initialize MaxHitPoints
                    }

                    _monsterCache[era] = monsterData.Monsters;
                    System.Diagnostics.Debug.WriteLine($"Loaded {monsterData.Monsters.Count} monsters for {era}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading {fileName}: {ex.Message}");
                // Initialize empty list for this era so it doesn't crash
                _monsterCache[era] = new List<Monster>();
            }
        }

        // GET MONSTERS BY ERA - THE MISSING METHOD
        public List<Monster> GetMonstersByEra(TechnologyEra era)
        {
            if (!_isLoaded)
            {
                System.Diagnostics.Debug.WriteLine("Warning: Monsters not loaded yet. Call LoadAllMonsters() first.");
                return new List<Monster>();
            }

            if (_monsterCache.ContainsKey(era))
            {
                return _monsterCache[era];
            }

            System.Diagnostics.Debug.WriteLine($"No monsters found for era: {era}");
            return new List<Monster>();
        }

        public Monster GetRandomMonster(TechnologyEra era)
        {
            var monsters = GetMonstersByEra(era);

            if (monsters == null || monsters.Count == 0)
                return null;

            var selectedMonster = monsters[Random.Shared.Next(monsters.Count)];

            // Create a copy so we don't modify the cached version
            return new Monster
            {
                Name = selectedMonster.Name,
                Description = selectedMonster.Description,
                Type = selectedMonster.Type,
                Era = selectedMonster.Era,
                ChallengeRating = selectedMonster.ChallengeRating,
                ArmorClass = selectedMonster.ArmorClass,
                HitPoints = selectedMonster.HitPoints,
                MaxHitPoints = selectedMonster.HitPoints,
                HitDice = selectedMonster.HitDice,
                HitDieSize = selectedMonster.HitDieSize,
                Speed = selectedMonster.Speed,
                Stats = selectedMonster.Stats,
                Attacks = selectedMonster.Attacks,
                SpecialAbilities = selectedMonster.SpecialAbilities,
                MinGold = selectedMonster.MinGold,
                MaxGold = selectedMonster.MaxGold,
                ExperienceValue = selectedMonster.ExperienceValue,
                PossibleLoot = selectedMonster.PossibleLoot,
                FlavorText = selectedMonster.FlavorText,
                DefeatedText = selectedMonster.DefeatedText
            };
        }

        public List<Monster> GetMonstersByType(MonsterType type)
        {
            var result = new List<Monster>();

            foreach (var monsterList in _monsterCache.Values)
            {
                result.AddRange(monsterList.Where(m => m.Type == type));
            }

            return result;
        }

        public List<Monster> GetMonstersByChallengeRating(int minCR, int maxCR)
        {
            var result = new List<Monster>();

            foreach (var monsterList in _monsterCache.Values)
            {
                result.AddRange(monsterList.Where(m => m.ChallengeRating >= minCR && m.ChallengeRating <= maxCR));
            }

            return result;
        }

        public bool IsLoaded => _isLoaded;
    }

    // Helper class for JSON deserialization
    public class MonsterDataFile
    {
        public string Era { get; set; }
        public List<Monster> Monsters { get; set; }
    }
}