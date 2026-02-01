namespace CavemanChronicles
{
    public partial class SettingsPage : ContentPage
    {
        private readonly AudioService _audioService;
        private readonly SettingsService _settingsService;

        public SettingsPage()
        {
            InitializeComponent();

            // Get services with proper fallbacks
            var settingsService = Handler?.MauiContext?.Services.GetService<SettingsService>()
                ?? new SettingsService();

            _settingsService = settingsService;
            _audioService = Handler?.MauiContext?.Services.GetService<AudioService>()
                ?? new AudioService(Plugin.Maui.Audio.AudioManager.Current, settingsService);

            LoadSettings();
        }

        private void LoadSettings()
        {
            // Load current settings
            MusicSwitch.IsToggled = _settingsService.MusicEnabled;
            SoundSwitch.IsToggled = _settingsService.SoundEnabled;
            MusicVolumeSlider.Value = _settingsService.MusicVolume * 100;
            SoundVolumeSlider.Value = _settingsService.SoundVolume * 100;

            // Update UI based on switches
            UpdateMusicVolumeVisibility();
            UpdateSoundVolumeVisibility();
        }

        private void OnMusicToggled(object sender, ToggledEventArgs e)
        {
            _audioService.IsMusicEnabled = e.Value;
            UpdateMusicVolumeVisibility();
        }

        private void OnSoundToggled(object sender, ToggledEventArgs e)
        {
            _audioService.IsSoundEnabled = e.Value;
            UpdateSoundVolumeVisibility();
        }

        private void OnMusicVolumeChanged(object sender, ValueChangedEventArgs e)
        {
            double volume = e.NewValue / 100.0;
            _audioService.MusicVolume = volume;
            MusicVolumeLabel.Text = $"{(int)e.NewValue}%";
        }

        private void OnSoundVolumeChanged(object sender, ValueChangedEventArgs e)
        {
            double volume = e.NewValue / 100.0;
            _audioService.SoundVolume = volume;
            SoundVolumeLabel.Text = $"{(int)e.NewValue}%";
        }

        private void UpdateMusicVolumeVisibility()
        {
            MusicVolumePanel.IsVisible = MusicSwitch.IsToggled;
        }

        private void UpdateSoundVolumeVisibility()
        {
            SoundVolumePanel.IsVisible = SoundSwitch.IsToggled;
        }

        private async void OnTestSound(object sender, EventArgs e)
        {
            await _audioService.PlaySoundEffect("button_click.wav");
        }

        private async void OnResetDefaults(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert(
                "Reset Settings",
                "Reset all settings to default values?",
                "Reset",
                "Cancel");

            if (confirm)
            {
                _settingsService.ResetToDefaults();
                LoadSettings();

                // Apply to audio service
                _audioService.IsMusicEnabled = _settingsService.MusicEnabled;
                _audioService.IsSoundEnabled = _settingsService.SoundEnabled;
                _audioService.MusicVolume = _settingsService.MusicVolume;
                _audioService.SoundVolume = _settingsService.SoundVolume;

                await DisplayAlert("Success", "Settings reset to defaults!", "OK");
            }
        }

        private async void OnBack(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}