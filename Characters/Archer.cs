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
            Symbol = Constants.ArcherSymbol;
            Setup();
        }
    }
}
