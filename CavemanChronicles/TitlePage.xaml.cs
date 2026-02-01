namespace CavemanChronicles
{
    public partial class TitlePage : ContentPage
    {
        private readonly AudioService _audioService;

        public TitlePage()
        {
            InitializeComponent();

            // Get audio service from dependency injection with fallback
            _audioService = Handler?.MauiContext?.Services.GetService<AudioService>()
                ?? new AudioService(Plugin.Maui.Audio.AudioManager.Current, new SettingsService());
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Start background music when page appears
            await _audioService.PlayBackgroundMusic("title_music.mp3");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Stop music when leaving the page
            _audioService.StopBackgroundMusic();
        }

        private async void OnNewGame(object sender, EventArgs e)
        {
            await _audioService.PlaySoundEffect("button_click.wav");
            await Task.Delay(200); // Brief delay for sound to play
            await Navigation.PushAsync(new CharacterCreationPage());
        }

        private async void OnLoadGame(object sender, EventArgs e)
        {
            await _audioService.PlaySoundEffect("button_click.wav");
            await Task.Delay(200);
            await Navigation.PushAsync(new LoadGamePage());
        }

        private async void OnSettings(object sender, EventArgs e)
        {
            await _audioService.PlaySoundEffect("button_click.wav");
            await Task.Delay(200);
            await Navigation.PushAsync(new SettingsPage());
        }

        // New Game Button Hover Effects
        private async void OnNewGamePointerEntered(object sender, PointerEventArgs e)
        {
            // Play hover sound
            await _audioService.PlaySoundEffect("button_hover.wav");

            // Brighten the glow
            await NewGameGlow.FadeTo(1.0, 150);

            // Scale up slightly for "lift" effect
            await NewGameBorder.ScaleTo(1.05, 150, Easing.CubicOut);

            // Brighten text
            NewGameText.TextColor = Color.FromArgb("#FFFFFF");

            // Animate icons
            await Task.WhenAll(
                NewGameLeftIcon.ScaleTo(1.2, 150, Easing.BounceOut),
                NewGameRightIcon.ScaleTo(1.2, 150, Easing.BounceOut)
            );
        }

        private async void OnNewGamePointerExited(object sender, PointerEventArgs e)
        {
            // Return glow to normal
            await NewGameGlow.FadeTo(0.5, 150);

            // Scale back to normal
            await NewGameBorder.ScaleTo(1.0, 150, Easing.CubicIn);

            // Return text color
            NewGameText.TextColor = Color.FromArgb("#FFD700");

            // Return icons to normal
            await Task.WhenAll(
                NewGameLeftIcon.ScaleTo(1.0, 150, Easing.CubicIn),
                NewGameRightIcon.ScaleTo(1.0, 150, Easing.CubicIn)
            );
        }

        // Load Game Button Hover Effects
        private async void OnLoadGamePointerEntered(object sender, PointerEventArgs e)
        {
            // Play hover sound
            await _audioService.PlaySoundEffect("button_hover.wav");

            // Brighten the glow
            await LoadGameGlow.FadeTo(1.0, 150);

            // Scale up slightly
            await LoadGameBorder.ScaleTo(1.05, 150, Easing.CubicOut);

            // Brighten text
            LoadGameText.TextColor = Color.FromArgb("#FFFFFF");

            // Animate icons
            await Task.WhenAll(
                LoadGameLeftIcon.ScaleTo(1.2, 150, Easing.BounceOut),
                LoadGameRightIcon.ScaleTo(1.2, 150, Easing.BounceOut)
            );
        }

        private async void OnLoadGamePointerExited(object sender, PointerEventArgs e)
        {
            // Return glow to normal
            await LoadGameGlow.FadeTo(0.5, 150);

            // Scale back to normal
            await LoadGameBorder.ScaleTo(1.0, 150, Easing.CubicIn);

            // Return text color
            LoadGameText.TextColor = Color.FromArgb("#00FFFF");

            // Return icons to normal
            await Task.WhenAll(
                LoadGameLeftIcon.ScaleTo(1.0, 150, Easing.CubicIn),
                LoadGameRightIcon.ScaleTo(1.0, 150, Easing.CubicIn)
            );
        }

        // Settings Button Hover Effects
        private async void OnSettingsPointerEntered(object sender, PointerEventArgs e)
        {
            await _audioService.PlaySoundEffect("button_hover.wav");
            await SettingsGameGlow.FadeTo(1.0, 150);
            await SettingsGameBorder.ScaleTo(1.05, 150, Easing.CubicOut);
            SettingsText.TextColor = Color.FromArgb("#FFFFFF");

            await Task.WhenAll(
                SettingsLeftIcon.ScaleTo(1.2, 150, Easing.BounceOut),
                SettingsRightIcon.ScaleTo(1.2, 150, Easing.BounceOut)
            );
        }

        private async void OnSettingsPointerExited(object sender, PointerEventArgs e)
        {
            await SettingsGameGlow.FadeTo(0.5, 150);
            await SettingsGameBorder.ScaleTo(1.0, 150, Easing.CubicIn);
            SettingsText.TextColor = Color.FromArgb("#00FF00");

            await Task.WhenAll(
                SettingsLeftIcon.ScaleTo(1.0, 150, Easing.CubicIn),
                SettingsRightIcon.ScaleTo(1.0, 150, Easing.CubicIn)
            );
        }
    }
}