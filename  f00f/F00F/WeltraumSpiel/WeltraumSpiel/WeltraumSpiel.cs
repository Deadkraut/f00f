#region ::::Changelog::::
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
 * @version: 1.5
 * @author: Mikael Wolff
 * Ich habe das Modell des Schiffes durch ein eigenes ersetzt und die Lademethode entsprechend angepasst.
 * Die Steuerung wurde ver�ndert und sieht nun folgenderma�en aus:
 * W/S - Das Schiff wird entweder nach unten oder nach oben geneigt.
 * A/D - Das Schiff dreht sich entweder nach links oder nach rechts.
 * Q/E - Das Schiff dreht sich um seine L�ngsachse.
 * Shift(links) - Das Schiff fliegt mit doppelter Geschwindigkeit.
 * Geschossen wurde ein Soundeffekt beigef�gt.
 * 
 * @version: 1.6
 * @author: Marcel Abel
 * Ich habe ein cyanfarbenes Fadenkreuz hinzugef�gt.
 * Au�erdem einen Ordner HUD und 2 neue Klassen: Sprite und Crosshair.
 * Der Code der Klasse Sprite stammt aus dem Buch: XNA 4.0 Game Development by Example Beginner's Guide - Chapter 5 von Kurt Jaegers
 * 
 * @version: 1.7
 * @author: Marcel Abel
 * Ich habe ein paar Sounds zu unserem Menu hinzugef�gt.
 * 
 * @version: 1.8
 * @author: Marcel Abel
 * Ich habe einen "Explosions-Effect" eingef�gt, wenn der Spieler "zerst�rt" und somit das Spiel beendet wird.
 * Sobald kein Leben in der Lebensanzeige mehr vorhanden ist, wird der J�ger als zerst�rt gemeldet, eine Explosion wird abgespielt und er zerbricht in 3,
 * sich langsam fortbewegende und rotierende Teile.
 * 
 * @version: 1.9
 * @author: Mikael Wolff
 * Ich habe das Zielmodell durch ein eigenes ausgetauscht, zudem wurde im HUD eine Punkteanzeige eingebaut, die mit den
 * zerst�rten zielen hochz�hlt. Zudem wurde die Geschwindigkeitssteigerung durch Absch�sse neu eingegrenzt und die Steuerung auf die
 * Geschwindigkeitserh�hung hin angepasst.
 * Austausch des Zielmodelles durch ein eigenes Asteroidenmodell.
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

        public WeltraumSpiel(bool gameSta)
        {
            Content.RootDirectory = "Content";

            graphics = new GraphicsDeviceManager(this);
            //this.IsMouseVisible = true;        //Macht die Maus sichtbar
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 900;
            //graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            Window.Title = "Fly in Space";
            device = graphics.GraphicsDevice;

            // Create the screen manager component.
            screenManager = new ScreenManager(this);

            if (gameSta == false)
            {
                Components.Add(screenManager);

                // Activate the first screens.
                screenManager.AddScreen(new BackgroundScreen(), null);
                screenManager.AddScreen(new MainMenuScreen(), null);
            }
            else if (gameSta == true)
            {
                System.Console.WriteLine("Spiel;-)");
            }

            
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            //foreach (string asset in preloadAssets)
            //{
            //    Content.Load<object>(asset);
            //}
          
        }



        #endregion

        #region Draw


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            //The real drawing happens inside the screen manager component.
            base.Draw(gameTime);
        }



        #endregion


        static void Main(string[] args)
        {
            using (WeltraumSpiel game = new WeltraumSpiel(false))
            {
                game.Run();
            }
        }
    }
}
