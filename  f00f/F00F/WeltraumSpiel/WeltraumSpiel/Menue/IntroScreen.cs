/*
 * Das hier ist die IntorScreen Klasse die brauchen um das Intro anzuspielen
 * @author: Alexander Stoldt
 * @version: 1.0
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;




namespace WeltraumSpiel
{
    class IntroScreen : MenuScreen
    {
        //Background texture for the screen
        Texture2D introScreenBackground;

        public IntroScreen(ContentManager theContent, EventHandler theScreenEvent): base(theScreenEvent)
        {
            //Load the background texture for the screen
            introScreenBackground = theContent.Load<Texture2D>(@"Textures\intro");
        }

        //Call the screen event associated with this screen
        public override void update(GameTime theTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Space) == true)
            {
                ScreenEvent.Invoke(this, new EventArgs());
                return;
            }
          base.update(theTime);
        }

        public override void Draw(SpriteBatch theBatch)
        {
            theBatch.Draw(introScreenBackground, Vector2.Zero, Color.White);
            base.Draw(theBatch);
        }
    }
}
