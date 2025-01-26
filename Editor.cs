using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace CrimeGame
{
    public class EditorMode : Game
    {
        //Technical
        private GraphicsDeviceManager _graphics;
        private SpriteBatch spriteBatch;
        private Stopwatch cpuStopwatch;
        private TimeSpan lastCpuTime;
        private float cpuUsage = 0f;
        private long maxMemory = 0;
        private bool showFPS = true;
        private FPS fps;
        //Editor
        private int gridCellSize = 32; // Default grid cell size
        private Color gridColor = Color.Gray * 0.5f; // Semi-transparent grid lines
        private Point selectedCell = Point.Zero; // Selected cell on the grid
        public SpriteFont font;


        private void WindowDetail(GameWindow window)
        {
            // Configure the editor window
            Window.Title = "Level Editor";
            Window.AllowUserResizing = true;
            _graphics.PreferredBackBufferWidth = 1280; // Editor window width
            _graphics.PreferredBackBufferHeight = 720; // Editor window height
            IsFixedTimeStep = false;
            _graphics.SynchronizeWithVerticalRetrace = false; // Disables VSync
            _graphics.ApplyChanges();
        }
        public EditorMode()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            WindowDetail(this.Window);
            
            cpuStopwatch = Stopwatch.StartNew();
            lastCpuTime = Process.GetCurrentProcess().TotalProcessorTime;

        }

        protected override void Initialize()
        {
            base.Initialize();
            fps = new FPS();
            // Initialize editor-specific logic here
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Font");
            //fps.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            var currentKeyboardState = Keyboard.GetState();
            var currentMouseState = Mouse.GetState();
            
            HandleInput(currentKeyboardState, currentMouseState);
            fps.Update(gameTime);

            UpdateCpuUsage();
            maxMemory = Math.Max(maxMemory, Process.GetCurrentProcess().PrivateMemorySize64 / (1024 * 1024)); // Convert to MB
            base.Update(gameTime);
        }

        private void HandleInput(KeyboardState currentKeyboardState, MouseState currentMouseState)
        {
            if (currentKeyboardState.IsKeyDown(Keys.L)) // && !previousKeyboardState.IsKeyDown(Keys.L))
            {showFPS = !showFPS;}

            if (currentKeyboardState.IsKeyDown(Keys.OemPlus))
                gridCellSize += 2;
            if (currentKeyboardState.IsKeyDown(Keys.OemMinus) && gridCellSize > 2)
                gridCellSize -= 2;

            selectedCell = new Point(
                currentMouseState.X / gridCellSize,
                currentMouseState.Y / gridCellSize
            );
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            DrawGrid();
            if (showFPS)
            {   spriteBatch.DrawString(font, "DEBUG", new Vector2(10,10), Color.White);
                spriteBatch.DrawString(font, $"FPS: {fps.fpsCounter(gameTime)}\n" + $"Co-ordinates: {selectedCell}", new Vector2(10, 30), Color.White);
                DrawDebugInfo();}

            spriteBatch.End();
            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            spriteBatch.Dispose();
        }
    

        private void DrawGrid()
        {
            int screenWidth = GraphicsDevice.Viewport.Width;
            int screenHeight = GraphicsDevice.Viewport.Height;

            // Draw vertical lines
            for (int x = 0; x < screenWidth; x += gridCellSize)
            {
                spriteBatch.DrawLine(new Vector2(x, 0), new Vector2(x, screenHeight), gridColor);
            }

            // Draw horizontal lines
            for (int y = 0; y < screenHeight; y += gridCellSize)
            {
                spriteBatch.DrawLine(new Vector2(0, y), new Vector2(screenWidth, y), gridColor);
            }

            // Highlight the selected cell
            Rectangle cellRect = new Rectangle(
                selectedCell.X * gridCellSize,
                selectedCell.Y * gridCellSize,
                gridCellSize,
                gridCellSize
            );

            spriteBatch.DrawRectangle(cellRect, Color.Yellow * 0.5f);

        }

        private void DrawDebugInfo()
        {
            var totalMemory = GC.GetTotalMemory(false) / 1024.0 / 1024.0; // Convert to MB
            var process = Process.GetCurrentProcess();
            var ramUsage = process.WorkingSet64 / 1024.0 / 1024.0; // Convert to MB
            string debugText =             $"Grid Size: {gridCellSize}px\n" +
                                           $"Memory Usage: {process.PrivateMemorySize64 / (1024 * 1024)} MB\n" +
                                           $"Max Memory Usage: {maxMemory}" + //$"/{totalMemory} MB"
                                           $"\nCPU Usage: {cpuUsage:F2}%";

            Vector2 position = new Vector2(10, 80);
            spriteBatch.DrawString(font, debugText, position, Color.White);
        }

        private void UpdateCpuUsage()
        {
            var process = Process.GetCurrentProcess();
            var currentCpuTime = process.TotalProcessorTime;

            var elapsedCpuTime = currentCpuTime - lastCpuTime;
            var elapsedRealTime = cpuStopwatch.Elapsed;

            if (elapsedRealTime.TotalMilliseconds > 0)
            {
                cpuUsage = (float)(elapsedCpuTime.TotalMilliseconds / (Environment.ProcessorCount * elapsedRealTime.TotalMilliseconds) * 100);
            }

            lastCpuTime = currentCpuTime;
            cpuStopwatch.Restart();
        }

    }

}