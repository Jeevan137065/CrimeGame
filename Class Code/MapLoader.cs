using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CrimeGame.MapSystem
{
        public class MapLoader
        {
            public string MapFilePath { get; private set; }
            public LDtkMap CurrentMap { get; private set; }

            // Constructor
            public MapLoader(string mapFilePath)
            {
                if (string.IsNullOrEmpty(mapFilePath))
                    throw new ArgumentException("Map file path cannot be null or empty.");

                MapFilePath = mapFilePath;
            }

            // Load the LDtk map from a file
            public void LoadMap()
            {
                if (!File.Exists(MapFilePath))
                    throw new FileNotFoundException($"Map file not found at path: {MapFilePath}");

                string mapJson = File.ReadAllText(MapFilePath);
                CurrentMap = JsonConvert.DeserializeObject<LDtkMap>(mapJson);

                if (CurrentMap == null)
                    throw new Exception("Failed to load map data.");
            }

            // Get tile data from a specific layer
            public List<TileData> GetTileData(string layerName)
            {
                if (CurrentMap == null)
                    throw new Exception("No map is currently loaded.");

                var layer = CurrentMap.GetLayerByName(layerName);
                if (layer == null)
                    throw new Exception($"Layer '{layerName}' not found in the map.");

                return layer.GetTiles();
            }

            // Get all entities in a level
            public List<EntityData> GetEntities(string levelName)
            {
                if (CurrentMap == null)
                    throw new Exception("No map is currently loaded.");

                var level = CurrentMap.GetLevelByName(levelName);
                if (level == null)
                    throw new Exception($"Level '{levelName}' not found in the map.");

                return level.Entities;
            }

            // Get collision data from an IntGrid layer
            public List<CollisionCell> GetCollisionData(string layerName)
            {
                if (CurrentMap == null)
                    throw new Exception("No map is currently loaded.");

                var layer = CurrentMap.GetLayerByName(layerName);
                if (layer == null || layer.Type != "IntGrid")
                    throw new Exception($"IntGrid layer '{layerName}' not found or invalid.");

                return layer.GetCollisionCells();
            }

            // Get connected levels
            public List<string> GetConnectedLevels(string levelName)
            {
                if (CurrentMap == null)
                    throw new Exception("No map is currently loaded.");

                var level = CurrentMap.GetLevelByName(levelName);
                if (level == null)
                    throw new Exception($"Level '{levelName}' not found in the map.");

                return level.ConnectedLevels;
            }

            // Get auto-layer tiles
            public List<TileData> GetAutoLayerTiles(string layerName)
            {
                if (CurrentMap == null)
                    throw new Exception("No map is currently loaded.");

                var layer = CurrentMap.GetLayerByName(layerName);
                if (layer == null || layer.Type != "AutoLayer")
                    throw new Exception($"AutoLayer '{layerName}' not found or invalid.");

                return layer.GetTiles();
            }

            // Debug map info
            public void PrintMapInfo()
            {
                if (CurrentMap == null)
                    throw new Exception("No map is currently loaded.");

                Console.WriteLine($"Map Name: {CurrentMap.Name}");
                Console.WriteLine($"Number of Levels: {CurrentMap.Levels.Count}");

                foreach (var level in CurrentMap.Levels)
                {
                    Console.WriteLine($"Level: {level.Name}, Width: {level.Width}, Height: {level.Height}");
                }
            }
        }

        // Mock classes for LDtk structures. Replace with actual structures in your project.
        public class LDtkMap
        {
            public string Name { get; set; }
            public List<LevelData> Levels { get; set; }

            public LevelData GetLevelByName(string name) => Levels.Find(level => level.Name == name);
            public LayerData GetLayerByName(string name) => Levels.SelectMany(level => level.Layers).FirstOrDefault(layer => layer.Name == name);
        }

        public class LevelData
        {
            public string Name { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public List<LayerData> Layers { get; set; }
            public List<EntityData> Entities { get; set; }
            public List<string> ConnectedLevels { get; set; }
        }

        public class LayerData
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public List<TileData> Tiles { get; set; }

            public List<CollisionCell> GetCollisionCells() => new List<CollisionCell>();
            public List<TileData> GetTiles() => Tiles;
        }

        public class EntityData
        {
            public string Name { get; set; }
            public Dictionary<string, object> Fields { get; set; }
        }

        public class TileData
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int TileId { get; set; }
        }

        public class CollisionCell
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Value { get; set; }
        }
    }


