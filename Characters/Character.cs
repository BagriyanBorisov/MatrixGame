namespace MatrixGame.Models
{
    public abstract class Character
    {

        // Base stats
        public int Strength;
        public int Agility;
        public int Intelligence;
        public int Range;
        public char Symbol;

        // Setup
        public int Health;
        public int Mana;
        public int Damage;
        

        // Position
        public int X = 1;
        public int Y = 1;

        public void Setup()
        {
            Health = Strength * 5;
            Mana = Intelligence * 3;
            Damage = Agility * 2;
        }
    }
}
