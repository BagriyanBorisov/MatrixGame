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
            Symbol = Constants.WarriorSymbol;
            Setup();
        }
    }
}
