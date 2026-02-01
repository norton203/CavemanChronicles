using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace CavemanChronicles
{
    public partial class MainPage : ContentPage
    {
        private readonly GameService _gameService;
        private List<string> _gameTextHistory = new List<string>();

        public MainPage(Character character)
        {
            InitializeComponent();
            _gameService = new GameService();
            _gameService.Player = character;

            // Add welcome message with race info
            var raceStats = RaceData.GetRaceStats(character.Race);
            _gameTextHistory.Add($"Welcome to Caveman Chronicles, {character.Name} the {raceStats.Name}!");
            _gameTextHistory.Add("");
            _gameTextHistory.Add("You awaken in a primitive cave. The smell of smoke and dirt fills your nostrils.");
            _gameTextHistory.Add($"As a {raceStats.Name}, you possess unique strengths that will aid your journey through the ages.");
            _gameTextHistory.Add("");
            _gameTextHistory.Add("Type 'help' to see available commands.");

            GameTextLabel.Text = string.Join("\n\n", _gameTextHistory);
            UpdateUI();
        }

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;

            canvas.Clear(SKColors.Black);

            // Draw pixel art character in center
            DrawPixelCharacter(canvas, info.Width / 2, info.Height / 2);

            // Draw decorative border (retro CRT effect)
            DrawRetroBorder(canvas, info.Width, info.Height);
        }

        private void DrawPixelCharacter(SKCanvas canvas, float centerX, float centerY)
        {
            // Pixel size (makes it look retro/chunky)
            int pixelSize = 8;

            // Simple caveman sprite (8x8 grid)
            // 1 = skin color, 2 = hair/beard, 0 = transparent
            int[,] cavemanSprite = new int[,]
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

            // Color palette based on tech era and race
            SKColor skinColor = GetColorForRace(_gameService.Player?.Race ?? Race.Human,
                                                _gameService.Player?.CurrentEra ?? TechnologyEra.Caveman);
            SKColor hairColor = SKColors.SaddleBrown;

            var paint = new SKPaint();

            // Draw the sprite
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (cavemanSprite[y, x] == 0) continue; // Transparent

                    paint.Color = cavemanSprite[y, x] == 1 ? skinColor : hairColor;

                    float drawX = centerX - (4 * pixelSize) + (x * pixelSize);
                    float drawY = centerY - (4 * pixelSize) + (y * pixelSize);

                    canvas.DrawRect(drawX, drawY, pixelSize, pixelSize, paint);
                }
            }
        }

        private SKColor GetColorForRace(Race race, TechnologyEra era)
        {
            // Base color varies by era
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

            // Slight tint based on race
            return race switch
            {
                Race.Elf => AdjustColor(baseColor, 1.1f, 1.0f, 0.9f),      // Slightly greenish
                Race.Dwarf => AdjustColor(baseColor, 0.9f, 0.85f, 0.8f),   // Slightly darker/brownish
                Race.HalfOrc => AdjustColor(baseColor, 0.85f, 1.0f, 0.85f),// Slightly greenish
                Race.Halfling => AdjustColor(baseColor, 1.05f, 0.95f, 0.9f),// Slightly peachy
                _ => baseColor // Human - no adjustment
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

        private void DrawRetroBorder(SKCanvas canvas, int width, int height)
        {
            var paint = new SKPaint
            {
                Color = SKColor.Parse("#00FF00"),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2,
                IsAntialias = false // Pixel-perfect edges
            };

            // Draw scanline effect (optional - gives CRT monitor feel)
            paint.StrokeWidth = 1;
            for (int y = 0; y < height; y += 4)
            {
                paint.Color = SKColor.Parse("#00FF00").WithAlpha(30);
                canvas.DrawLine(0, y, width, y, paint);
            }
        }

        private async void OnCommandEntered(object sender, EventArgs e)
        {
            var command = CommandEntry.Text?.Trim().ToLower();
            if (string.IsNullOrEmpty(command)) return;

            // Add command to history
            AppendGameText($"> {command}");

            // Process command (now async)
            string response = await _gameService.ProcessCommand(command);
            AppendGameText(response);

            // Clear input
            CommandEntry.Text = string.Empty;

            // Update UI
            UpdateUI();

            // Redraw graphics
            GraphicsCanvas.InvalidateSurface();
        }

        private async void AppendGameText(string text)
        {
            _gameTextHistory.Add(text);

            // Keep only last 50 lines to prevent memory issues
            if (_gameTextHistory.Count > 50)
            {
                _gameTextHistory.RemoveAt(0);
            }

            GameTextLabel.Text = string.Join("\n\n", _gameTextHistory);

            // Auto-scroll to bottom with a small delay to let UI update
            await Task.Delay(50);
            await GameTextScroll.ScrollToAsync(GameTextLabel, ScrollToPosition.End, true);
        }

        private void UpdateUI()
        {
            if (_gameService.Player == null) return;

            var raceStats = RaceData.GetRaceStats(_gameService.Player.Race);
            CharacterNameLabel.Text = $"{_gameService.Player.Name.ToUpper()} ({raceStats.Name.ToUpper()})";
            LevelLabel.Text = _gameService.Player.Level.ToString();
            HealthLabel.Text = $"{_gameService.Player.Health}/{_gameService.Player.MaxHealth}";
            EraLabel.Text = $"{_gameService.Player.CurrentEra.ToString().ToUpper()} ERA";
        }
    }
}