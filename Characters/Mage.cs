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
            Symbol = Constants.MageSymbol;
            Setup();
        }
    }
}
