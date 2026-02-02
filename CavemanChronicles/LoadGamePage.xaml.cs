

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
                NoSavesPanel.IsVisible = true;
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
                Padding = 0,
                BackgroundColor = Color.FromArgb("#001100")
            };

            // Use Grid for precise layout control
            var mainGrid = new Grid
            {
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = GridLength.Auto }, // Header
                    new RowDefinition { Height = GridLength.Auto }, // Stats
                    new RowDefinition { Height = GridLength.Auto }  // Buttons
                },
                RowSpacing = 0
            };

            // === HEADER SECTION ===
            var headerBorder = new Border
            {
                BackgroundColor = Color.FromArgb("#003300"),
                Padding = 15
            };

            var headerStack = new VerticalStackLayout { Spacing = 5 };

            var nameLabel = new Label
            {
                Text = $"▶ {save.Character.Name}",
                TextColor = Color.FromArgb("#00FF00"),
                FontFamily = "Courier New",
                FontSize = 20,
                FontAttributes = FontAttributes.Bold
            };

            var raceLabel = new Label
            {
                Text = $"Race: {raceStats.Name}",
                TextColor = Color.FromArgb("#00FF00"),
                FontFamily = "Courier New",
                FontSize = 14
            };

            headerStack.Add(nameLabel);
            headerStack.Add(raceLabel);
            headerBorder.Content = headerStack;

            Grid.SetRow(headerBorder, 0);
            mainGrid.Add(headerBorder);

            // === STATS SECTION ===
            var statsGrid = new Grid
            {
                Padding = 15,
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                },
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                },
                RowSpacing = 10,
                ColumnSpacing = 20
            };

            // Level
            var levelTitle = new Label
            {
                Text = "Level:",
                TextColor = Color.FromArgb("#00AA00"),
                FontFamily = "Courier New",
                FontSize = 13
            };
            Grid.SetRow(levelTitle, 0);
            Grid.SetColumn(levelTitle, 0);

            var levelValue = new Label
            {
                Text = save.Character.Level.ToString(),
                TextColor = Color.FromArgb("#00FF00"),
                FontFamily = "Courier New",
                FontSize = 13,
                FontAttributes = FontAttributes.Bold
            };
            Grid.SetRow(levelValue, 0);
            Grid.SetColumn(levelValue, 1);

            // Era
            var eraTitle = new Label
            {
                Text = "Era:",
                TextColor = Color.FromArgb("#00AA00"),
                FontFamily = "Courier New",
                FontSize = 13
            };
            Grid.SetRow(eraTitle, 1);
            Grid.SetColumn(eraTitle, 0);

            var eraValue = new Label
            {
                Text = save.Character.CurrentEra.ToString(),
                TextColor = Color.FromArgb("#00FF00"),
                FontFamily = "Courier New",
                FontSize = 13,
                FontAttributes = FontAttributes.Bold
            };
            Grid.SetRow(eraValue, 1);
            Grid.SetColumn(eraValue, 1);

            // Health
            var hpTitle = new Label
            {
                Text = "Health:",
                TextColor = Color.FromArgb("#00AA00"),
                FontFamily = "Courier New",
                FontSize = 13
            };
            Grid.SetRow(hpTitle, 2);
            Grid.SetColumn(hpTitle, 0);

            var hpValue = new Label
            {
                Text = $"{save.Character.Health}/{save.Character.MaxHealth}",
                TextColor = Color.FromArgb("#00FF00"),
                FontFamily = "Courier New",
                FontSize = 13,
                FontAttributes = FontAttributes.Bold
            };
            Grid.SetRow(hpValue, 2);
            Grid.SetColumn(hpValue, 1);

            statsGrid.Add(levelTitle);
            statsGrid.Add(levelValue);
            statsGrid.Add(eraTitle);
            statsGrid.Add(eraValue);
            statsGrid.Add(hpTitle);
            statsGrid.Add(hpValue);

            Grid.SetRow(statsGrid, 1);
            mainGrid.Add(statsGrid);

            // === FOOTER WITH DATE AND BUTTONS ===
            var footerStack = new VerticalStackLayout
            {
                Spacing = 10,
                Padding = 15,
                BackgroundColor = Color.FromArgb("#000000")
            };

            // Date
            var dateLabel = new Label
            {
                Text = $"Saved: {save.DisplayDate}",
                TextColor = Color.FromArgb("#006600"),
                FontFamily = "Courier New",
                FontSize = 11,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Buttons Grid
            var buttonGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                },
                ColumnSpacing = 10,
                HeightRequest = 45
            };

            // Load button
            var loadButton = new Border
            {
                Stroke = Color.FromArgb("#00FF00"),
                StrokeThickness = 2,
                Padding = 12,
                BackgroundColor = Color.FromArgb("#003300"),
                HorizontalOptions = LayoutOptions.Fill
            };
            loadButton.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () => await LoadCharacter(save))
            });
            loadButton.Content = new Label
            {
                Text = "▶ LOAD",
                TextColor = Color.FromArgb("#00FF00"),
                FontFamily = "Courier New",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            Grid.SetColumn(loadButton, 0);

            // Delete button
            var deleteButton = new Border
            {
                Stroke = Color.FromArgb("#FF0000"),
                StrokeThickness = 2,
                Padding = 12,
                BackgroundColor = Color.FromArgb("#330000"),
                HorizontalOptions = LayoutOptions.Fill
            };
            deleteButton.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () => await DeleteSave(save))
            });
            deleteButton.Content = new Label
            {
                Text = "✕ DELETE",
                TextColor = Color.FromArgb("#FF0000"),
                FontFamily = "Courier New",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            Grid.SetColumn(deleteButton, 1);

            buttonGrid.Add(loadButton);
            buttonGrid.Add(deleteButton);

            footerStack.Add(dateLabel);
            footerStack.Add(buttonGrid);

            Grid.SetRow(footerStack, 2);
            mainGrid.Add(footerStack);

            border.Content = mainGrid;

            return border;
        }

        // In LoadGamePage.xaml.cs, update LoadCharacter method:

        private async Task LoadCharacter(SavedCharacterInfo save)
        {
            var character = await _saveService.LoadCharacter(save.FilePath);

            if (character != null)
            {
                // Get services from DI
                var gameService = Handler?.MauiContext?.Services.GetService<GameService>();
                var combatService = Handler?.MauiContext?.Services.GetService<CombatService>();
                var monsterLoader = Handler?.MauiContext?.Services.GetService<MonsterLoaderService>();
                var audioService = Handler?.MauiContext?.Services.GetService<AudioService>();

                // Navigate to main game with loaded character and services
                var mainPage = new MainPage(character, gameService, combatService, monsterLoader, audioService);
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
