// Services/SaveService.cs
using System.Text.Json;

namespace CavemanChronicles
{
    public class SaveService
    {
        private const string SaveDirectory = "Saves";
        private readonly string _savePath;

        public SaveService()
        {
            _savePath = Path.Combine(FileSystem.AppDataDirectory, SaveDirectory);

            // Create save directory if it doesn't exist
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }
        }

        public async Task<bool> SaveCharacter(Character character)
        {
            try
            {
                var fileName = $"{character.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var filePath = Path.Combine(_savePath, fileName);

                var json = JsonSerializer.Serialize(character, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(filePath, json).ConfigureAwait(false);  // ✅ Added
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Save failed: {ex.Message}");
                return false;
            }
        }

        public async Task<List<SavedCharacterInfo>> GetSavedCharacters()
        {
            var savedChars = new List<SavedCharacterInfo>();

            try
            {
                var files = Directory.GetFiles(_savePath, "*.json");

                foreach (var file in files)
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(file).ConfigureAwait(false);  
                        var character = JsonSerializer.Deserialize<Character>(json);

                        if (character != null)
                        {
                            savedChars.Add(new SavedCharacterInfo
                            {
                                Character = character,
                                FileName = Path.GetFileName(file),
                                FilePath = file,
                                SaveDate = File.GetLastWriteTime(file)
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to load {file}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get saved characters: {ex.Message}");
            }

            return savedChars.OrderByDescending(s => s.SaveDate).ToList();
        }

        public async Task<Character?> LoadCharacter(string filePath)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);  
                return JsonSerializer.Deserialize<Character>(json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Load failed: {ex.Message}");
                return null;
            }
        }
    

      public bool DeleteSave(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete failed: {ex.Message}");
                return false;
            }
        }
    }
        public class SavedCharacterInfo
    {
        public Character Character { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime SaveDate { get; set; }

        public string DisplayName => $"{Character.Name} - Level {Character.Level} {Character.Race}";
        public string DisplayDate => SaveDate.ToString("yyyy-MM-dd HH:mm");
    }
}