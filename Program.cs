using MatrixGame.Enums;
using MatrixGame.GameScreens;

namespace MatrixGame
{
    internal class Program
    {

        static GameState currentState = GameState.MainMenu;

        static void Main(string[] args)
        {
            while (currentState != GameState.Exit)
            {
                switch (currentState)
                {
                    case GameState.MainMenu:
                        currentState = MainMenu.Run();
                        break;
                    case GameState.CharacterSelect:
                        currentState = CharacterSelect.Run();
                        break;
                    case GameState.InGame: InGame.Run();
                        return;

                }
            }

            Console.WriteLine("Thanks for playing!");
        }
    }
}

