﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.VisualBasic;
namespace CrimeGame
{
    public class DevInfo
    {
        private int frameCount;
        private double elapsedTime;
        public int fps;
        
        public void LoadContent(ContentManager content)
        {}

        public void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            frameCount++;

            if (elapsedTime >= 1000)
            {
                fps = frameCount;
                frameCount = 0;
                elapsedTime = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {}
    }
}
