using Microsoft.Maui.Platform;

namespace CavemanChronicles
{
    public partial class CharacterCreationPage : ContentPage
    {
        private RaceStats _selectedRace;
        private ClassData _selectedClass;
        private BackgroundData _selectedBackground;
        private readonly SaveService _saveService;
       

        public CharacterCreationPage()
        {
            InitializeComponent();
            _saveService = new SaveService();

            RaceCollectionView.ItemsSource = RaceData.AllRaces;
            ClassCollectionView.ItemsSource = ClassData.AllClasses;
            BackgroundCollectionView.ItemsSource = BackgroundData.AllBackgrounds;
        }

        private async void OnRaceSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count == 0)
                return;

            _selectedRace = e.CurrentSelection[0] as RaceStats;

            if (_selectedRace != null)
            {
                // Update display label
                RaceDisplayLabel.Text = _selectedRace.Name.ToUpper();

                UpdateStatModifiers();
                CheckIfCanProceed();

                // Auto-scroll
                await Task.Delay(100);
                await MainScrollView.ScrollToAsync(ClassCollectionView, ScrollToPosition.MakeVisible, true);
            }
        }

        private async void OnClassSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count == 0)
                return;

            _selectedClass = e.CurrentSelection[0] as ClassData;

            if (_selectedClass != null)
            {
                // Update display label
                ClassDisplayLabel.Text = _selectedClass.Name.ToUpper();

                RollInitialStats();
                UpdateSummary();
                CheckIfCanProceed();

                await Task.Delay(100);
                await MainScrollView.ScrollToAsync(BackgroundCollectionView, ScrollToPosition.MakeVisible, true);
            }
        }

        private void OnBackgroundSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count == 0)
            {
                _selectedBackground = null;
                BackgroundDisplayLabel.Text = "NONE";
                return;
            }

            _selectedBackground = e.CurrentSelection[0] as BackgroundData;

            if (_selectedBackground != null)
            {
                BackgroundDisplayLabel.Text = _selectedBackground.Name.ToUpper();
            }

            CheckIfCanProceed();
        }

        private void OnRollStats(object sender, EventArgs e)
        {
            RollInitialStats();
        }

        private void RollInitialStats()
        {
            // Roll 4d6, drop lowest - classic D&D method
            StrengthEntry.Text = RollAbilityScore().ToString();
            DexterityEntry.Text = RollAbilityScore().ToString();
            ConstitutionEntry.Text = RollAbilityScore().ToString();
            IntelligenceEntry.Text = RollAbilityScore().ToString();
            WisdomEntry.Text = RollAbilityScore().ToString();
            CharismaEntry.Text = RollAbilityScore().ToString();
        }

        private int RollAbilityScore()
        {
            // Roll 4d6, drop the lowest
            var rolls = new List<int>
            {
                Random.Shared.Next(1, 7),
                Random.Shared.Next(1, 7),
                Random.Shared.Next(1, 7),
                Random.Shared.Next(1, 7)
            };

            rolls.Sort();
            return rolls[1] + rolls[2] + rolls[3]; // Sum the three highest
        }

        private void OnStatChanged(object sender, TextChangedEventArgs e)
        {
            UpdateStatModifiers();
            UpdateSummary();
            CheckIfCanProceed();
        }

        private void UpdateStatModifiers()
        {
            // Update modifier labels as stats change
            if (int.TryParse(StrengthEntry.Text, out int str))
            {
                int totalStr = str + (_selectedRace?.StrengthBonus ?? 0);
                StrengthModLabel.Text = GameMath.FormatModifier(GameMath.CalculateModifier(totalStr));
            }

            if (int.TryParse(DexterityEntry.Text, out int dex))
            {
                int totalDex = dex + (_selectedRace?.DexterityBonus ?? 0);
                DexterityModLabel.Text = GameMath.FormatModifier(GameMath.CalculateModifier(totalDex));

                // Update initiative
                InitiativeLabel.Text = GameMath.FormatModifier(GameMath.CalculateModifier(totalDex));
            }

            if (int.TryParse(ConstitutionEntry.Text, out int con))
            {
                ConstitutionModLabel.Text = GameMath.FormatModifier(GameMath.CalculateModifier(con));
            }

            if (int.TryParse(IntelligenceEntry.Text, out int intel))
            {
                int totalInt = intel + (_selectedRace?.IntelligenceBonus ?? 0);
                IntelligenceModLabel.Text = GameMath.FormatModifier(GameMath.CalculateModifier(totalInt));
            }

            if (int.TryParse(WisdomEntry.Text, out int wis))
            {
                WisdomModLabel.Text = GameMath.FormatModifier(GameMath.CalculateModifier(wis));
            }

            if (int.TryParse(CharismaEntry.Text, out int cha))
            {
                CharismaModLabel.Text = GameMath.FormatModifier(GameMath.CalculateModifier(cha));
            }
        }

       

        private CavemanChronicles.Background ParseBackground(string name)
        {
            // Remove spaces for enum parsing
            string enumName = name.Replace(" ", "");

            // Try to parse the string to Background enum
            if (Enum.TryParse<CavemanChronicles.Background>(enumName, out var result))
            {
                return result;
            }

            // Default to None if parsing fails
            return CavemanChronicles.Background.None;
        }

       

        private void UpdateSummary()
        {
            if (_selectedClass == null) return;

            // Calculate HP based on class hit die + CON modifier
            int baseHP = _selectedClass.HitDie;
            if (int.TryParse(ConstitutionEntry.Text, out int con))
            {
                int conMod = GameMath.CalculateModifier(con);
                int totalHP = baseHP + conMod + (_selectedRace?.HealthBonus ?? 0);
                HitPointsLabel.Text = Math.Max(1, totalHP).ToString(); // Minimum 1 HP
            }
        }

        private void OnNameChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfCanProceed();
        }

        private async void CheckIfCanProceed()
        {
            bool hasName = !string.IsNullOrWhiteSpace(NameEntry.Text);
            bool hasRace = _selectedRace != null;
            bool hasClass = _selectedClass != null;
            bool hasStats = HasValidStats();

            bool wasVisible = StartButton.IsVisible;

            if (hasName && hasRace && hasClass && hasStats)
            {
                BackstoryPanel.IsVisible = true;
                StartButton.IsVisible = true;

                // Auto-scroll to start button when it appears
                if (!wasVisible)
                {
                    await Task.Delay(100);
                    await MainScrollView.ScrollToAsync(StartButton, ScrollToPosition.End, true);
                }
            }
            else
            {
                StartButton.IsVisible = false;
            }
        }

        private bool HasValidStats()
        {
            return int.TryParse(StrengthEntry.Text, out _) &&
                   int.TryParse(DexterityEntry.Text, out _) &&
                   int.TryParse(ConstitutionEntry.Text, out _) &&
                   int.TryParse(IntelligenceEntry.Text, out _) &&
                   int.TryParse(WisdomEntry.Text, out _) &&
                   int.TryParse(CharismaEntry.Text, out _);
        }

        // In CharacterCreationPage.xaml.cs, update the OnStartAdventure method:

        private async void OnStartAdventure(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameEntry.Text))
            {
                await DisplayAlert("Missing Name", "Please enter a character name.", "OK");
                return;
            }

            if (_selectedRace == null)
            {
                await DisplayAlert("Missing Race", "Please select a race.", "OK");
                return;
            }

            if (_selectedClass == null)
            {
                await DisplayAlert("Missing Class", "Please select a class.", "OK");
                return;
            }

            if (!HasValidStats())
            {
                await DisplayAlert("Invalid Stats", "Please ensure all ability scores are filled in.", "OK");
                return;
            }

            // Parse stats
            int.TryParse(StrengthEntry.Text, out int str);
            int.TryParse(DexterityEntry.Text, out int dex);
            int.TryParse(ConstitutionEntry.Text, out int con);
            int.TryParse(IntelligenceEntry.Text, out int intel);
            int.TryParse(WisdomEntry.Text, out int wis);
            int.TryParse(CharismaEntry.Text, out int cha);

            // Apply racial bonuses
            str += _selectedRace.StrengthBonus;
            dex += _selectedRace.DexterityBonus;
            intel += _selectedRace.IntelligenceBonus;

            // Calculate derived stats
            int conMod = GameMath.CalculateModifier(con);
            int maxHP = _selectedClass.HitDie + conMod + _selectedRace.HealthBonus;
            maxHP = Math.Max(1, maxHP);

            int initiative = GameMath.CalculateModifier(dex);
            int armorClass = 10 + GameMath.CalculateModifier(dex);

            // Create character
            var character = new Character
            {
                Name = NameEntry.Text.Trim(),
                Race = _selectedRace.Race,
                Class = _selectedClass.Class,
                             
                Backstory = BackstoryEditor.Text?.Trim() ?? "",
                Alignment = "Neutral",

                Level = 1,
                Experience = 0,
                MaxHealth = maxHP,
                Health = maxHP,
                HitDice = _selectedClass.HitDie,

                CurrentEra = TechnologyEra.Caveman,

                Stats = new CharacterStats
                {
                    Strength = str,
                    Dexterity = dex,
                    Constitution = con,
                    Intelligence = intel,
                    Wisdom = wis,
                    Charisma = cha
                },

                ArmorClass = armorClass,
                Initiative = initiative,
                Speed = 30,
            

                Skills = InitializeSkills(_selectedClass, _selectedBackground),
                Inventory = InitializeInventory(_selectedClass),
                Languages = new List<string> { "Common" },
                Gold = Random.Shared.Next(50, 150)
            };

            // Save the character
            bool saved = await _saveService.SaveCharacter(character);

            if (saved)
            {
                // Get services from DI
                var gameService = Handler?.MauiContext?.Services.GetService<GameService>();
                var combatService = Handler?.MauiContext?.Services.GetService<CombatService>();
                var monsterLoader = Handler?.MauiContext?.Services.GetService<MonsterLoaderService>();
                var audioService = Handler?.MauiContext?.Services.GetService<AudioService>();

                // Navigate to main game page with character and services
                var mainPage = new MainPage(character, gameService, combatService, monsterLoader, audioService);
                await Navigation.PushAsync(mainPage);
            }
            else
            {
                await DisplayAlert("Save Failed", "Could not save character, but you can still play.", "OK");

                var gameService = Handler?.MauiContext?.Services.GetService<GameService>();
                var combatService = Handler?.MauiContext?.Services.GetService<CombatService>();
                var monsterLoader = Handler?.MauiContext?.Services.GetService<MonsterLoaderService>();
                var audioService = Handler?.MauiContext?.Services.GetService<AudioService>();

                var mainPage = new MainPage(character, gameService, combatService, monsterLoader, audioService);
                await Navigation.PushAsync(mainPage);
            }
        }
        private List<Skill> InitializeSkills(ClassData classData, BackgroundData background)
        {
            var skills = new List<Skill>
            {
                // Strength
                new Skill { Name = "Athletics", Ability = "STR", IsProficient = false },
                
                // Dexterity
                new Skill { Name = "Acrobatics", Ability = "DEX", IsProficient = false },
                new Skill { Name = "Sleight of Hand", Ability = "DEX", IsProficient = false },
                new Skill { Name = "Stealth", Ability = "DEX", IsProficient = false },
                
                // Intelligence
                new Skill { Name = "Arcana", Ability = "INT", IsProficient = false },
                new Skill { Name = "History", Ability = "INT", IsProficient = false },
                new Skill { Name = "Investigation", Ability = "INT", IsProficient = false },
                new Skill { Name = "Nature", Ability = "INT", IsProficient = false },
                new Skill { Name = "Religion", Ability = "INT", IsProficient = false },
                
                // Wisdom
                new Skill { Name = "Animal Handling", Ability = "WIS", IsProficient = false },
                new Skill { Name = "Insight", Ability = "WIS", IsProficient = false },
                new Skill { Name = "Medicine", Ability = "WIS", IsProficient = false },
                new Skill { Name = "Perception", Ability = "WIS", IsProficient = false },
                new Skill { Name = "Survival", Ability = "WIS", IsProficient = false },
                
                // Charisma
                new Skill { Name = "Deception", Ability = "CHA", IsProficient = false },
                new Skill { Name = "Intimidation", Ability = "CHA", IsProficient = false },
                new Skill { Name = "Performance", Ability = "CHA", IsProficient = false },
                new Skill { Name = "Persuasion", Ability = "CHA", IsProficient = false }
            };

            // For now, give proficiency in a couple skills based on class
            // In a full implementation, you'd let the player choose
            var classSkills = classData.SkillProficiencies.Take(2);
            foreach (var skillName in classSkills)
            {
                var skill = skills.FirstOrDefault(s => s.Name == skillName);
                if (skill != null)
                    skill.IsProficient = true;
            }

            // Add background proficiencies
            if (background != null)
            {
                foreach (var skillName in background.SkillProficiencies)
                {
                    var skill = skills.FirstOrDefault(s => s.Name == skillName);
                    if (skill != null)
                        skill.IsProficient = true;
                }
            }

            return skills;
        }




        private List<Item> InitializeInventory(ClassData classData)
        {
            var inventory = new List<Item>();

            // Give starting health potions
            var healthPotion = ItemDatabase.GetItem("health_potion_minor");
            if (healthPotion != null)
            {
                healthPotion.Quantity = 3;
                inventory.Add(healthPotion);
            }

            // Give era-appropriate starting weapon based on class
            string weaponId = classData.Class switch
            {
                CharacterClass.Fighter => "wooden_club",
                CharacterClass.Barbarian => "wooden_club",
                CharacterClass.Rogue => "sharp_rock",
                CharacterClass.Ranger => "bone_spear",
                CharacterClass.Wizard => "sharp_rock",
                CharacterClass.Cleric => "wooden_club",
                _ => "wooden_club"
            };

            var weapon = ItemDatabase.GetItem(weaponId);
            if (weapon != null)
            {
                inventory.Add(weapon);
            }

            // Give starting armor
            var armor = ItemDatabase.GetItem("hide_armor");
            if (armor != null)
            {
                inventory.Add(armor);
            }

            return inventory;
        }

        private string AdaptToCavemanEra(string modernEquipment)
        {
            // Convert modern/medieval equipment to caveman equivalents
            return modernEquipment.ToLower() switch
            {
                string s when s.Contains("sword") => "Sharp Stone",
                string s when s.Contains("axe") => "Stone Axe",
                string s when s.Contains("mail") || s.Contains("armor") => "Hide Armor",
                string s when s.Contains("shield") => "Wooden Shield",
                string s when s.Contains("bow") => "Crude Bow",
                string s when s.Contains("staff") => "Wooden Staff",
                string s when s.Contains("dagger") => "Flint Knife",
                string s when s.Contains("spellbook") => "Cave Drawings",
                _ => $"Primitive {modernEquipment}"
            };
        }

        private ItemType GetItemType(string equipName)
        {
            string lower = equipName.ToLower();
            if (lower.Contains("armor") || lower.Contains("mail") || lower.Contains("shield") || lower.Contains("hide"))
                return ItemType.Armor;
            if (lower.Contains("tool"))
                return ItemType.Misc;
            return ItemType.Weapon;
        }
    }
}