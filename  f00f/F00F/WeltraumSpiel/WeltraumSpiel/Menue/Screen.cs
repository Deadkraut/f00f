/*
 * Das hier ist eine Screen Klasse die brauchen um die anderen Screens mit Vererbung zu nutzen
 * @author: Alexander Stoldt
 * @version: 1.0
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;



namespace WeltraumSpiel.Menue
{
    class Screen
    {

        //The event associated with the Screen. This event is used to raise events
        //back in the main game class to notify the game that something has changed
        //or needs to be changed
        protected EventHandler ScreenEvent;

        public Screen(EventHandler theScreenEvent)
        {
            ScreenEvent = theScreenEvent;
        }

        //Update any information specific to the screen
        public virtual void update(GameTime theTime)
        {
        }



        //Draw any objects specific to the screen
        public virtual void Draw(SpriteBatch theBatch)
        {
        }

    }
}
