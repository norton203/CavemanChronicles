
namespace CavemanChronicles
{
    public partial class LoadGamePage : ContentPage
    {
        private readonly SaveService _saveService;

        public LoadGamePage()
        {
            InitializeComponent();
            _saveService = new SaveService();
            LoadSavedGames();
        }

        private async void LoadSavedGames()
        {
            var savedCharacters = await _saveService.GetSavedCharacters();

            if (savedCharacters.Count == 0)
            {
                NoSavesLabel.IsVisible = true;
                return;
            }

            SavesContainer.Clear();

            foreach (var save in savedCharacters)
            {
                var saveCard = CreateSaveCard(save);
                SavesContainer.Add(saveCard);
            }
        }

        private Border CreateSaveCard(SavedCharacterInfo save)
        {
            var raceStats = RaceData.GetRaceStats(save.Character.Race);

            var border = new Border
            {
                Stroke = Color.FromArgb("#00FF00"),
                StrokeThickness = 2,
                Padding = 15,
                BackgroundColor = Colors.Black
            };

            var stackLayout = new VerticalStackLayout { Spacing = 8 };

            // Character name and race
            var nameLabel = new Label
            {
                Text = $"{save.Character.Name} ({raceStats.Name})",
                TextColor = Color.FromArgb("#00FF00"),
                FontFamily = "Courier New",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold
            };

            // Level and era
            var levelLabel = new Label
            {
                Text = $"Level {save.Character.Level} - {save.Character.CurrentEra} Era",
                TextColor = Color.FromArgb("#00FF00"),
                FontFamily = "Courier New",
                FontSize = 14
            };

            // HP
            var hpLabel = new Label
            {
                Text = $"HP: {save.Character.Health}/{save.Character.MaxHealth}",
                TextColor = Color.FromArgb("#00AA00"),
                FontFamily = "Courier New",
                FontSize = 12
            };

            // Save date
            var dateLabel = new Label
            {
                Text = $"Saved: {save.DisplayDate}",
                TextColor = Color.FromArgb("#006600"),
                FontFamily = "Courier New",
                FontSize = 12
            };

            // Buttons
            var buttonGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                },
                ColumnSpacing = 10,
                Margin = new Thickness(0, 10, 0, 0)
            };

            // Load button
            var loadButton = new Border
            {
                Stroke = Color.FromArgb("#00FF00"),
                StrokeThickness = 1,
                Padding = 10,
                BackgroundColor = Color.FromArgb("#003300")
            };
            loadButton.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () => await LoadCharacter(save))
            });
            loadButton.Content = new Label
            {
                Text = "LOAD",
                TextColor = Color.FromArgb("#00FF00"),
                FontFamily = "Courier New",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center
            };

            // Delete button
            var deleteButton = new Border
            {
                Stroke = Color.FromArgb("#FF0000"),
                StrokeThickness = 1,
                Padding = 10,
                BackgroundColor = Colors.Black
            };
            deleteButton.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () => await DeleteSave(save))
            });
            deleteButton.Content = new Label
            {
                Text = "DELETE",
                TextColor = Color.FromArgb("#FF0000"),
                FontFamily = "Courier New",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center
            };

            Grid.SetColumn(loadButton, 0);
            Grid.SetColumn(deleteButton, 1);

            buttonGrid.Add(loadButton);
            buttonGrid.Add(deleteButton);

            stackLayout.Add(nameLabel);
            stackLayout.Add(levelLabel);
            stackLayout.Add(hpLabel);
            stackLayout.Add(dateLabel);
            stackLayout.Add(buttonGrid);

            border.Content = stackLayout;

            return border;
        }

        private async Task LoadCharacter(SavedCharacterInfo save)
        {
            var character = await _saveService.LoadCharacter(save.FilePath);

            if (character != null)
            {
                // Navigate to main game with loaded character
                var mainPage = new MainPage(character);
                await Navigation.PushAsync(mainPage);
            }
            else
            {
                await DisplayAlert("Error", "Failed to load character.", "OK");
            }
        }

        private async Task DeleteSave(SavedCharacterInfo save)
        {
            bool confirm = await DisplayAlert(
                "Delete Save",
                $"Are you sure you want to delete {save.Character.Name}?",
                "Delete",
                "Cancel");

            if (confirm)
            {
                if (_saveService.DeleteSave(save.FilePath))
                {
                    await DisplayAlert("Deleted", "Save file deleted successfully.", "OK");
                    LoadSavedGames(); // Refresh the list
                }
                else
                {
                    await DisplayAlert("Error", "Failed to delete save file.", "OK");
                }
            }
        }

        private async void OnBack(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}