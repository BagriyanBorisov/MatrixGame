using MatrixGame.Data;
using MatrixGame.Models;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static MatrixGame.Constants;

namespace MatrixGame.GameScreens
{
    public static class InGame
    {
        static char[,] map = new char[MatrixSize, MatrixSize];
        static Character player;
        static List<Enemy> enemies = new List<Enemy>();
        private static List<string> BattleLog = new();
        static Random rng = new Random();
        static int EnemiesKilled = 0;
        static int playerTurns = 0;


        static int LeftPadding = (Console.WindowWidth - MatrixSize * 2) / 2;
        static int TopPadding = (Console.WindowHeight - (MatrixSize + HudHeight)) / 2;
        const int HudHeight = 5;
        const int SidePanelWidth = 25;
        const int LogLinesToShow = 15;
        static int MapLeft => (Console.WindowWidth - (MatrixSize * 2 + 2)) / 2;
        static int MapTop => (Console.WindowHeight - (MatrixSize + 2 + HudHeight)) / 2 + HudHeight;


        public static void Run()
        {
            WriteLineCentered("Welcome to the Matrix Game!", 5);
            player = LoadLastPlayerAsCharacter();
            SpawnEnemy();

            while (true)
            {
                Console.Clear();
                if (player.Health <= 0)
                {
                    WriteLineCentered("You have died. Game over.", 5);
                    WriteLineCentered("Press any key to exit.", 6);
                    Console.ReadKey();
                    return;
                }
                DrawMap();
                var validAction = false;
                WriteLineCentered("Use W/A/S/D/Q/E/Z/X to move, [F] to cast your spell, ESC to exit.", 5);
                ConsoleKey? input = Console.ReadKey(true).Key;

                if (input == ConsoleKey.Escape)
                    return;

                if (input == ConsoleKey.F)
                {
                    validAction = UseSpell();
                }
                else
                {
                    if (IsMovementKey(input))
                    {
                        if (Move(input))
                        {
                            validAction = true;
                            playerTurns++;
                        }
                    }
                    else
                    {
                        if (PromptForAttack(input))
                        {
                            validAction = true;
                            playerTurns++;
                        }
                    }
                }

                if (validAction)
                {
                    if (playerTurns >= 2)
                    {
                        UpdateEnemies();
                        SpawnEnemy();
                        playerTurns = 0;
                    }
                }
                else
                {
                    WriteLineCentered("Invalid action. Please try again.", 7);
                    Console.ReadKey();
                }


            }
        }

        private static void AnimateWhirlwind(Character player)
        {
            var offsets = new (int dx, int dy)[]
            {
                (-1,-1),(0,-1),(1,-1),
                (1, 0),(1, 1),(0, 1),
                (-1, 1),(-1,0)
            };

            foreach (var (dx, dy) in offsets)
            {
                int x = player.X + dx, y = player.Y + dy;
                if (x < 0 || y < 0 || x >= MatrixSize || y >= MatrixSize) continue;

                // draw slash
                Console.SetCursorPosition(LeftPadding + x * 2, TopPadding + y + HudHeight);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("*");
                Console.ResetColor();

                Thread.Sleep(75);

                // restore tile
                Console.SetCursorPosition(LeftPadding + x * 2, TopPadding + y + HudHeight);
                DrawSingleTile(x, y);
            }
        }

        private static void AnimateFireSpell()
        {
            int px = player.X, py = player.Y;
            int frameDelay = 50;

            // 1) draw vertical
            Console.ForegroundColor = ConsoleColor.Red;
            for (int dy = -MatrixSize; dy <= MatrixSize; dy++)
            {
                int y = py + dy;
                if (y < 0 || y >= MatrixSize) continue;
                int screenX = LeftPadding + px * 2;
                int screenY = TopPadding + HudHeight + y;
                Console.SetCursorPosition(screenX, screenY);
                Console.Write("█");
                Thread.Sleep(frameDelay);
            }

            // 2) draw horizontal
            for (int dx = -MatrixSize; dx <= MatrixSize; dx++)
            {
                int x = px + dx;
                if (x < 0 || x >= MatrixSize) continue;
                int screenX = LeftPadding + x * 2;
                int screenY = TopPadding + HudHeight + py;
                Console.SetCursorPosition(screenX, screenY);
                Console.Write("█");
                Thread.Sleep(frameDelay);
            }
            Console.ResetColor();

            // 3) linger on full cross
            Thread.Sleep(200);

            // 4) clear it by redrawing only those cross tiles
            //    (or you can simply call DrawMap() to redraw everything)
            for (int dy = -MatrixSize; dy <= MatrixSize; dy++)
            {
                int y = py + dy;
                if (y < 0 || y >= MatrixSize) continue;
                int screenX = LeftPadding + px * 2;
                int screenY = TopPadding + HudHeight + y;
                Console.SetCursorPosition(screenX, screenY);
                DrawSingleTile(px, y);
            }
            for (int dx = -MatrixSize; dx <= MatrixSize; dx++)
            {
                int x = px + dx;
                if (x < 0 || x >= MatrixSize) continue;
                int screenX = LeftPadding + x * 2;
                int screenY = TopPadding + HudHeight + py;
                Console.SetCursorPosition(screenX, screenY);
                DrawSingleTile(x, py);
            }
        }

        private static void AnimatePiercingShot(Character player, int dx, int dy)
        {
            int tx = player.X + dx, ty = player.Y + dy;

            string arrowSymbol = (dx, dy) switch
            {
                (0, -1) => "↑",
                (0, 1) => "↓",
                (-1, 0) => "←",
                (1, 0) => "→",
                (-1, -1) => "↖",
                (-1, 1) => "↙",
                (1, -1) => "↗",
                (1, 1) => "↘",
                _ => "?"
            };

            while (tx >= 0 && ty >= 0 && tx < MatrixSize && ty < MatrixSize)
            {
                Console.SetCursorPosition(LeftPadding + tx * 2, TopPadding + ty + HudHeight);
                Console.ForegroundColor = ConsoleColor.Green; Console.Write(arrowSymbol); Console.ResetColor();
                Thread.Sleep(100);

                // restore
                Console.SetCursorPosition(LeftPadding + tx * 2, TopPadding + ty + HudHeight);
                DrawSingleTile(tx, ty);

                tx += dx; ty += dy;
            }
        }

        static void DrawSingleTile(int x, int y)
        {
            var tile = map[y, x];
            char symbol = tile == '\0' ? TileEmpty : tile;
            Console.Write(symbol);
            if (x < MatrixSize - 1) Console.Write(" ");
        }

        private static bool UseSpell()
        {
            if (player.Mana < player.SpellCost)
            {
                BattleLog.Add("Not enough mana!");
                return false;
            }
            else
            {
                // pay cost
                player.Mana -= player.SpellCost;

                IEnumerable<(int dx, int dy)> pattern;
                switch (player)
                {
                    case Warrior _:
                        AnimateWhirlwind(player);
                        pattern = player.GetSpellPattern();
                        break;

                    case Mage _:
                        AnimateFireSpell();
                        pattern = player.GetSpellPattern();
                        break;

                    case Archer archer:
                        WriteLineCentered("Choose direction for Piercing Shot (W/A/S/D):", 6);
                        var key = Console.ReadKey(true).Key;
                        var (dx, dy) = key switch
                        {
                            ConsoleKey.W => (0, -1),
                            ConsoleKey.S => (0, 1),
                            ConsoleKey.A => (-1, 0),
                            ConsoleKey.D => (1, 0),
                            _ => (1, 0) // // default to right if invalid key
                        };
                        AnimatePiercingShot(player, dx, dy);
                        pattern = archer.GetSpellPattern(dx, dy);
                        break;

                    default:
                        pattern = Enumerable.Empty<(int, int)>();
                        break;
                }

                foreach (var (dx, dy) in pattern)
                {
                    int tx = player.X + dx;
                    int ty = player.Y + dy;

                    if (tx < 0 || ty < 0 || tx >= MatrixSize || ty >= MatrixSize)
                        continue;

                    var target = enemies.FirstOrDefault(e => e.X == tx && e.Y == ty);
                    if (target != null)
                    {
                        target.Health -= player.Damage;
                        BattleLog.Add($"{player.Symbol} hits enemy at ({tx},{ty}) for {player.Damage}!");

                        if (target.Health <= 0)
                            KillEnemy(target);
                    }
                }

                return true;
            }
        }

        public static void KillEnemy(Enemy target)
        {
            enemies.Remove(target);
            EnemiesKilled++;
            BattleLog.Add($"Enemy at ({target.X},{target.Y}) died!");

            if (EnemiesKilled % 4 == 0)
            {
                player.Mana += 1;
                player.Health += 1;
                BattleLog.Add($"+1 Mana restored for {EnemiesKilled} kills!");
            }
        }

        private static bool IsMovementKey(ConsoleKey? key)
        {
            return key is ConsoleKey.W or ConsoleKey.A or ConsoleKey.S or ConsoleKey.D
                or ConsoleKey.Q or ConsoleKey.E or ConsoleKey.Z or ConsoleKey.X;
        }

        private static bool PromptForAttack(ConsoleKey? input)
        {
            var inRange = GetEnemiesInRange(player);

            if (inRange.Count == 0)
                return false;

            if (input.HasValue)
            {
                int index = input.Value switch
                {
                    >= ConsoleKey.D1 and <= ConsoleKey.D9 => input.Value - ConsoleKey.D1,
                    >= ConsoleKey.NumPad1 and <= ConsoleKey.NumPad9 => input.Value - ConsoleKey.NumPad1,
                    _ => -1
                };

                if (index >= 0 && index < inRange.Count)
                {
                    var enemy = inRange[index];
                    enemy.Health -= player.Damage;
                    BattleLog.Add($"You attacked enemy at ({enemy.X},{enemy.Y}) for {player.Damage}!");

                    if (enemy.Health <= 0)
                        KillEnemy(enemy);

                    return true;
                }
            }

            return false;
        }

        static void WriteLineCentered(string text, int row)
        {
            int x = (Console.WindowWidth - text.Length) / 2;
            Console.SetCursorPosition(x, row);
            Console.Write(text);
        }

        static (int col, int row) MapToScreen(int mapX, int mapY)
        {
            int originRow = (Console.WindowHeight - (MatrixSize + 2 + 4)) / 2 + 5; 
            int originCol = (Console.WindowWidth - (MatrixSize * 2 + 2)) / 2 + 1; 

            return (originCol + mapX * 2, originRow + mapY);
        }

        private static List<Enemy> GetEnemiesInRange(Character attacker)
        {
            return enemies
                .Where(e => CheckRange(attacker, e))
                .ToList();
        }

        private static void DrawMap(bool showLog = true)
        {
            Console.Clear();
            var hudLeft = (Console.WindowWidth - 40) / 2;
            Console.SetCursorPosition(hudLeft, 1);
            WriteLineCentered($"Health: {player.Health}  Mana: {player.Mana}  Dmg: {player.Damage}",1);
            Console.SetCursorPosition(hudLeft, 2);
            WriteLineCentered($"STR: {player.Strength}  AGI: {player.Agility}  INT: {player.Intelligence}  Range: {player.Range}", 2);

            Array.Clear(map, 0, map.Length);
            map[player.Y, player.X] = player.Symbol;
            foreach (var e in enemies)
                map[e.Y, e.X] = EnemySymbol;

            int topBorderY = MapToScreen(0, 0).row - 1;
            int leftBorderX = MapToScreen(0, 0).col - 1;

            Console.SetCursorPosition(leftBorderX, topBorderY);
            Console.Write("╔" + new string('═', MatrixSize * 2) + "╗");

            for (int y = 0; y < MatrixSize; y++)
            {
                Console.SetCursorPosition(leftBorderX, topBorderY + y + 1);
                Console.Write("║");

                for (int x = 0; x < MatrixSize; x++)
                {
                    var tile = map[y, x];
                    Console.ForegroundColor = tile switch
                    {
                        var t when t == player.Symbol => player.GetColor(),
                        var t when t == EnemySymbol => ConsoleColor.Red,
                        _ => ConsoleColor.DarkGray
                    };
                    
                    Console.Write(tile == '\0' ? $"{TileEmpty} " : $"{tile} ");
       
                }

                Console.ResetColor();
                Console.Write("║");
            }

            Console.SetCursorPosition(leftBorderX, topBorderY + MatrixSize + 1);
            Console.Write("╚" + new string('═', MatrixSize * 2) + "╝");

            int logStartX = MapLeft + MatrixSize * 2 + 4;
            int logStartY = MapTop;

            Console.ForegroundColor = ConsoleColor.Gray;
            var recentLines = BattleLog.TakeLast(LogLinesToShow).ToList();

            for (int i = 0; i < recentLines.Count; i++)
            {
                Console.SetCursorPosition(logStartX, logStartY + i);
                Console.Write(recentLines[i].PadRight(SidePanelWidth));
            }
            Console.ResetColor();


            var inRange = GetEnemiesInRange(player);
            int optionX = MapLeft - SidePanelWidth - 2;
            int optionY = MapTop;

            // Draw options for enemies in range
            
            if (inRange.Count == 0)
            {
                Console.SetCursorPosition(optionX, optionY);
                Console.Write("No enemies in range".PadRight(SidePanelWidth - 2));
                return;
            }

            Console.SetCursorPosition(optionX, optionY);
            Console.WriteLine("Enemies in range:".PadRight(SidePanelWidth - 2 ));


            for (int i = 0; i < inRange.Count; i++)
            {
                var enemy = inRange[i];
                Console.SetCursorPosition(optionX, optionY + i);
                Console.Write($"{i + 1}) Enemy ({enemy.X},{enemy.Y}) HP: {enemy.Health}".PadRight(SidePanelWidth));
            }

        }

        private static bool Move(ConsoleKey? key)
        {
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
                default:
                    return false;
            }

            if (!CheckBoundaries(newX, newY))
            {
                WriteLineCentered("You can't move there!",6);
                return false;
            }

            bool isOccupied = enemies.Any(e => e.X == newX && e.Y == newY);
            if (isOccupied)
            {
                WriteLineCentered("You can't walk onto an enemy!", 6);
                return false;
            }


            player.X = newX;
            player.Y = newY;
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
                    BattleLog.Add($" Monster at ({e.X},{e.Y}) hit you for {e.Damage}!");
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
