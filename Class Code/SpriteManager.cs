using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled.Renderers;
using System;
using System.Collections.Generic;

namespace CrimeGame
{
    public class Canvas {
        public Rectangle Bounds { get; private set; }
        public int CellSize { get; private set; }
        public int WidthInCells { get; private set; }
        public int HeightInCells { get; private set; }
        
        public Canvas(int width,int height, int cellSize) {
            WidthInCells = width;
            HeightInCells = height;
            CellSize = cellSize;
            Bounds = new Rectangle(0, 0, WidthInCells * CellSize, HeightInCells * CellSize);
        }

        public void CenterCanvas(Viewport viewport) { 
            int screenWidth = viewport.Width;
            int screenHeight = viewport.Height;

            Bounds = new Rectangle(
                (screenWidth - Bounds.Width) / 2, (screenHeight - Bounds.Height) / 2,
                Bounds.Width, Bounds.Height
            );
        }
        public void DrawCanvasBorder(SpriteBatch spriteBatch, Texture2D WhiteTexture, Color borderColor, int thickness = 4){
            spriteBatch.Draw(WhiteTexture, new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, thickness), borderColor); // Top
            spriteBatch.Draw(WhiteTexture, new Rectangle(Bounds.X, Bounds.Y, thickness, Bounds.Height), borderColor); // Left
            spriteBatch.Draw(WhiteTexture, new Rectangle(Bounds.X, Bounds.Y + Bounds.Height - thickness, Bounds.Width, thickness), borderColor); // Bottom
            spriteBatch.Draw(WhiteTexture, new Rectangle(Bounds.X + Bounds.Width - thickness, Bounds.Y, thickness, Bounds.Height), borderColor); // Right
        }

        public Point GetCellAtMousePosition(Vector2 mousePosition, Matrix transformMatrix)
        {
            Vector2 transformedMouse = Vector2.Transform(mousePosition, Matrix.Invert(transformMatrix));
            if (!Bounds.Contains(transformedMouse))
                return Point.Zero; // Mouse outside canvas

            int cellX = (int)((transformedMouse.X - Bounds.X) / CellSize);
            int cellY = (int)((transformedMouse.Y - Bounds.Y) / CellSize);

            return new Point(cellX, cellY);
        }

    }
    public class AbsoluteBound {
        public float MinZoom { get; private set; }
        public float MaxZoom { get; private set; }
        public AbsoluteBound(float minZoom, float maxZoom)
        {
            MinZoom = minZoom;
            MaxZoom = maxZoom;
        }
        public float Clamp(float value)
        {
            return MathHelper.Clamp(value, MinZoom, MaxZoom);
        }

    }
    public class Tile
    {   public int TileID { get; set; } = -1; 
        public Texture2D Texture { get; set; } 
        public Tile(int tileID, Texture2D texture)
        {
            TileID = tileID;
            Texture = texture;
        }
    }
    public class TileSet
    {   private GraphicsDevice GraphicsDevice;
        //public Texture2D TileTexture;
        public Dictionary<int, Texture2D> Atlas;
        public int tileSize,tileCount;

        public TileSet(GraphicsDevice graphicsDevice, int cellSize) {
            GraphicsDevice = graphicsDevice;
            tileSize = cellSize;
            Atlas = new Dictionary<int,Texture2D>();
            tileCount = 0;
        }
        public void LoadTileSet()
        {
            //TileTexture = Content.Load<Texture2D>(path);
            //TileTexture = texture;
            //SliceIntoAtlas(texture);
        }

        public void SliceIntoAtlas(Texture2D TileTexture) {
            if (TileTexture == null)
            {
                throw new Exception("Error: Attempted to slice an uninitialized tileset texture.");
            }
            int tileX = TileTexture.Width / tileSize;
            int tileY = TileTexture.Height / tileSize;

            for(int y = 0; y < tileY; y++)
            {
                for (int x = 0; x < tileX; x++)
                {
                    Rectangle sourceRect = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);
                    Texture2D tileTexture = new Texture2D(GraphicsDevice, tileSize, tileSize);
                    Color[] data = new Color[tileSize * tileSize];
                    TileTexture.GetData(0, sourceRect, data, 0, data.Length);
                    bool isEmpty = true;
                    foreach (var pixel in data)
                    {   if (pixel.A > 0)
                        {   isEmpty = false;
                            break;}}
                    if (!isEmpty)
                    {
                        tileTexture.SetData(data);
                        Atlas[tileCount++] = tileTexture;
                    }

                }
            }
        }
        public Texture2D GetTile( int tileID)
        {
            return Atlas.ContainsKey(tileID) ? Atlas[tileID] : null;
        }
    
        public void DrawTileSet(SpriteBatch spriteBatch, Vector2 position) {
            int xOffset = (int)position.X, yOffset = (int)position.Y;
            int index = 0;
            foreach (var tile in Atlas.Values)
            {
                spriteBatch.Draw(tile, new Vector2(xOffset,yOffset * (tileSize+5)),Color.White);
                index++;
            }

        }
    }
    public class Palette
    {
        private List<int> paletteBox; 
        private List<int> tileBar;
        private TileSet tileSet;
        private int selectedTileID;
        private int tileSize;
        private Rectangle tileBarBounds;
        public Palette()
        {
            paletteBox = new List<int> { -1, -1, -1, -1 };
            tileBarBounds = new Rectangle(10,500, 300, 64);
        }
        public void LoadTileSet(TileSet tileSet)
        {
            this.tileSet = tileSet;
            tileBar = new List<int>(tileSet.Atlas.Keys);
            tileSize = tileSet.tileSize;
            InitializePalette();
        }
        public void InitializePalette()
        {
            for (int i = 0; i < paletteBox.Count; i++)
            {
                paletteBox[i] = i;
            }
        }
        public void Update(MouseState moiseState)
        {
            if (moiseState.LeftButton == ButtonState.Pressed && isMouseOverPalette(moiseState.Position))
            { selectedTileID = selectTileFromPalette(moiseState.Position); }
        }
        public int selectTileFromPalette(Point mousePosition)
            {
                int index = (mousePosition.X - tileBarBounds.X) / tileSize;
                return (index >= 0 && index < paletteBox.Count ? paletteBox[index] : -1);}
        public bool isMouseOverPalette(Point mousePosition)
        {   return tileBarBounds.Contains(mousePosition);}
        public int GetSelectedTileID() { return selectedTileID; }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawRectangle(tileBarBounds, Color.Gray * 0.8f, 2);
            for (int i = 0; i < paletteBox.Count; i++)
            {
                Texture2D tileTexture = tileSet.GetTile(paletteBox[i]);
                if (tileTexture != null)
                {
                    spriteBatch.Draw(tileTexture, new Vector2(tileBarBounds.X + i * tileSize, tileBarBounds.Y), Color.White);
                }
            }
        }

    }
    public class ColorSelector
    {
        public List<Color> Colors { get; private set; }
        private int _selectedColorIndex;
        private Texture2D whiteTexture;
        private bool isVisible = false;

        public void Toggle() { isVisible = !isVisible; }
        public ColorSelector(GraphicsDevice graphicsDevice)
        {
            Colors = new List<Color>
        {
            Color.Red, Color.Green, Color.Blue, Color.Yellow,
            Color.Cyan, Color.Magenta, Color.White, Color.Black
        };
            _selectedColorIndex = 0;
            whiteTexture = new Texture2D(graphicsDevice, 1, 1);
            whiteTexture.SetData(new[] { Color.White });

        }

        public Color GetSelectedColor()
        {
            return Colors[_selectedColorIndex];
        }

        public void CycleColors()
        {
            _selectedColorIndex = (_selectedColorIndex + 1) % Colors.Count;
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle position)
        {
            for (int i = 0; i < Colors.Count; i++)
            {
                spriteBatch.Draw(
                    whiteTexture,
                    new Rectangle(position.X + i * 20, position.Y, 20, 20),
                    Colors[i]
                );

                if (i == _selectedColorIndex)
                {
                    spriteBatch.Draw(whiteTexture, new Rectangle(position.X + i * 20, position.Y, 20, 20), Color.Black * 0.5f);
                }
            }
        }
    }

    public class TileManager
    {
        private Dictionary<Point, int> grid;
        private TileSet TileSet;
        private int CellSize;
        public TileLayer CurrentLayer { get; set; }
        public TileManager(int cellSize) {
            CellSize = cellSize;
            grid = new Dictionary<Point, int>();
        }
        public void LoadTileSet(TileSet tileSet) { TileSet = tileSet; }
        public void PlaceTile(Point cell, int tileID)
        {   if (!grid.ContainsKey(cell) && !CurrentLayer.IsLocked) { grid[cell] = tileID; }}
        public void RemoveTile(Point cell)
        {if (grid.ContainsKey(cell) && !CurrentLayer.IsLocked) { grid.Remove(cell); }}
        public Point GetCell(Point mousePosition, Camera camera) { 
            Vector2 worldPos = Vector2.Transform(mousePosition.ToVector2(), Matrix.Invert(camera.GetTransformMatrix()));
            return new Point((int)worldPos.X / CellSize, (int)worldPos.Y / CellSize);
        }
        
        public void Draw(SpriteBatch spriteBatch) { 
            foreach (var tile in grid)
            {
                Texture2D tileTexture = TileSet.GetTile(tile.Value);
                if (tileTexture != null)
                {
                    spriteBatch.Draw(tileTexture, new Rectangle(tile.Key.X * CellSize, tile.Key.Y * CellSize, CellSize, CellSize),Color.White);
                }
            }
        }
    }

}