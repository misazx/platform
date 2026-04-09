using Godot;
using System;
using System.Collections.Generic;
using RoguelikeGame.Core;

namespace RoguelikeGame.Generation
{
    public enum DungeonLayout
    {
        Linear,
        Branching,
        Loop,
        Grid
    }

    public class DungeonData
    {
        public int Floor { get; set; }
        public uint Seed { get; set; }
        public Dictionary<int, Room> Rooms { get; set; } = new();
        public int StartRoomId { get; set; }
        public int BossRoomId { get; set; }
        public List<int> ShopRoomIds { get; set; } = new();
        public List<int> TreasureRoomIds { get; set; } = new();
        public DungeonLayout Layout { get; set; }
    }

    public partial class DungeonGenerator : Node
    {
        public static DungeonGenerator Instance { get; private set; }

        private RandomGenerator _rng;
        private RoomGenerator _roomGenerator;
        private DungeonData _currentDungeon;

        [Export]
        public int MinRooms { get; set; } = 10;

        [Export]
        public int MaxRooms { get; set; } = 20;

        [Export]
        public int RoomSpacing { get; set; } = 3;

        [Export]
        public float BranchChance { get; set; } = 0.3f;

        [Export]
        public float LoopChance { get; set; } = 0.2f;

        [Signal]
        public delegate void DungeonGeneratedEventHandler(string dungeonJson);

        public DungeonData CurrentDungeon => _currentDungeon;

        public override void _Ready()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }
            Instance = this;
        }

        public DungeonData GenerateDungeon(int floor, uint seed)
        {
            _rng = new RandomGenerator(seed + (uint)floor);
            _roomGenerator = new RoomGenerator(_rng);

            _currentDungeon = new DungeonData
            {
                Floor = floor,
                Seed = seed,
                Layout = ChooseLayout(floor)
            };

            int roomCount = _rng.Next(MinRooms, MaxRooms + 1);
            GD.Print($"[DungeonGenerator] Generating floor {floor} with {roomCount} rooms");

            GenerateRooms(roomCount);
            ConnectRooms();
            AssignSpecialRooms();

            foreach (var room in _currentDungeon.Rooms.Values)
            {
                _roomGenerator.PopulateRoom(room);
            }

            EmitSignal(SignalName.DungeonGenerated, _currentDungeon.Floor);
            GD.Print($"[DungeonGenerator] Dungeon generated: {_currentDungeon.Rooms.Count} rooms");

            return _currentDungeon;
        }

        private DungeonLayout ChooseLayout(int floor)
        {
            if (floor <= 3)
                return DungeonLayout.Linear;
            
            float rand = _rng.NextFloat();
            if (rand < 0.4f)
                return DungeonLayout.Branching;
            else if (rand < 0.7f)
                return DungeonLayout.Loop;
            else
                return DungeonLayout.Grid;
        }

        private void GenerateRooms(int count)
        {
            var occupiedPositions = new HashSet<Vector2I>();
            var frontier = new List<Vector2I>();

            var startRoom = _roomGenerator.GenerateRoom(0, Vector2I.Zero, RoomType.Start);
            _currentDungeon.Rooms[0] = startRoom;
            _currentDungeon.StartRoomId = 0;
            occupiedPositions.Add(Vector2I.Zero);

            var directions = new Vector2I[]
            {
                Vector2I.Up,
                Vector2I.Down,
                Vector2I.Left,
                Vector2I.Right
            };

            foreach (var dir in directions)
            {
                frontier.Add(dir * RoomSpacing);
            }

            for (int i = 1; i < count; i++)
            {
                if (frontier.Count == 0)
                {
                    GD.PrintErr("[DungeonGenerator] No more frontier positions available");
                    break;
                }

                int frontierIndex = _rng.Next(frontier.Count);
                var position = frontier[frontierIndex];
                frontier.RemoveAt(frontierIndex);

                if (occupiedPositions.Contains(position))
                    continue;

                var room = _roomGenerator.GenerateRoom(i, position, RoomType.Normal);
                _currentDungeon.Rooms[i] = room;
                occupiedPositions.Add(position);

                foreach (var dir in directions)
                {
                    var newPos = position + dir * RoomSpacing;
                    if (!occupiedPositions.Contains(newPos))
                    {
                        frontier.Add(newPos);
                    }
                }

                if (_rng.NextBool(BranchChance) && frontier.Count > 2)
                {
                    int branchStart = _rng.Next(1, i);
                    if (_currentDungeon.Rooms.ContainsKey(branchStart))
                    {
                        var branchPos = _currentDungeon.Rooms[branchStart].Position + 
                                       directions[_rng.Next(4)] * RoomSpacing;
                        if (!occupiedPositions.Contains(branchPos))
                        {
                            frontier.Insert(0, branchPos);
                        }
                    }
                }
            }
        }

        private void ConnectRooms()
        {
            var directions = new Vector2I[]
            {
                Vector2I.Up,
                Vector2I.Down,
                Vector2I.Left,
                Vector2I.Right
            };

            foreach (var room in _currentDungeon.Rooms.Values)
            {
                foreach (var dir in directions)
                {
                    var neighborPos = room.Position + dir * RoomSpacing;
                    var neighbor = FindRoomAtPosition(neighborPos);
                    
                    if (neighbor != null && !room.ConnectedRooms.Contains(neighbor.Id))
                    {
                        room.ConnectedRooms.Add(neighbor.Id);
                        neighbor.ConnectedRooms.Add(room.Id);
                    }
                }
            }

            if (_currentDungeon.Layout == DungeonLayout.Loop)
            {
                CreateLoops();
            }
        }

        private void CreateLoops()
        {
            int loopCount = _rng.Next(2, 5);
            var rooms = new List<Room>(_currentDungeon.Rooms.Values);
            
            for (int i = 0; i < loopCount && rooms.Count >= 2; i++)
            {
                int idx1 = (int)(GD.Randi() % rooms.Count);
                int idx2 = (int)(GD.Randi() % rooms.Count);
                var room1 = rooms[idx1];
                var room2 = rooms[idx2];
                
                if (room1 != null && room2 != null && room1.Id != room2.Id)
                {
                    if (!room1.ConnectedRooms.Contains(room2.Id))
                    {
                        room1.ConnectedRooms.Add(room2.Id);
                        room2.ConnectedRooms.Add(room1.Id);
                    }
                }
            }
        }

        private void AssignSpecialRooms()
        {
            var normalRooms = new List<Room>();
            foreach (var room in _currentDungeon.Rooms.Values)
            {
                if (room.Type == RoomType.Normal)
                {
                    normalRooms.Add(room);
                }
            }

            _rng.Shuffle(normalRooms);

            if (normalRooms.Count > 0)
            {
                var bossRoom = normalRooms[0];
                bossRoom.Type = RoomType.Boss;
                _currentDungeon.BossRoomId = bossRoom.Id;
                normalRooms.RemoveAt(0);
            }

            int shopCount = Math.Max(1, _currentDungeon.Rooms.Count / 10);
            for (int i = 0; i < shopCount && normalRooms.Count > 0; i++)
            {
                var shopRoom = normalRooms[0];
                shopRoom.Type = RoomType.Shop;
                _currentDungeon.ShopRoomIds.Add(shopRoom.Id);
                normalRooms.RemoveAt(0);
            }

            int treasureCount = Math.Max(2, _currentDungeon.Rooms.Count / 8);
            for (int i = 0; i < treasureCount && normalRooms.Count > 0; i++)
            {
                var treasureRoom = normalRooms[0];
                treasureRoom.Type = RoomType.Treasure;
                _currentDungeon.TreasureRoomIds.Add(treasureRoom.Id);
                normalRooms.RemoveAt(0);
            }

            if (_rng.NextBool(0.15f) && normalRooms.Count > 0)
            {
                var secretRoom = normalRooms[0];
                secretRoom.Type = RoomType.Secret;
                normalRooms.RemoveAt(0);
            }
        }

        private Room FindRoomAtPosition(Vector2I position)
        {
            foreach (var room in _currentDungeon.Rooms.Values)
            {
                if (room.Position == position)
                    return room;
            }
            return null;
        }

        public Room GetRoom(int roomId)
        {
            return _currentDungeon?.Rooms.TryGetValue(roomId, out var room) == true ? room : null;
        }

        public List<Room> GetConnectedRooms(int roomId)
        {
            var result = new List<Room>();
            var room = GetRoom(roomId);
            
            if (room != null)
            {
                foreach (var connectedId in room.ConnectedRooms)
                {
                    var connectedRoom = GetRoom(connectedId);
                    if (connectedRoom != null)
                        result.Add(connectedRoom);
                }
            }
            
            return result;
        }

        public void ClearDungeon()
        {
            _currentDungeon = null;
            GD.Print("[DungeonGenerator] Dungeon cleared");
        }
    }
}
