using MatrixGame.Data;
using MatrixGame.Models;
using System.Threading;
using static MatrixGame.Constants;

namespace MatrixGame.GameScreens
{
    public static class InGame
    {
        static char[,] map = new char[MatrixSize, MatrixSize];
        static Character player;
        static List<Enemy> enemies = new List<Enemy>();
        static Random rng = new Random();



        public static void Run()
        {
           Console.WriteLine("Welcome to the Matrix Game!");
           player = LoadLastPlayerAsCharacter();

            
           while(true)
            {
                Console.Clear();
                if(player.Health <= 0)
                {
                    Console.WriteLine("You have died. Game over.");
                    Console.WriteLine("Press any key to exit.");
                    Console.ReadKey();
                    return;
                }

                SpawnEnemy();
                DrawMap();
                Console.WriteLine("[1]Move or [2]Attack? (ESC to Exit)");

                var input = Console.ReadKey(true).Key;
                if (input == ConsoleKey.Escape)
                    return;

                if (input == ConsoleKey.D1 || input == ConsoleKey.NumPad1) Move();

                else if (input == ConsoleKey.D2 || input == ConsoleKey.NumPad2) return; // todo

                else Console.WriteLine("Invalid option. Try again.");

                UpdateEnemies();


            }

           
        }

        private static void UpdateEnemies()
        {
            foreach (var e in enemies.ToList())
            {
                int dx = player.X - e.X;
                int dy = player.Y - e.Y;

                if (Math.Abs(dx) <= 1 && Math.Abs(dy) <= 1)
                {
                    player.Health -= e.Damage;
                    Console.WriteLine($" Monster at ({e.X},{e.Y}) hit you for {e.Damage}!");
                    if (player.Health < 0) player.Health = 0;
                }
                else
                {
                    if (dx != 0) e.X += Math.Sign(dx);
                    if (dy != 0) e.Y += Math.Sign(dy);
                }
            }
            Console.ReadKey();
        }

        private static void SpawnEnemy()
        {
            Enemy e = new Enemy();
            int x, y;
            do
            {
                x = rng.Next(0, MatrixSize);
                y = rng.Next(0, MatrixSize);
            } while ((x == player.X && y == player.Y) || enemies.Any(е => е.X == x && е.Y == y));

            e.X = x;
            e.Y = y;
            enemies.Add(e);
        }

        private static void Move()
        {
            Console.WriteLine($"Move with: {MoveKeys}");
            ConsoleKey key = Console.ReadKey(true).Key;

            int newX = player.X;
            int newY = player.Y;

            switch (key)
            {
                case ConsoleKey.W: newY--; break;
                case ConsoleKey.S: newY++; break;
                case ConsoleKey.A: newX--; break;
                case ConsoleKey.D: newX++; break;
                case ConsoleKey.Q: newX--; newY--; break;
                case ConsoleKey.E: newX++; newY--; break;
                case ConsoleKey.Z: newX--; newY++; break;
                case ConsoleKey.X: newX++; newY++; break;
            }

            if (CheckBoundaries(newX, newY))
            {
                player.X = newX;
                player.Y = newY;
            }
        }


        private static bool CheckBoundaries(int x, int y)
        {
            return x >= 0 && x < MatrixSize &&
                   y >= 0 && y < MatrixSize;
        }


        private static void DrawMap()
        {
            Console.Clear();

            int verticalPadding = (Console.WindowHeight - MatrixSize - 5) / 2;
            for (int i = 0; i < verticalPadding; i++)
                Console.WriteLine();


            //HUD
            Console.WriteLine($"Health: {player.Health}   Mana: {player.Mana}   Damage: {player.Damage}");
            Console.WriteLine($"STR: {player.Strength}    AGI: {player.Agility}   INT: {player.Intelligence}   Range: {player.Range}");
            Console.WriteLine($"Position: ({player.X}, {player.Y})");
            Console.WriteLine();

            Array.Clear(map, 0, map.Length);
            map[player.Y, player.X] = player.Symbol;

            foreach (var e in enemies)
                map[e.Y, e.X] = EnemySymbol;

            for (int y = 0; y < MatrixSize; y++)
            {
                string row = "";
                for (int x = 0; x < MatrixSize; x++)
                {
                    char tile = map[y, x];
                    row += tile == '\0' ? TileEmpty : tile;
                }

                int pad = Console.WindowWidth - row.Length;
                if (pad > 0)
                    row = row.PadRight(Console.WindowWidth);

                Console.WriteLine(row);
            }

            Console.WriteLine();
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
