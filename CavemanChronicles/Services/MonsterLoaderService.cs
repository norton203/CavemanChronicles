using System.Text.Json;
using System.Text.Json.Serialization;

namespace CavemanChronicles
{
    public class MonsterLoaderService
    {
        private Dictionary<TechnologyEra, List<Monster>> _monstersByEra;
        private bool _isLoaded = false;

        public MonsterLoaderService()
        {
            _monstersByEra = new Dictionary<TechnologyEra, List<Monster>>();
        }

        public async Task LoadAllMonsters()
        {
            if (_isLoaded)
                return;

            try
            {
                // Load monsters for each era
                await LoadMonstersForEra("monsters_caveman.json", TechnologyEra.Caveman);
                await LoadMonstersForEra("monsters_stoneage.json", TechnologyEra.StoneAge);
                await LoadMonstersForEra("monsters_bronzeage.json", TechnologyEra.BronzeAge);
                await LoadMonstersForEra("monsters_ironage.json", TechnologyEra.IronAge);
                await LoadMonstersForEra("monsters_medieval.json", TechnologyEra.Medieval);
                await LoadMonstersForEra("monsters_renaissance.json", TechnologyEra.Renaissance);
                await LoadMonstersForEra("monsters_industrial.json", TechnologyEra.Industrial);
                await LoadMonstersForEra("monsters_modern.json", TechnologyEra.Modern);
                await LoadMonstersForEra("monsters_future.json", TechnologyEra.Future);

                _isLoaded = true;
                System.Diagnostics.Debug.WriteLine($"Loaded {GetTotalMonsterCount()} monsters across all eras.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading monsters: {ex.Message}");
            }
        }

        private async Task LoadMonstersForEra(string fileName, TechnologyEra era)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Attempting to load {fileName} for era {era}...");

                var filePath = Path.Combine("GameData", fileName);
                System.Diagnostics.Debug.WriteLine($"Full path: {filePath}");

                using var stream = await FileSystem.OpenAppPackageFileAsync(filePath);
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();

                System.Diagnostics.Debug.WriteLine($"JSON loaded, length: {json.Length}");

                // ADD JsonStringEnumConverter to handle string->enum conversion
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                };

                var monsterFile = JsonSerializer.Deserialize<MonsterFile>(json, options);

                if (monsterFile?.Monsters != null)
                {
                    _monstersByEra[era] = monsterFile.Monsters;
                    System.Diagnostics.Debug.WriteLine($"✓ Loaded {monsterFile.Monsters.Count} monsters for {era}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"✗ MonsterFile or Monsters list is null for {fileName}");
                    _monstersByEra[era] = new List<Monster>();
                }
            }
            catch (FileNotFoundException ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ File not found: {fileName}");
                System.Diagnostics.Debug.WriteLine($"  Error: {ex.Message}");
                _monstersByEra[era] = new List<Monster>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Error loading {fileName}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"  Stack: {ex.StackTrace}");
                _monstersByEra[era] = new List<Monster>();
            }
        }

        public List<Monster> GetMonstersForEra(TechnologyEra era)
        {
            if (!_isLoaded)
            {
                System.Diagnostics.Debug.WriteLine("Warning: Monsters not loaded yet!");
                return new List<Monster>();
            }

            return _monstersByEra.ContainsKey(era) ? _monstersByEra[era] : new List<Monster>();
        }

        public List<Monster> GetMonstersForEra(TechnologyEra era, int minCR, int maxCR)
        {
            return GetMonstersForEra(era)
                .Where(m => m.ChallengeRating >= minCR && m.ChallengeRating <= maxCR)
                .ToList();
        }

        public Monster? GetRandomMonster(TechnologyEra era, int playerLevel)
        {
            if (!_isLoaded)
            {
                System.Diagnostics.Debug.WriteLine("Warning: Monsters not loaded yet!");
                return null;
            }

            // Get appropriate CR range (player level-based)
            int minCR = Math.Max(0, (playerLevel - 2) / 5);
            int maxCR = (playerLevel + 2) / 5;

            var appropriateMonsters = GetMonstersForEra(era, minCR, maxCR);

            if (appropriateMonsters.Count == 0)
            {
                // Fallback to any monster from the era
                appropriateMonsters = GetMonstersForEra(era);
            }

            if (appropriateMonsters.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine($"No monsters found for era {era}");
                return null;
            }

            var random = new Random();
            return appropriateMonsters[random.Next(appropriateMonsters.Count)];
        }

        public Monster? GetMonsterByName(string name, TechnologyEra era)
        {
            return GetMonstersForEra(era)
                .FirstOrDefault(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public int GetTotalMonsterCount()
        {
            return _monstersByEra.Values.Sum(list => list.Count);
        }

        public List<Monster> GetAllMonsters()
        {
            var allMonsters = new List<Monster>();
            foreach (var monsterList in _monstersByEra.Values)
            {
                allMonsters.AddRange(monsterList);
            }
            return allMonsters;
        }
    }

    // Helper class for JSON deserialization
    public class MonsterFile
    {
        public string Era { get; set; }
        public List<Monster> Monsters { get; set; }
    }
}