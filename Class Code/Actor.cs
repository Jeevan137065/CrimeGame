using CrimeGame.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Graphics;
using System;

namespace CrimeGame
{
    public enum Direction { Down, Left, Right, Up }
    public class Actor
    {
        protected int age;
        protected string Name;
        protected bool isInteractable;


        protected Texture2D Texture;
        public Vector2 Position;
        public Vector2 Velocity = Vector2.Zero;
        public Actor() { }
        public void LoadContent() { }
        public void Update(GameTime gameTime) { }
        public void Draw() { }
    }

    public class Player : Actor
    {
        //Player data
        protected int health;
        protected int energy;
        protected Emotions emotions;

        //Player Tech data
        public float Speed;
        private int frameWidth = 32;
        private int frameHeight = 48;
        public int currentframe = 0;
        private double animationTimer = 0;
        private double animationInterval = 0.125;
        public Direction CurrentDirection = Direction.Down;
        //Visuals
        private Texture2D UpSpriteSheet;
        private Texture2D DownSpriteSheet;
        private Texture2D LeftSpriteSheet;
        private Texture2D RightSpriteSheet;


        public Player(float startX, float startY)
        {
            Position = new Vector2(startX, startY);
            Speed = 120f;
        }

        public void LoadContent(ContentManager content)
        {   UpSpriteSheet = content.Load<Texture2D>("NORTH");
            DownSpriteSheet = content.Load<Texture2D>("SOUTH");
            LeftSpriteSheet = content.Load<Texture2D>("WEST");
            RightSpriteSheet = content.Load<Texture2D>("EAST");
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            Velocity = Vector2.Zero;
            if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W)) { 
                Velocity.Y -= 1;
                CurrentDirection = Direction.Up;
            }
            if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S)) {
                Velocity.Y += 1;
                CurrentDirection = Direction.Down;
            }
            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A)) {
                Velocity.X -= 1;
                CurrentDirection = Direction.Left;
            }
            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
            {
                Velocity.X += 1;
                CurrentDirection = Direction.Right;
            }
            if (Velocity != Vector2.Zero) { 
                Velocity.Normalize();
                animationTimer += gameTime.ElapsedGameTime.TotalSeconds;
                if (animationTimer > animationInterval)
                {
                    animationTimer = 0;
                    currentframe = (currentframe+ 1) % 8;
                }
            }
            else { currentframe = 0; }

            //float delta = Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += Velocity * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }



        public void Draw(SpriteBatch spriteBatch)
        {
            Texture2D currentSpriteSheet = CurrentDirection switch
            {
                Direction.Up => UpSpriteSheet,
                Direction.Down => DownSpriteSheet,
                Direction.Left => LeftSpriteSheet,
                Direction.Right => RightSpriteSheet,
                _ => DownSpriteSheet
            };

            Rectangle sourceRectangle = new Rectangle(currentframe * frameWidth, 0, frameWidth, frameHeight);


            //spriteBatch.Draw(Texture, Position, Color.White);

            spriteBatch.Draw(currentSpriteSheet, Position, sourceRectangle, Color.White);

        }

    }

    public class SNPC : Actor { }
    public class NPC : Actor { }
    public class Animal : Actor { }
    public class AI : Actor { }
}