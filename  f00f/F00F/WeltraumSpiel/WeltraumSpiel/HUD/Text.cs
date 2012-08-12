#region Using Statements

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion


namespace WeltraumSpiel.HUD
{
    class Text
    {
        #region Fields

        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        private GraphicsDevice graphicsDevice;

        private Vector2 position;

        private String textLabel;
        private String textValue;
        private Color textColor;
        private bool textEnabled = true;

        #endregion

        #region Properties

        public bool TextEnabled
        {
            get { return textEnabled; }
            set { textEnabled = value; }
        }

        #endregion

        #region Initialization

        public Text(String textLabel, Vector2 position, SpriteBatch spriteBatch, SpriteFont spriteFont, GraphicsDevice graphicsDevice)
        {
            this.textLabel = textLabel.ToUpper();
            this.position = position;

            this.spriteBatch = spriteBatch;
            this.spriteFont = spriteFont;
            this.graphicsDevice = graphicsDevice;
        }

        #endregion

        #region Update & Draw

        public void Update(String textValue, Color textColor)
        {
            this.textValue = textValue.ToUpper();
            this.textColor = textColor;
        }


        public void Draw()
        {
            if (textEnabled)
            {
                Color myTransparentColor = new Color(0, 0, 0, 127);

                Vector2 stringDimensions = spriteFont.MeasureString(textLabel + ": " + textValue);
                float width = stringDimensions.X;
                float height = stringDimensions.Y;

                Rectangle backgroundRectangle = new Rectangle();
                backgroundRectangle.Width = (int)width + 10;
                backgroundRectangle.Height = (int)height + 10;
                backgroundRectangle.X = (int)position.X - 5;
                backgroundRectangle.Y = (int)position.Y - 5;

                Texture2D dummyTexture = new Texture2D(graphicsDevice, 1, 1);
                dummyTexture.SetData(new Color[] { myTransparentColor });

                spriteBatch.Begin();
                spriteBatch.Draw(dummyTexture, backgroundRectangle, myTransparentColor);
                spriteBatch.DrawString(spriteFont, textLabel + ": " + textValue, position, textColor);
                spriteBatch.End();
            }
        }

        #endregion

    }
}
