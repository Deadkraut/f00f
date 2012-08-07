#region Using Statements

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace WeltraumSpiel.HUD
{
    class Crosshair
    {
        private int screenWidth = 800;
        private int screenHeight = 600;
        int chFrames;
        Sprite chColors;
        Vector2 chLocation;
        Texture2D chTexture;
        Rectangle initialFrame;
        bool chEnabled = true;

        public bool CrosshairIsEnabled
        {
            get { return chEnabled; }
            set { chEnabled = value; }
        }

        public Sprite AddCrosshair()
        {
            Sprite newCrosshair = new Sprite(chLocation, chTexture, initialFrame, Vector2.Zero);

            for (int x = 1; x < chFrames; x++)
            {
                newCrosshair.AddFrame(new Rectangle(initialFrame.X + (initialFrame.Width * x), initialFrame.Y, initialFrame.Width, initialFrame.Height));

            }
            return newCrosshair;
        }

        public Crosshair(Texture2D chTexture, Rectangle initialFrame, int chFrames, int screenWidth, int screenHeight)
        {
            this.chTexture = chTexture;
            this.initialFrame = initialFrame;
            this.chFrames = chFrames;
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;

            chLocation = new Vector2(screenWidth / 2 - 25, screenHeight / 2 - 200);

            chColors = AddCrosshair();
        }

        public void Update(GameTime gameTime)
        {
            chColors.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (chEnabled)
            {
                chColors.Draw(spriteBatch);
            }
        }

    }
}
