using Godot;
using System;
using RoguelikeGame.Core;
using RoguelikeGame.Editor;

namespace RoguelikeGame.Tests
{
    public partial class ConfigTest : Node
    {
        public override void _Ready()
        {
            GD.Print("========================================");
            GD.Print("[ConfigTest] Starting configuration tests...");
            GD.Print("========================================");

            TestJsonLoading();
            TestCompiledLoading();
            TestDatabaseLoading();

            GD.Print("========================================");
            GD.Print("[ConfigTest] All tests completed!");
            GD.Print("========================================");
        }

        private void TestJsonLoading()
        {
            GD.Print("\n[ConfigTest] Testing JSON loading...");

            ConfigLoader.UseCompiledConfig = false;

            var cards = ConfigLoader.LoadConfig<CardConfigData>("cards");
            GD.Print($"  Cards: {(cards != null ? $"✓ {cards.Cards.Count} cards" : "✗ Failed")}");

            var characters = ConfigLoader.LoadConfig<CharacterConfigData>("characters");
            GD.Print($"  Characters: {(characters != null ? $"✓ {characters.Characters.Count} characters" : "✗ Failed")}");

            var enemies = ConfigLoader.LoadConfig<EnemyConfigData>("enemies");
            GD.Print($"  Enemies: {(enemies != null ? $"✓ {enemies.Enemies.Count} enemies" : "✗ Failed")}");
        }

        private void TestCompiledLoading()
        {
            GD.Print("\n[ConfigTest] Testing compiled bytes loading...");

            GD.Print("  Compiling configs...");
            ConfigCompiler.CompileAllConfigs();

            ConfigLoader.UseCompiledConfig = true;
            ConfigLoader.ClearCache();

            var cards = ConfigLoader.LoadConfig<CardConfigData>("cards");
            GD.Print($"  Cards: {(cards != null ? $"✓ {cards.Cards.Count} cards" : "✗ Failed")}");

            var characters = ConfigLoader.LoadConfig<CharacterConfigData>("characters");
            GD.Print($"  Characters: {(characters != null ? $"✓ {characters.Characters.Count} characters" : "✗ Failed")}");

            ConfigLoader.UseCompiledConfig = false;
        }

        private void TestDatabaseLoading()
        {
            GD.Print("\n[ConfigTest] Testing database loading...");

            var cardDb = Database.CardDatabase.Instance;
            GD.Print($"  CardDatabase: {(cardDb != null ? $"✓ {cardDb.TotalCards} cards" : "✗ Failed")}");

            var charDb = Database.CharacterDatabase.Instance;
            GD.Print($"  CharacterDatabase: {(charDb != null ? $"✓ {charDb.TotalCharacters} characters" : "✗ Failed")}");

            var enemyDb = Database.EnemyDatabase.Instance;
            GD.Print($"  EnemyDatabase: {(enemyDb != null ? $"✓ {enemyDb.TotalEnemies} enemies" : "✗ Failed")}");

            var relicDb = Database.RelicDatabase.Instance;
            GD.Print($"  RelicDatabase: {(relicDb != null ? $"✓ {relicDb.TotalRelics} relics" : "✗ Failed")}");

            var potionDb = Database.PotionDatabase.Instance;
            GD.Print($"  PotionDatabase: {(potionDb != null ? $"✓ {potionDb.TotalPotions} potions" : "✗ Failed")}");

            var eventDb = Database.EventDatabase.Instance;
            GD.Print($"  EventDatabase: {(eventDb != null ? $"✓ {eventDb.TotalEvents} events" : "✗ Failed")}");
        }
    }
}
