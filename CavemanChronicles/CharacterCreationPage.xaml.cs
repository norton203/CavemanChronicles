using Microsoft.Maui.Platform;

namespace CavemanChronicles
{
    public partial class CharacterCreationPage : ContentPage
    {
        private RaceStats _selectedRace;
        private ClassData _selectedClass;
        private BackgroundData _selectedBackground;
        private readonly SaveService _saveService;
        private int _currentStep = 1;

        public CharacterCreationPage()
        {
            InitializeComponent();
            _saveService = new SaveService();

            RaceCollectionView.ItemsSource = RaceData.AllRaces;
            ClassCollectionView.ItemsSource = ClassData.AllClasses;
            BackgroundCollectionView.ItemsSource = BackgroundData.AllBackgrounds;
        }

        // ══════════════════════════════════════════════════
        // STEP NAVIGATION
        // ══════════════════════════════════════════════════

        private async void OnNext(object sender, EventArgs e)
        {
            if (_currentStep == 1)
            {
                if (string.IsNullOrWhiteSpace(NameEntry.Text))
                {
                    await DisplayAlert("Missing Name", "Please enter your character's name.", "OK");
                    return;
                }
                if (_selectedRace == null)
                {
                    await DisplayAlert("Missing Species", "Please select a species (race).", "OK");
                    return;
                }
                GoToStep(2);
            }
            else if (_currentStep == 2)
            {
                if (_selectedClass == null)
                {
                    await DisplayAlert("Missing Class", "Please select a class.", "OK");
                    return;
                }
                GoToStep(3);
            }
        }

        private void OnBack(object sender, EventArgs e)
        {
            if (_currentStep > 1)
                GoToStep(_currentStep - 1);
        }

        private async void GoToStep(int step)
        {
            _currentStep = step;

            Step1Panel.IsVisible = step == 1;
            Step2Panel.IsVisible = step == 2;
            Step3Panel.IsVisible = step == 3;

            BackButton.IsVisible = step > 1;
            NextButton.IsVisible = step < 3;

            // Update step indicator dots
            UpdateStepIndicator();

            // On step 3 — roll stats if not yet rolled, refresh derived stats
            if (step == 3)
            {
                if (string.IsNullOrWhiteSpace(StrengthEntry.Text))
                    RollInitialStats();
                UpdateDerivedStats();
                CheckIfCanProceed();
            }

            // Scroll to top of page on step change
            await MainScrollView.ScrollToAsync(0, 0, false);
        }

        private void UpdateStepIndicator()
        {
            // Step 1 dot
            SetDotActive(Step1Dot, Step1DotLabel ?? null, Step1Label, _currentStep == 1, "#FF00FF");
            // Step 2 dot
            SetDotActive(Step2Dot, Step2DotLabel, Step2Label, _currentStep == 2, "#00FFFF");
            // Step 3 dot
            SetDotActive(Step3Dot, Step3DotLabel, Step3Label, _currentStep == 3, "#FFD700");
        }

        private void SetDotActive(Border dot, Label dotLabel, Label stepLabel, bool active, string activeColor)
        {
            if (active)
            {
                dot.BackgroundColor = Color.FromArgb(activeColor);
                dot.Stroke = Color.FromArgb(activeColor);
                if (dotLabel != null) dotLabel.TextColor = Colors.Black;
                stepLabel.TextColor = Color.FromArgb(activeColor);
            }
            else
            {
                dot.BackgroundColor = Color.FromArgb("#333333");
                dot.Stroke = Color.FromArgb("#333333");
                if (dotLabel != null) dotLabel.TextColor = Color.FromArgb("#666666");
                stepLabel.TextColor = Color.FromArgb("#444444");
            }
        }

        // ══════════════════════════════════════════════════
        // STEP 1: RACE SELECTION
        // ══════════════════════════════════════════════════

        private void OnRaceSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count == 0) return;

            _selectedRace = e.CurrentSelection[0] as RaceStats;
            if (_selectedRace != null)
            {
                RaceDisplayLabel.Text = _selectedRace.Name.ToUpper();
                RaceSelectedBanner.IsVisible = true;
                UpdateStatModifiers();
            }
        }

        private void OnNameChanged(object sender, TextChangedEventArgs e)
        {
            // Nothing needed here for step wizard — validation happens on Next tap
        }

        // ══════════════════════════════════════════════════
        // STEP 2: CLASS & BACKGROUND
        // ══════════════════════════════════════════════════

        private void OnClassSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count == 0) return;

            _selectedClass = e.CurrentSelection[0] as ClassData;
            if (_selectedClass != null)
            {
                ClassDisplayLabel.Text = _selectedClass.Name.ToUpper();
                ClassSelectedBanner.IsVisible = true;
                RollInitialStats();
            }
        }

        private void OnBackgroundSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count == 0)
            {
                _selectedBackground = null;
                BackgroundDisplayLabel.Text = "NONE";
                BackgroundSelectedBanner.IsVisible = false;
                return;
            }

            _selectedBackground = e.CurrentSelection[0] as BackgroundData;
            if (_selectedBackground != null)
            {
                BackgroundDisplayLabel.Text = _selectedBackground.Name.ToUpper();
                BackgroundSelectedBanner.IsVisible = true;
            }
        }

        // ══════════════════════════════════════════════════
        // STEP 3: ABILITY SCORES
        // ══════════════════════════════════════════════════

        private void OnRollStats(object sender, EventArgs e)
        {
            RollInitialStats();
            UpdateDerivedStats();
        }

        private void RollInitialStats()
        {
            if (_selectedClass == null) return;

            var random = new Random();
            int[] stats = new int[6];
            for (int i = 0; i < 6; i++)
                stats[i] = Roll4d6DropLowest(random);

            // Assign based on class priority
            int[] sorted = stats.OrderByDescending(x => x).ToArray();

            int[] indices = _selectedClass.Class switch
            {
                CharacterClass.Fighter or CharacterClass.Barbarian => new[] { 0, 1, 2, 3, 4, 5 },
                CharacterClass.Wizard => new[] { 3, 1, 2, 0, 4, 5 },
                CharacterClass.Rogue => new[] { 1, 0, 2, 3, 4, 5 },
                CharacterClass.Cleric => new[] { 2, 1, 0, 3, 4, 5 },
                CharacterClass.Ranger => new[] { 1, 0, 2, 3, 4, 5 },
                _ => new[] { 0, 1, 2, 3, 4, 5 }
            };

            StrengthEntry.Text     = sorted[indices[0]].ToString();
            DexterityEntry.Text    = sorted[indices[1]].ToString();
            ConstitutionEntry.Text = sorted[indices[2]].ToString();
            IntelligenceEntry.Text = sorted[indices[3]].ToString();
            WisdomEntry.Text       = sorted[indices[4]].ToString();
            CharismaEntry.Text     = sorted[indices[5]].ToString();

            UpdateStatModifiers();
            UpdateDerivedStats();
            CheckIfCanProceed();
        }

        private int Roll4d6DropLowest(Random random)
        {
            int[] rolls = { random.Next(1, 7), random.Next(1, 7), random.Next(1, 7), random.Next(1, 7) };
            return rolls.Sum() - rolls.Min();
        }

        private void OnStatChanged(object sender, TextChangedEventArgs e)
        {
            UpdateStatModifiers();
            UpdateDerivedStats();
            CheckIfCanProceed();
        }

        private void UpdateStatModifiers()
        {
            UpdateModifierLabel(StrengthEntry, StrengthModLabel);
            UpdateModifierLabel(DexterityEntry, DexterityModLabel);
            UpdateModifierLabel(ConstitutionEntry, ConstitutionModLabel);
            UpdateModifierLabel(IntelligenceEntry, IntelligenceModLabel);
            UpdateModifierLabel(WisdomEntry, WisdomModLabel);
            UpdateModifierLabel(CharismaEntry, CharismaModLabel);
        }

        private void UpdateModifierLabel(Entry entry, Label modLabel)
        {
            if (int.TryParse(entry?.Text, out int score))
            {
                int mod = GameMath.CalculateModifier(score);
                modLabel.Text = mod >= 0 ? $"+{mod}" : $"{mod}";
            }
            else
            {
                modLabel.Text = "--";
            }
        }

        private void UpdateDerivedStats()
        {
            if (!HasValidStats() || _selectedClass == null || _selectedRace == null)
            {
                DerivedStatsPanel.IsVisible = false;
                return;
            }

            int.TryParse(ConstitutionEntry.Text, out int con);
            int.TryParse(DexterityEntry.Text, out int dex);

            int conMod = GameMath.CalculateModifier(con);
            int dexMod = GameMath.CalculateModifier(dex);
            int maxHP = Math.Max(1, _selectedClass.HitDie + conMod + _selectedRace.HealthBonus);
            int ac = 10 + dexMod;

            HitPointsLabel.Text = maxHP.ToString();
            ArmorClassLabel.Text = ac.ToString();
            InitiativeLabel.Text = dexMod >= 0 ? $"+{dexMod}" : $"{dexMod}";

            DerivedStatsPanel.IsVisible = true;
        }

        private void CheckIfCanProceed()
        {
            bool canStart = !string.IsNullOrWhiteSpace(NameEntry.Text)
                            && _selectedRace != null
                            && _selectedClass != null
                            && HasValidStats();

            StartButton.IsVisible = canStart && _currentStep == 3;
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

        // ══════════════════════════════════════════════════
        // START ADVENTURE
        // ══════════════════════════════════════════════════

        private async void OnStartAdventure(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameEntry.Text))
            {
                await DisplayAlert("Missing Name", "Please enter a character name.", "OK");
                return;
            }
            if (_selectedRace == null)
            {
                await DisplayAlert("Missing Species", "Please select a species.", "OK");
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

            int.TryParse(StrengthEntry.Text, out int str);
            int.TryParse(DexterityEntry.Text, out int dex);
            int.TryParse(ConstitutionEntry.Text, out int con);
            int.TryParse(IntelligenceEntry.Text, out int intel);
            int.TryParse(WisdomEntry.Text, out int wis);
            int.TryParse(CharismaEntry.Text, out int cha);

            str   += _selectedRace.StrengthBonus;
            dex   += _selectedRace.DexterityBonus;
            intel += _selectedRace.IntelligenceBonus;

            int conMod = GameMath.CalculateModifier(con);
            int dexMod = GameMath.CalculateModifier(dex);
            int maxHP = Math.Max(1, _selectedClass.HitDie + conMod + _selectedRace.HealthBonus);
            int initiative = dexMod;
            int armorClass = 10 + dexMod;

            var character = new Character
            {
                Name       = NameEntry.Text.Trim(),
                Race       = _selectedRace.Race,
                Class      = _selectedClass.Class,
                Backstory  = BackstoryEditor.Text?.Trim() ?? string.Empty,
                Level      = 1,
                Experience = 0,
                Strength     = str,
                Dexterity    = dex,
                Constitution = con,
                Intelligence = intel,
                Wisdom       = wis,
                Charisma     = cha,
                MaxHP        = maxHP,
                CurrentHP    = maxHP,
                Initiative   = initiative,
                ArmorClass   = armorClass,
                ProficiencyBonus = 2,
                Speed        = 30,
                CurrentTechEra = TechEra.StoneAge
            };

            await _saveService.SaveCharacterAsync(character).ConfigureAwait(false);

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Navigation.PushAsync(new GamePage(character)).ConfigureAwait(false);
            });
        }
    }
}