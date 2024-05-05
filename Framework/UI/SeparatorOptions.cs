using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;

namespace SplitScreenRegions.Framework.UI
{
    public class SeparatorOptions
    {
        private const int separatorHeight = 3;

        private static Texture2D CreateEmptyTexture(GraphicsDevice graphicsDevice)
        {
            Texture2D texture = new Texture2D(graphicsDevice, 1, 1);
            Color[] pixels = [Color.White];
            texture.SetData(pixels);
            return texture;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            int top = (int)position.Y;
            int usableWidth = Math.Min(1200, Game1.uiViewport.Width - 200);
            int left = (Game1.uiViewport.Width - usableWidth) / 2;

            spriteBatch.Draw(CreateEmptyTexture(Game1.graphics.GraphicsDevice), new Rectangle(left, top, usableWidth, separatorHeight), Game1.textColor);
        }
    }
}
