using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System.Collections.Generic;
using System.Text;

namespace CrimeGame
{
    public abstract class Layer
    {
        public string Name { get; set; }
        public bool IsVisible { get; set; } = true;
        public bool IsLocked { get; set; } = false;

        public Layer(string name)
        {
            Name = name;
        }

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch spriteBatch);
    }

    public class TileLayer : Layer
    {
        private Dictionary<Point, int> grid;
        private TileSet tileSet;
        private int cellSize;

        public TileLayer(string name, int cellSize, TileSet tileSet) : base(name)
        {
            this.cellSize = cellSize;
            this.tileSet = tileSet;
            grid = new Dictionary<Point, int>();
        }

        public void PlaceTile(Point cell, int tileID)
        {
            if (!grid.ContainsKey(cell))
                grid[cell] = tileID;
        }

        public void RemoveTile(Point cell)
        {
            if (grid.ContainsKey(cell))
                grid.Remove(cell);
        }

        // Returns the underlying grid data for debug/info purposes.
        public Dictionary<Point, int> GetTileGrid()
        {
            return grid;
        }

        public override void Update(GameTime gameTime)
        {
            // For now, nothing to update inside a tile layer.
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (var tile in grid)
            {
                Texture2D tileTexture = tileSet.GetTile(tile.Value);
                if (tileTexture != null)
                {
                    Rectangle dest = new Rectangle(tile.Key.X * cellSize, tile.Key.Y * cellSize, cellSize, cellSize);
                    spriteBatch.Draw(tileTexture, dest, Color.White);
                }
            }
        }
    }
    public class LayerTab
    {
        private LayerManager layerManager;
        private TileManager tileManager;
        private SpriteFont font;
        private Texture2D background;
        private Rectangle tabArea;
        private int tabHeight = 30;
        private int tabWidth = 150;
        private int margin = 5;
        private GraphicsDevice graphicsDevice;

        public LayerTab(GraphicsDevice graphicsDevice, LayerManager layerManager, TileManager tileManager)
        {
            this.graphicsDevice = graphicsDevice;
            this.layerManager = layerManager;
            this.tileManager = tileManager;

            // Create a simple background texture for the tabs.
            background = new Texture2D(graphicsDevice, 1, 1);
            background.SetData(new[] { Color.DarkGray });

            // Initially update the tab area position.
            UpdateTabArea();
        }

        public void LoadTabArea(SpriteFont font)
        {
            this.font = font;
        }

        // Update tab area based on the current viewport and number of layers.
        private void UpdateTabArea()
        {
            int totalHeight = layerManager.Layers.Count * tabHeight;
            tabArea = new Rectangle(
                graphicsDevice.Viewport.Width - tabWidth - margin,
                graphicsDevice.Viewport.Height - totalHeight - margin,
                tabWidth,
                totalHeight
            );
        }

        public void Update(GameTime gameTime)
        {
            UpdateTabArea(); // Adjust if window size changes.
            MouseState mouseState = Mouse.GetState();

            // Check if any layer tab is clicked.
            for (int i = 0; i < layerManager.Layers.Count; i++)
            {
                Rectangle layerRect = new Rectangle(tabArea.X, tabArea.Y + i * tabHeight, tabWidth, tabHeight);
                if (layerRect.Contains(mouseState.Position) && mouseState.LeftButton == ButtonState.Pressed)
                {
                    // Update current layer in TileManager and active index in LayerManager.
                    tileManager.CurrentLayer = layerManager.Layers[i] as TileLayer;
                    layerManager.ActiveLayerIndex = i;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the background for the entire tab area.
            spriteBatch.Draw(background, tabArea, Color.Gray * 0.5f);

            // Draw each individual layer tab.
            for (int i = 0; i < layerManager.Layers.Count; i++)
            {
                Layer layer = layerManager.Layers[i];
                Rectangle layerRect = new Rectangle(tabArea.X, tabArea.Y + i * tabHeight, tabWidth, tabHeight);

                // Highlight the currently selected layer.
                Color textColor = (tileManager.CurrentLayer == layer) ? Color.Yellow : Color.White;
                spriteBatch.DrawString(font, layer.Name, new Vector2(layerRect.X + margin, layerRect.Y + margin), textColor);

                // Optionally draw a border for each tab.
                spriteBatch.DrawRectangle(layerRect, Color.Black, 1);
            }
        }
    }
    public class LayerManager
    {
        public List<Layer> Layers { get; private set; }
        public int ActiveLayerIndex { get; set; } = 0;

        public LayerManager(TileSet tileSet, int cellSize)
        {
            Layers = new List<Layer>();


            // Default layers:
            // 1. Diffuse layer (main visuals)
            Layers.Add(new TileLayer("Diffuse", cellSize, tileSet));
            // 2. Normal layer (for lighting effects)
            Layers.Add(new TileLayer("Normal", cellSize, tileSet));
            // 3. Other layer (collision, triggers, etc.)
            Layers.Add(new TileLayer("Other", cellSize, tileSet));
        }
        public TileLayer GetActiveTileLayer()
        {
            return Layers[ActiveLayerIndex] as TileLayer;
        }
        public void AddDiffuseLayer(TileSet tileSet, int cellSize)
        {
            // When adding a diffuse layer, you might want to automatically create a matching normal layer.
            Layers.Add(new TileLayer("Diffuse", cellSize, tileSet));
            Layers.Add(new TileLayer("Normal", cellSize, tileSet));
        }

        public void Update(GameTime gameTime)
        {
            foreach (var layer in Layers)
            {
                if (layer.IsVisible && !layer.IsLocked)
                {
                    layer.Update(gameTime);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var layer in Layers)
            {
                if (layer.IsVisible)
                {
                    layer.Draw(spriteBatch);
                }
            }
        }

        public string GetLayerDebugInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Layer Info:");
            for (int i = 0; i < Layers.Count; i++)
            {
                var layer = Layers[i];
                sb.AppendLine($"[{i}] {layer.Name} - Visible: {layer.IsVisible}, Locked: {layer.IsLocked}");
            }
            return sb.ToString();
        }
    }
}