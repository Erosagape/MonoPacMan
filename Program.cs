using System;

namespace MyPacMan
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new MonoPacman())
                game.Run();
        }
    }
}
