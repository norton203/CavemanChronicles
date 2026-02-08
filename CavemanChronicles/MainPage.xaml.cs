using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;


namespace CavemanChronicles
{
    public partial class MainPage : ContentPage
    {
        private readonly GameService _gameService;
        private readonly CombatService _combatService;
        private readonly MonsterLoaderService _monsterLoader;
        private readonly AudioService _audioService;
        private readonly InventoryService _inventoryService;

        // Dungeon state
        private int _playerX = 5;
        private int _playerY = 5;
        private Direction _playerDirection = Direction.North;
        private int _turnCount = 1;

        // Dungeon map (0 = floor, 1 = wall)
        private int[,] _dungeonMap;

        public MainPage(Character character, GameService gameService, CombatService combatService,
                       MonsterLoaderService monsterLoader, AudioService audioService)
        {
            InitializeComponent();

            _gameService = gameService;
            _combatService = combatService;
            _monsterLoader = monsterLoader;
            _audioService = audioService;
            _inventoryService = Handler?.MauiContext?.Services.GetService<InventoryService>()
                ?? new InventoryService();

            _gameService.Player = character;
            _gameService.SetAudioService(_audioService);
            _gameService.SetMonsterLoader(_monsterLoader);
            _gameService.SetCombatService(_combatService);
            _gameService.SetNavigation(Navigation);

            InitializeDungeon();
            UpdateUI();
            AddMessage($"Welcome {character.Name}! You find yourself in a dark cavern.");
            AddMessage("Use the arrow buttons to explore the dungeon.");
        }

        private void InitializeDungeon()
        {
            // Simple 10x10 dungeon for now
            _dungeonMap = new int[10, 10]
            {
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                { 1, 0, 0, 0, 1, 0, 0, 0, 0, 1 },
                { 1, 0, 1, 0, 1, 0, 1, 1, 0, 1 },
                { 1, 0, 1, 0, 0, 0, 0, 1, 0, 1 },
                { 1, 0, 1, 1, 1, 1, 0, 1, 0, 1 },
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                { 1, 0, 1, 1, 0, 1, 1, 1, 0, 1 },
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                { 1, 0, 1, 0, 1, 0, 1, 0, 1, 1 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
            };
        }

        // ==================== MOVEMENT ====================

        private async void OnMoveForward(object sender, EventArgs e)
        {
            await _audioService.PlaySoundEffect("button_click.wav");

            int newX = _playerX;
            int newY = _playerY;

            switch (_playerDirection)
            {
                case Direction.North: newY--; break;
                case Direction.South: newY++; break;
                case Direction.East: newX++; break;
                case Direction.West: newX--; break;
            }

            if (IsValidMove(newX, newY))
            {
                _playerX = newX;
                _playerY = newY;
                _turnCount++;
                AddMessage("You move forward.");
                UpdateUI();
                CheckForEncounter();
            }
            else
            {
                AddMessage("You bump into a wall!");
            }
        }

        private async void OnMoveBack(object sender, EventArgs e)
        {
            await _audioService.PlaySoundEffect("button_click.wav");

            int newX = _playerX;
            int newY = _playerY;

            // Move backwards (opposite of forward direction)
            switch (_playerDirection)
            {
                case Direction.North: newY++; break;
                case Direction.South: newY--; break;
                case Direction.East: newX--; break;
                case Direction.West: newX++; break;
            }

            if (IsValidMove(newX, newY))
            {
                _playerX = newX;
                _playerY = newY;
                _turnCount++;
                AddMessage("You back up carefully.");
                UpdateUI();
            }
            else
            {
                AddMessage("You can't back up - there's a wall!");
            }
        }

        private async void OnTurnLeft(object sender, EventArgs e)
        {
            await _audioService.PlaySoundEffect("button_click.wav");

            _playerDirection = _playerDirection switch
            {
                Direction.North => Direction.West,
                Direction.West => Direction.South,
                Direction.South => Direction.East,
                Direction.East => Direction.North,
                _ => Direction.North
            };

            AddMessage($"You turn left, now facing {_playerDirection}.");
            UpdateUI();
        }

        private async void OnTurnRight(object sender, EventArgs e)
        {
            await _audioService.PlaySoundEffect("button_click.wav");

            _playerDirection = _playerDirection switch
            {
                Direction.North => Direction.East,
                Direction.East => Direction.South,
                Direction.South => Direction.West,
                Direction.West => Direction.North,
                _ => Direction.North
            };

            AddMessage($"You turn right, now facing {_playerDirection}.");
            UpdateUI();
        }

        private bool IsValidMove(int x, int y)
        {
            if (x < 0 || x >= 10 || y < 0 || y >= 10)
                return false;

            return _dungeonMap[y, x] == 0;
        }

        private void CheckForEncounter()
        {
            // 20% chance of encounter on each move
            if (Random.Shared.Next(100) < 20)
            {
                AddMessage("You hear something approaching...");
                // Delay slightly then trigger combat
                Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await TriggerCombat();
                    });
                });
            }
        }

        // ==================== ACTION BUTTONS ====================

        private async void OnAttackClicked(object sender, EventArgs e)
        {
            await _audioService.PlaySoundEffect("button_click.wav");
            await TriggerCombat();
        }

        private async Task TriggerCombat()
        {
            var monsters = _monsterLoader.GetMonstersByEra(_gameService.Player.CurrentEra);
            if (monsters == null || monsters.Count == 0)
            {
                AddMessage("No monsters found for combat!");
                return;
            }

            int encounterSize = Random.Shared.Next(1, 3); // 1-2 enemies
            var enemyList = new List<Monster>();

            for (int i = 0; i < encounterSize; i++)
            {
                var randomMonster = monsters[Random.Shared.Next(monsters.Count)];
                var enemy = new Monster
                {
                    Name = randomMonster.Name,
                    Description = randomMonster.Description,
                    Type = randomMonster.Type,
                    Era = randomMonster.Era,
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

            var combatPage = new CombatPage(_gameService.Player, enemyList, _combatService, _audioService);
            await Navigation.PushAsync(combatPage);
        }

        private async void OnRestClicked(object sender, EventArgs e)
        {
            await _audioService.PlaySoundEffect("button_click.wav");

            // Heal 25% HP
            int healAmount = _gameService.Player.MaxHealth / 4;
            _gameService.Player.Health = Math.Min(
                _gameService.Player.Health + healAmount,
                _gameService.Player.MaxHealth
            );

            AddMessage($"You rest and recover {healAmount} HP.");
            _turnCount += 5; // Resting takes 5 turns
            UpdateUI();
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            await _audioService.PlaySoundEffect("button_click.wav");

            // 30% chance to find something
            if (Random.Shared.Next(100) < 30)
            {
                int goldFound = Random.Shared.Next(10, 50);
                _gameService.Player.Gold += goldFound;
                AddMessage($"You search the area and find {goldFound} gold!");
            }
            else
            {
                AddMessage("You search carefully but find nothing.");
            }

            UpdateUI();
        }

        private async void OnInventoryClicked(object sender, EventArgs e)
        {
            await _audioService.PlaySoundEffect("button_click.wav");
            var inventoryPage = new InventoryPage(_gameService.Player, _inventoryService, _audioService);
            await Navigation.PushAsync(inventoryPage);
        }

        private async void OnCampClicked(object sender, EventArgs e)
        {
            await _audioService.PlaySoundEffect("button_click.wav");

            bool confirm = await DisplayAlert(
                "Make Camp",
                "Rest until fully healed? This will advance time significantly.",
                "Yes",
                "No"
            );

            if (confirm)
            {
                _gameService.Player.Health = _gameService.Player.MaxHealth;
                _turnCount += 50;
                AddMessage("You make camp and sleep deeply. You wake fully rested.");
                UpdateUI();
            }
        }

        private async void OnMenuClicked(object sender, EventArgs e)
        {
            await _audioService.PlaySoundEffect("button_click.wav");

            string action = await DisplayActionSheet(
                "Menu",
                "Cancel",
                null,
                "Save Game",
                "Settings",
                "Character Sheet",
                "Return to Title"
            );

            if (action == "Save Game")
            {
                var saveService = new SaveService();
                bool saved = await saveService.SaveCharacter(_gameService.Player);
                if (saved)
                    await DisplayAlert("Saved", "Game saved successfully!", "OK");
                else
                    await DisplayAlert("Error", "Failed to save game.", "OK");
            }
            else if (action == "Settings")
            {
                await Navigation.PushAsync(new SettingsPage());
            }
            else if (action == "Character Sheet")
            {
                await ShowCharacterSheet();
            }
            else if (action == "Return to Title")
            {
                bool confirm = await DisplayAlert(
                    "Return to Title",
                    "Are you sure? Unsaved progress will be lost.",
                    "Yes",
                    "No"
                );
                if (confirm)
                    await Navigation.PopToRootAsync();
            }
        }

        private async void OnCharacter1Tapped(object sender, EventArgs e)
        {
            await _audioService.PlaySoundEffect("button_click.wav");
            await ShowCharacterSheet();
        }

        private async Task ShowCharacterSheet()
        {
            var raceStats = RaceData.GetRaceStats(_gameService.Player.Race);
            var classData = ClassData.GetClassData(_gameService.Player.Class);

            string sheet = $"=== CHARACTER SHEET ===\n\n" +
                          $"Name: {_gameService.Player.Name}\n" +
                          $"Race: {raceStats.Name}\n" +
                          $"Class: {classData.Name}\n" +
                          $"Level: {_gameService.Player.Level}\n" +
                          $"XP: {_gameService.Player.Experience}\n" +
                          $"HP: {_gameService.Player.Health}/{_gameService.Player.MaxHealth}\n" +
                          $"AC: {_gameService.Player.ArmorClass}\n" +
                          $"Gold: {_gameService.Player.Gold}\n\n" +
                          $"STR: {_gameService.Player.Stats.Strength} ({_gameService.Player.Stats.StrengthMod:+0;-#})\n" +
                          $"DEX: {_gameService.Player.Stats.Dexterity} ({_gameService.Player.Stats.DexterityMod:+0;-#})\n" +
                          $"CON: {_gameService.Player.Stats.Constitution} ({_gameService.Player.Stats.ConstitutionMod:+0;-#})\n" +
                          $"INT: {_gameService.Player.Stats.Intelligence} ({_gameService.Player.Stats.IntelligenceMod:+0;-#})\n" +
                          $"WIS: {_gameService.Player.Stats.Wisdom} ({_gameService.Player.Stats.WisdomMod:+0;-#})\n" +
                          $"CHA: {_gameService.Player.Stats.Charisma} ({_gameService.Player.Stats.CharismaMod:+0;-#})";

            await DisplayAlert("Character", sheet, "OK");
        }

        // ==================== UI UPDATES ====================

        private void UpdateUI()
        {
            // Update status bar
            LocationLabel.Text = $"{_gameService.Player.CurrentEra.ToString().ToUpper()} ERA - CAVERN";
            DirectionLabel.Text = GetDirectionSymbol(_playerDirection) + " " + _playerDirection.ToString()[0];
            TurnLabel.Text = $"TURN: {_turnCount}";
            GoldLabel.Text = $"💰 {_gameService.Player.Gold} GP";

            // Update character portrait
            Char1Name.Text = _gameService.Player.Name.ToUpper();
            Char1HP.Text = $"{_gameService.Player.Health}/{_gameService.Player.MaxHealth}";

            // Update HP bar
            double hpPercent = (double)_gameService.Player.Health / _gameService.Player.MaxHealth;
            Char1HPBar.WidthRequest = 100 * hpPercent;
            Char1HPBar.BackgroundColor = hpPercent > 0.5 ? Color.FromArgb("#00FF00") :
                                          hpPercent > 0.25 ? Color.FromArgb("#FFAA00") :
                                          Color.FromArgb("#FF0000");

            // Redraw canvases
            DungeonCanvas.InvalidateSurface();
            Portrait1Canvas.InvalidateSurface();
        }

        private string GetDirectionSymbol(Direction dir)
        {
            return dir switch
            {
                Direction.North => "▲",
                Direction.South => "▼",
                Direction.East => "►",
                Direction.West => "◄",
                _ => "●"
            };
        }

        private void AddMessage(string message)
        {
            string currentText = MessageLabel.Text;
            MessageLabel.Text = currentText + "\n" + message;

            // Auto-scroll to bottom
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(50);
                await MessageScrollView.ScrollToAsync(MessageLabel, ScrollToPosition.End, true);
            });
        }

        // ==================== CANVAS DRAWING ====================

        private void OnDungeonCanvasPaint(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;
            canvas.Clear(SKColors.Black);

            // Draw a simple first-person 3D view
            DrawFirstPersonDungeon(canvas, info.Width, info.Height);
        }

        private void DrawFirstPersonDungeon(SKCanvas canvas, int width, int height)
        {
            // Simple pseudo-3D dungeon view (Eye of the Beholder style)
            var paint = new SKPaint();

            // Check walls in front, left, and right
            bool wallFront = !IsWalkableTile(_playerDirection);
            bool wallLeft = !IsWalkableTile(TurnLeft(_playerDirection));
            bool wallRight = !IsWalkableTile(TurnRight(_playerDirection));

            // Background (sky/ceiling)
            paint.Color = new SKColor(10, 5, 20);
            canvas.DrawRect(0, 0, width, height / 2, paint);

            // Floor
            paint.Color = new SKColor(40, 30, 20);
            canvas.DrawRect(0, height / 2, width, height / 2, paint);

            // Draw perspective walls
            if (wallFront)
            {
                // Front wall
                paint.Color = new SKColor(80, 60, 40);
                canvas.DrawRect(width * 0.25f, height * 0.25f, width * 0.5f, height * 0.5f, paint);

                // Wall details
                paint.Color = new SKColor(60, 45, 30);
                paint.StrokeWidth = 2;
                canvas.DrawLine(width * 0.5f, height * 0.25f, width * 0.5f, height * 0.75f, paint);
                canvas.DrawLine(width * 0.25f, height * 0.5f, width * 0.75f, height * 0.5f, paint);
            }
            else
            {
                // Corridor ahead - draw perspective lines
                paint.Color = new SKColor(60, 45, 30);
                paint.StrokeWidth = 3;

                // Left wall perspective
                canvas.DrawLine(0, height / 2, width * 0.25f, height * 0.25f, paint);
                canvas.DrawLine(0, height / 2, width * 0.25f, height * 0.75f, paint);

                // Right wall perspective  
                canvas.DrawLine(width, height / 2, width * 0.75f, height * 0.25f, paint);
                canvas.DrawLine(width, height / 2, width * 0.75f, height * 0.75f, paint);
            }

            if (wallLeft)
            {
                // Left wall
                paint.Color = new SKColor(70, 52, 35);
                var path = new SKPath();
                path.MoveTo(0, 0);
                path.LineTo(width * 0.25f, height * 0.25f);
                path.LineTo(width * 0.25f, height * 0.75f);
                path.LineTo(0, height);
                path.Close();
                canvas.DrawPath(path, paint);
            }

            if (wallRight)
            {
                // Right wall
                paint.Color = new SKColor(70, 52, 35);
                var path = new SKPath();
                path.MoveTo(width, 0);
                path.LineTo(width * 0.75f, height * 0.25f);
                path.LineTo(width * 0.75f, height * 0.75f);
                path.LineTo(width, height);
                path.Close();
                canvas.DrawPath(path, paint);
            }

            // Add some atmospheric lighting
            paint.Color = new SKColor(255, 200, 100, 30);
            paint.Style = SKPaintStyle.Fill;
            canvas.DrawCircle(width / 2, height / 2, 100, paint);
        }

        private bool IsWalkableTile(Direction dir)
        {
            int checkX = _playerX;
            int checkY = _playerY;

            switch (dir)
            {
                case Direction.North: checkY--; break;
                case Direction.South: checkY++; break;
                case Direction.East: checkX++; break;
                case Direction.West: checkX--; break;
            }

            return IsValidMove(checkX, checkY);
        }

        private Direction TurnLeft(Direction dir)
        {
            return dir switch
            {
                Direction.North => Direction.West,
                Direction.West => Direction.South,
                Direction.South => Direction.East,
                Direction.East => Direction.North,
                _ => Direction.North
            };
        }

        private Direction TurnRight(Direction dir)
        {
            return dir switch
            {
                Direction.North => Direction.East,
                Direction.East => Direction.South,
                Direction.South => Direction.West,
                Direction.West => Direction.North,
                _ => Direction.North
            };
        }

        private void OnPortrait1Paint(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;
            canvas.Clear(SKColors.Black);

            // Draw simple character sprite
            DrawCharacterPortrait(canvas, info.Width / 2, info.Height / 2, _gameService.Player);
        }

        private void DrawCharacterPortrait(SKCanvas canvas, float centerX, float centerY, Character character)
        {
            int pixelSize = 4;

            int[,] sprite = new int[,]
            {
                { 0, 0, 2, 2, 2, 2, 0, 0 },
                { 0, 2, 2, 2, 2, 2, 2, 0 },
                { 0, 2, 1, 2, 2, 1, 2, 0 },
                { 0, 2, 1, 1, 1, 1, 2, 0 },
                { 0, 0, 1, 1, 1, 1, 0, 0 },
                { 0, 0, 1, 1, 1, 1, 0, 0 },
                { 0, 1, 1, 0, 0, 1, 1, 0 },
                { 0, 1, 1, 0, 0, 1, 1, 0 }
            };

            var skinColor = GetColorForRace(character.Race, character.CurrentEra);
            var hairColor = SKColors.SaddleBrown;
            var paint = new SKPaint();

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (sprite[y, x] == 0) continue;

                    paint.Color = sprite[y, x] == 1 ? skinColor : hairColor;

                    float px = centerX - (4 * pixelSize) + (x * pixelSize);
                    float py = centerY - (4 * pixelSize) + (y * pixelSize);

                    canvas.DrawRect(px, py, pixelSize, pixelSize, paint);
                }
            }
        }

        private SKColor GetColorForRace(Race race, TechnologyEra era)
        {
            return race switch
            {
                Race.Human => new SKColor(210, 180, 140),
                Race.Elf => new SKColor(255, 220, 200),
                Race.Dwarf => new SKColor(190, 150, 120),
                Race.HalfOrc => new SKColor(140, 180, 120),
                Race.Halfling => new SKColor(230, 190, 160),
                _ => new SKColor(210, 180, 140)
            };
        }
    }

    public enum Direction
    {
        North,
        South,
        East,
        West
    }
}