using MatrixGame.GameScreens;

namespace MatrixGame.Models
{
    public class Warrior : Character
    {
        public Warrior()
        {
            Strength = 3;
            Agility = 3;
            Intelligence = 0;
            Range = 1;
            SpellCost = 2;
            Symbol = Constants.WarriorSymbol;
            Setup();
        }
        public override ConsoleColor GetColor() => ConsoleColor.DarkYellow;

        public override IEnumerable<(int dx, int dy)> GetSpellPattern()
        {
            // all eight neighbors
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    if (dx != 0 || dy != 0)
                        yield return (dx, dy);
        }
    }
}
