public class GameService
{
    public Character Player { get; set; }

    public void StartNewGame(string playerName)
    {
        Player = new Character
        {
            Name = playerName,
            Level = 1,
            CurrentEra = TechnologyEra.Caveman,
            Health = 100,
            MaxHealth = 100,
            Inventory = new List<Item>()
        };
    }

    public string ProcessCommand(string command)
    {
        // Parse and execute commands
        // Return narrative text response
    }

    public void LevelUp()
    {
        Player.Level++;
        UpdateTechnologyEra();
    }

    private void UpdateTechnologyEra()
    {
        Player.CurrentEra = Player.Level switch
        {
            <= 5 => TechnologyEra.Caveman,
            <= 10 => TechnologyEra.StoneAge,
            <= 15 => TechnologyEra.BronzeAge,
            // ... etc
        };
    }
}