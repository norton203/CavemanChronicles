namespace CavemanChronicles
{
    public class GameService
    {
        public Character Player { get; set; }
        private AudioService? _audioService;
        private MonsterLoaderService? _monsterLoader;
        private CombatService? _combatService;
        private INavigation? _navigation;

        public void SetAudioService(AudioService audioService)
        {
            _audioService = audioService;
        }

        public void SetMonsterLoader(MonsterLoaderService monsterLoader)
        {
            _monsterLoader = monsterLoader;
        }

        public void SetCombatService(CombatService combatService)
        {
            _combatService = combatService;
        }

        public void SetNavigation(INavigation navigation)
        {
            _navigation = navigation;
        }

        public void StartNewGame(string playerName)
        {
            Player = new Character
            {
                Name = playerName,
                Level = 1,
                Experience = 0,
                Health = 100,
                MaxHealth = 100,
                CurrentEra = TechnologyEra.Caveman,
                Inventory = new List<Item>(),
                Stats = new CharacterStats
                {
                    Strength = 5,
                    Dexterity = 5,
                    Intelligence = 5
                }
            };
        }

        public async Task<string> ProcessCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return "You stand idle, contemplating your existence.";

            command = command.ToLower().Trim();

            // Basic command parsing
            if (command.Contains("attack") || command.Contains("fight") || command.Contains("battle"))
                return await HandleAttack(command);

            if (command.Contains("look") || command.Contains("examine"))
                return HandleLook(command);

            if (command.Contains("inventory") || command.Contains("items"))
                return HandleInventory();

            if (command.Contains("stats") || command.Contains("status"))
                return HandleStats();

            if (command.Contains("rest") || command.Contains("sleep"))
                return await HandleRest();

            if (command.Contains("explore") || command.Contains("search"))
                return await HandleExplore();

            if (command.Contains("help"))
                return HandleHelp();

            // Unknown command
            return $"You don't know how to '{command}'. Try 'help' for available commands.";
        }

        private async Task<string> HandleAttack(string command)
        {
            // Check if we have required services
            if (_monsterLoader == null || _combatService == null)
            {
                return "Combat system not fully initialized!";
            }

            // Load monsters for current era
            var monsters = _monsterLoader.GetMonstersByEra(Player.CurrentEra);
            if (monsters == null || monsters.Count == 0)
            {
                return "No monsters found for your current era!";
            }

            // Select random monster(s) for encounter
            int encounterSize = Random.Shared.Next(1, 4); // 1-3 enemies
            var enemyList = new List<Monster>();

            for (int i = 0; i < encounterSize; i++)
            {
                var randomMonster = monsters[Random.Shared.Next(monsters.Count)];
                // Create a copy so each enemy is independent
                var enemy = new Monster
                {
                    Name = randomMonster.Name,
                    Description = randomMonster.Description,
                    Type = randomMonster.Type,
                    ChallengeRating = randomMonster.ChallengeRating,
                    ArmorClass = randomMonster.ArmorClass,
                    HitPoints = randomMonster.HitPoints,
                    MaxHitPoints = randomMonster.HitPoints,
                    HitDice = randomMonster.HitDice,
                    HitDieSize = randomMonster.HitDieSize,
                    Speed = randomMonster.Speed,
                    Stats = randomMonster.Stats,
                    Attacks = randomMonster.Attacks,
                    SpecialAbilities = randomMonster.SpecialAbilities,
                    MinGold = randomMonster.MinGold,
                    MaxGold = randomMonster.MaxGold,
                    ExperienceValue = randomMonster.ExperienceValue,
                    PossibleLoot = randomMonster.PossibleLoot,
                    FlavorText = randomMonster.FlavorText,
                    DefeatedText = randomMonster.DefeatedText
                };
                enemyList.Add(enemy);
            }

            // Launch visual combat page
            if (_navigation != null && _audioService != null)
            {
                var combatPage = new CombatPage(Player, enemyList, _combatService, _audioService);
                await _navigation.PushAsync(combatPage);
                return ""; // Combat page handles everything
            }

            return "Unable to start combat (navigation not set up).";
        }

        private string HandleLook(string command)
        {
            return Player.CurrentEra switch
            {
                TechnologyEra.Caveman => "You see rough stone walls, primitive fire, and animal bones scattered about.",
                TechnologyEra.StoneAge => "Carved stones and simple tools surround you. Progress is evident.",
                TechnologyEra.BronzeAge => "Bronze weapons and pottery line the walls. Civilization advances.",
                TechnologyEra.IronAge => "Iron tools and fortifications mark this era of strength.",
                TechnologyEra.Medieval => "Castles, swords, and tapestries define this age of knights.",
                TechnologyEra.Renaissance => "Art, science, and innovation flourish around you.",
                TechnologyEra.Industrial => "Steam engines and factories dominate the landscape.",
                TechnologyEra.Modern => "Technology and skyscrapers reach toward the sky.",
                TechnologyEra.Future => "Advanced technology beyond comprehension surrounds you.",
                _ => "You see.. things."
            };
        }

        private string HandleInventory()
        {
            if (Player.Inventory == null || Player.Inventory.Count == 0)
                return "Your inventory is empty. You carry only your determination.";

            string items = "INVENTORY:\n";
            foreach (var item in Player.Inventory)
            {
                items += $"- {item.Name}\n";
            }
            return items;
        }

        private string HandleStats()
        {
            return $"=== CHARACTER STATUS ===\n" +
                   $"Name: {Player.Name}\n" +
                   $"Level: {Player.Level}\n" +
                   $"Era: {Player.CurrentEra}\n" +
                   $"Health: {Player.Health}/{Player.MaxHealth}\n" +
                   $"Experience: {Player.Experience}/{Player.Level * 100}\n" +
                   $"Gold: {Player.Gold} GP\n" +
                   $"Strength: {Player.Stats.Strength}\n" +
                   $"Dexterity: {Player.Stats.Dexterity}\n" +
                   $"Intelligence: {Player.Stats.Intelligence}";
        }

        private async Task<string> HandleRest()
        {
            int healAmount = Random.Shared.Next(20, 40);
            Player.Health = Math.Min(Player.Health + healAmount, Player.MaxHealth);

            if (_audioService != null)
                await _audioService.PlaySoundEffect("button_click.wav");

            return $"You rest and recover {healAmount} health. Current HP: {Player.Health}/{Player.MaxHealth}";
        }

        private async Task<string> HandleExplore()
        {
            int outcome = Random.Shared.Next(1, 4);

            return outcome switch
            {
                1 => "You explore and find nothing of interest.",
                2 => await FindRandomItem(),
                3 => "You discover a hidden path, but decide not to follow it.. yet.",
                _ => "Your exploration yields no results."
            };
        }

        private async Task<string> FindRandomItem()
        {
            var item = new Item
            {
                Name = GetRandomItemForEra(),
                ItemType = ItemType.Weapon
            };

            Player.Inventory.Add(item);

            if (_audioService != null)
                await _audioService.PlaySoundEffect("button_click.wav");

            return $"You found: {item.Name}!";
        }

        private string GetRandomItemForEra()
        {
            return Player.CurrentEra switch
            {
                TechnologyEra.Caveman => Random.Shared.Next(2) == 0 ? "Sharp Rock" : "Wooden Club",
                TechnologyEra.StoneAge => Random.Shared.Next(2) == 0 ? "Stone Axe" : "Flint Spear",
                TechnologyEra.BronzeAge => Random.Shared.Next(2) == 0 ? "Bronze Sword" : "Bronze Shield",
                TechnologyEra.IronAge => Random.Shared.Next(2) == 0 ? "Iron Blade" : "Iron Armor",
                TechnologyEra.Medieval => Random.Shared.Next(2) == 0 ? "Steel Longsword" : "Chainmail",
                TechnologyEra.Renaissance => Random.Shared.Next(2) == 0 ? "Rapier" : "Musket",
                TechnologyEra.Industrial => Random.Shared.Next(2) == 0 ? "Revolver" : "Rifle",
                TechnologyEra.Modern => Random.Shared.Next(2) == 0 ? "Tactical Gear" : "Combat Rifle",
                TechnologyEra.Future => Random.Shared.Next(2) == 0 ? "Plasma Sword" : "Energy Shield",
                _ => "Mysterious Object"
            };
        }

        private string HandleHelp()
        {
            return "=== AVAILABLE COMMANDS ===\n" +
                   "attack / fight / battle - Start combat\n" +
                   "look / examine - Observe surroundings\n" +
                   "inventory / items - Check inventory\n" +
                   "stats / status - View character stats\n" +
                   "rest / sleep - Recover health\n" +
                   "explore / search - Search for items\n" +
                   "help - Show this message";
        }
    }
}