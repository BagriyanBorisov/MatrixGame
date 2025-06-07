using MatrixGame.Data;
using MatrixGame.Enums;
using MatrixGame.Models;

namespace MatrixGame.GameScreens
{
    public static class CharacterSelect
    {
        public static GameState Run()
        {
            Console.Clear();
            Console.WriteLine("Choose character type:");
            Console.WriteLine("Options:");
            Console.WriteLine("1) Warrior");
            Console.WriteLine("2) Archer");
            Console.WriteLine("3) Mage");
            Console.Write("Your pick: ");

            Character selectedCharacter = null;
            while (selectedCharacter == null)
            {
                string input = Console.ReadLine();
                switch (input)
                {
                    case "1": selectedCharacter = new Warrior(); break;
                    case "2": selectedCharacter = new Archer(); break;
                    case "3": selectedCharacter = new Mage(); break;
                    default: Console.Write("Invalid option. Try again: "); break;
                }
            }

            // Optional buffing
            Console.WriteLine("\nWould you like to buff up your stats before starting? (Limit: 3 points total)");
            Console.Write("Response (Y/N): ");
            string response = Console.ReadLine().ToUpper();

            if (response == "Y")
            {
                int remainingPoints = 3;
                int addedStr = 0, addedAgi = 0, addedInt = 0;

                while (remainingPoints > 0)
                {
                    Console.WriteLine($"\nRemaining Points: {remainingPoints}");

                    addedStr = ReadStat("Add to Strength", remainingPoints);
                    remainingPoints -= addedStr;

                    if (remainingPoints == 0) break;

                    addedAgi = ReadStat("Add to Agility", remainingPoints);
                    remainingPoints -= addedAgi;

                    if (remainingPoints == 0) break;

                    addedInt = ReadStat("Add to Intelligence", remainingPoints);
                    remainingPoints -= addedInt;
                }

                selectedCharacter.Strength += addedStr;
                selectedCharacter.Agility += addedAgi;
                selectedCharacter.Intelligence += addedInt;

                selectedCharacter.Setup();
            }

            SaveCharacterToDatabase(selectedCharacter);
            return GameState.InGame;
        }

        private static int ReadStat(string label, int max)
        {
            int value = -1;
            while (value < 0 || value > max)
            {
                Console.Write($"{label} (0 to {max}): ");
                if (!int.TryParse(Console.ReadLine(), out value) || value < 0 || value > max)
                {
                    Console.WriteLine("Invalid input. Try again.");
                }
            }
            return value;
        }

        private static void SaveCharacterToDatabase(Character character)
        {
            using var db = new GameDbContext();

            var entity = new Player
            {
                Type = character.GetType().Name,
                Strength = character.Strength,
                Agility = character.Agility,
                Intelligence = character.Intelligence,
                CreatedAt = DateTime.UtcNow
            };

            db.Players.Add(entity);
            db.SaveChanges();

            Console.WriteLine($"\nCharacter saved to database as '{entity.Type}'. Press any key to begin...");
            Console.ReadKey();
        }
    }
}
