namespace CavemanChronicles
{
    public class SettingsService
    {
        private const string MusicEnabledKey = "music_enabled";
        private const string SoundEnabledKey = "sound_enabled";
        private const string MusicVolumeKey = "music_volume";
        private const string SoundVolumeKey = "sound_volume";

        // Default values
        private const bool DefaultMusicEnabled = true;
        private const bool DefaultSoundEnabled = true;
        private const double DefaultMusicVolume = 0.4;
        private const double DefaultSoundVolume = 0.6;

        public bool MusicEnabled
        {
            get => Preferences.Get(MusicEnabledKey, DefaultMusicEnabled);
            set => Preferences.Set(MusicEnabledKey, value);
        }

        public bool SoundEnabled
        {
            get => Preferences.Get(SoundEnabledKey, DefaultSoundEnabled);
            set => Preferences.Set(SoundEnabledKey, value);
        }

        public double MusicVolume
        {
            get => Preferences.Get(MusicVolumeKey, DefaultMusicVolume);
            set => Preferences.Set(MusicVolumeKey, Math.Clamp(value, 0.0, 1.0));
        }

        public double SoundVolume
        {
            get => Preferences.Get(SoundVolumeKey, DefaultSoundVolume);
            set => Preferences.Set(SoundVolumeKey, Math.Clamp(value, 0.0, 1.0));
        }

        public void ResetToDefaults()
        {
            MusicEnabled = DefaultMusicEnabled;
            SoundEnabled = DefaultSoundEnabled;
            MusicVolume = DefaultMusicVolume;
            SoundVolume = DefaultSoundVolume;
        }
    }
}