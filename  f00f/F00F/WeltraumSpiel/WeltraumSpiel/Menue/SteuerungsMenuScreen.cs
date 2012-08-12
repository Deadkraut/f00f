#region ::::Changelog::::
/*
 *SteuerungsMenuScreen.cs
 * 
 *
 * @author: Alexander Stoldt
 * @version: 1.0
 */
#endregion
#region Using Statements
using WeltraumSpiel.MenueManager; // Dient dazu das MenuEntry hier auch geladen wird
using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Content;  // Wird für den ContendManager benötigt
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace WeltraumSpiel
{
    class SteuerungsMenuScreen : MenuScreen
    {
        #region Fields
    
        #endregion

        #region Initialization

        public SteuerungsMenuScreen()
            : base("Steuerung")
        {

            // Create our menu entries.

            MenuEntry back = new MenuEntry("Zurueck");

            // Hook up menu event handlers.
            back.Selected += OnCancel;

            MenuEntries.Add(back);
        }

        #endregion

        public override void  DrawOtherComponents()
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Rectangle rectangle = new Rectangle(125, 200, this.KeyboardMouseTexture.Width, this.KeyboardMouseTexture.Height);
            spriteBatch.Begin();

            spriteBatch.Draw(this.KeyboardMouseTexture, rectangle,
                            new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));
            spriteBatch.End();
 	         base.DrawOtherComponents();
        }
    }
}
