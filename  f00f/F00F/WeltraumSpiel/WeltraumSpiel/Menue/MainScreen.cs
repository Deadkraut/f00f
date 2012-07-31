/*
 * Das hier ist eine MainScreen Klasse die wir für unser Haupmenue benoetigen
 * @author: Alexander Stoldt
 * @version: 1.0
 */ 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace WeltraumSpiel.Menue
{
    class MainScreen : Screen
    {
        //Background texture for the screen
        Texture2D mainScreenBackground;

        public MainScreen(ContentManager theContent, EventHandler theScreenEvent): base(theScreenEvent)
        {
            //Load the background texture for the screen
            mainScreenBackground = theContent.Load<Texture2D>(@"Textures\TitleScreen");
        }

        public override void update(GameTime theTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) == true)
            {
                ScreenEvent.Invoke(this, new EventArgs());
                return;
            }
            base.update(theTime);
        }

        public override void Draw(SpriteBatch theBatch)
        {
            theBatch.Draw(mainScreenBackground, Vector2.Zero, Color.White);
            base.Draw(theBatch);
        }
    }
}
