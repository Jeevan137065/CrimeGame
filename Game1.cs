using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


// using GeonBit UI elements
namespace CrimeGame
{
    public class Game1 : Game
    {
        //Techincal
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch spriteBatch;
        //Main
        private Player ball;
        private DevInfo fpscounter;
        private bool debugLog = true, showFPS;
        public SpriteFont font;
        //Game constructor
        private string jsonFilePath = "NewFolder/beach.json"; // JSON map file
        private string texturePath = "Content/basi.png"; // Texture file path
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.Title = "Game 0.1";
            //IsMouseVisible = true;
            //IsFixedTimeStep = false;

        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            ball = new Player(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2);
            fpscounter = new DevInfo();
            //UserInterface.Initialize(Content, BuiltinThemes.hd);
            //create UI logic here


            base.Initialize();
        }




        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            ball.LoadContent(Content);
            fpscounter.LoadContent(Content);
            font = Content.Load<SpriteFont>("Font");



        }

        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here

            //UserInterface.Active.Update(gameTime);
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
            //UserInterface.Active.Draw(spriteBatch);
            spriteBatch.Begin();
            ball.Draw(spriteBatch);
            if (showFPS)
            {
                Vector2 Momentum = ball.Velocity * ball.Speed;
                string info = $"Direction: {ball.CurrentDirection.ToString()}\n" +
                        $"          Frame:{ball.currentframe}\n" +
                        $"          Position: {ball.Position}\n" +
                        $"          Velocity: {ball.Velocity}\n" +
                        $"          Momentum: {Momentum}";
                spriteBatch.DrawString(font, $"FPS: {fpscounter.fps}\n", new Vector2(10, 10), Color.White);
                spriteBatch.DrawString(font, info, new Vector2(10, 24), Color.White);


            }
            spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
