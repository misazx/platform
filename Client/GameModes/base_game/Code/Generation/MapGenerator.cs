using Godot;
using System.Linq;using System;using System;
using System.Collections.Generic;
using RoguelikeGame.Core;
using RoguelikeGame.Database;

namespace RoguelikeGame.Generation
{
    public enum NodeType
    {
        Unknown,
        Monster,
        Elite,
        Boss,
        Event,
        Shop,
        Rest,
        Treasure,
        SmallChest,
        LargeChest
    }

    public enum MapNodeStatus
    {
        Available,
        Completed,
        Locked,
        Current
    }

    public class MapNode
    {
        public int Id { get; set; }
        public Vector2I Position { get; set; }
        public NodeType Type { get; set; }
        public MapNodeStatus Status { get; set; } = MapNodeStatus.Locked;
        
        public List<int> ConnectedNodes { get; set; } = new();
        public string EventId { get; set; }
        public string EnemyEncounterId { get; set; }
        public bool IsVisited { get; set; }
        
        public Dictionary<string, object> CustomData { get; set; } = new();
    }

    public class FloorMap
    {
        public int FloorNumber { get; set; }
        public string FloorName { get; set; }
        public string BackgroundPath { get; set; }
        
        public List<MapNode> Nodes { get; set; } = new();
        public int StartNodeId { get; set; }
        public int BossNodeId { get; set; }
        
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public partial class MapGenerator : SingletonBase<MapGenerator>
    {
        private readonly Dictionary<string, FloorMap> _floorMaps = new();
        private readonly List<FloorMap> _mapHistory = new();
        private RandomGenerator _rng;

        [Export]
        public int MinNodesPerFloor { get; set; } = 12;
        
        [Export]
        public int MaxNodesPerFloor { get; set; } = 18;
        
        [Export]
        public float EliteChance { get; set; } = 0.15f;
        
        [Export]
        public float EventChance { get; set; } = 0.20f;
        
        [Export]
        public float ShopChance { get; set; } = 0.10f;
        
        [Export]
        public float RestChance { get; set; } = 0.12f;
        
        [Export]
        public float TreasureChance { get; set; } = 0.08f;

        [Signal]
        public delegate void MapGeneratedEventHandler(string mapJson);

        [Signal]
        public delegate void NodeVisitedEventHandler(int nodeId);
        
        [Signal]
        public delegate void FloorCompletedEventHandler(int floor);

        protected override void OnInitialize()
        {
        }

        public void Initialize(uint seed)
        {
            _rng = new RandomGenerator(seed);
            _floorMaps.Clear();
            _mapHistory.Clear();
        }

        public FloorMap GenerateFloor(int floorNumber)
        {
            var floorConfig = GetFloorConfiguration(floorNumber);
            
            var map = new FloorMap
            {
                FloorNumber = floorNumber,
                FloorName = floorConfig.Item1,
                BackgroundPath = floorConfig.Item2,
                Width = 800,
                Height = 600
            };

            int nodeCount = _rng.Next(MinNodesPerFloor, MaxNodesPerFloor + 1);
            
            // Generate nodes in layers
            int layers = 8 + (floorNumber / 5); // More layers for higher floors
            int nodesPerLayer = nodeCount / layers;
            
            int nodeIdCounter = 0;
            var allNodes = new List<MapNode>();
            
            // Create start node
            var startNode = new MapNode
            {
                Id = nodeIdCounter++,
                Position = new Vector2I(50, map.Height / 2),
                Type = NodeType.Unknown,
                Status = MapNodeStatus.Available,
                IsVisited = false
            };
            allNodes.Add(startNode);
            map.StartNodeId = startNode.Id;
            map.Nodes.Add(startNode);

            // Generate nodes layer by layer
            for (int layer = 1; layer < layers; layer++)
            {
                int x = 50 + (layer * (map.Width - 100) / layers);
                int nodesInThisLayer = nodesPerLayer + (_rng.Next(-1, 2)); // Slight variation
                
                for (int i = 0; i < Math.Max(1, nodesInThisLayer); i++)
                {
                    int y = (i * map.Height / Math.Max(1, nodesInThisLayer)) + 
                          _rng.Next(-30, 31); // Add some vertical variation
                    
                    y = Mathf.Clamp(y, 50, map.Height - 50);
                    
                    var nodeType = DetermineNodeType(layer, isLastLayer: layer >= layers - 1);
                    
                    var node = new MapNode
                    {
                        Id = nodeIdCounter++,
                        Position = new Vector2I(x, y),
                        Type = nodeType,
                        Status = MapNodeStatus.Locked,
                        IsVisited = false
                    };
                    
                    // Assign specific data based on type
                    AssignNodeData(node, floorNumber);
                    
                    allNodes.Add(node);
                    map.Nodes.Add(node);
                }
            }

            // Create boss node (always last layer)
            var bossNode = new MapNode
            {
                Id = nodeIdCounter++,
                Position = new Vector2I(map.Width - 50, map.Height / 2),
                Type = NodeType.Boss,
                Status = MapNodeStatus.Locked,
                IsVisited = false,
                EnemyEncounterId = GetBossForFloor(floorNumber)
            };
            allNodes.Add(bossNode);
            map.Nodes.Add(bossNode);
            map.BossNodeId = bossNode.Id;

            // Connect nodes
            ConnectNodes(map, allNodes, layers);

            // Unlock starting connections
            UnlockConnectedNodes(startNode);

            _floorMaps[floorNumber.ToString()] = map;
            _mapHistory.Add(map);

            EmitSignal(SignalName.MapGenerated, map.FloorNumber.ToString());
            
            GD.Print($"[MapGenerator] Generated floor {floorNumber}: '{floorConfig.Item1}' with {map.Nodes.Count} nodes");
            
            return map;
        }

        private Tuple<string, string> GetFloorConfiguration(int floor)
        {
            // Alternate between two environments per act
            var acts = new[]
            {
                Tuple.Create("Glory", "res://GameModes/base_game/Resources/Images/Backgrounds/glory.png"),
                Tuple.Create("Hive", "res://GameModes/base_game/Resources/Images/Backgrounds/hive.png"),
                Tuple.Create("Overgrowth", "res://GameModes/base_game/Resources/Images/Backgrounds/overgrowth.png"),
                Tuple.Create("Underdocks", "res://GameModes/base_game/Resources/Images/Backgrounds/underdocks.png")
            };
            
            return acts[(floor - 1) % acts.Length];
        }

        private NodeType DetermineNodeType(int layer, bool isLastLayer = false)
        {
            if (isLastLayer)
                return NodeType.Boss;
            
            // First few layers are always monster fights
            if (layer <= 2)
                return NodeType.Monster;
            
            float roll = _rng.NextFloat();
            float cumulative = 0f;
            
            cumulative += EliteChance;
            if (roll < cumulative)
                return NodeType.Elite;
            
            cumulative += EventChance;
            if (roll < cumulative)
                return NodeType.Event;
            
            cumulative += ShopChance;
            if (roll < cumulative)
                return NodeType.Shop;
            
            cumulative += RestChance;
            if (roll < cumulative)
                return NodeType.Rest;
            
            cumulative += TreasureChance;
            if (roll < cumulative)
                return NodeType.Treasure;
            
            return NodeType.Monster; // Default
        }

        private void AssignNodeData(MapNode node, int floor)
        {
            switch (node.Type)
            {
                case NodeType.Monster:
                    node.EnemyEncounterId = GetRandomNormalEncounter(floor);
                    break;
                    
                case NodeType.Elite:
                    node.EnemyEncounterId = GetRandomEliteEncounter(floor);
                    break;
                    
                case NodeType.Event:
                    node.EventId = GetRandomEvent(floor);
                    break;
                    
                case NodeType.Shop:
                    node.CustomData["shop_tier"] = _rng.Next(1, 4);
                    break;
                    
                case NodeType.Rest:
                    node.CustomData["rest_type"] = _rng.Choose("heal", "upgrade", "smith");
                    break;
                    
                case NodeType.Treasure:
                    node.CustomData["chest_type"] = _rng.Choose("small", "large");
                    break;
            }
        }

        private string GetRandomNormalEncounter(int floor)
        {
            var encounters = new[] { "cultist_group", "jaw_worm_pack", "slavers", "fungi_beasts" };
            return _rng.Choose(encounters);
        }

        private string GetRandomEliteEncounter(int floor)
        {
            var elites = new[] { "lagavulin", "gremlin_leader", "three_sentries" };
            return _rng.Choose(elites);
        }

        private string GetBossForFloor(int floor)
        {
            var bosses = new[] 
            { 
                "the_guardian",      // Act 1 Boss
                "the_collector",     // Act 2 Boss  
                "the_automaton",    // Act 3 Boss
                "the_awakener"      // Act 4 Boss
            };
            return bosses[(floor - 1) % bosses.Length];
        }

        private string GetRandomEvent(int floor)
        {
            var events = EventDatabase.Instance.GetAllEvents();
            if (events.Count == 0)
                return "big_fish";
            
            return _rng.Choose((IList<EventData>)events).Id;
        }

        private void ConnectNodes(FloorMap map, List<MapNode> allNodes, int layers)
        {
            // Group nodes by approximate x position (layer)
            var layersDict = new Dictionary<int, List<MapNode>>();
            foreach (var node in allNodes)
            {
                int layer = (node.Position.X - 50) / ((map.Width - 100) / layers);
                if (!layersDict.ContainsKey(layer))
                    layersDict[layer] = new List<MapNode>();
                layersDict[layer].Add(node);
            }

            // Connect adjacent layers
            var sortedLayers = new List<int>(layersDict.Keys);
            sortedLayers.Sort();

            for (int i = 0; i < sortedLayers.Count - 1; i++)
            {
                var currentLayer = layersDict[sortedLayers[i]];
                var nextLayer = layersDict[sortedLayers[i + 1]];

                foreach (var currentNode in currentLayer)
                {
                    // Connect to 1-3 nodes in next layer
                    int connectionCount = _rng.Next(1, Math.Min(4, nextLayer.Count + 1));
                    var shuffledNext = _rng.GetShuffled(nextLayer);

                    for (int j = 0; j < connectionCount && j < shuffledNext.Count; j++)
                    {
                        var targetNode = shuffledNext[j];
                        if (!currentNode.ConnectedNodes.Contains(targetNode.Id))
                        {
                            currentNode.ConnectedNodes.Add(targetNode.Id);
                            
                            // Also add reverse connection for bidirectional traversal
                            if (!targetNode.ConnectedNodes.Contains(currentNode.Id))
                            {
                                targetNode.ConnectedNodes.Add(currentNode.Id);
                            }
                        }
                    }
                }
            }
        }

        private void UnlockConnectedNodes(MapNode node)
        {
            foreach (var connectedId in node.ConnectedNodes)
            {
                var connectedNode = _floorMaps.Values
                    .SelectMany(m => m.Nodes)
                    .FirstOrDefault(n => n.Id == connectedId);
                
                if (connectedNode != null && connectedNode.Status == MapNodeStatus.Locked)
                {
                    connectedNode.Status = MapNodeStatus.Available;
                }
            }
        }

        public bool VisitNode(FloorMap map, MapNode node)
        {
            if (node.Status != MapNodeStatus.Available)
                return false;

            node.Status = MapNodeStatus.Completed;
            node.IsVisited = true;

            // Unlock connected nodes
            UnlockConnectedNodes(node);

            EmitSignal(SignalName.NodeVisited, node.Id);

            // Record timeline
            TimelineManager.Instance?.AddEventTriggered(
                $"Node_{node.Type}",
                $"Visited {node.Type} node",
                map.FloorNumber,
                map.Nodes.IndexOf(node)
            );

            // Check if this was the boss
            if (node.Type == NodeType.Boss)
            {
                EmitSignal(SignalName.FloorCompleted, map.FloorNumber);
                TimelineManager.Instance?.AddBossDefeated(
                    node.EnemyEncounterId ?? "Unknown Boss",
                    map.FloorNumber
                );
            }

            GD.Print($"[MapGenerator] Visited node {node.Id} ({node.Type}) on floor {map.FloorNumber}");

            return true;
        }

        public FloorMap GetCurrentFloor()
        {
            return _mapHistory.Count > 0 ? _mapHistory[_mapHistory.Count - 1] : null;
        }

        public List<FloorMap> GetAllFloors()
        {
            return new List<FloorMap>(_mapHistory);
        }

        public MapNode FindNodeById(FloorMap map, int nodeId)
        {
            return map.Nodes.FirstOrDefault(n => n.Id == nodeId);
        }

        public List<MapNode> GetAvailableNodes(FloorMap map)
        {
            return map.Nodes.Where(n => n.Status == MapNodeStatus.Available).ToList();
        }

        public List<MapNode> GetNodesByType(FloorMap map, NodeType type)
        {
            return map.Nodes.Where(n => n.Type == type).ToList();
        }

        public string GetNodeTypeDisplayName(NodeType type)
        {
            return type switch
            {
                NodeType.Monster => "怪物",
                NodeType.Elite => "精英",
                NodeType.Boss => "首领",
                NodeType.Event => "事件",
                NodeType.Shop => "商店",
                NodeType.Rest => "篝火",
                NodeType.Treasure => "宝箱",
                _ => "未知"
            };
        }

        public Color GetNodeTypeColor(NodeType type)
        {
            return type switch
            {
                NodeType.Monster => Colors.Red,
                NodeType.Elite => Colors.Orange,
                NodeType.Boss => Colors.DarkRed,
                NodeType.Event => Colors.Purple,
                NodeType.Shop => Colors.Yellow,
                NodeType.Rest => Colors.Green,
                NodeType.Treasure => Colors.Gold,
                _ => Colors.Gray
            };
        }
    }
}
