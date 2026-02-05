namespace CavemanChronicles
{
    public class CombatState
    {
        public Character Player { get; set; }
        public Monster Enemy { get; set; }
        public int PlayerInitiative { get; set; }
        public int EnemyInitiative { get; set; }
        public bool IsPlayerTurn { get; set; }
        public int RoundNumber { get; set; }
        public List<string> CombatLog { get; set; }
        public CombatStatus Status { get; set; }
        public int PlayerTempHP { get; set; }
        public int EnemyCurrentHP { get; set; }
        public List<CombatEffect> ActiveEffects { get; set; }

        public CombatState()
        {
            CombatLog = new List<string>();
            ActiveEffects = new List<CombatEffect>();
            RoundNumber = 1;
            Status = CombatStatus.Ongoing;
        }
    }

    public class CombatEffect
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; } // Rounds remaining
        public EffectTarget Target { get; set; }
        public EffectType Type { get; set; }
    }

    public enum CombatStatus
    {
        Ongoing,
        PlayerVictory,
        PlayerDefeated,
        PlayerFled
    }

    public enum EffectTarget
    {
        Player,
        Enemy
    }

    public enum StatusEffectType
    {
        Advantage,
        Disadvantage,
        Prone,
        Grappled,
        Poisoned,
        Blessed,
        Frightened
    }

    public class AttackResult
    {
        public bool IsHit { get; set; }
        public bool IsCritical { get; set; }
        public bool IsMiss { get; set; }
        public int AttackRoll { get; set; }
        public int TotalAttackBonus { get; set; }
        public int TargetAC { get; set; }
        public int DamageRoll { get; set; }
        public int TotalDamage { get; set; }
        public DamageType DamageType { get; set; }
        public string AttackName { get; set; }
        public string Description { get; set; }
        public List<string> DiceRolls { get; set; }

        public AttackResult()
        {
            DiceRolls = new List<string>();
        }
    }

    public class CombatAction
    {
        public CombatActionType ActionType { get; set; }
        public string ActionName { get; set; }
        public int? AttackIndex { get; set; } // For selecting specific attacks
    }

    public enum CombatActionType
    {
        MeleeAttack,
        RangedAttack,
        CastSpell,
        UseItem,
        Dodge,
        Disengage,
        Dash,
        Help,
        Hide,
        Ready,
        Flee
    }
}