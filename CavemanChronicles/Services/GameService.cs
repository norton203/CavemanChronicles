namespace CavemanChronicles
{
    public class GameService
    {
        public Character Player { get; set; }
        private Random _random = new Random();
        private AudioService? _audioService;
        private MonsterLoaderService? _monsterLoader;
        private CombatService? _combatService;

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
            _combatService.SetAudioService(_audioService);
        }

        public async Task<string> ProcessCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return "You stand idle, contemplating your existence.";

            command = command.ToLower().Trim();

            // Check if in combat
            if (_combatService?.CurrentCombat != null && _combatService.CurrentCombat.Status == CombatStatus.Ongoing)
            {
                return "You're in combat! Use combat commands: 'attack', 'dodge', 'flee'";
            }

            // Basic command parsing
            if (command.Contains("attack") || command.Contains("fight") || command.Contains("battle"))
                return await HandleStartCombat();

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

            return $"You don't know how to '{command}'. Try 'help' for available commands.";
        }

        private async Task<string> HandleStartCombat()
        {
            if (_monsterLoader == null || _combatService == null)
                return "Combat system not available!";

            // Get random encounter
            var monster = _monsterLoader.GetRandomMonster(Player.CurrentEra, Player.Level);

            if (monster == null)
                return "No monsters found for your current era!";

            // Start combat
            var combat = _combatService.InitiateCombat(Player, monster);

            // Return combat log
            return string.Join("\n", combat.CombatLog);
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
                _ => "You see... things."
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
                   $"Strength: {Player.Stats.Strength}\n" +
                   $"Dexterity: {Player.Stats.Dexterity}\n" +
                   $"Intelligence: {Player.Stats.Intelligence}";
        }

        private async Task<string> HandleRest()
        {
            int healAmount = _random.Next(20, 40);
            Player.Health = Math.Min(Player.Health + healAmount, Player.MaxHealth);

            // Optional: Play rest sound
            if (_audioService != null)
                await _audioService.PlaySoundEffect("button_click.wav");

            return $"You rest and recover {healAmount} health. Current HP: {Player.Health}/{Player.MaxHealth}";
        }

        private async Task<string> HandleExplore()
        {
            // Random exploration outcomes
            int outcome = _random.Next(1, 4);

            return outcome switch
            {
                1 => "You explore and find nothing of interest.",
                2 => await FindRandomItem(),
                3 => "You discover a hidden path, but decide not to follow it... yet.",
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

            // Optional: Play item found sound
            if (_audioService != null)
                await _audioService.PlaySoundEffect("button_click.wav");

            return $"You found: {item.Name}!";
        }

        private string GetRandomItemForEra()
        {
            return Player.CurrentEra switch
            {
                TechnologyEra.Caveman => _random.Next(2) == 0 ? "Sharp Rock" : "Wooden Club",
                TechnologyEra.StoneAge => _random.Next(2) == 0 ? "Stone Axe" : "Flint Spear",
                TechnologyEra.BronzeAge => _random.Next(2) == 0 ? "Bronze Sword" : "Bronze Shield",
                TechnologyEra.IronAge => _random.Next(2) == 0 ? "Iron Blade" : "Iron Armor",
                TechnologyEra.Medieval => _random.Next(2) == 0 ? "Steel Longsword" : "Chainmail",
                TechnologyEra.Renaissance => _random.Next(2) == 0 ? "Rapier" : "Musket",
                TechnologyEra.Industrial => _random.Next(2) == 0 ? "Revolver" : "Rifle",
                TechnologyEra.Modern => _random.Next(2) == 0 ? "Tactical Gear" : "Combat Rifle",
                TechnologyEra.Future => _random.Next(2) == 0 ? "Plasma Sword" : "Energy Shield",
                _ => "Mysterious Object"
            };
        }

        private string HandleHelp()
        {
            return "=== AVAILABLE COMMANDS ===\n" +
                   "attack / fight - Engage in combat\n" +
                   "look / examine - Observe surroundings\n" +
                   "inventory / items - Check inventory\n" +
                   "stats / status - View character stats\n" +
                   "rest / sleep - Recover health\n" +
                   "explore / search - Search for items\n" +
                   "help - Show this message";
        }

        private async Task<string> LevelUp()
        {
            // Play level up sound
            if (_audioService != null)
                await _audioService.PlaySoundEffect("level_up.wav");

            Player.Level++;
            Player.Experience = 0;
            Player.MaxHealth += 10;
            Player.Health = Player.MaxHealth;
            Player.Stats.Strength++;
            Player.Stats.Dexterity++;
            Player.Stats.Intelligence++;

            UpdateTechnologyEra();

            string message = $"*** LEVEL UP! ***\nYou are now level {Player.Level}!";

            if (HasEraChanged())
            {
                message += $"\n\nYou have advanced to the {Player.CurrentEra} ERA!\nNew technologies are now available!";
            }

            return message;
        }

        private bool HasEraChanged()
        {
            var oldEra = GetEraForLevel(Player.Level - 1);
            var newEra = GetEraForLevel(Player.Level);
            return oldEra != newEra;
        }

        private void UpdateTechnologyEra()
        {
            Player.CurrentEra = GetEraForLevel(Player.Level);
        }

        private TechnologyEra GetEraForLevel(int level)
        {
            return level switch
            {
                <= 5 => TechnologyEra.Caveman,
                <= 10 => TechnologyEra.StoneAge,
                <= 15 => TechnologyEra.BronzeAge,
                <= 20 => TechnologyEra.IronAge,
                <= 25 => TechnologyEra.Medieval,
                <= 30 => TechnologyEra.Renaissance,
                <= 35 => TechnologyEra.Industrial,
                <= 40 => TechnologyEra.Modern,
                _ => TechnologyEra.Future
            };
        }
    }
}