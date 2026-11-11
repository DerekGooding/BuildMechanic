using System;
using System.Linq;
using BuildMechanic.Inspector;

namespace BuildMechanic
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            bool launchViewer = args != null && args.Any(a =>
                a.Equals("--model-viewer", StringComparison.OrdinalIgnoreCase) ||
                a.Equals("--inspect", StringComparison.OrdinalIgnoreCase) ||
                a.Equals("-m", StringComparison.OrdinalIgnoreCase));

            if (launchViewer)
            {
                using var viewer = new ModelViewerGame();
                viewer.Run();
            }
            else
            {
                using var game = new Game1();
                game.Run();
            }
        }
    }
}
