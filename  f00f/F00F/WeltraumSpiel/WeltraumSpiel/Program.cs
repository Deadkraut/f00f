#region ::::Chancelog::::
/*
 * Program.cs
 * 
 * Hierüber wird das Spiel dann gestartet
 * @author: 
 * @version: 1.0
 */
#endregion
using System;

namespace WeltraumSpiel
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Game1 game = new Game1())
            {
                game.Run();
            }
        }
    }
#endif
}

