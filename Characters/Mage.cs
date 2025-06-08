using static MatrixGame.Constants;
namespace MatrixGame.Models
{
    public class Mage : Character
    {
        public Mage()
        {
            Strength = 2;
            Agility = 1;
            Intelligence = 3;
            Range = 3;
            SpellCost = 3;
            Symbol = MageSymbol;
            Setup();
        }
        public override ConsoleColor GetColor() => ConsoleColor.Cyan;

        public override IEnumerable<(int dx, int dy)> GetSpellPattern()
        {
            // row
            for (int dx = -MatrixSize; dx <= MatrixSize; dx++)
                if (dx != 0) yield return (dx, 0);

            // column
            for (int dy = -MatrixSize; dy <= MatrixSize; dy++)
                if (dy != 0) yield return (0, dy);
        }
    }
}
