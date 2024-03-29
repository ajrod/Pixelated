using System;

namespace Pixelated
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (PixelArt game = new PixelArt())
            {
                game.Run();
            }
        }
    }
#endif
}

