using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CrimeGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch spriteBatch;
        Texture2D ballTexture;
        Vector2 ballPosition;
        float ballSpeed;
        private SpriteFont font;
        private int framecount;
        private double elapsedTime;
        private int fps;
        private bool debugLog = true, showFPS;
        KeyboardState previousKeyboardState;
        //Game constructor
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = false;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            ballPosition = new Vector2(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2);
            ballSpeed = 120f;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Font");
            ballTexture = Content.Load<Texture2D>("ball");
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here
            KeyboardState currentKeyboardState = Keyboard.GetState();
            float updateBallSpeed = ballSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (currentKeyboardState.IsKeyDown(Keys.Up)) { ballPosition.Y -= updateBallSpeed; }
            if (currentKeyboardState.IsKeyDown(Keys.Down)) { ballPosition.Y += updateBallSpeed; }
            if (currentKeyboardState.IsKeyDown(Keys.Left)) { ballPosition.X -= updateBallSpeed; }
            if (currentKeyboardState.IsKeyDown(Keys.Right)) { ballPosition.X += updateBallSpeed; }

            if (currentKeyboardState.IsKeyDown(Keys.L) && !previousKeyboardState.IsKeyDown(Keys.L) && debugLog)
            {showFPS = !showFPS;}
            previousKeyboardState = currentKeyboardState;
            elapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            framecount++;
            if(elapsedTime >= 1000) {
                fps = framecount;
                framecount = 0;
                elapsedTime = 0;
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            spriteBatch.Draw(ballTexture, ballPosition, Color.White);
            if (showFPS)
            {
                spriteBatch.DrawString(font, $"FPS: {fps}", new Vector2(10, 10), Color.White);
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
