using Iguina;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CrimeGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch spriteBatch;

        private Player ball;
        private DevInfo fpscounter;
        private bool debugLog = true, showFPS;
        KeyboardState previousKeyboardState;
        UISystem uiSystem = null!;
        //Game constructor
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.Title = "Game 0.1";
            IsMouseVisible = true;
            IsFixedTimeStep = false;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            ball = new Player(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2);
            fpscounter = new DevInfo();
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            ball.LoadContent(Content, "Ball");
            fpscounter.LoadContent(Content, "Font");
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here
            KeyboardState currentKeyboardState = Keyboard.GetState();

            HandleInput(currentKeyboardState);
            ball.Update(gameTime, currentKeyboardState);
            fpscounter.Update(gameTime);
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
            if (showFPS)    {fpscounter.Draw(spriteBatch);}
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
