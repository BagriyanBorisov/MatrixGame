using MatrixGame.Enums;

namespace MatrixGame.GameScreens
{
    public static class MainMenu
    {
        public static GameState Run()
        {
            Console.Clear();
            Console.WriteLine("Welcome!");
            Console.WriteLine("Press any key to play.");
            Console.ReadKey();
            return GameState.CharacterSelect;
        }
    }
}
