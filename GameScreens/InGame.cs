using MatrixGame.Data;
using MatrixGame.Models;
using static MatrixGame.Constants;

namespace MatrixGame.GameScreens
{
    public static class InGame
    {
        static char[,] map = new char[MatrixSize, MatrixSize];
        static Character player;
        static List<Enemy> enemies = new List<Enemy>();
        

        public static void Run()
        {
           Console.WriteLine("Welcome to the Matrix Game!");
           player = LoadLastPlayerAsCharacter();

           DrawMap();
        }


        private static void DrawMap()
        {
            Console.Clear();


            //HUD
            Console.WriteLine($"Health: {player.Health}   Mana: {player.Mana}   Damage: {player.Damage}");
            Console.WriteLine($"STR: {player.Strength}    AGI: {player.Agility}   INT: {player.Intelligence}   Range: {player.Range}");
            Console.WriteLine($"Position: ({player.X}, {player.Y})");
            Console.WriteLine();

            Array.Clear(map, 0, map.Length);
            map[player.Y, player.X] = player.Symbol;

            foreach (var m in enemies)
                map[m.Y, m.X] = EnemySymbol;

            for (int y = 0; y < MatrixSize; y++)
            {
                for (int x = 0; x < MatrixSize; x++)
                {
                    char tile = map[y, x];
                    Console.Write(tile == '\0' ? TileEmpty : tile);
                }
                Console.WriteLine();
            }
        }


        private static Character LoadLastPlayerAsCharacter()
        {
            using var db = new GameDbContext();
            var last = db.Players.OrderByDescending(p => p.Id).First();

            Character c = last.Type switch
            {
                "Warrior" => new Warrior(),
                "Archer" => new Archer(),
                "Mage" => new Mage(),
                _ => throw new Exception("Unknown type")
            };

            c.Strength = last.Strength;
            c.Agility = last.Agility;
            c.Intelligence = last.Intelligence;
            c.Setup();

            return c;
        }
    }
}
