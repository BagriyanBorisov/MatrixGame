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
        static Random rng = new Random();


        public static void Run()
        {
            Console.WriteLine("Welcome to the Matrix Game!");
            player = LoadLastPlayerAsCharacter();
            var validAction = false;
            SpawnEnemy();

            while (true)
            {
                //Console.Clear();
                if (player.Health <= 0)
                {
                    Console.WriteLine("You have died. Game over.");
                    Console.WriteLine("Press any key to exit.");
                    Console.ReadKey();
                    return;
                }
                DrawMap();
                Console.WriteLine("[1]Move or [2]Attack? (ESC to Exit)");

                validAction = false;
                var input = Console.ReadKey(true).Key;

                if (input == ConsoleKey.Escape)
                    return;

                if (input == ConsoleKey.D1 || input == ConsoleKey.NumPad1)
                    validAction = Move();

                else if (input == ConsoleKey.D2 || input == ConsoleKey.NumPad2)
                    validAction = Attack();

                else Console.WriteLine("Invalid option. Try again.");

                if (validAction)
                {
                    UpdateEnemies();
                    SpawnEnemy();
                }
            }
        }

        private static void DrawMap()
        {
            //Console.Clear();

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

        private static bool Move()
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

            if (!CheckBoundaries(newX, newY))
            {
                Console.WriteLine("You can't move there!");
                return false;
            }

            bool isOccupied = enemies.Any(e => e.X == newX && e.Y == newY);
            if (isOccupied)
            {
                Console.WriteLine("You can't walk onto an enemy!");
                return false;
            }


            player.X = newX;
            player.Y = newY;
            return true;

        }

        private static bool Attack()
        {
            var targets = enemies
                .Where(e => CheckRange(player, e))
                .ToList();

            if (targets.Count == 0)
            {
                Console.WriteLine("No available targets in your range.");
                Console.ReadKey();
                return false;
            }

            Console.WriteLine("Choose a target to attack:");
            for (int i = 0; i < targets.Count; i++)
            {
                var e = targets[i];
                Console.WriteLine($"{i + 1}) Monster at ({e.X},{e.Y}) - HP: {e.Health}");
            }

            int choice = -1;
            while (choice < 1 || choice > targets.Count)
            {
                Console.Write("Your pick: ");
                int.TryParse(Console.ReadLine(), out choice);
            }

            var target = targets[choice - 1];
            target.Health -= player.Damage;

            Console.WriteLine($"You dealt {player.Damage} damage!");
            if (target.Health <= 0)
            {
                Console.WriteLine($"Monster at position({target.X},{target.Y}) defeated!");
                enemies.Remove(target);
            }

            Console.ReadKey();
            return true;
        }

        private static void UpdateEnemies()
        {
            foreach (var e in enemies.ToList())
            {
                int dx = player.X - e.X;
                int dy = player.Y - e.Y;


                bool isNextToPlayer = Math.Abs(dx) <= 1 && Math.Abs(dy) <= 1;

                if (isNextToPlayer)
                {
                    player.Health -= e.Damage;
                    Console.WriteLine($" Monster at ({e.X},{e.Y}) hit you for {e.Damage}!");
                    if (player.Health < 0) player.Health = 0;
                    continue;
                }

                int newX = e.X + (dx != 0 ? Math.Sign(dx) : 0);
                int newY = e.Y + (dy != 0 ? Math.Sign(dy) : 0);

                bool occupied = enemies.Any(other => other != e && other.X == newX && other.Y == newY);

                if (!occupied)
                {
                    e.X = newX;
                    e.Y = newY;
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

        private static bool CheckBoundaries(int x, int y)
        {
            return x >= 0 && x < MatrixSize &&
                   y >= 0 && y < MatrixSize;
        }

        private static bool CheckRange(Character attacker, Character target)
        {
            int dx = Math.Abs(attacker.X - target.X);
            int dy = Math.Abs(attacker.Y - target.Y);
            return dx <= attacker.Range && dy <= attacker.Range;
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
