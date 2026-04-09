using Godot;
using System;
using System.Collections.Generic;
using RoguelikeGame.Core;

namespace RoguelikeGame.Generation
{
    public enum RoomType
    {
        Normal,
        Start,
        Boss,
        Shop,
        Treasure,
        Secret
    }

    public class Room
    {
        public int Id { get; set; }
        public RoomType Type { get; set; }
        public Vector2I Position { get; set; }
        public Vector2I Size { get; set; }
        public List<int> ConnectedRooms { get; set; } = new();
        public bool IsVisited { get; set; } = false;
        public bool IsCleared { get; set; } = false;
        public Dictionary<string, object> CustomData { get; set; } = new();
    }

    public class RoomGenerator
    {
        private RandomGenerator _rng;

        public RoomGenerator(RandomGenerator rng)
        {
            _rng = rng;
        }

        public Room GenerateRoom(int id, Vector2I position, RoomType type = RoomType.Normal)
        {
            var room = new Room
            {
                Id = id,
                Type = type,
                Position = position,
                Size = GenerateRoomSize(type)
            };

            return room;
        }

        private Vector2I GenerateRoomSize(RoomType type)
        {
            int minSize = type == RoomType.Boss ? 15 : 8;
            int maxSize = type == RoomType.Boss ? 25 : 15;

            return new Vector2I(
                _rng.Next(minSize, maxSize),
                _rng.Next(minSize, maxSize)
            );
        }

        public void PopulateRoom(Room room)
        {
            switch (room.Type)
            {
                case RoomType.Start:
                    PopulateStartRoom(room);
                    break;
                case RoomType.Boss:
                    PopulateBossRoom(room);
                    break;
                case RoomType.Shop:
                    PopulateShopRoom(room);
                    break;
                case RoomType.Treasure:
                    PopulateTreasureRoom(room);
                    break;
                case RoomType.Secret:
                    PopulateSecretRoom(room);
                    break;
                default:
                    PopulateNormalRoom(room);
                    break;
            }
        }

        private void PopulateStartRoom(Room room)
        {
            room.CustomData["HasPlayerSpawn"] = true;
            room.CustomData["EnemyCount"] = 0;
        }

        private void PopulateBossRoom(Room room)
        {
            room.CustomData["BossType"] = _rng.Choose("Dragon", "Demon", "Golem");
            room.CustomData["EnemyCount"] = 1;
        }

        private void PopulateShopRoom(Room room)
        {
            int itemCount = _rng.Next(3, 6);
            room.CustomData["ShopItems"] = itemCount;
            room.CustomData["EnemyCount"] = 0;
        }

        private void PopulateTreasureRoom(Room room)
        {
            room.CustomData["ChestCount"] = _rng.Next(1, 4);
            room.CustomData["EnemyCount"] = _rng.Next(0, 3);
        }

        private void PopulateSecretRoom(Room room)
        {
            room.CustomData["HasSecretItem"] = true;
            room.CustomData["EnemyCount"] = _rng.Next(2, 5);
        }

        private void PopulateNormalRoom(Room room)
        {
            int difficulty = GameManager.Instance?.CurrentRun?.CurrentFloor ?? 1;
            int baseEnemies = 3 + difficulty;
            int enemyCount = _rng.Next(baseEnemies, baseEnemies + 5);

            room.CustomData["EnemyCount"] = enemyCount;
            room.CustomData["HasTrap"] = _rng.NextBool(0.3f);
            room.CustomData["HasChest"] = _rng.NextBool(0.2f);
        }
    }
}
