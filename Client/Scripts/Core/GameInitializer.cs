using Godot;
using System;
using RoguelikeGame.Core;
using RoguelikeGame.Systems;
using RoguelikeGame.Generation;
using RoguelikeGame.Packages;

namespace RoguelikeGame.Core
{
    public partial class GameInitializer : Node
    {
        [Export]
        public bool AutoStart { get; set; } = false;

        [Export]
        public int TestSeed { get; set; } = -1;

        public override void _Ready()
        {
            GD.Print("[GameInitializer] Initializing game systems...");

            InitializeManagers();

            if (AutoStart)
            {
                CallDeferred(nameof(StartGame));
            }
        }

        private void InitializeManagers()
        {
            var eventBus = new EventBus();
            eventBus.Name = "EventBus";
            GetTree().Root.CallDeferred("add_child", eventBus);

            var gameManager = new GameManager();
            gameManager.Name = "GameManager";
            GetTree().Root.CallDeferred("add_child", gameManager);

            var unitManager = new UnitManager();
            unitManager.Name = "UnitManager";
            GetTree().Root.CallDeferred("add_child", unitManager);

            var waveManager = new WaveManager();
            waveManager.Name = "WaveManager";
            GetTree().Root.CallDeferred("add_child", waveManager);

            var dungeonGenerator = new DungeonGenerator();
            dungeonGenerator.Name = "DungeonGenerator";
            GetTree().Root.CallDeferred("add_child", dungeonGenerator);

            var packageManager = new PackageManager();
            packageManager.Name = "PackageManager";
            GetTree().Root.CallDeferred("add_child", packageManager);

            GD.Print("[GameInitializer] All managers initialized (including Package Manager)");
        }

        private void StartGame()
        {
            int seed = TestSeed == -1 ? (int)GD.Randi() : TestSeed;
            GameManager.Instance.StartNewRun("ironclad", (uint)seed);
        }

        public static void QuickStart(int seed = -1)
        {
            if (GameManager.Instance == null)
            {
                GD.PrintErr("[GameInitializer] GameManager not found!");
                return;
            }

            GameManager.Instance.StartNewRun("ironclad", (uint)seed);
        }
    }
}
