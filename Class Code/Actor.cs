using CrimeGame.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CrimeGame
{
    public class Actor
    {
        protected int age;
        protected string Name;
        protected bool isInteractable;


        protected Texture2D Texture;
        protected Vector2 Position;
        public Actor()                          {}
        public void LoadContent()               {}
        public void Update(GameTime gameTime)   {}
        public void Draw()                      {}
    }

    public class Player : Actor
    {
        protected int health;
        protected int energy;
        protected float Speed;
        protected Emotions emotions;
        public Player(float startX, float startY)
        {
            Position = new Vector2(startX, startY);
            Speed = 120f;
 
        }

        public void LoadContent(ContentManager content, string textureName)
        { Texture = content.Load<Texture2D>(textureName);}

        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            float delta = Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (keyboardState.IsKeyDown(Keys.Up)) Position.Y -= delta;
            if (keyboardState.IsKeyDown(Keys.Down)) Position.Y += delta;
            if (keyboardState.IsKeyDown(Keys.Left)) Position.X -= delta;
            if (keyboardState.IsKeyDown(Keys.Right)) Position.X += delta;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, Color.White);
        }

    }

    public class SNPC : Actor { }
    public class NPC : Actor { }
    public class Animal : Actor { }
    public class AI : Actor { }
}