namespace MatrixGame.Models
{
    public class Enemy : Character
    {
        private static Random rng = new Random();

        public Enemy()
        {
            Strength = rng.Next(1, 4);
            Agility = rng.Next(1, 4);
            Intelligence = rng.Next(1, 4);
            Range = 1;
            Symbol = '◙';
            Setup();
        }
    }
}
