namespace CavemanChronicles
{
    public partial class InventoryPage : ContentPage
    {
        private readonly Character _player;
        private readonly InventoryService _inventoryService;
        private readonly AudioService _audioService;
        private ItemType _currentFilter;
        private Item _selectedItem;

        public InventoryPage(Character player, InventoryService inventoryService, AudioService audioService)
        {
            InitializeComponent();
            
            _player = player;
            _inventoryService = inventoryService;
            _audioService = audioService;
            _currentFilter = ItemType.Consumable; // Default to All (will be set in OnAppearing)

            // Initialize ItemDatabase if not already done
            ItemDatabase.Initialize();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _currentFilter = ItemType.Consumable; // Reset to show all
            RefreshInventory();
            UpdateEquipmentDisplay();
            UpdateHeaderInfo();
        }

        private void RefreshInventory()
        {
            ItemListContainer.Clear();

            var items = _currentFilter == ItemType.Consumable // Using Consumable as "All" flag
                ? _player.Inventory
                : _inventoryService.GetItemsByType(_player, _currentFilter);

            if (items.Count == 0)
            {
                var emptyLabel = new Label
                {
                    Text = "No items",
                    TextColor = Color.FromArgb("#888888"),
                    FontFamily = "Courier New",
                    FontSize = 14,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 50, 0, 0)
                };
                ItemListContainer.Add(emptyLabel);
                return;
            }

            foreach (var item in items.OrderBy(i => i.Name))
            {
                var itemCard = CreateItemCard(item);
                ItemListContainer.Add(itemCard);
            }
        }

        private View CreateItemCard(Item item)
        {
            var border = new Border
            {
                Stroke = GetRarityColor(item.Rarity),
                StrokeThickness = 2,
                Padding = 10,
                BackgroundColor = Color.FromArgb("#001100"),
                Margin = new Thickness(0, 0, 0, 5)
            };

            border.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => OnItemClicked(item))
            });

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                ColumnSpacing = 15
            };

            // Icon
            var iconLabel = new Label
            {
                Text = item.IconEmoji,
                FontSize = 30,
                VerticalOptions = LayoutOptions.Center
            };
            Grid.SetColumn(iconLabel, 0);
            grid.Add(iconLabel);

            // Info
            var infoStack = new VerticalStackLayout { Spacing = 5 };
            
            var nameLabel = new Label
            {
                Text = item.Name,
                TextColor = GetRarityColor(item.Rarity),
                FontFamily = "Courier New",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold
            };
            infoStack.Add(nameLabel);

            var typeLabel = new Label
            {
                Text = item.ItemType.ToString(),
                TextColor = Color.FromArgb("#888888"),
                FontFamily = "Courier New",
                FontSize = 10
            };
            infoStack.Add(typeLabel);

            Grid.SetColumn(infoStack, 1);
            grid.Add(infoStack);

            // Quantity (if stackable)
            if (item.IsStackable)
            {
                var quantityLabel = new Label
                {
                    Text = $"x{item.Quantity}",
                    TextColor = Color.FromArgb("#FFD700"),
                    FontFamily = "Courier New",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    VerticalOptions = LayoutOptions.Center
                };
                Grid.SetColumn(quantityLabel, 2);
                grid.Add(quantityLabel);
            }

            border.Content = grid;
            return border;
        }

        private void OnItemClicked(Item item)
        {
            _selectedItem = item;
            ShowItemDetail(item);
        }

        private void ShowItemDetail(Item item)
        {
            DetailIconLabel.Text = item.IconEmoji;
            DetailNameLabel.Text = item.Name;
            DetailRarityLabel.Text = $"{item.Rarity} {item.ItemType}";
            DetailRarityLabel.TextColor = GetRarityColor(item.Rarity);
            DetailDescriptionLabel.Text = item.Description;
            DetailValueLabel.Text = $"Value: {item.Value} GP";
            DetailWeightLabel.Text = $"Weight: {item.Weight}";

            // Show stats/effects
            string stats = "";
            if (item.ItemType == ItemType.Consumable && item.Effect != null)
            {
                stats = $"Effect: {item.Effect.Description}";
            }
            else if (item.EquipmentStats != null)
            {
                var statsList = new List<string>();
                
                if (item.EquipmentStats.ArmorClass > 0)
                    statsList.Add($"AC: +{item.EquipmentStats.ArmorClass}");
                
                if (item.EquipmentStats.AttackBonus > 0)
                    statsList.Add($"Attack: +{item.EquipmentStats.AttackBonus}");
                
                if (item.EquipmentStats.DamageDiceCount > 0)
                    statsList.Add($"Damage: {item.EquipmentStats.DamageDiceCount}d{item.EquipmentStats.DamageDieSize}+{item.EquipmentStats.DamageBonus}");
                
                if (item.EquipmentStats.StrengthBonus != 0)
                    statsList.Add($"STR: +{item.EquipmentStats.StrengthBonus}");
                if (item.EquipmentStats.DexterityBonus != 0)
                    statsList.Add($"DEX: +{item.EquipmentStats.DexterityBonus}");
                if (item.EquipmentStats.ConstitutionBonus != 0)
                    statsList.Add($"CON: +{item.EquipmentStats.ConstitutionBonus}");
                if (item.EquipmentStats.IntelligenceBonus != 0)
                    statsList.Add($"INT: +{item.EquipmentStats.IntelligenceBonus}");
                if (item.EquipmentStats.WisdomBonus != 0)
                    statsList.Add($"WIS: +{item.EquipmentStats.WisdomBonus}");
                if (item.EquipmentStats.CharismaBonus != 0)
                    statsList.Add($"CHA: +{item.EquipmentStats.CharismaBonus}");
                
                stats = string.Join("\n", statsList);
            }
            DetailStatsLabel.Text = stats;

            // Set button text
            if (item.ItemType == ItemType.Consumable)
            {
                UseEquipButtonLabel.Text = "USE";
                UseEquipButton.Stroke = Color.FromArgb("#00FF00");
                UseEquipButton.BackgroundColor = Color.FromArgb("#003300");
            }
            else if (item.ItemType == ItemType.Weapon || item.ItemType == ItemType.Armor || 
                     item.ItemType == ItemType.Shield || item.ItemType == ItemType.Accessory)
            {
                UseEquipButtonLabel.Text = "EQUIP";
                UseEquipButton.Stroke = Color.FromArgb("#0000FF");
                UseEquipButton.BackgroundColor = Color.FromArgb("#000033");
            }
            else
            {
                UseEquipButtonLabel.Text = "N/A";
                UseEquipButton.Stroke = Color.FromArgb("#666666");
                UseEquipButton.BackgroundColor = Color.FromArgb("#222222");
            }

            ItemDetailOverlay.IsVisible = true;
        }

        private async void OnUseEquipClicked(object sender, EventArgs e)
        {
            if (_selectedItem == null)
                return;

            await _audioService.PlaySoundEffect("button_click.wav");

            if (_selectedItem.ItemType == ItemType.Consumable)
            {
                // Use consumable
                bool success = _inventoryService.UseConsumable(_player, _selectedItem);
                if (success)
                {
                    await DisplayAlert("Used", 
                        $"Used {_selectedItem.Name}!\n{_selectedItem.Effect.Description}", 
                        "OK");
                    ItemDetailOverlay.IsVisible = false;
                    RefreshInventory();
                    UpdateHeaderInfo();
                }
                else
                {
                    await DisplayAlert("Error", "Cannot use this item right now.", "OK");
                }
            }
            else if (_selectedItem.ItemType == ItemType.Weapon || 
                     _selectedItem.ItemType == ItemType.Armor || 
                     _selectedItem.ItemType == ItemType.Shield ||
                     _selectedItem.ItemType == ItemType.Accessory)
            {
                // Equip item
                bool success = _inventoryService.EquipItem(_player, _selectedItem);
                if (success)
                {
                    await DisplayAlert("Equipped", $"Equipped {_selectedItem.Name}!", "OK");
                    ItemDetailOverlay.IsVisible = false;
                    RefreshInventory();
                    UpdateEquipmentDisplay();
                }
                else
                {
                    await DisplayAlert("Error", "Cannot equip this item.", "OK");
                }
            }
        }

        private async void OnDropClicked(object sender, EventArgs e)
        {
            if (_selectedItem == null)
                return;

            bool confirm = await DisplayAlert("Drop Item", 
                $"Are you sure you want to drop {_selectedItem.Name}?", 
                "Yes", "No");

            if (confirm)
            {
                await _audioService.PlaySoundEffect("button_click.wav");
                _inventoryService.RemoveItem(_player, _selectedItem.Id, 1);
                ItemDetailOverlay.IsVisible = false;
                RefreshInventory();
                UpdateHeaderInfo();
            }
        }

        private void OnCloseDetailClicked(object sender, EventArgs e)
        {
            ItemDetailOverlay.IsVisible = false;
        }

        private void UpdateEquipmentDisplay()
        {
            // Main Hand
            var mainHand = _inventoryService.GetEquippedItem(_player, EquipmentSlot.MainHand);
            MainHandLabel.Text = mainHand != null ? $"{mainHand.IconEmoji}\n{mainHand.Name}" : "Empty";

            // Chest/Armor
            var chest = _inventoryService.GetEquippedItem(_player, EquipmentSlot.Chest);
            ChestLabel.Text = chest != null ? $"{chest.IconEmoji}\n{chest.Name}" : "Empty";

            // Off Hand
            var offHand = _inventoryService.GetEquippedItem(_player, EquipmentSlot.OffHand);
            OffHandLabel.Text = offHand != null ? $"{offHand.IconEmoji}\n{offHand.Name}" : "Empty";
        }

        private void UpdateHeaderInfo()
        {
            int totalWeight = _inventoryService.GetTotalWeight(_player);
            WeightLabel.Text = $"Weight: {totalWeight}/200";
            GoldLabel.Text = $"Gold: {_player.Gold} GP";
        }

        // Tab click handlers
        private void OnAllTabClicked(object sender, EventArgs e)
        {
            _currentFilter = ItemType.Consumable; // Using as "All" flag
            UpdateTabStyles();
            SetActiveTab(AllTab);
            RefreshInventory();
        }

        private void OnConsumablesTabClicked(object sender, EventArgs e)
        {
            _currentFilter = ItemType.Consumable;
            UpdateTabStyles();
            SetActiveTab(ConsumablesTab);
            RefreshInventory();
        }

        private void OnWeaponsTabClicked(object sender, EventArgs e)
        {
            _currentFilter = ItemType.Weapon;
            UpdateTabStyles();
            SetActiveTab(WeaponsTab);
            RefreshInventory();
        }

        private void OnArmorTabClicked(object sender, EventArgs e)
        {
            _currentFilter = ItemType.Armor;
            UpdateTabStyles();
            SetActiveTab(ArmorTab);
            RefreshInventory();
        }

        private void OnQuestTabClicked(object sender, EventArgs e)
        {
            _currentFilter = ItemType.QuestItem;
            UpdateTabStyles();
            SetActiveTab(QuestTab);
            RefreshInventory();
        }

        private void UpdateTabStyles()
        {
            // Reset all tabs
            AllTab.Stroke = Color.FromArgb("#888888");
            AllTab.BackgroundColor = Color.FromArgb("#111111");
            ((Label)AllTab.Content).TextColor = Color.FromArgb("#888888");

            ConsumablesTab.Stroke = Color.FromArgb("#888888");
            ConsumablesTab.BackgroundColor = Color.FromArgb("#111111");
            ((Label)ConsumablesTab.Content).TextColor = Color.FromArgb("#888888");

            WeaponsTab.Stroke = Color.FromArgb("#888888");
            WeaponsTab.BackgroundColor = Color.FromArgb("#111111");
            ((Label)WeaponsTab.Content).TextColor = Color.FromArgb("#888888");

            ArmorTab.Stroke = Color.FromArgb("#888888");
            ArmorTab.BackgroundColor = Color.FromArgb("#111111");
            ((Label)ArmorTab.Content).TextColor = Color.FromArgb("#888888");

            QuestTab.Stroke = Color.FromArgb("#888888");
            QuestTab.BackgroundColor = Color.FromArgb("#111111");
            ((Label)QuestTab.Content).TextColor = Color.FromArgb("#888888");
        }

        private void SetActiveTab(Border tab)
        {
            tab.Stroke = Color.FromArgb("#00FF00");
            tab.BackgroundColor = Color.FromArgb("#003300");
            ((Label)tab.Content).TextColor = Color.FromArgb("#00FF00");
        }

        private Color GetRarityColor(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => Color.FromArgb("#AAAAAA"),
                ItemRarity.Uncommon => Color.FromArgb("#00FF00"),
                ItemRarity.Rare => Color.FromArgb("#0088FF"),
                ItemRarity.Epic => Color.FromArgb("#AA00FF"),
                ItemRarity.Legendary => Color.FromArgb("#FFD700"),
                _ => Color.FromArgb("#FFFFFF")
            };
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await _audioService.PlaySoundEffect("button_click.wav");
            await Navigation.PopAsync();
        }
    }
}
