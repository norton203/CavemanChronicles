namespace CavemanChronicles
{
    public class InventoryService
    {
        private const int MAX_INVENTORY_SLOTS = 50;

        public bool AddItem(Character character, Item item, int quantity = 1)
        {
            if (character.Inventory == null)
                character.Inventory = new List<Item>();

            // Check if item is stackable
            if (item.IsStackable)
            {
                var existingItem = character.Inventory.FirstOrDefault(i => i.Id == item.Id);
                if (existingItem != null)
                {
                    // Stack with existing item
                    int spaceLeft = existingItem.MaxStackSize - existingItem.Quantity;
                    int amountToAdd = Math.Min(quantity, spaceLeft);

                    existingItem.Quantity += amountToAdd;
                    quantity -= amountToAdd;

                    // If there's still more to add, create new stack
                    if (quantity > 0)
                    {
                        return AddNewItemStack(character, item, quantity);
                    }
                    return true;
                }
            }

            return AddNewItemStack(character, item, quantity);
        }

        private bool AddNewItemStack(Character character, Item item, int quantity)
        {
            // Check inventory space
            if (character.Inventory.Count >= MAX_INVENTORY_SLOTS)
                return false;

            var newItem = CloneItem(item);
            newItem.Quantity = quantity;
            character.Inventory.Add(newItem);
            return true;
        }

        public bool RemoveItem(Character character, string itemId, int quantity = 1)
        {
            var item = character.Inventory.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
                return false;

            if (item.Quantity > quantity)
            {
                item.Quantity -= quantity;
            }
            else
            {
                character.Inventory.Remove(item);
            }

            return true;
        }

        public Item GetItem(Character character, string itemId)
        {
            return character.Inventory.FirstOrDefault(i => i.Id == itemId);
        }

        public int GetItemCount(Character character, string itemId)
        {
            var items = character.Inventory.Where(i => i.Id == itemId);
            return items.Sum(i => i.Quantity);
        }

        public bool HasItem(Character character, string itemId, int quantity = 1)
        {
            return GetItemCount(character, itemId) >= quantity;
        }

        public bool UseConsumable(Character character, Item item)
        {
            if (item.ItemType != ItemType.Consumable || item.Effect == null)
                return false;

            bool success = ApplyConsumableEffect(character, item.Effect);

            if (success)
            {
                RemoveItem(character, item.Id, 1);
            }

            return success;
        }

        private bool ApplyConsumableEffect(Character character, ConsumableEffect effect)
        {
            switch (effect.Type)
            {
                case EffectType.HealHP:
                    int healAmount = effect.Value;
                    int actualHeal = Math.Min(healAmount, character.MaxHealth - character.Health);
                    character.Health += actualHeal;
                    return actualHeal > 0;

                case EffectType.BuffStrength:
                    character.Stats.Strength += effect.Value;
                    return true;

                case EffectType.BuffDexterity:
                    character.Stats.Dexterity += effect.Value;
                    return true;

                case EffectType.BuffConstitution:
                    character.Stats.Constitution += effect.Value;
                    return true;

                case EffectType.BuffIntelligence:
                    character.Stats.Intelligence += effect.Value;
                    return true;

                case EffectType.BuffWisdom:
                    character.Stats.Wisdom += effect.Value;
                    return true;

                case EffectType.BuffCharisma:
                    character.Stats.Charisma += effect.Value;
                    return true;

                default:
                    return false;
            }
        }

        public bool EquipItem(Character character, Item item)
        {
            if (item.ItemType != ItemType.Weapon &&
                item.ItemType != ItemType.Armor &&
                item.ItemType != ItemType.Shield &&
                item.ItemType != ItemType.Accessory)
                return false;

            if (item.EquipmentSlot == EquipmentSlot.None)
                return false;

            // Unequip item in that slot first
            var currentEquipped = GetEquippedItem(character, item.EquipmentSlot);
            if (currentEquipped != null)
            {
                UnequipItem(character, currentEquipped);
            }

            // Equip new item
            if (character.EquippedItems == null)
                character.EquippedItems = new Dictionary<EquipmentSlot, Item>();

            character.EquippedItems[item.EquipmentSlot] = item;
            RemoveItem(character, item.Id, 1);

            // Apply equipment bonuses
            ApplyEquipmentBonuses(character, item.EquipmentStats, true);

            return true;
        }

        public bool UnequipItem(Character character, Item item)
        {
            if (character.EquippedItems == null || !character.EquippedItems.ContainsKey(item.EquipmentSlot))
                return false;

            character.EquippedItems.Remove(item.EquipmentSlot);

            // Remove equipment bonuses
            ApplyEquipmentBonuses(character, item.EquipmentStats, false);

            // Add back to inventory
            return AddItem(character, item, 1);
        }

        private void ApplyEquipmentBonuses(Character character, EquipmentStats stats, bool apply)
        {
            if (stats == null)
                return;

            int multiplier = apply ? 1 : -1;

            character.Stats.Strength += stats.StrengthBonus * multiplier;
            character.Stats.Dexterity += stats.DexterityBonus * multiplier;
            character.Stats.Constitution += stats.ConstitutionBonus * multiplier;
            character.Stats.Intelligence += stats.IntelligenceBonus * multiplier;
            character.Stats.Wisdom += stats.WisdomBonus * multiplier;
            character.Stats.Charisma += stats.CharismaBonus * multiplier;

            character.ArmorClass += stats.ArmorClass * multiplier;
        }

        public Item GetEquippedItem(Character character, EquipmentSlot slot)
        {
            if (character.EquippedItems == null)
                return null;

            character.EquippedItems.TryGetValue(slot, out var item);
            return item;
        }

        public List<Item> GetItemsByType(Character character, ItemType type)
        {
            return character.Inventory.Where(i => i.ItemType == type).ToList();
        }

        public List<Item> SortInventory(Character character, InventorySortMode sortMode)
        {
            return sortMode switch
            {
                InventorySortMode.Name => character.Inventory.OrderBy(i => i.Name).ToList(),
                InventorySortMode.Type => character.Inventory.OrderBy(i => i.ItemType).ThenBy(i => i.Name).ToList(),
                InventorySortMode.Rarity => character.Inventory.OrderByDescending(i => i.Rarity).ThenBy(i => i.Name).ToList(),
                InventorySortMode.Value => character.Inventory.OrderByDescending(i => i.Value).ToList(),
                _ => character.Inventory
            };
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

        public int GetTotalWeight(Character character)
        {
            return character.Inventory.Sum(i => i.Weight * i.Quantity);
        }

        public int GetInventoryValue(Character character)
        {
            return character.Inventory.Sum(i => i.Value * i.Quantity);
        }
    }

    public enum InventorySortMode
    {
        Name,
        Type,
        Rarity,
        Value
    }
}