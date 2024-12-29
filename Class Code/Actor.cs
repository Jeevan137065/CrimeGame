using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CrimeGame
{
    public class Actor
    {
        private Texture2D _texture;
        private Vector2 _position;
        private float _speed;

        public Actor(float startX, float startY, float speed)
        {
            _position = new Vector2(startX, startY);
            _speed = speed;
        }

        public void LoadContent(ContentManager content, string textureName)
        {
            _texture = content.Load<Texture2D>(textureName);
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            float delta = _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (keyboardState.IsKeyDown(Keys.Up)) _position.Y -= delta;
            if (keyboardState.IsKeyDown(Keys.Down)) _position.Y += delta;
            if (keyboardState.IsKeyDown(Keys.Left)) _position.X -= delta;
            if (keyboardState.IsKeyDown(Keys.Right)) _position.X += delta;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _position, Color.White);
        }
    }
}