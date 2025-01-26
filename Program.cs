using System;

namespace CrimeGame
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "-editor")
            {
                using (var editor = new EditorMode())
                    editor.Run();
            }
            else
            {
                using (var game = new Game1())
                    game.Run();
            }
        }
    }
}
