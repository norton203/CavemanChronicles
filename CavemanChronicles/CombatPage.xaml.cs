using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace CavemanChronicles
{
    public partial class CombatPage : ContentPage
    {
        private readonly Character _player;
        private readonly List<Monster> _enemies;
        private readonly CombatService _combatService;
        private readonly AudioService _audioService;

        private List<string> _combatLog;
        private int _roundNumber;
        private bool _isPlayerTurn;
        private bool _playerDefending;
        private Monster _selectedTarget;

        public CombatPage(Character player, List<Monster> enemies, CombatService combatService, AudioService audioService)
        {
            InitializeComponent();

            _player = player;
            _enemies = enemies;
            _combatService = combatService;
            _audioService = audioService;
            _combatLog = new List<string>();
            _roundNumber = 1;
            _isPlayerTurn = true;

            InitializeCombat();
        }

        private async void InitializeCombat()
        {
            // Set era label
            EraLabel.Text = $"{_player.CurrentEra.ToString().ToUpper()} ERA BATTLE";

            // Set player info
            PlayerNameLabel.Text = _player.Name.ToUpper();

            // Add initial combat log
            AddToCombatLog("=== BATTLE START ===");
            if (_enemies.Count == 1)
                AddToCombatLog($"A {_enemies[0].Name} appears!");
            else
                AddToCombatLog($"{_enemies.Count} enemies appear!");
            AddToCombatLog("");

            // Update displays
            UpdatePlayerDisplay();
            UpdateEnemyDisplay();

            // Play battle music
            await _audioService.PlaySoundEffect("combat_start.wav");

            // Start first turn
            StartPlayerTurn();
        }

        private void StartPlayerTurn()
        {
            _isPlayerTurn = true;
            _playerDefending = false;

            TurnIndicator.IsVisible = true;
            TurnIndicator.Stroke = Color.FromArgb("#FFD700");
            TurnIndicator.BackgroundColor = Color.FromArgb("#332200");
            ((Label)TurnIndicator.Content).Text = "YOUR\nTURN";
            ((Label)TurnIndicator.Content).TextColor = Color.FromArgb("#FFD700");

            EnableActionButtons(true);
            AddToCombatLog("► Your turn!");
        }

        private async void StartEnemyTurn()
        {
            _isPlayerTurn = false;

            TurnIndicator.Stroke = Color.FromArgb("#FF0000");
            TurnIndicator.BackgroundColor = Color.FromArgb("#330000");
            ((Label)TurnIndicator.Content).Text = "ENEMY\nTURN";
            ((Label)TurnIndicator.Content).TextColor = Color.FromArgb("#FF0000");

            EnableActionButtons(false);
            AddToCombatLog("▼ Enemy turn!");

            await Task.Delay(1000);

            // Each enemy attacks
            foreach (var enemy in _enemies.Where(e => e.HitPoints > 0))
            {
                await ExecuteEnemyAttack(enemy);
                await Task.Delay(800);

                // Check if player defeated
                if (_player.Health <= 0)
                {
                    await HandleDefeat();
                    return;
                }
            }

            // Start next round
            _roundNumber++;
            await Task.Delay(500);
            StartPlayerTurn();
        }

        private async Task ExecuteEnemyAttack(Monster enemy)
        {
            // Select random attack
            var attack = enemy.Attacks[Random.Shared.Next(enemy.Attacks.Count)];

            AddToCombatLog($"{enemy.Name} uses {attack.Name}!");

            // Roll to hit (d20 + attack bonus vs player AC)
            
            int attackRoll = Random.Shared.Next(1, 21) + attack.AttackBonus;

            if (attackRoll >= _player.ArmorClass)
            {
                // Hit! Roll damage
                int totalDamage = 0;
                for (int i = 0; i < attack.DamageDiceCount; i++)
                {
                    totalDamage += Random.Shared.Next(1, attack.DamageDieSize + 1);
                }
                totalDamage += attack.DamageBonus;

                // Apply defend reduction
                if (_playerDefending)
                {
                    totalDamage = totalDamage / 2;
                    AddToCombatLog($"You defend! Damage reduced by half!");
                }

                _player.Health -= totalDamage;
                if (_player.Health < 0) _player.Health = 0;

                AddToCombatLog($"Hit! You take {totalDamage} {attack.DamageType} damage!");

                await _audioService.PlaySoundEffect("combat_hit.wav");
                await FlashPlayer();
            }
            else
            {
                AddToCombatLog($"Miss! You dodge the attack!");
            }

            UpdatePlayerDisplay();
        }

        private void EnableActionButtons(bool enabled)
        {
            AttackButton.Opacity = enabled ? 1.0 : 0.5;
            DefendButton.Opacity = enabled ? 1.0 : 0.5;
            FleeButton.Opacity = enabled ? 1.0 : 0.5;

            AttackButton.IsEnabled = enabled;
            DefendButton.IsEnabled = enabled;
            FleeButton.IsEnabled = enabled;
        }

        private void UpdatePlayerDisplay()
        {
            PlayerHPLabel.Text = $"{_player.Health}/{_player.MaxHealth}";

            double hpPercent = (double)_player.Health / _player.MaxHealth;
            PlayerHPBar.Progress = hpPercent;

            // Change color based on health
            if (hpPercent > 0.5)
                PlayerHPBar.ProgressColor = Color.FromArgb("#00FF00");
            else if (hpPercent > 0.25)
                PlayerHPBar.ProgressColor = Color.FromArgb("#FFAA00");
            else
                PlayerHPBar.ProgressColor = Color.FromArgb("#FF0000");

            PlayerCanvas.InvalidateSurface();
        }

        private void UpdateEnemyDisplay()
        {
            EnemyStatsContainer.Clear();

            for (int i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                if (enemy.HitPoints <= 0)
                    continue;

                var enemyCard = CreateEnemyCard(enemy, i);
                EnemyStatsContainer.Add(enemyCard);
            }

            EnemyCanvas.InvalidateSurface();
        }

        private View CreateEnemyCard(Monster enemy, int index)
        {
            var border = new Border
            {
                Stroke = Color.FromArgb("#FF0000"),
                StrokeThickness = 2,
                Padding = 10,
                BackgroundColor = Color.FromArgb("#110000"),
                WidthRequest = 150
            };

            var stack = new VerticalStackLayout { Spacing = 5 };

            // Enemy name
            var nameLabel = new Label
            {
                Text = _enemies.Count > 1 ? $"{enemy.Name} #{index + 1}" : enemy.Name,
                TextColor = Color.FromArgb("#FF0000"),
                FontFamily = "Courier New",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold
            };
            stack.Add(nameLabel);

            // HP label
            var hpTextLabel = new Label
            {
                Text = "HP",
                TextColor = Color.FromArgb("#FF0000"),
                FontFamily = "Courier New",
                FontSize = 10
            };
            stack.Add(hpTextLabel);

            // HP bar
            double hpPercent = (double)enemy.HitPoints / enemy.MaxHitPoints;
            var hpBarBorder = new Border
            {
                Stroke = Color.FromArgb("#FF0000"),
                StrokeThickness = 1,
                HeightRequest = 15,
                BackgroundColor = Color.FromArgb("#000000")
            };

            var hpBar = new ProgressBar
            {
                Progress = hpPercent,
                ProgressColor = hpPercent > 0.5 ? Color.FromArgb("#FF0000") :
                               hpPercent > 0.25 ? Color.FromArgb("#FF8800") :
                               Color.FromArgb("#880000"),
                BackgroundColor = Color.FromArgb("#000000")
            };
            hpBarBorder.Content = hpBar;
            stack.Add(hpBarBorder);

            // HP value
            var hpValueLabel = new Label
            {
                Text = $"{enemy.HitPoints}/{enemy.MaxHitPoints}",
                TextColor = Color.FromArgb("#FF0000"),
                FontFamily = "Courier New",
                FontSize = 10
            };
            stack.Add(hpValueLabel);

            border.Content = stack;
            return border;
        }

        private void AddToCombatLog(string message)
        {
            _combatLog.Add(message);
            if (_combatLog.Count > 30)
                _combatLog.RemoveAt(0);

            CombatLogLabel.Text = string.Join("\n", _combatLog);

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(50);
                await CombatLogScroll.ScrollToAsync(CombatLogLabel, ScrollToPosition.End, false);
            });
        }

        // ==================== BUTTON EVENT HANDLERS ====================

        private void OnAttackClicked(object sender, EventArgs e)
        {
            if (!_isPlayerTurn) return;

            // If multiple enemies alive, show target selection
            var aliveEnemies = _enemies.Where(m => m.HitPoints > 0).ToList();

            if (aliveEnemies.Count > 1)
            {
                ShowTargetSelection();
            }
            else if (aliveEnemies.Count == 1)
            {
                ExecutePlayerAttack(aliveEnemies[0]);
            }
        }

        private void ShowTargetSelection()
        {
            TargetButtonsContainer.Clear();

            for (int i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                if (enemy.HitPoints <= 0)
                    continue;

                int enemyIndex = i;

                var targetButton = new Border
                {
                    Stroke = Color.FromArgb("#FFD700"),
                    StrokeThickness = 2,
                    Padding = 15,
                    BackgroundColor = Color.FromArgb("#332200"),
                    Margin = new Thickness(0, 5)
                };

                targetButton.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() => OnTargetSelected(enemy))
                });

                var label = new Label
                {
                    Text = $"► {enemy.Name} #{enemyIndex + 1}\nHP: {enemy.HitPoints}/{enemy.MaxHitPoints}",
                    TextColor = Color.FromArgb("#FFD700"),
                    FontFamily = "Courier New",
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold
                };

                targetButton.Content = label;
                TargetButtonsContainer.Add(targetButton);
            }

            TargetSelectionOverlay.IsVisible = true;
        }

        private void OnTargetSelected(Monster target)
        {
            TargetSelectionOverlay.IsVisible = false;
            ExecutePlayerAttack(target);
        }

        private void OnCancelTargetSelection(object sender, EventArgs e)
        {
            TargetSelectionOverlay.IsVisible = false;
        }

        private async void ExecutePlayerAttack(Monster target)
        {
            AddToCombatLog($"You attack {target.Name}!");

            // Get player's weapon attack bonus (simplified)
            int attackBonus = _player.Stats.StrengthMod + _player.ProficiencyBonus;

            // Roll to hit
            
            int attackRoll = Random.Shared.Next(1, 21) + attackBonus;

            if (attackRoll >= target.ArmorClass)
            {
                // Hit! Calculate damage
                int damage = Random.Shared.Next(1, 9) + _player.Stats.StrengthMod; // d8 + STR mod
                damage = Math.Max(1, damage);

                target.HitPoints -= damage;
                if (target.HitPoints < 0) target.HitPoints = 0;

                AddToCombatLog($"Hit! Dealt {damage} damage!");

                await _audioService.PlaySoundEffect("combat_hit.wav");

                // Check if enemy defeated
                if (target.HitPoints <= 0)
                {
                    AddToCombatLog($"{target.Name} defeated!");
                    await _audioService.PlaySoundEffect("enemy_defeat.wav");

                    // Check if all enemies defeated
                    if (_enemies.All(e => e.HitPoints <= 0))
                    {
                        await HandleVictory();
                        return;
                    }
                }
            }
            else
            {
                AddToCombatLog("Miss! Your attack fails to connect!");
            }

            UpdateEnemyDisplay();

            await Task.Delay(1000);
            StartEnemyTurn();
        }

        private async void OnDefendClicked(object sender, EventArgs e)
        {
            if (!_isPlayerTurn) return;

            _playerDefending = true;
            PlayerStatusLabel.Text = "Defending...";
            AddToCombatLog("You brace for impact!");

            await Task.Delay(1000);
            StartEnemyTurn();
        }

        private async void OnFleeClicked(object sender, EventArgs e)
        {
            if (!_isPlayerTurn) return;

            
            int fleeChance = Random.Shared.Next(1, 101);

            if (fleeChance > 50)
            {
                AddToCombatLog("You successfully flee from battle!");
                await _audioService.PlaySoundEffect("button_click.wav");
                await Task.Delay(1000);
                await Navigation.PopAsync();
            }
            else
            {
                AddToCombatLog("Escape failed!");
                await Task.Delay(1000);
                StartEnemyTurn();
            }
        }

        // ==================== VICTORY/DEFEAT HANDLING ====================

        private async Task HandleVictory()
        {
            await _audioService.PlaySoundEffect("level_up.wav");

            AddToCombatLog("");
            AddToCombatLog("=== VICTORY! ===");

            // Calculate rewards
            int totalExp = _enemies.Sum(e => e.ExperienceValue);
            int totalGold = _enemies.Sum(e => Random.Shared.Next(e.MinGold, e.MaxGold + 1));

            _player.Experience += totalExp;
            _player.Gold += totalGold;

            AddToCombatLog($"Gained {totalExp} XP!");
            AddToCombatLog($"Found {totalGold} Gold!");

            // Check for level up
            while (_player.Experience >= _player.Level * 100)
            {
                LevelUp();
            }

            TurnIndicator.IsVisible = false;
            EnableActionButtons(false);

            await Task.Delay(3000);
            await Navigation.PopAsync();
        }

        private void LevelUp()
        {
            _player.Level++;
            _player.Experience -= (_player.Level - 1) * 100;

            var classData = ClassData.GetClassData(_player.Class);
            int conMod = GameMath.CalculateModifier(_player.Stats.Constitution );
            int hpGain = Random.Shared.Next(1, classData.HitDie + 1) + conMod;
            hpGain = Math.Max(1, hpGain);

            _player.MaxHealth += hpGain;
            _player.Health = _player.MaxHealth;

            AddToCombatLog("");
            AddToCombatLog($"*** LEVEL UP! Now Level {_player.Level}! ***");
            AddToCombatLog($"HP increased by {hpGain}!");

            UpdatePlayerDisplay();
        }

        private async Task HandleDefeat()
        {
            await _audioService.PlaySoundEffect("game_over.wav");

            AddToCombatLog("");
            AddToCombatLog("=== DEFEAT ===");
            AddToCombatLog("You have fallen in battle...");

            TurnIndicator.IsVisible = false;
            EnableActionButtons(false);

            await Task.Delay(2000);
            await DisplayAlert("Defeat", "You have been defeated. Game Over.", "OK");
            await Navigation.PopToRootAsync();
        }

        private async Task FlashPlayer()
        {
            PlayerCanvas.Opacity = 0.3;
            await Task.Delay(100);
            PlayerCanvas.Opacity = 1.0;
        }

        // ==================== SPRITE DRAWING METHODS ====================

        private void OnPlayerCanvasPaint(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;

            canvas.Clear(SKColors.Transparent);
            DrawPlayerSprite(canvas, info.Width / 2, info.Height / 2);
        }

        private void OnEnemyCanvasPaint(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;

            canvas.Clear(SKColors.Transparent);

            // Draw all alive enemies
            var aliveEnemies = _enemies.Where(m => m.HitPoints > 0).ToList();
            int enemyCount = aliveEnemies.Count;

            if (enemyCount == 0) return;

            // Position enemies horizontally
            float spacing = info.Width / (enemyCount + 1);

            for (int i = 0; i < aliveEnemies.Count; i++)
            {
                float x = spacing * (i + 1);
                float y = info.Height / 2;
                DrawEnemySprite(canvas, x, y, aliveEnemies[i]);
            }
        }

        private void DrawPlayerSprite(SKCanvas canvas, float centerX, float centerY)
        {
            int pixelSize = 8;

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

            var skinColor = GetColorForRace(_player.Race, _player.CurrentEra);
            var hairColor = SKColors.SaddleBrown;
            var paint = new SKPaint();

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (sprite[y, x] == 0) continue;
                    paint.Color = sprite[y, x] == 1 ? skinColor : hairColor;
                    float drawX = centerX - (4 * pixelSize) + (x * pixelSize);
                    float drawY = centerY - (4 * pixelSize) + (y * pixelSize);
                    canvas.DrawRect(drawX, drawY, pixelSize, pixelSize, paint);
                }
            }
        }

        private void DrawEnemySprite(SKCanvas canvas, float centerX, float centerY, Monster enemy)
        {
            int pixelSize = 12;

            int[,] sprite = enemy.Type switch
            {
                MonsterType.Beast => new int[,]
                {
                    { 0, 1, 1, 0, 0, 1, 1, 0 },
                    { 0, 1, 1, 1, 1, 1, 1, 0 },
                    { 0, 0, 1, 1, 1, 1, 0, 0 },
                    { 1, 1, 1, 1, 1, 1, 1, 1 },
                    { 0, 1, 1, 1, 1, 1, 1, 0 },
                    { 0, 1, 0, 1, 1, 0, 1, 0 }
                },
                MonsterType.Dragon => new int[,]
                {
                    { 0, 0, 1, 1, 1, 1, 0, 0 },
                    { 0, 1, 1, 1, 1, 1, 1, 0 },
                    { 1, 1, 0, 1, 1, 0, 1, 1 },
                    { 1, 1, 1, 1, 1, 1, 1, 1 },
                    { 0, 1, 1, 1, 1, 1, 1, 0 },
                    { 1, 0, 1, 0, 0, 1, 0, 1 }
                },
                MonsterType.Undead => new int[,]
                {
                    { 0, 1, 1, 1, 1, 1, 1, 0 },
                    { 1, 0, 1, 0, 0, 1, 0, 1 },
                    { 1, 1, 1, 1, 1, 1, 1, 1 },
                    { 0, 1, 0, 1, 1, 0, 1, 0 },
                    { 0, 1, 1, 0, 0, 1, 1, 0 }
                },
                MonsterType.Construct => new int[,]
                {
                    { 1, 1, 1, 1, 1, 1, 1, 1 },
                    { 1, 0, 1, 0, 0, 1, 0, 1 },
                    { 1, 1, 1, 1, 1, 1, 1, 1 },
                    { 0, 1, 1, 1, 1, 1, 1, 0 },
                    { 0, 1, 0, 1, 1, 0, 1, 0 }
                },
                _ => new int[,]
                {
                    { 0, 1, 1, 1, 1, 1, 1, 0 },
                    { 1, 1, 1, 1, 1, 1, 1, 1 },
                    { 1, 1, 0, 1, 1, 0, 1, 1 },
                    { 1, 1, 1, 1, 1, 1, 1, 1 },
                    { 0, 1, 1, 0, 0, 1, 1, 0 }
                }
            };

            var color = GetColorForMonsterType(enemy.Type);
            var paint = new SKPaint { Color = color };

            int height = sprite.GetLength(0);
            int width = sprite.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (sprite[y, x] == 0) continue;
                    float drawX = centerX - (width * pixelSize / 2) + (x * pixelSize);
                    float drawY = centerY - (height * pixelSize / 2) + (y * pixelSize);
                    canvas.DrawRect(drawX, drawY, pixelSize, pixelSize, paint);
                }
            }
        }

        // ==================== COLOR HELPER METHODS ====================

        private SKColor GetColorForRace(Race race, TechnologyEra era)
        {
            SKColor baseColor = era switch
            {
                TechnologyEra.Caveman => SKColors.BurlyWood,
                TechnologyEra.StoneAge => SKColors.Tan,
                TechnologyEra.BronzeAge => SKColors.Peru,
                TechnologyEra.IronAge => SKColors.Gray,
                TechnologyEra.Medieval => SKColors.SteelBlue,
                TechnologyEra.Renaissance => SKColors.Gold,
                TechnologyEra.Industrial => SKColors.DarkSlateGray,
                TechnologyEra.Modern => SKColors.CornflowerBlue,
                TechnologyEra.Future => SKColors.Cyan,
                _ => SKColors.BurlyWood
            };

            return race switch
            {
                Race.Elf => AdjustColor(baseColor, 1.1f, 1.0f, 0.9f),
                Race.Dwarf => AdjustColor(baseColor, 0.9f, 0.85f, 0.8f),
                Race.HalfOrc => AdjustColor(baseColor, 0.85f, 1.0f, 0.85f),
                Race.Halfling => AdjustColor(baseColor, 1.05f, 0.95f, 0.9f),
                _ => baseColor
            };
        }

        private SKColor GetColorForMonsterType(MonsterType type)
        {
            return type switch
            {
                MonsterType.Beast => SKColors.SaddleBrown,
                MonsterType.Dragon => SKColors.DarkRed,
                MonsterType.Undead => SKColors.Gray,
                MonsterType.Humanoid => SKColors.Tan,
                MonsterType.Construct => SKColors.Silver,
                MonsterType.Elemental => SKColors.Orange,
                MonsterType.Aberration => SKColors.Purple,
                MonsterType.Monstrosity => SKColors.DarkGreen,
                _ => SKColors.Red
            };
        }

        private SKColor AdjustColor(SKColor color, float rMult, float gMult, float bMult)
        {
            return new SKColor(
                (byte)Math.Min(255, color.Red * rMult),
                (byte)Math.Min(255, color.Green * gMult),
                (byte)Math.Min(255, color.Blue * bMult)
            );
        }
    }
}