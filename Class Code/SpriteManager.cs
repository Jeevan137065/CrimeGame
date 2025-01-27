using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace CrimeGame
{
    public static class SpriteBatchExtensions
    {
        /// Draws a line between two points.
        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, int thickness = 1)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);

            spriteBatch.Draw(
                CreateTexture(spriteBatch.GraphicsDevice, thickness, 1, color),
                start,
                null,
                color,
                angle,
                Vector2.Zero,
                new Vector2(edge.Length(), thickness),
                SpriteEffects.None,
                0
            );
        }

        /// Draws a rectangle.
        public static void DrawRectangle(this SpriteBatch spriteBatch, Rectangle rectangle, Color color, int thickness = 1)
        {
            // Top line
            spriteBatch.DrawLine(new Vector2(rectangle.Left, rectangle.Top), new Vector2(rectangle.Right, rectangle.Top), color, thickness);
            // Bottom line
            spriteBatch.DrawLine(new Vector2(rectangle.Left, rectangle.Bottom), new Vector2(rectangle.Right, rectangle.Bottom), color, thickness);
            // Left line
            spriteBatch.DrawLine(new Vector2(rectangle.Left, rectangle.Top), new Vector2(rectangle.Left, rectangle.Bottom), color, thickness);
            // Right line
            spriteBatch.DrawLine(new Vector2(rectangle.Right, rectangle.Top), new Vector2(rectangle.Right, rectangle.Bottom), color, thickness);
        }

        /// Creates a 1x1 texture for drawing purposes.
        private static Texture2D CreateTexture(GraphicsDevice graphicsDevice, int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; ++i) data[i] = color;
            texture.SetData(data);
            return texture;
        }
    }

    public class Palette
    {
        private int tileSize = 64;
        private Rectangle paletteArea;
        private Texture2D[] tiles;
        private int selectedIndex = 0;

        public Palette(Texture2D[] tileTexture)     {   tiles = tileTexture; }
        public void Update(MouseState currentMouseState) {
            if (currentMouseState.LeftButton == ButtonState.Pressed && paletteArea.Contains(currentMouseState.Position))
            {
                int clickedIndex = (currentMouseState.X - paletteArea.X) / tileSize;
                if (clickedIndex >= 0 && clickedIndex < tiles.Length) { selectedIndex = clickedIndex; }
            }
        }
        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice) {
            paletteArea = new Rectangle(10, graphicsDevice.Viewport.Height - tileSize - 10,
                                         tiles.Length * tileSize, tileSize);

            for (int i = 0; i < tiles.Length; i++)
            {
                var destination = new Rectangle(paletteArea.X + i * tileSize, paletteArea.Y, tileSize, tileSize);
                spriteBatch.Draw(tiles[i], destination, Color.White);

                if (i == selectedIndex)
                {
                    spriteBatch.DrawRectangle(destination, Color.Yellow, 3);
                }
            }
        }


    }
}