namespace CavemanChronicles
{
    /// <summary>
    /// Utility class for D&D game calculations
    /// </summary>
    public static class GameMath
    {
        /// <summary>
        /// Calculates D&D ability score modifier
        /// Formula: (score - 10) / 2
        /// </summary>
        /// <param name="score">Ability score (typically 1-20)</param>
        /// <returns>Modifier value (typically -5 to +5)</returns>
        public static int CalculateModifier(int score)
        {
            return (score - 10) / 2;
        }

        /// <summary>
        /// Formats a modifier with proper +/- sign
        /// </summary>
        /// <param name="modifier">The modifier value</param>
        /// <returns>Formatted string (e.g., "+3" or "-2")</returns>
        public static string FormatModifier(int modifier)
        {
            return modifier >= 0 ? $"+{modifier}" : modifier.ToString();
        }
    }
}