using Plugin.Maui.Audio;

namespace CavemanChronicles
{
    public class AudioService : IDisposable
    {
        private readonly IAudioManager _audioManager;
        private readonly SettingsService _settingsService;
        private IAudioPlayer? _backgroundMusic;
        private Dictionary<string, IAudioPlayer> _soundEffects;
        private bool _disposed = false;

        public AudioService(IAudioManager audioManager, SettingsService settingsService)
        {
            _audioManager = audioManager;
            _settingsService = settingsService;
            _soundEffects = new Dictionary<string, IAudioPlayer>();
        }

        public bool IsMusicEnabled
        {
            get => _settingsService.MusicEnabled;
            set
            {
                _settingsService.MusicEnabled = value;
                if (!value && _backgroundMusic != null)
                {
                    _backgroundMusic.Pause();
                }
                else if (value && _backgroundMusic != null)
                {
                    _backgroundMusic.Play();
                }
            }
        }

        public bool IsSoundEnabled
        {
            get => _settingsService.SoundEnabled;
            set => _settingsService.SoundEnabled = value;
        }

        public double MusicVolume
        {
            get => _settingsService.MusicVolume;
            set
            {
                _settingsService.MusicVolume = value;
                if (_backgroundMusic != null)
                {
                    _backgroundMusic.Volume = value;
                }
            }
        }

        public double SoundVolume
        {
            get => _settingsService.SoundVolume;
            set
            {
                _settingsService.SoundVolume = value;
                foreach (var sound in _soundEffects.Values)
                {
                    sound.Volume = value;
                }
            }
        }

        public async Task PlayBackgroundMusic(string fileName)
        {
            ThrowIfDisposed();  

            try
            {
                // Stop current music if playing
                if (_backgroundMusic != null)
                {
                    _backgroundMusic.Stop();
                    _backgroundMusic.Dispose();
                }

                var audioStream = await FileSystem.OpenAppPackageFileAsync(fileName).ConfigureAwait(false);
                _backgroundMusic = _audioManager.CreatePlayer(audioStream);
                _backgroundMusic.Volume = MusicVolume;
                _backgroundMusic.Loop = true;

                if (IsMusicEnabled)
                {
                    _backgroundMusic.Play();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing background music: {ex.Message}");
            }
        }

        public async Task PlaySoundEffect(string fileName)
        {
            ThrowIfDisposed();  // ✅ Added

            if (!IsSoundEnabled) return;

            try
            {
                // Check if we already have this sound loaded
                if (!_soundEffects.ContainsKey(fileName))
                {
                    var audioStream = await FileSystem.OpenAppPackageFileAsync(fileName).ConfigureAwait(false);
                    var player = _audioManager.CreatePlayer(audioStream);
                    player.Volume = SoundVolume;
                    _soundEffects[fileName] = player;
                }

                var soundPlayer = _soundEffects[fileName];

                // Reset to beginning if already playing
                soundPlayer.Stop();
                soundPlayer.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing sound effect {fileName}: {ex.Message}");
            }
        }
    
        public void StopBackgroundMusic()
        {
            _backgroundMusic?.Stop();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Dispose managed resources
                try
                {
                    _backgroundMusic?.Stop();
                    _backgroundMusic?.Dispose();
                    _backgroundMusic = null;

                    foreach (var sound in _soundEffects.Values)
                    {
                        try
                        {
                            sound?.Stop();
                            sound?.Dispose();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error disposing sound: {ex.Message}");
                        }
                    }
                    _soundEffects.Clear();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error disposing AudioService: {ex.Message}");
                }
            }

            _disposed = true;
        }
        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(AudioService));
            }
        }

    }

}