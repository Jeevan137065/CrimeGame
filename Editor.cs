using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using CrimeGame.Class_Code;

namespace CrimeGame
{
    public class EditorMode : Game
    {
        //Technical
        private SpriteBatch spriteBatch;
        private GraphicsDeviceManager graphics;
        private Stopwatch cpuStopwatch;
        private TimeSpan lastCpuTime;
        private float cpuUsage = 0f;
        private long maxMemory = 0;
        
        //Editor
        private int gridCellSize = 16; // Default grid cell size
        private Color gridColor = Color.Gray * 0.5f; // Semi-transparent grid lines
        private Point selectedCell = Point.Zero; // Selected cell on the grid
        private int[,] gridTiles;
        
        //Features
        private Camera camera;
        private Palette tilePalette;
        private FPS fps;
        private Stack<int[,]> undoStack;
        private Stack<int[,]> redoStack;
        
        //UI
        private bool showFPS;
        public SpriteFont font;

        public EditorMode()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            WindowDetail(this.Window);
            

            cpuStopwatch = Stopwatch.StartNew();
            lastCpuTime = Process.GetCurrentProcess().TotalProcessorTime;

        }

        protected override void Initialize()
        {
            base.Initialize();
            
            // Initialize editor-specific logic here
            fps = new FPS();
            camera = new Camera();
            
            int gridWidth = GraphicsDevice.Viewport.Width / gridCellSize;
            int gridHeight = GraphicsDevice.Viewport.Height / gridCellSize;
            gridTiles = new int[gridWidth, gridHeight];

            undoStack = new Stack<int[,]>();
            redoStack = new Stack<int[,]>();

        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Font");

            var grassTexture = new Texture2D(GraphicsDevice, 1, 1);
            grassTexture.SetData(new[] { Color.Green });

            var blueTexture = new Texture2D(GraphicsDevice, 1, 1);
            blueTexture.SetData(new[] { Color.Blue });

            var yellowTexture = new Texture2D(GraphicsDevice, 1, 1);
            yellowTexture.SetData(new[] { Color.Yellow });

            tilePalette = new Palette(new[] { grassTexture, blueTexture, yellowTexture });
        }

        protected override void Update(GameTime gameTime)
        {
            var currentKeyboardState = Keyboard.GetState();
            var currentMouseState = Mouse.GetState();
            var lastMouseState = Point.Zero; 
            
            lastMouseState = HandleInput(currentKeyboardState, currentMouseState,lastMouseState,gameTime);

            tilePalette.Update(currentMouseState);
            fps.Update(gameTime);
            UpdateCpuUsage();
            maxMemory = Math.Max(maxMemory, Process.GetCurrentProcess().PrivateMemorySize64 / (1024 * 1024)); // Convert to MB
            base.Update(gameTime);
        }

        private Point HandleInput(KeyboardState currentKeyboardState, MouseState currentMouseState, Point lastMouseState, GameTime gameTime)
        {

            //KEYBOARD
            if (currentKeyboardState.IsKeyDown(Keys.L))                                                     {   showFPS = !showFPS;}
            if (currentKeyboardState.IsKeyDown(Keys.OemPlus))                                               {   gridCellSize += 2;}
            if (currentKeyboardState.IsKeyDown(Keys.OemMinus) && gridCellSize > 2)                          {   gridCellSize -= 2;}
            if (currentKeyboardState.IsKeyDown(Keys.Z) && undoStack.Count > 0)                              {   Undo();}
            if (currentKeyboardState.IsKeyDown(Keys.Y) && redoStack.Count > 0)                              {   Redo();}

            //MOUSE
            selectedCell = new Point(
                currentMouseState.X / gridCellSize,
                currentMouseState.Y / gridCellSize); 
            float targetZoom = camera.Zoom;
            float mouseScroll = currentMouseState.ScrollWheelValue;
            float scrollDelta = (mouseScroll - lastMouseState.Y) / 120; // Adjust zoom sensitivity
            if (scrollDelta != 0)
            {
                targetZoom += scrollDelta * 0.05f;
                targetZoom = MathHelper.Clamp(targetZoom, 0.5f, 2.0f);
            }
            if (mouseScroll > 0) // Zoom in
            {
                camera.SmoothZoom(camera.Zoom + 0.1f, (float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            else if (mouseScroll < 0) // Zoom out
            {
                camera.SmoothZoom(camera.Zoom - 0.1f, (float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            if (currentMouseState.RightButton == ButtonState.Pressed)   {   var delta = currentMouseState.Position.ToVector2(); }
            if (currentMouseState.MiddleButton == ButtonState.Pressed)  {   var delta = currentMouseState.Position.ToVector2() - lastMouseState.ToVector2();camera.Pan(delta);}
            
            return lastMouseState = currentMouseState.Position;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            //Main Pass
            spriteBatch.Begin(transformMatrix: camera.GetTransformMatrix());
                    DrawGrid();
            spriteBatch.End();
            //SecondaryPass
            spriteBatch.Begin();        
                tilePalette.Draw(spriteBatch,GraphicsDevice);
                if (showFPS){   
                    spriteBatch.DrawString(font, "DEBUG", new Vector2(10,10), Color.White);
                    spriteBatch.DrawString(font, $"FPS: {fps.fpsCounter(gameTime)}\n" + $"Co-ordinates: {selectedCell}", new Vector2(10, 30), Color.White);
                    DrawDebugInfo();}
            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void WindowDetail(GameWindow window)
        {
            // Configure the editor window
            Window.Title = "Level Editor";
            Window.AllowUserResizing = true;
            graphics.PreferredBackBufferWidth = 1280; // Editor window width
            graphics.PreferredBackBufferHeight = 720; // Editor window height
            IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false; // Disables VSync
            graphics.ApplyChanges();
        }
        protected override void UnloadContent()
        {
            spriteBatch.Dispose();
        }
        private bool IsValidCell(Point cell)
        {
            return cell.X >= 0 && cell.Y >= 0 && cell.X < gridTiles.GetLength(0) && cell.Y < gridTiles.GetLength(1);
        }
        private void SaveStateForUndo()
        {
            // Clone gridTiles to save state
            undoStack.Push((int[,])gridTiles.Clone());
            redoStack.Clear(); // Clear redo history when making new changes
        }
        private void Undo()
        {
            if (undoStack.Count > 0)
            {
                redoStack.Push((int[,])gridTiles.Clone());
                gridTiles = undoStack.Pop();
            }
        }
        private void Redo()
        {
            if (redoStack.Count > 0)
            {
                undoStack.Push((int[,])gridTiles.Clone());
                gridTiles = redoStack.Pop();
            }
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

            spriteBatch.DrawRectangle(cellRect, Color.Yellow * 0.8f);

        }
        private void DrawDebugInfo()
        {
            var totalMemory = GC.GetTotalMemory(false) / 1024.0 / 1024.0; // Convert to MB
            var process = Process.GetCurrentProcess();
            var ramUsage = process.WorkingSet64 / 1024.0 / 1024.0; // Convert to MB
            string debugText = $"Grid Size: {gridCellSize}px\n" +
                                           $"Memory Usage: {process.PrivateMemorySize64 / (1024 * 1024)} MB\n" +
                                           $"Max Memory Usage: {maxMemory}" + //$"/{totalMemory} MB"
                                           $"\nCPU Usage: {cpuUsage:F2}%" +
                                           $"\nViewport resolution : {GraphicsDevice.Viewport.Width}x{GraphicsDevice.Viewport.Height}" +
                                           $"\nWindow resolution : {graphics.PreferredBackBufferWidth}x{graphics.PreferredBackBufferHeight}";
                                           

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