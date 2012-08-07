#region ::::Chancelog::::
/*
 * Game1.cs
 * Das hier ist ein WeltraumSpiel in dem man mit einem Raumgleiter eine Mission erf�llen muss.
 * 
 * Version: 1.0
 * author: Mikael Wolff, Alexander Stoldt, Marcel Abel
 * Wir haben das Tutorial von der Seite http://www.riemers.net/eng/Tutorials/XNA/Csharp/series2.php durchgearbeitet
 * 
 * @version: 1.1
 * @author: Alexander Stoldt
 * 
 * Ich habe mit Hilfe des Tutorials auf der Seite http://www.xnadevelopment.com/tutorials/notsohealthy/NotSoHealthy.shtml
 * eine Lebensanzeig f�r unseren Raumgleiter intigriert. Dazu die Methode healtbar eingef�gt.
 * Au�erdem habe ich mit diesem Tutorial http://www.xnamag.de/article.php?aid=16 in eine GameComponent integriet
 * 
 * @version: 1.2 (Beobachten)
 * @author: Alexander Stoldt
 * Ich habe ein paar Bugs bei der healthbar beseitigt und unn�tigen Code gel�scht. Au�erdem habe ich ein GameOver eingef�hrt
 * 
 * @version: 1.3 (TODO)
 * @author: Alexander Stoldt
 * Ich habe damit angefangen eine Menue zu implementieren. Dazu habe ich genutzt http://www.xnadevelopment.com/tutorials/thestateofthings/thestateofthings.shtml
 * und hiervon das 3 Bsp:
 * 
 * @version: 1.4
 * @author: Marcel Abel
 * Ich habe eine Lebendauer f�r unsere Geschosse (die Bullets) implementiert und eine BoundaryBox eingef�gt, die den Missionsbereich begrenzt und
 * in der die Ziele (targets) erzeugt werden. 
 * Die BoundaryBox wird in der Methode AddBoundaryBox() erzeugt, dazu wird zus�tzlich das Feld dimension ben�tigt, um die Dimension der Box festzulegen.
 * Die Bullets verf�gen nun �ber ein Feld persistence, in das die aktuelle GameTime gespeichert wird. Nach ca. 10 Sekunden werden die Bullets mittels einer If-Abfrage
 * in der Methode UpdateBulletPositions() aus der bulletList entfernt.
 * 
 */

#endregion
#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace WeltraumSpiel
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class WeltraumSpiel : Microsoft.Xna.Framework.Game
    {

        #region Constants

        const int screenWidth = 800;
        const int screenHeigth = 600;
        #endregion

        #region Fields

       public  GraphicsDeviceManager graphics;
       public  ScreenManager screenManager;
       public  GraphicsDevice device;
     

        // By preloading any assets used by UI rendering, we avoid framerate glitches
        // when they suddenly need to be loaded in the middle of a menu transition.
        static readonly string[] preloadAssets =
        {
            "gradient",
        };


        #endregion

        #region Initialization

        public WeltraumSpiel()
        {
            Content.RootDirectory = "Content";

            graphics = new GraphicsDeviceManager(this);
            //this.IsMouseVisible = true;        //Macht die Maus sichtbar
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 900;
            //graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            Window.Title = "Fly in Space";

            // Create the screen manager component.
            screenManager = new ScreenManager(this);
            device = graphics.GraphicsDevice;
                
     
                Components.Add(screenManager);

                // Activate the first screens.
                screenManager.AddScreen(new BackgroundScreen(), null);
                screenManager.AddScreen(new MainMenuScreen(), null);

            
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            foreach (string asset in preloadAssets)
            {
                Content.Load<object>(asset);
            }
          
        }



        #endregion

        #region Draw


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            // The real drawing happens inside the screen manager component.
            base.Draw(gameTime);
        }



        #endregion
 
    }
}
