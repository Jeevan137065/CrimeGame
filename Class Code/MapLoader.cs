using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace CrimeGame
{
    public class MapLoader
    {
        private Dictionary<int, Texture2D> tileTextures;
        private int tileWidth;
        private int tileHeight;
        private int mapWidth;
        private int mapHeight;
        private List<CustomLayer> mapLayers;

        public MapLoader()
        {
            tileTextures = new Dictionary<int, Texture2D>();
            mapLayers = new List<CustomLayer>();
        }

        public void LoadMap(GraphicsDevice graphicsDevice, string mapPath, string texturePath)
        {
            try
            {
                if (mapPath.EndsWith(".tmx"))
                {
                    LoadTmxMap(graphicsDevice, mapPath, texturePath);
                }
                else if (mapPath.EndsWith(".json"))
                {
                    LoadJsonMap(graphicsDevice, mapPath, texturePath);
                }
                else if (mapPath.EndsWith(".csv"))
                {
                    LoadCsvMap(graphicsDevice, mapPath, texturePath);
                }
                else
                {
                    throw new NotSupportedException("Unsupported map format: " + mapPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading map: {ex.Message}");
                throw;
            }
        }

        private void LoadTmxMap(GraphicsDevice graphicsDevice, string tmxPath, string texturePath)
        {
            try
            {
                TiledSharp.TmxMap map = new TiledSharp.TmxMap(tmxPath);

                tileWidth = map.TileWidth;
                tileHeight = map.TileHeight;
                mapWidth = map.Width;
                mapHeight = map.Height;
                foreach (var tileset in map.Tilesets)
                {
                    if (string.IsNullOrEmpty(texturePath))
                    {
                        throw new ArgumentException("Texture path cannot be null or empty.");
                    }

                    if (string.IsNullOrEmpty(tileset.Image.Source))
                    {
                        throw new InvalidOperationException($"Tileset image source is null or empty for tileset starting at GID {tileset.FirstGid}.");
                    }

                    string textureFile = Path.Combine(texturePath, tileset.Image.Source);
                    if (!File.Exists(textureFile))
                    {
                        throw new FileNotFoundException($"Tileset texture not found: {textureFile}");
                    }

                    using (FileStream fileStream = new FileStream(textureFile, FileMode.Open))
                    {
                        tileTextures[(int)tileset.FirstGid] = Texture2D.FromStream(graphicsDevice, fileStream);
                    }
                }

                foreach (var layer in map.Layers)
                {
                    var customLayer = new CustomLayer
                    {
                        Name = layer.Name,
                        Tiles = new int[mapWidth * mapHeight]
                    };

                    for (int y = 0; y < mapHeight; y++)
                    {
                        for (int x = 0; x < mapWidth; x++)
                        {
                            int tileIndex = x + y * mapWidth;
                            customLayer.Tiles[tileIndex] = layer.Tiles[tileIndex].Gid;
                        }
                    }

                    mapLayers.Add(customLayer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading TMX map: {ex.Message}");
                throw;
            }
        }

        public void LoadJsonMap(GraphicsDevice graphicsDevice, string jsonPath, string texturePath)
        {
            string jsonData = File.ReadAllText(jsonPath);
            var jsonMap = JsonConvert.DeserializeObject<JsonMap>(jsonData);

            tileWidth = jsonMap.TileWidth;
            tileHeight = jsonMap.TileHeight;
            mapWidth = jsonMap.Width;
            mapHeight = jsonMap.Height;

            foreach (var tileset in jsonMap.Tilesets)
            {
                string tilesetImage = tileset.Image;

                // Handle external tilesets
                if (string.IsNullOrEmpty(tileset.Image) && !string.IsNullOrEmpty(tileset.Image))
                {
                    string tsxPath = Path.Combine(jsonPath, tileset.Image);
                    if (!File.Exists(tsxPath))
                    {
                        throw new FileNotFoundException($"External tileset file not found: {tsxPath}");
                    }

                    // Parse external .tsx file
                    var externalTilesetXml = System.Xml.Linq.XDocument.Load(tsxPath);
                    var externalTileset = new TiledSharp.TmxTileset(externalTilesetXml.Root, null);

                    tilesetImage = externalTileset.Image.Source;
                }

                if (string.IsNullOrEmpty(tilesetImage))
                {
                    throw new InvalidOperationException($"Tileset image is null or empty for tileset starting at GID {tileset.FirstGid}.");
                }

                // Load tileset texture
                //string textureFile = Path.Combine(texturePath, null);
                
                //if (!File.Exists(textureFile))
                //{throw new FileNotFoundException($"Tileset texture not found: {textureFile}");}

                using (FileStream fileStream = new FileStream(texturePath, FileMode.Open))
                {
                    tileTextures[tileset.FirstGid] = Texture2D.FromStream(graphicsDevice, fileStream);
                }
            }

            foreach (var layer in jsonMap.Layers)
            {
                var customLayer = new CustomLayer
                {
                    Name = layer.Name,
                    Tiles = layer.Data ?? new int[mapWidth * mapHeight]
                };

                mapLayers.Add(customLayer);
            }
        }

        private void LoadCsvMap(GraphicsDevice graphicsDevice, string csvPath, string texturePath)
        {
            try
            {
                if (!File.Exists(csvPath))
                {
                    throw new FileNotFoundException($"CSV map file not found: {csvPath}");
                }

                string[] csvLines = File.ReadAllLines(csvPath);
                tileWidth = 32; // Assuming default size, adjust as needed
                tileHeight = 32;
                mapWidth = csvLines[0].Split(',').Length;
                mapHeight = csvLines.Length;

                var customLayer = new CustomLayer
                {
                    Name = "CSV Layer",
                    Tiles = new int[mapWidth * mapHeight]
                };

                for (int y = 0; y < mapHeight; y++)
                {
                    string[] row = csvLines[y].Split(',');
                    for (int x = 0; x < mapWidth; x++)
                    {
                        if (!int.TryParse(row[x], out int tileId))
                        {
                            throw new FormatException($"Invalid tile ID in CSV file at line {y + 1}, column {x + 1}");
                        }

                        customLayer.Tiles[x + y * mapWidth] = tileId;
                    }
                }

                mapLayers.Add(customLayer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading CSV map: {ex.Message}");
                throw;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            try
            {
                foreach (var layer in mapLayers)
                {
                    for (int y = 0; y < mapHeight; y++)
                    {
                        for (int x = 0; x < mapWidth; x++)
                        {
                            int tileId = layer.Tiles[x + y * mapWidth];
                            if (tileId == 0) continue; // Empty tile

                            Texture2D texture = null;
                            int textureOffset = 0;

                            foreach (var gid in tileTextures.Keys)
                            {
                                if (tileId >= gid)
                                {
                                    texture = tileTextures[gid];
                                    textureOffset = gid;
                                }
                            }

                            if (texture != null)
                            {
                                int tileIndex = tileId - textureOffset;
                                int tilesPerRow = texture.Width / tileWidth;

                                Rectangle sourceRectangle = new Rectangle(
                                    (tileIndex % tilesPerRow) * tileWidth,
                                    (tileIndex / tilesPerRow) * tileHeight,
                                    tileWidth,
                                    tileHeight);

                                Vector2 position = new Vector2(x * tileWidth, y * tileHeight);
                                spriteBatch.Draw(texture, position, sourceRectangle, Color.White);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error drawing map: {ex.Message}");
                throw;
            }
        }
    }
    public class CustomLayer
    {
        public string Name { get; set; }
        public int[] Tiles { get; set; }
    }

    public class JsonMap
    {
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<JsonTileset> Tilesets { get; set; }
        public List<JsonLayer> Layers { get; set; }
    }

    public class JsonTileset
    {
        public int FirstGid { get; set; }
        public string Image { get; set; }
    }

    public class JsonLayer
    {
        public string Name { get; set; }
        public int[] Data { get; set; }
    }
}
