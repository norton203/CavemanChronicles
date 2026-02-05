namespace CavemanChronicles
{
    /// <summary>
    /// Static wrapper around ItemLoaderService for convenient access to items.
    /// Items are now loaded from JSON files in Resources/Raw/GameData/
    /// </summary>
    public static class ItemDatabase
    {
        private static ItemLoaderService _loaderService;
        private static bool _initialized = false;

        public static void Initialize(ItemLoaderService loaderService = null)
        {
            if (_initialized)
                return;

            _loaderService = loaderService ?? new ItemLoaderService();
            
            // Load items asynchronously
            _ = _loaderService.LoadAllItems();
            
            _initialized = true;
        }

        public static Item GetItem(string itemId)
        {
            if (!_initialized || _loaderService == null)
            {
                System.Diagnostics.Debug.WriteLine("ItemDatabase not initialized! Call Initialize() first.");
                return null;
            }

            return _loaderService.GetItem(itemId);
        }

        public static List<Item> GetAllItems()
        {
            if (!_initialized || _loaderService == null)
            {
                System.Diagnostics.Debug.WriteLine("ItemDatabase not initialized! Call Initialize() first.");
                return new List<Item>();
            }

            return _loaderService.GetAllItems();
        }

        public static List<Item> GetItemsByEra(TechnologyEra era)
        {
            if (!_initialized || _loaderService == null)
            {
                System.Diagnostics.Debug.WriteLine("ItemDatabase not initialized! Call Initialize() first.");
                return new List<Item>();
            }

            return _loaderService.GetItemsByEra(era);
        }

        public static List<Item> GetItemsByType(ItemType type)
        {
            if (!_initialized || _loaderService == null)
            {
                System.Diagnostics.Debug.WriteLine("ItemDatabase not initialized! Call Initialize() first.");
                return new List<Item>();
            }

            return _loaderService.GetItemsByType(type);
        }

        public static List<Item> GetItemsByRarity(ItemRarity rarity)
        {
            if (!_initialized || _loaderService == null)
            {
                System.Diagnostics.Debug.WriteLine("ItemDatabase not initialized! Call Initialize() first.");
                return new List<Item>();
            }

            return _loaderService.GetItemsByRarity(rarity);
        }

        public static List<Item> GetConsumables()
        {
            return GetItemsByType(ItemType.Consumable);
        }

        public static List<Item> GetWeapons()
        {
            return GetItemsByType(ItemType.Weapon);
        }

        public static List<Item> GetArmor()
        {
            if (!_initialized || _loaderService == null)
            {
                System.Diagnostics.Debug.WriteLine("ItemDatabase not initialized! Call Initialize() first.");
                return new List<Item>();
            }

            return _loaderService.GetArmor();
        }

        public static bool IsLoaded => _loaderService?.IsLoaded ?? false;
    }
}
