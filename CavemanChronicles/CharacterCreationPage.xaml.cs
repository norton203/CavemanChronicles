using Microsoft.Maui.Platform;

namespace CavemanChronicles
{
    public partial class CharacterCreationPage : ContentPage
    {
        private RaceStats _selectedRace;
        private readonly SaveService _saveService;

        public CharacterCreationPage()
        {
            InitializeComponent();
            _saveService = new SaveService();
            RaceCollectionView.ItemsSource = RaceData.AllRaces;
        }

        private async void OnRaceSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count == 0)
                return;

            _selectedRace = e.CurrentSelection[0] as RaceStats;

            if (_selectedRace != null)
            {
                UpdateStatsDisplay();
                StatsPanel.IsVisible = true;
                CheckIfCanStart();

                // Auto-scroll to show the stats panel
                await Task.Delay(100); // Small delay to let the UI update
                await MainScrollView.ScrollToAsync(StatsPanel, ScrollToPosition.MakeVisible, true);
            }
        }

        private void UpdateStatsDisplay()
        {
            StrengthLabel.Text = FormatBonus(_selectedRace.StrengthBonus);
            DexterityLabel.Text = FormatBonus(_selectedRace.DexterityBonus);
            IntelligenceLabel.Text = FormatBonus(_selectedRace.IntelligenceBonus);

            int totalHealth = 100 + _selectedRace.HealthBonus;
            HealthLabel.Text = totalHealth.ToString();
        }

        private string FormatBonus(int bonus)
        {
            if (bonus > 0)
                return $"+{bonus}";
            else if (bonus < 0)
                return bonus.ToString();
            else
                return "0";
        }

        private async void CheckIfCanStart()
        {
            bool hasName = !string.IsNullOrWhiteSpace(NameEntry.Text);
            bool hasRace = _selectedRace != null;

            bool wasVisible = StartButton.IsVisible;
            StartButton.IsVisible = hasName && hasRace;

            // Auto-scroll to show the start button when it appears
            if (!wasVisible && StartButton.IsVisible)
            {
                await Task.Delay(100);
                await MainScrollView.ScrollToAsync(StartButton, ScrollToPosition.End, true);
            }
        }

        private void OnNameChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfCanStart();
        }

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

            // Create character with selected race
            var character = new Character
            {
                Name = NameEntry.Text.Trim(),
                Race = _selectedRace.Race,
                Level = 1,
                Experience = 0,
                MaxHealth = 100 + _selectedRace.HealthBonus,
                CurrentEra = TechnologyEra.Caveman,
                Inventory = new List<Item>(),
                Stats = new CharacterStats
                {
                    Strength = 5 + _selectedRace.StrengthBonus,
                    Dexterity = 5 + _selectedRace.DexterityBonus,
                    Intelligence = 5 + _selectedRace.IntelligenceBonus
                }
            };

            character.Health = character.MaxHealth;

            // Save the character
            bool saved = await _saveService.SaveCharacter(character);

            if (saved)
            {
                // Navigate to main game page with character
                var mainPage = new MainPage(character);
                await Navigation.PushAsync(mainPage);
            }
            else
            {
                await DisplayAlert("Save Failed", "Could not save character, but you can still play.", "OK");
                var mainPage = new MainPage(character);
                await Navigation.PushAsync(mainPage);
            }
        }
    }
}