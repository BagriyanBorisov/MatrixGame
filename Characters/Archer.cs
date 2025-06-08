using static MatrixGame.Constants;
namespace MatrixGame.Models
{
    public class Archer : Character
    {
        public Archer()
        {
            Strength = 2;
            Agility = 4;
            Intelligence = 0;
            Range = 2;
            SpellCost = 1;
            Symbol = ArcherSymbol;
            Setup();
        }
        public override ConsoleColor GetColor() => ConsoleColor.Green;

        public override IEnumerable<(int dx, int dy)> GetSpellPattern()
        {
            yield break;
        }

        public IEnumerable<(int dx, int dy)> GetSpellPattern(int dirX, int dirY)
        {
            for (int step = 1; step < MatrixSize; step++)
                yield return (dirX * step, dirY * step);
        }
    }
}
