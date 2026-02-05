using System.Text.Json;

namespace CavemanChronicles
{
    public class ItemLoaderService
    {
        private Dictionary<string, Item> _itemCache;
        private Dictionary<TechnologyEra, List<Item>> _itemsByEra;
        private bool _isLoaded = false;

        public ItemLoaderService()
        {
            _itemCache = new Dictionary<string, Item>();
            _itemsByEra = new Dictionary<TechnologyEra, List<Item>>();
        }

        public async Task LoadAllItems()
        {
            if (_isLoaded)
                return;

            try
            {
                // Load universal consumables
                await LoadItemsFromFile("items_consumables.json");
                
                // Load era-specific items
                await LoadItemsFromFile("items_caveman.json", TechnologyEra.Caveman);
                await LoadItemsFromFile("items_stoneage.json", TechnologyEra.StoneAge);
                await LoadItemsFromFile("items_bronzeage.json", TechnologyEra.BronzeAge);
                await LoadItemsFromFile("items_ironage.json", TechnologyEra.IronAge);
                await LoadItemsFromFile("items_medieval.json", TechnologyEra.Medieval);
                await LoadItemsFromFile("items_renaissance.json", TechnologyEra.Renaissance);
                await LoadItemsFromFile("items_industrial.json", TechnologyEra.Industrial);
                await LoadItemsFromFile("items_modern.json", TechnologyEra.Modern);
                await LoadItemsFromFile("items_future.json", TechnologyEra.Future);

                _isLoaded = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading items: {ex.Message}");
            }
        }

        private async Task LoadItemsFromFile(string fileName, TechnologyEra? era = null)
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                };

                var itemData = JsonSerializer.Deserialize<ItemDataFile>(json, options);

                if (itemData?.Items != null)
                {
                    foreach (var item in itemData.Items)
                    {
                        // Add to main cache
                        _itemCache[item.Id] = item;

                        // Add to era-specific list if applicable
                        if (item.Era != default(TechnologyEra))
                        {
                            if (!_itemsByEra.ContainsKey(item.Era))
                                _itemsByEra[item.Era] = new List<Item>();
                            
                            _itemsByEra[item.Era].Add(item);
                        }
                        else if (era.HasValue)
                        {
                            // If era not set in JSON but provided as parameter
                            item.Era = era.Value;
                            
                            if (!_itemsByEra.ContainsKey(era.Value))
                                _itemsByEra[era.Value] = new List<Item>();
                            
                            _itemsByEra[era.Value].Add(item);
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"Loaded {itemData.Items.Count} items from {fileName}");
                }
            }
            catch (FileNotFoundException)
            {
                System.Diagnostics.Debug.WriteLine($"Item file not found (optional): {fileName}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading {fileName}: {ex.Message}");
            }
        }

        public Item GetItem(string itemId)
        {
            if (!_isLoaded)
            {
                System.Diagnostics.Debug.WriteLine("Warning: Items not loaded yet. Call LoadAllItems() first.");
                return null;
            }

            if (_itemCache.ContainsKey(itemId))
            {
                return CloneItem(_itemCache[itemId]);
            }

            System.Diagnostics.Debug.WriteLine($"Item not found: {itemId}");
            return null;
        }

        public List<Item> GetAllItems()
        {
            if (!_isLoaded)
            {
                System.Diagnostics.Debug.WriteLine("Warning: Items not loaded yet. Call LoadAllItems() first.");
                return new List<Item>();
            }

            return _itemCache.Values.Select(CloneItem).ToList();
        }

        public List<Item> GetItemsByEra(TechnologyEra era)
        {
            if (!_isLoaded)
            {
                System.Diagnostics.Debug.WriteLine("Warning: Items not loaded yet. Call LoadAllItems() first.");
                return new List<Item>();
            }

            if (_itemsByEra.ContainsKey(era))
            {
                return _itemsByEra[era].Select(CloneItem).ToList();
            }

            return new List<Item>();
        }

        public List<Item> GetItemsByType(ItemType type)
        {
            if (!_isLoaded)
            {
                System.Diagnostics.Debug.WriteLine("Warning: Items not loaded yet. Call LoadAllItems() first.");
                return new List<Item>();
            }

            return _itemCache.Values
                .Where(i => i.ItemType == type)
                .Select(CloneItem)
                .ToList();
        }

        public List<Item> GetItemsByRarity(ItemRarity rarity)
        {
            if (!_isLoaded)
            {
                System.Diagnostics.Debug.WriteLine("Warning: Items not loaded yet. Call LoadAllItems() first.");
                return new List<Item>();
            }

            return _itemCache.Values
                .Where(i => i.Rarity == rarity)
                .Select(CloneItem)
                .ToList();
        }

        public List<Item> GetConsumables()
        {
            return GetItemsByType(ItemType.Consumable);
        }

        public List<Item> GetWeapons()
        {
            return GetItemsByType(ItemType.Weapon);
        }

        public List<Item> GetArmor()
        {
            return _itemCache.Values
                .Where(i => i.ItemType == ItemType.Armor || i.ItemType == ItemType.Shield)
                .Select(CloneItem)
                .ToList();
        }

        private Item CloneItem(Item item)
        {
            return new Item
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                ItemType = item.ItemType,
                Rarity = item.Rarity,
                Era = item.Era,
                Value = item.Value,
                Weight = item.Weight,
                IsStackable = item.IsStackable,
                MaxStackSize = item.MaxStackSize,
                Quantity = item.Quantity,
                Effect = item.Effect,
                EquipmentStats = item.EquipmentStats,
                EquipmentSlot = item.EquipmentSlot,
                IconEmoji = item.IconEmoji
            };
        }

        public bool IsLoaded => _isLoaded;
    }

    // Helper class for JSON deserialization
    public class ItemDataFile
    {
        public string Era { get; set; }
        public string Category { get; set; }
        public List<Item> Items { get; set; }
    }
}
