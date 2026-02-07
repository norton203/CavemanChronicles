namespace CavemanChronicles
{
    public class CombatService
    {
        private Random _random;
        private AudioService? _audioService;

        public CombatState? CurrentCombat { get; private set; }

        public CombatService()
        {
            _random = new Random();
        }

        public void SetAudioService(AudioService audioService)
        {
            _audioService = audioService;
        }

        #region Combat Initialization

        public CombatState InitiateCombat(Character player, Monster enemy)
        {
            CurrentCombat = new CombatState
            {
                Player = player,
                Enemy = enemy,
                EnemyCurrentHP = enemy.HitPoints,
                PlayerTempHP = 0
            };

            // Roll initiative
            CurrentCombat.PlayerInitiative = RollInitiative(player.Stats.DexterityMod);
            CurrentCombat.EnemyInitiative = RollInitiative(enemy.Stats.DexterityMod);

            // Determine who goes first
            CurrentCombat.IsPlayerTurn = CurrentCombat.PlayerInitiative >= CurrentCombat.EnemyInitiative;

            // Log combat start
            CurrentCombat.CombatLog.Add($"=== COMBAT INITIATED ===");
            CurrentCombat.CombatLog.Add($"{enemy.GetRandomFlavorText()}");
            CurrentCombat.CombatLog.Add("");
            CurrentCombat.CombatLog.Add($"Initiative: {player.Name} ({CurrentCombat.PlayerInitiative}) vs {enemy.Name} ({CurrentCombat.EnemyInitiative})");
            CurrentCombat.CombatLog.Add("");

            if (CurrentCombat.IsPlayerTurn)
            {
                CurrentCombat.CombatLog.Add($"You act first! What will you do?");
            }
            else
            {
                CurrentCombat.CombatLog.Add($"The {enemy.Name} acts first!");
            }

            return CurrentCombat;
        }

        private int RollInitiative(int dexterityMod)
        {
            return RollD20() + dexterityMod;
        }

        #endregion

        #region Turn Management

        public async Task<List<string>> ProcessPlayerAction(CombatActionType actionType, int? attackIndex = null)
        {
            if (CurrentCombat == null || CurrentCombat.Status != CombatStatus.Ongoing)
                return new List<string> { "No active combat!" };

            var turnLog = new List<string>();
            turnLog.Add($"--- ROUND {CurrentCombat.RoundNumber} ---");
            turnLog.Add("");

            if (!CurrentCombat.IsPlayerTurn)
            {
                turnLog.Add("It's not your turn!");
                return turnLog;
            }

            // Process player action
            switch (actionType)
            {
                case CombatActionType.MeleeAttack:
                    await ProcessPlayerAttack(turnLog);
                    break;
                case CombatActionType.Dodge:
                    ProcessDodgeAction(turnLog, true);
                    break;
                case CombatActionType.Flee:
                    await ProcessFleeAttempt(turnLog);
                    break;
                default:
                    turnLog.Add($"Action {actionType} not yet implemented!");
                    break;
            }

            // Check if enemy is defeated
            if (CurrentCombat.EnemyCurrentHP <= 0)
            {
                await HandleCombatVictory(turnLog);
                return turnLog;
            }

            // Enemy's turn
            CurrentCombat.IsPlayerTurn = false;
            turnLog.Add("");
            await ProcessEnemyTurn(turnLog);

            // Check if player is defeated
            if (CurrentCombat.Player.Health <= 0)
            {
                await HandleCombatDefeat(turnLog);
                return turnLog;
            }

            // Process end of round effects
            ProcessEndOfRound(turnLog);

            // Next round
            CurrentCombat.RoundNumber++;
            CurrentCombat.IsPlayerTurn = true;
            turnLog.Add("");
            turnLog.Add("Your turn! What will you do?");

            // Add to combat log
            CurrentCombat.CombatLog.AddRange(turnLog);

            return turnLog;
        }

        private async Task ProcessPlayerAttack(List<string> turnLog)
        {
            if (CurrentCombat == null) return;

            turnLog.Add($">>> {CurrentCombat.Player.Name}'s Turn <<<");

            // Determine weapon/attack
            var weapon = GetPlayerWeapon();
            var attackBonus = CalculatePlayerAttackBonus(weapon);

            // Get weapon stats
            int diceCount = weapon.EquipmentStats?.DamageDiceCount ?? 1;
            int dieSize = weapon.EquipmentStats?.DamageDieSize ?? 4;
            int weaponDamageBonus = weapon.EquipmentStats?.DamageBonus ?? 0;

            var result = MakeAttackRoll(
                attackBonus,
                CurrentCombat.Enemy.ArmorClass,
                diceCount,
                dieSize,
                weaponDamageBonus + CalculatePlayerDamageBonus(),
                weapon.EquipmentStats?.DamageType ?? DamageType.Bludgeoning,
                weapon.Name
            );

            turnLog.Add($"You attack with {weapon.Name}!");
            turnLog.Add($"Attack Roll: d20({result.AttackRoll}) + {result.TotalAttackBonus} = {result.AttackRoll + result.TotalAttackBonus} vs AC {result.TargetAC}");

            if (result.IsCritical)
            {
                await _audioService?.PlaySoundEffect("combat_crit.wav");
                turnLog.Add("*** CRITICAL HIT! ***");
                turnLog.Add($"Damage: {string.Join(" + ", result.DiceRolls)} = {result.TotalDamage} {result.DamageType} damage!");
                CurrentCombat.EnemyCurrentHP -= result.TotalDamage;
                turnLog.Add($"{CurrentCombat.Enemy.Name} takes {result.TotalDamage} damage! ({CurrentCombat.EnemyCurrentHP}/{CurrentCombat.Enemy.HitPoints} HP remaining)");
            }
            else if (result.IsHit)
            {
                await _audioService?.PlaySoundEffect("combat_hit.wav");
                turnLog.Add("*** HIT! ***");
                turnLog.Add($"Damage: {string.Join(" + ", result.DiceRolls)} = {result.TotalDamage} {result.DamageType} damage!");
                CurrentCombat.EnemyCurrentHP -= result.TotalDamage;
                turnLog.Add($"{CurrentCombat.Enemy.Name} takes {result.TotalDamage} damage! ({CurrentCombat.EnemyCurrentHP}/{CurrentCombat.Enemy.HitPoints} HP remaining)");
            }
            else
            {
                await _audioService?.PlaySoundEffect("combat_miss.wav");
                turnLog.Add("*** MISS! ***");
                turnLog.Add($"Your attack fails to connect!");
            }
        }

        private async Task ProcessEnemyTurn(List<string> turnLog)
        {
            if (CurrentCombat == null) return;

            turnLog.Add($">>> {CurrentCombat.Enemy.Name}'s Turn <<<");

            // Choose random attack
            if (CurrentCombat.Enemy.Attacks == null || CurrentCombat.Enemy.Attacks.Count == 0)
            {
                turnLog.Add($"The {CurrentCombat.Enemy.Name} cannot attack!");
                return;
            }

            var attack = CurrentCombat.Enemy.Attacks[_random.Next(CurrentCombat.Enemy.Attacks.Count)];

            // Make attack roll
            var result = MakeAttackRoll(
                attack.AttackBonus,
                CurrentCombat.Player.ArmorClass,
                attack.DamageDiceCount,
                attack.DamageDieSize,
                attack.DamageBonus,
                attack.DamageType,
                attack.Name
            );

            turnLog.Add($"The {CurrentCombat.Enemy.Name} uses {attack.Name}!");
            turnLog.Add(attack.Description);
            turnLog.Add($"Attack Roll: d20({result.AttackRoll}) + {attack.AttackBonus} = {result.AttackRoll + attack.AttackBonus} vs AC {result.TargetAC}");

            if (result.IsCritical)
            {
                await _audioService?.PlaySoundEffect("combat_crit.wav");
                turnLog.Add("*** CRITICAL HIT! ***");
                turnLog.Add($"Damage: {string.Join(" + ", result.DiceRolls)} = {result.TotalDamage} {result.DamageType} damage!");
                ApplyDamageToPlayer(result.TotalDamage, turnLog);
            }
            else if (result.IsHit)
            {
                await _audioService?.PlaySoundEffect("combat_hit.wav");
                turnLog.Add("*** HIT! ***");
                turnLog.Add($"Damage: {string.Join(" + ", result.DiceRolls)} = {result.TotalDamage} {result.DamageType} damage!");
                ApplyDamageToPlayer(result.TotalDamage, turnLog);
            }
            else
            {
                await _audioService?.PlaySoundEffect("combat_miss.wav");
                turnLog.Add("*** MISS! ***");
                turnLog.Add($"You dodge the attack!");
            }
        }

        #endregion

        #region Attack Resolution

        private AttackResult MakeAttackRoll(int attackBonus, int targetAC, int damageDice, int damageBonus, string attackName)
        {
            // For simple weapons (single damage die)
            return MakeAttackRoll(attackBonus, targetAC, 1, damageDice, damageBonus, DamageType.Slashing, attackName);
        }

        private AttackResult MakeAttackRoll(int attackBonus, int targetAC, int damageDiceCount, int damageDieSize, int damageBonus, DamageType damageType, string attackName)
        {
            var result = new AttackResult
            {
                TotalAttackBonus = attackBonus,
                TargetAC = targetAC,
                AttackName = attackName,
                DamageType = damageType
            };

            // Roll d20 for attack
            result.AttackRoll = RollD20();

            // Check for critical hit (natural 20)
            if (result.AttackRoll == 20)
            {
                result.IsCritical = true;
                result.IsHit = true;

                // Critical hits double the dice
                for (int i = 0; i < damageDiceCount * 2; i++)
                {
                    int roll = RollDie(damageDieSize);
                    result.DamageRoll += roll;
                    result.DiceRolls.Add($"d{damageDieSize}({roll})");
                }

                result.TotalDamage = result.DamageRoll + damageBonus;
            }
            // Check for critical miss (natural 1)
            else if (result.AttackRoll == 1)
            {
                result.IsMiss = true;
                result.IsHit = false;
                result.TotalDamage = 0;
            }
            // Normal hit/miss
            else
            {
                int totalAttack = result.AttackRoll + attackBonus;
                result.IsHit = totalAttack >= targetAC;

                if (result.IsHit)
                {
                    // Roll damage
                    for (int i = 0; i < damageDiceCount; i++)
                    {
                        int roll = RollDie(damageDieSize);
                        result.DamageRoll += roll;
                        result.DiceRolls.Add($"d{damageDieSize}({roll})");
                    }

                    if (damageBonus > 0)
                    {
                        result.DiceRolls.Add($"+{damageBonus}");
                    }

                    result.TotalDamage = result.DamageRoll + damageBonus;
                }
            }

            return result;
        }

        private void ApplyDamageToPlayer(int damage, List<string> turnLog)
        {
            if (CurrentCombat == null) return;

            // Apply temp HP first
            if (CurrentCombat.PlayerTempHP > 0)
            {
                if (CurrentCombat.PlayerTempHP >= damage)
                {
                    CurrentCombat.PlayerTempHP -= damage;
                    turnLog.Add($"Temporary HP absorbs all damage! ({CurrentCombat.PlayerTempHP} temp HP remaining)");
                    return;
                }
                else
                {
                    damage -= CurrentCombat.PlayerTempHP;
                    turnLog.Add($"Temporary HP absorbs {CurrentCombat.PlayerTempHP} damage!");
                    CurrentCombat.PlayerTempHP = 0;
                }
            }

            // Apply to actual HP
            CurrentCombat.Player.Health -= damage;
            turnLog.Add($"You take {damage} damage! ({CurrentCombat.Player.Health}/{CurrentCombat.Player.MaxHealth} HP remaining)");
        }

        #endregion

        #region Combat Actions

        private void ProcessDodgeAction(List<string> turnLog, bool isPlayer)
        {
            if (CurrentCombat == null) return;

            if (isPlayer)
            {
                turnLog.Add("You take the Dodge action!");
                turnLog.Add("Until your next turn, attacks against you have disadvantage.");

                CurrentCombat.ActiveEffects.Add(new CombatEffect
                {
                    Name = "Dodging",
                    Description = "Attacks against you have disadvantage",
                    Duration = 1,
                    Target = EffectTarget.Player,
                    Type = EffectType.BuffArmor  // ✓ Use existing enum value
                });
            }
        }

        private async Task ProcessFleeAttempt(List<string> turnLog)
        {
            if (CurrentCombat == null) return;

            turnLog.Add("You attempt to flee!");

            // Roll contested check: Player DEX vs Enemy DEX (or WIS for tracking)
            int playerRoll = RollD20() + CurrentCombat.Player.Stats.DexterityMod;
            int enemyRoll = RollD20() + CurrentCombat.Enemy.Stats.WisdomMod;

            turnLog.Add($"Escape Check: You rolled {playerRoll} vs Enemy {enemyRoll}");

            if (playerRoll > enemyRoll)
            {
                await _audioService?.PlaySoundEffect("combat_flee.wav");
                turnLog.Add("*** YOU ESCAPED! ***");
                turnLog.Add("You manage to flee from combat!");
                CurrentCombat.Status = CombatStatus.PlayerFled;
            }
            else
            {
                turnLog.Add("*** FAILED TO ESCAPE! ***");
                turnLog.Add($"The {CurrentCombat.Enemy.Name} blocks your escape!");
                // Enemy gets opportunity attack
                turnLog.Add("");
                turnLog.Add($"The {CurrentCombat.Enemy.Name} gets an opportunity attack!");
                await ProcessEnemyTurn(turnLog);
            }
        }

        #endregion

        #region Combat End

        private async Task HandleCombatVictory(List<string> turnLog)
        {
            if (CurrentCombat == null) return;

            await _audioService?.PlaySoundEffect("victory.wav");

            CurrentCombat.Status = CombatStatus.PlayerVictory;
            turnLog.Add("");
            turnLog.Add("=================================");
            turnLog.Add("*** VICTORY! ***");
            turnLog.Add(CurrentCombat.Enemy.DefeatedText);
            turnLog.Add("=================================");
            turnLog.Add("");

            // Award experience
            int expGained = CurrentCombat.Enemy.ExperienceValue;
            CurrentCombat.Player.Experience += expGained;
            turnLog.Add($"You gained {expGained} XP!");

            // Award gold
            int goldGained = _random.Next(CurrentCombat.Enemy.MinGold, CurrentCombat.Enemy.MaxGold + 1);
            if (goldGained > 0)
            {
                CurrentCombat.Player.Gold += goldGained;
                turnLog.Add($"You found {goldGained} gold!");
            }

            // Award loot
            if (CurrentCombat.Enemy.PossibleLoot != null && CurrentCombat.Enemy.PossibleLoot.Count > 0)
            {
                int lootRoll = _random.Next(100);
                if (lootRoll < 50) // 50% chance for loot
                {
                    var lootItem = CurrentCombat.Enemy.PossibleLoot[_random.Next(CurrentCombat.Enemy.PossibleLoot.Count)];
                    var item = new Item
                    {
                        Name = lootItem,
                        Description = $"Looted from {CurrentCombat.Enemy.Name}",
                        ItemType = ItemType.Misc,
                        Value = 10
                    };
                    CurrentCombat.Player.Inventory.Add(item);
                    turnLog.Add($"You found: {lootItem}!");
                }
            }

            // Check for level up
            int expNeeded = CurrentCombat.Player.Level * 100;
            if (CurrentCombat.Player.Experience >= expNeeded)
            {
                turnLog.Add("");
                turnLog.Add("*** LEVEL UP! ***");
                LevelUpPlayer(turnLog);
            }
        }

        private async Task HandleCombatDefeat(List<string> turnLog)
        {
            if (CurrentCombat == null) return;

            await _audioService?.PlaySoundEffect("game_over.wav");

            CurrentCombat.Status = CombatStatus.PlayerDefeated;
            turnLog.Add("");
            turnLog.Add("=================================");
            turnLog.Add("*** DEFEATED! ***");
            turnLog.Add("You have fallen in combat...");
            turnLog.Add("=================================");
        }

        private void LevelUpPlayer(List<string> turnLog)
        {
            if (CurrentCombat == null) return;

            CurrentCombat.Player.Level++;
            CurrentCombat.Player.Experience = 0;

            // Increase max HP
            int hpGain = CurrentCombat.Player.HitDice + CurrentCombat.Player.Stats.ConstitutionMod;
            hpGain = Math.Max(1, hpGain);
            CurrentCombat.Player.MaxHealth += hpGain;
            CurrentCombat.Player.Health = CurrentCombat.Player.MaxHealth; // Full heal on level up

            // Increase stats
            CurrentCombat.Player.Stats.Strength++;
            CurrentCombat.Player.Stats.Dexterity++;
            CurrentCombat.Player.Stats.Constitution++;

           
            // Check for era advancement
            var oldEra = CurrentCombat.Player.CurrentEra;
            UpdateTechnologyEra();

            turnLog.Add($"You are now level {CurrentCombat.Player.Level}!");
            turnLog.Add($"Max HP increased by {hpGain}! (Now {CurrentCombat.Player.MaxHealth})");
            turnLog.Add($"All ability scores increased by 1!");

            if (oldEra != CurrentCombat.Player.CurrentEra)
            {
                turnLog.Add("");
                turnLog.Add("*** ERA ADVANCEMENT! ***");
                turnLog.Add($"You have advanced to the {CurrentCombat.Player.CurrentEra} ERA!");
                turnLog.Add("New technologies and challenges await!");
            }
        }

        private void UpdateTechnologyEra()
        {
            if (CurrentCombat == null) return;

            CurrentCombat.Player.CurrentEra = CurrentCombat.Player.Level switch
            {
                <= 5 => TechnologyEra.Caveman,
                <= 10 => TechnologyEra.StoneAge,
                <= 15 => TechnologyEra.BronzeAge,
                <= 20 => TechnologyEra.IronAge,
                <= 25 => TechnologyEra.Medieval,
                <= 30 => TechnologyEra.Renaissance,
                <= 35 => TechnologyEra.Industrial,
                <= 40 => TechnologyEra.Modern,
                _ => TechnologyEra.Future
            };
        }

        #endregion

        #region Helper Methods

        private void ProcessEndOfRound(List<string> turnLog)
        {
            if (CurrentCombat == null) return;

            // Process active effects
            for (int i = CurrentCombat.ActiveEffects.Count - 1; i >= 0; i--)
            {
                var effect = CurrentCombat.ActiveEffects[i];
                effect.Duration--;

                if (effect.Duration <= 0)
                {
                    turnLog.Add($"{effect.Name} effect ends.");
                    CurrentCombat.ActiveEffects.RemoveAt(i);
                }
            }
        }

        private Item GetPlayerWeapon()
        {
            if (CurrentCombat == null)
                return CreateUnarmedStrike();

            // Find equipped weapon in inventory
            var weapon = CurrentCombat.Player.Inventory?
                .FirstOrDefault(i => i.ItemType == ItemType.Weapon);

            if (weapon == null)
            {
                return CreateUnarmedStrike();
            }

            return weapon;
        }

        private Item CreateUnarmedStrike()
        {
            return new Item
            {
                Name = "Unarmed Strike",
                Description = "Your fists",
                ItemType = ItemType.Weapon,
                Value = 0,
                EquipmentStats = new EquipmentStats
                {
                    DamageDiceCount = 1,
                    DamageDieSize = 4,  // 1d4
                    DamageBonus = 0,
                    AttackBonus = 0,
                    DamageType = DamageType.Bludgeoning
                }
            };
        }

        private int CalculatePlayerAttackBonus(Item weapon)
        {
            if (CurrentCombat == null) return 0;

            // Proficiency bonus + STR modifier (for melee weapons)
            return CurrentCombat.Player.ProficiencyBonus + CurrentCombat.Player.Stats.StrengthMod;
        }

        private int CalculatePlayerDamageBonus()
        {
            if (CurrentCombat == null) return 0;

            // STR modifier for melee weapons
            return CurrentCombat.Player.Stats.StrengthMod;
        }

        private int RollD20()
        {
            return _random.Next(1, 21);
        }

        private int RollDie(int sides)
        {
            return _random.Next(1, sides + 1);
        }

        public void EndCombat()
        {
            CurrentCombat = null;
        }

        #endregion
    }
}