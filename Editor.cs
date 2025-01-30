using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using MonoGame.Extended.VectorDraw;
using MonoGame.Extended;


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
        private bool showFPS;
        //private bool isImport = false;

        //Editor
        private int CellSize = 32;                      // Cell Size
        private float targetZoom;
        private Color gridColor = Color.DarkGray;
        private Point mouseGrid = Point.Zero;           // Grid Co ordinates
        private Point mousePos = Point.Zero;            // Absoulute Mouse Position
        private Point currentCell = Point.Zero;         // Cell Co ordinates
        private Vector2 palettePos;                     // Palette Box Location
        private Rectangle mouseRect = Rectangle.Empty;   // Cell Highlighter Rectangle

        //private bool isPlacingTile = false, showUI = true;
        private int selectedTileID = 0;

        //Features
        private Canvas canvas;                          
        private AbsoluteBound absoluteBound;
        private ColorSelector colorSelector;
        private Palette palette;
        private TileSet tileSet;
        private TileManager tileManager;
        public Texture2D buttonTexture;
        private Camera camera;
        private FPS fps;
        //UI
        private Toolbar toolbar;
        public SpriteFont font;

        public EditorMode()
        {   graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            WindowDetail(this.Window);
            cpuStopwatch = Stopwatch.StartNew();
            lastCpuTime = Process.GetCurrentProcess().TotalProcessorTime;}

        protected override void Initialize()
        {
            base.Initialize();
            canvas = new Canvas(25, 20, CellSize);
            absoluteBound = new AbsoluteBound(0.5f, 1.5f);
            colorSelector = new ColorSelector(GraphicsDevice);
            palettePos = new Vector2(20,640);
            tileSet = new TileSet(GraphicsDevice, CellSize);
            palette = new Palette(tileSet);
            tileManager = new TileManager(tileSet, CellSize);
            toolbar = new Toolbar(GraphicsDevice);
            camera = new Camera();
            fps = new FPS();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Font");
            tileSet.LoadTileSet(Content, "Basi");
            palette.InitializePalette();
            
        }

        protected override void Update(GameTime gameTime)
        {
            var currentKeyboardState = Keyboard.GetState();
            var currentMouseState = Mouse.GetState();
            var lastMouseState = Point.Zero;

            // User Update
            lastMouseState = HandleMouse(currentMouseState,lastMouseState,gameTime);
            HandleKeyBoard(currentKeyboardState, gameTime);
            // Object Update
            currentCell = canvas.GetCellAtMousePosition(currentMouseState.Position.ToVector2(), Matrix.Identity);
            canvas.CenterCanvas(GraphicsDevice.Viewport);
            palette.Update(currentMouseState);
            toolbar.Update(currentMouseState, currentKeyboardState, gameTime);
            //Techincal Update
            maxMemory = Math.Max(maxMemory, Process.GetCurrentProcess().PrivateMemorySize64 / (1024 * 1024)); // Convert to MB
            fps.Update(gameTime);
            UpdateCpuUsage();
            base.Update(gameTime);
        }

        private Point HandleMouse(MouseState currentMouseState, Point lastMouseState, GameTime gameTime)
        {
            //MOUSE
            targetZoom = camera.Zoom;
            targetZoom = absoluteBound.Clamp(targetZoom);
            float mouseScroll = currentMouseState.ScrollWheelValue;
            mousePos = currentMouseState.Position;
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
            if (currentMouseState.LeftButton == ButtonState.Pressed && canvas.Bounds.Contains(currentMouseState.Position))
            {
                if (palette.isMouseOverPalette(currentMouseState.Position)) { selectedTileID = palette.GetSelectedTileID(); }
                if (selectedTileID != -1)
                {
                    Point cellPos = tileManager.GetCell(mousePos,camera);
                    tileManager.PlaceTile(cellPos, selectedTileID);
                }
            }
            
            if (currentMouseState.MiddleButton == ButtonState.Pressed)  {   var delta = currentMouseState.Position.ToVector2() - lastMouseState.ToVector2();camera.Pan(delta);}
            

            return lastMouseState = currentMouseState.Position;}
        private void HandleKeyBoard(KeyboardState currentKeyboardState, GameTime gameTime)
        {   //KEYBOARD
            if (currentKeyboardState.IsKeyDown(Keys.L))     {   showFPS = !showFPS;}
            if (currentKeyboardState.IsKeyDown(Keys.O))     {   colorSelector.Toggle(); }
            
        }

        protected override void Draw(GameTime gameTime)
        {   GraphicsDevice.Clear(Color.CornflowerBlue);
            //Main Pass
                spriteBatch.Begin(transformMatrix: camera.GetTransformMatrix());
                    DrawGrid();
                    tileManager.Draw(spriteBatch);
                spriteBatch.End();
            //SecondaryPass
                spriteBatch.Begin();
                    palette.Draw(spriteBatch);
                    toolbar.Draw(spriteBatch);
                if (showFPS){   
                    spriteBatch.DrawString(font, "DEBUG", new Vector2(10,10), Color.White);
                    spriteBatch.DrawString(font, $"FPS: {fps.fpsCounter(gameTime)}\n", new Vector2(10, 100), Color.White);
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
        private void DrawGrid()
        {
            int viewportWidth = GraphicsDevice.Viewport.Width;
            int viewportHeight = GraphicsDevice.Viewport.Height;

            // Center the canvas in the viewport
            int canvasOriginX = (viewportWidth - canvas.Bounds.Width) / 2;
            int canvasOriginY = (viewportHeight - canvas.Bounds.Height) / 2;

            // Correct bounds for grid rendering
            int gridStartX = canvasOriginX;
            int gridEndX = canvasOriginX + canvas.Bounds.Width;
            int gridStartY = canvasOriginY;
            int gridEndY = canvasOriginY + canvas.Bounds.Height;

            // Draw vertical grid lines
            for (int x = gridStartX; x <= gridEndX; x += CellSize)
            {
                spriteBatch.DrawLine(new Vector2(x, gridStartY), new Vector2(x, gridEndY), gridColor);
            }

            // Draw horizontal grid lines
            for (int y = gridStartY; y <= gridEndY; y += CellSize)
            {
                spriteBatch.DrawLine(new Vector2(gridStartX, y), new Vector2(gridEndX, y), gridColor);
            }

            // Determine which cell is currently selected
            if (canvas.Bounds.Contains(mousePos))
            {
                mouseGrid.X = (mousePos.X - canvasOriginX); mouseGrid.Y = (mousePos.Y - canvasOriginY);
                int cellX = mouseGrid.X/ CellSize;
                int cellY = mouseGrid.Y/ CellSize;
                // Calculate the cell rectangle
                mouseRect = new Rectangle(
                    canvasOriginX + cellX * CellSize,
                    canvasOriginY + cellY * CellSize,
                    CellSize,
                    CellSize
                );

                // Draw the highlight for the selected cell
                spriteBatch.DrawRectangle(mouseRect, Color.Black, 2);
                spriteBatch.FillRectangle(mouseRect, Color.Yellow * 0.8f);
            }
            else    {mouseGrid.X = 0; mouseGrid.Y = 0;}
            canvas.DrawCanvasBorder(spriteBatch, buttonTexture,Color.Black, 2);
        }

        private void DrawDebugInfo()
        {
            var totalMemory = GC.GetTotalMemory(false) / 1024.0 / 1024.0; // Convert to MB
            var process = Process.GetCurrentProcess();
            var ramUsage = process.WorkingSet64 / 1024.0 / 1024.0; // Convert to MB

            string debugInfo =  $"Canvas Bounds: {canvas.Bounds}\n" +
                                $"Absolute Bounds: Zoom [{absoluteBound.MinZoom}, {absoluteBound.MaxZoom}]\n" +
                                $"Absolute Mouse Position: {mousePos}\n" +
                                $"Relative Mouse Position : {mouseGrid}\n" +
                                $"Current Cell ID: [ {currentCell}]\n" +
                                $"Current Zoom: {camera.Zoom}\n\n";
            string debugText =  $"Grid Size: {CellSize}px\n" +
                                $"Memory Usage: {process.PrivateMemorySize64 / (1024 * 1024)} MB\n" +
                                $"Max Memory Usage: {maxMemory}" + //$"/{totalMemory} MB"
                                $"\nCPU Usage: {cpuUsage:F2}%" +
                                $"\nViewport resolution : {GraphicsDevice.Viewport.Width}x{GraphicsDevice.Viewport.Height}" +
                                $"\nWindow resolution : {graphics.PreferredBackBufferWidth}x{graphics.PreferredBackBufferHeight}";
            Vector2 position = new Vector2(10, 140);
            spriteBatch.DrawString(font, debugInfo + debugText, position, Color.White);
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