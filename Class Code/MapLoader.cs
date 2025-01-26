using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CrimeGame.MapSystem
{
    public class MapLoader
    {
        public string MapFilePath { get; private set; }
        public TMXMap CurrentMap { get; private set; }

        // Constructor
        public MapLoader(string mapFilePath)
        {
            if (string.IsNullOrEmpty(mapFilePath))
                throw new ArgumentException("Map file path cannot be null or empty.");

            MapFilePath = mapFilePath;
        }

        // Load the TMX map from a file
        public void LoadMap()
        {
            if (!File.Exists(MapFilePath))
                throw new FileNotFoundException($"Map file not found at path: {MapFilePath}");

            XDocument mapDoc = XDocument.Load(MapFilePath);
            CurrentMap = TMXMap.FromXml(mapDoc);

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

        // Get object data from an object layer
        public List<MapObject> GetObjects(string objectGroupName)
        {
            if (CurrentMap == null)
                throw new Exception("No map is currently loaded.");

            var objectGroup = CurrentMap.GetObjectGroupByName(objectGroupName);
            if (objectGroup == null)
                throw new Exception($"Object group '{objectGroupName}' not found in the map.");

            return objectGroup.Objects;
        }

        // Debug map info
        public void PrintMapInfo()
        {
            if (CurrentMap == null)
                throw new Exception("No map is currently loaded.");

            Console.WriteLine($"Map Name: {CurrentMap.Name}");
            Console.WriteLine($"Map Size: {CurrentMap.Width}x{CurrentMap.Height} tiles");
            Console.WriteLine($"Tile Size: {CurrentMap.TileWidth}x{CurrentMap.TileHeight} pixels");

            foreach (var layer in CurrentMap.Layers)
            {
                Console.WriteLine($"Layer: {layer.Name}, Size: {layer.Width}x{layer.Height}");
            }

            foreach (var objectGroup in CurrentMap.ObjectGroups)
            {
                Console.WriteLine($"Object Group: {objectGroup.Name}, Objects: {objectGroup.Objects.Count}");
            }
        }
    }

    // TMX Map Structure
    public class TMXMap
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public List<TMXLayer> Layers { get; set; } = new List<TMXLayer>();
        public List<TMXObjectGroup> ObjectGroups { get; set; } = new List<TMXObjectGroup>();

        public static TMXMap FromXml(XDocument xml)
        {
            var mapElement = xml.Element("map");
            if (mapElement == null)
                throw new Exception("Invalid TMX map format.");

            var map = new TMXMap
            {
                Name = mapElement.Attribute("name")?.Value ?? "Unnamed Map",
                Width = int.Parse(mapElement.Attribute("width")?.Value ?? "0"),
                Height = int.Parse(mapElement.Attribute("height")?.Value ?? "0"),
                TileWidth = int.Parse(mapElement.Attribute("tilewidth")?.Value ?? "0"),
                TileHeight = int.Parse(mapElement.Attribute("tileheight")?.Value ?? "0")
            };

            foreach (var layerElement in mapElement.Elements("layer"))
            {
                map.Layers.Add(TMXLayer.FromXml(layerElement));
            }

            foreach (var objectGroupElement in mapElement.Elements("objectgroup"))
            {
                map.ObjectGroups.Add(TMXObjectGroup.FromXml(objectGroupElement));
            }

            return map;
        }

        public TMXLayer GetLayerByName(string name) => Layers.FirstOrDefault(layer => layer.Name == name);
        public TMXObjectGroup GetObjectGroupByName(string name) => ObjectGroups.FirstOrDefault(group => group.Name == name);
    }

    public class TMXLayer
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<TileData> Tiles { get; set; } = new List<TileData>();

        public static TMXLayer FromXml(XElement layerElement)
        {
            var layer = new TMXLayer
            {
                Name = layerElement.Attribute("name")?.Value ?? "Unnamed Layer",
                Width = int.Parse(layerElement.Attribute("width")?.Value ?? "0"),
                Height = int.Parse(layerElement.Attribute("height")?.Value ?? "0")
            };

            var dataElement = layerElement.Element("data");
            if (dataElement != null && dataElement.Value != null)
            {
                var tileIds = dataElement.Value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                               .Select(id => int.Parse(id.Trim())).ToList();

                for (int y = 0; y < layer.Height; y++)
                {
                    for (int x = 0; x < layer.Width; x++)
                    {
                        int index = y * layer.Width + x;
                        layer.Tiles.Add(new TileData { X = x, Y = y, TileId = tileIds[index] });
                    }
                }
            }

            return layer;
        }

        public List<TileData> GetTiles() => Tiles;
    }

    public class TMXObjectGroup
    {
        public string Name { get; set; }
        public List<MapObject> Objects { get; set; } = new List<MapObject>();

        public static TMXObjectGroup FromXml(XElement objectGroupElement)
        {
            var group = new TMXObjectGroup
            {
                Name = objectGroupElement.Attribute("name")?.Value ?? "Unnamed Object Group"
            };

            foreach (var objectElement in objectGroupElement.Elements("object"))
            {
                group.Objects.Add(MapObject.FromXml(objectElement));
            }

            return group;
        }
    }

    public class MapObject
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        public static MapObject FromXml(XElement objectElement)
        {
            var mapObject = new MapObject
            {
                Name = objectElement.Attribute("name")?.Value ?? "Unnamed Object",
                X = int.Parse(objectElement.Attribute("x")?.Value ?? "0"),
                Y = int.Parse(objectElement.Attribute("y")?.Value ?? "0"),
                Width = int.Parse(objectElement.Attribute("width")?.Value ?? "0"),
                Height = int.Parse(objectElement.Attribute("height")?.Value ?? "0")
            };

            foreach (var propertyElement in objectElement.Element("properties")?.Elements("property") ?? Enumerable.Empty<XElement>())
            {
                var propertyName = propertyElement.Attribute("name")?.Value;
                var propertyValue = propertyElement.Attribute("value")?.Value;
                if (propertyName != null && propertyValue != null)
                {
                    mapObject.Properties[propertyName] = propertyValue;
                }
            }

            return mapObject;
        }
    }

    public class TileData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int TileId { get; set; }
    }
}
