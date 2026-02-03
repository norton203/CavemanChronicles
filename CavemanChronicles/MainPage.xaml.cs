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
        private List<string> _gameTextHistory = new List<string>();

        public MainPage(Character character, GameService gameService, CombatService combatService,
                       MonsterLoaderService monsterLoader, AudioService audioService)
        {
            InitializeComponent();

            _gameService = gameService;
            _combatService = combatService;
            _monsterLoader = monsterLoader;
            _audioService = audioService;

            _gameService.Player = character;
            _gameService.SetAudioService(_audioService);
            _gameService.SetMonsterLoader(_monsterLoader);
            _gameService.SetCombatService(_combatService);
            _gameService.SetNavigation(Navigation); // SET NAVIGATION

            // Add welcome message with race info
            var raceStats = RaceData.GetRaceStats(character.Race);
            _gameTextHistory.Add($"Welcome to Caveman Chronicles, {character.Name} the {raceStats.Name}!");
            _gameTextHistory.Add("");
            _gameTextHistory.Add("You awaken in a primitive cave. The smell of smoke and dirt fills your nostrils.");
            _gameTextHistory.Add($"As a {raceStats.Name}, you possess unique strengths that will aid your journey through the ages.");
            _gameTextHistory.Add("");
            _gameTextHistory.Add("Type 'help' to see available commands, or 'attack' to start combat!");

            GameTextLabel.Text = string.Join("\n\n", _gameTextHistory);
            UpdateUI();
        }

        // Rest of MainPage code remains the same...
        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;

            canvas.Clear(SKColors.Black);
            DrawPixelCharacter(canvas, info.Width / 2, info.Height / 2);
            DrawRetroBorder(canvas, info.Width, info.Height);
        }

        private void DrawPixelCharacter(SKCanvas canvas, float centerX, float centerY)
        {
            int pixelSize = 8;

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

            SKColor skinColor = GetColorForRace(_gameService.Player?.Race ?? Race.Human,
                                                _gameService.Player?.CurrentEra ?? TechnologyEra.Caveman);
            SKColor hairColor = SKColors.SaddleBrown;

            var paint = new SKPaint();

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (cavemanSprite[y, x] == 0) continue;

                    paint.Color = cavemanSprite[y, x] == 1 ? skinColor : hairColor;

                    float drawX = centerX - (4 * pixelSize) + (x * pixelSize);
                    float drawY = centerY - (4 * pixelSize) + (y * pixelSize);

                    canvas.DrawRect(drawX, drawY, pixelSize, pixelSize, paint);
                }
            }
        }

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
                IsAntialias = false
            };

            paint.StrokeWidth = 1;
            for (int y = 0; y < height; y += 4)
            {
                paint.Color = SKColor.Parse("#00FF00").WithAlpha(30);
                canvas.DrawLine(0, y, width, y, paint);
            }
        }

        private async void OnCommandEntered(object sender, EventArgs e)
        {
            var command = CommandEntry.Text?.Trim();
            if (string.IsNullOrEmpty(command)) return;

            AppendGameText($"> {command}");

            string response = await _gameService.ProcessCommand(command);

            // Only append response if not empty (combat page handles its own display)
            if (!string.IsNullOrEmpty(response))
            {
                AppendGameText(response);
            }

            CommandEntry.Text = string.Empty;
            UpdateUI();
            GraphicsCanvas.InvalidateSurface();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Refresh UI when returning from combat
            UpdateUI();
        }

        private async void AppendGameText(string text)
        {
            _gameTextHistory.Add(text);

            if (_gameTextHistory.Count > 50)
            {
                _gameTextHistory.RemoveAt(0);
            }

            GameTextLabel.Text = string.Join("\n\n", _gameTextHistory);

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