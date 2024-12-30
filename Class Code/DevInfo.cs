using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.VisualBasic;
namespace CrimeGame
{
    public class DevInfo
    {
        private SpriteFont _font;
        private int _frameCount;
        private double _elapsedTime;
        private int _fps;
        
        public void LoadContent(ContentManager content, string fontName)
        {
            _font = content.Load<SpriteFont>(fontName);
        }

        public void Update(GameTime gameTime)
        {
            _elapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            _frameCount++;

            if (_elapsedTime >= 1000)
            {
                _fps = _frameCount;
                _frameCount = 0;
                _elapsedTime = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, $"FPS: {_fps}", new Vector2(10, 10), Color.White);
        }
    }
}
