using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace CrimeGame
{
    public class Game1 : Game
    {
        //Techincal
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch spriteBatch;
        private FPS fps;
        private bool debugLog = true, showFPS;
        //Main
        private Player ball;
        public SpriteFont font;
        private Texture2D test;
        //Game Data
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.Title = "Game 0.1";
            IsMouseVisible = true;
            IsFixedTimeStep = false;
            _graphics.SynchronizeWithVerticalRetrace = false; // Disables VSync
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            ball = new Player(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2);
            fps = new FPS();
            //create UI logic here
            base.Initialize();
        }

        protected override void LoadContent()
        {
            //Order 1. Techinical 2. UI and Meaningful crap 3. Assets
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            font = Content.Load<SpriteFont>("Font");
            test = Content.Load<Texture2D>("Basi");

            fps.LoadContent(Content);
            
            
            ball.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            //Techinical Stuff
            KeyboardState currentKeyboardState = Keyboard.GetState();
            MouseState  currentMouseState ,previousMouseState;
            
            //Logic
            currentMouseState = Mouse.GetState();
            previousMouseState = currentMouseState;
            HandleInput(currentKeyboardState);

            //Logical Aftermath
            ball.Update(gameTime, currentKeyboardState);
            fps.Update(gameTime);

            base.Update(gameTime);
        }

        private void HandleInput(KeyboardState currentKeyboardState)
        {
            if (currentKeyboardState.IsKeyDown(Keys.L) && debugLog) // && !previousKeyboardState.IsKeyDown(Keys.L))
            {
                showFPS = !showFPS;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            spriteBatch.Begin();

            ball.Draw(spriteBatch);
            if (showFPS)
            {   Vector2 Momentum = ball.Velocity * ball.Speed;
                string info = $"Direction: {ball.CurrentDirection.ToString()}\n" +
                        $"          Frame:{ball.currentframe}\n" +
                        $"          Position: {ball.Position}\n" +
                        $"          Velocity: {ball.Velocity}\n" +
                        $"          Momentum: {Momentum}";
                spriteBatch.DrawString(font, $"FPS: {fps.fpsCounter(gameTime)}\n", new Vector2(10, 10), Color.White);
                spriteBatch.DrawString(font, info, new Vector2(10, 24), Color.White);}

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
