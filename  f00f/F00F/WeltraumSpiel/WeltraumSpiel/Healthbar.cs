#region File Description

/*
 * Das hier ist die GameComponent die wir in Game1 für unsere Lebensleiste nutzen
 * @author: Alexander Stoldt
 * @version: 1.0
 */

#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

#endregion

namespace WeltraumSpiel
{
    /// <summary>
    /// Dies ist eine Spielkomponente, die IUpdateable implementiert.
    /// </summary>
    public class Healthbar : Microsoft.Xna.Framework.DrawableGameComponent
    {
        #region Fields

        SpriteBatch mbatch;
        Texture2D mHealthBar;
        Game1 game;

        int punktabzug = 0;

        #endregion

        #region Properties

        public int Punktabzug
        {
            get { return punktabzug; }
            set { punktabzug = value; }
        }

        #endregion

        #region Initialization

        public Healthbar(Game1 game) : base(game)
        {
            // TODO: Konstruieren Sie untergeordnete Komponenten hier
            this.game = game;
        }

        //public Healthbar(Game game, GraphicsDeviceManager graphics)
        //    : base(game)
        //{
        //    this.graphics = graphics;
        //}


        /// <summary>
        /// Ermöglicht der Spielkomponente die Durchführung von Initialisierungen, die sie benötigt, bevor sie ausgeführt
        /// werden kann.  Dort kann sie erforderliche Dienste abfragen und Inhalte laden.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Fügen Sie Ihren Initialisierungscode hier hinzu
            base.Initialize();
        }

        protected override void LoadContent()
        {
            mbatch = new SpriteBatch(game.graphics.GraphicsDevice);

            //Load the HealthBar image from the disk into the Texture2D object
            mHealthBar = game.Content.Load<Texture2D>(@"Textures\healthBar") as Texture2D;
            base.LoadContent();
        }

        #endregion

        #region Update & Draw

        /// <summary>
        /// Ermöglicht der Spielkomponente, sich selbst zu aktualisieren.
        /// </summary>
        /// <param name="gameTime">Bietet einen Schnappschuss der Timing-Werte.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Fügen Sie Ihren Aktualisierungscode hier hinzu
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            mbatch.Begin();
            //Draw the health for the health bar
            mbatch.Draw(mHealthBar, new Rectangle(game.Window.ClientBounds.Width / 2 - mHealthBar.Width / 2,
                                                  30, mHealthBar.Width - Punktabzug, 44), new Rectangle(0, 45, mHealthBar.Width, 44), Color.Red);

            //Draw the box around the health bar
            mbatch.Draw(mHealthBar, new Rectangle(game.Window.ClientBounds.Width / 2 - mHealthBar.Width / 2,
                                                  30, mHealthBar.Width - Punktabzug, 44), new Rectangle(0, 0, mHealthBar.Width, 44), Color.White);
            mbatch.End();
            base.Draw(gameTime);
        }

        #endregion
    }
}

