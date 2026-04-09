using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using RoguelikeGame.Core;

namespace RoguelikeGame.Tests
{
	public partial class AcceptanceTests : Node
	{
		public override void _Ready()
		{
			GD.Print("========== STS Combat System Acceptance Tests ==========");
			
			TestCardSystem();
			TestDamageCalculation();
			TestStatusEffects();
			TestEnemyAI();
			TestCombatFlow();
			
			GD.Print("========== All Tests Passed! ==========");
		}
		
		private void TestCardSystem()
		{
			GD.Print("\n[TEST] Card System...");
			
			var strike = StsCardData.CreateStrike();
			Assert(strike.Id == "Strike_R", "Strike ID correct");
			Assert(strike.Cost == 1, "Strike cost is 1");
			Assert(strike.Damage == 6, "Strike damage is 6");
			Assert(strike.Type == StsCardType.Attack, "Strike is Attack type");
			
			var defend = StsCardData.CreateDefend();
			Assert(defend.Block == 5, "Defend block is 5");
			Assert(defend.Type == StsCardType.Skill, "Defend is Skill type");
			
			var bash = StsCardData.CreateBash();
			Assert(bash.Cost == 2, "Bash cost is 2");
			Assert(bash.MagicNumber == 2, "Bash applies 2 Vulnerable");
			
			GD.Print("[PASS] Card System tests passed!");
		}
		
		private void TestDamageCalculation()
		{
			GD.Print("\n[TEST] Damage Calculation...");
			
			var engine = new StsCombatEngine();
			var enemies = new List<StsEnemyData>
			{
				new() { Id = "Test", Name = "Test", MaxHp = 100, CurrentHp = 100 }
			};
			engine.InitializeCombat(enemies, 12345);
			
			int baseDamage = engine.CalculateDamage(6, StsDamageType.Normal);
			Assert(baseDamage == 6, "Base damage calculation correct");
			
			engine.Player.Strength = 2;
			int damageWithStrength = engine.CalculateDamage(6, StsDamageType.Attack);
			Assert(damageWithStrength == 8, "Damage with +2 Strength: 6+2=8");
			
			engine.Player.AddStatus(StsStatusEffect.CreateWeak(1));
			int damageWithWeak = engine.CalculateDamage(8, StsDamageType.Attack);
			Assert(damageWithWeak == 6, "Damage with Weak (75%): 8*0.75=6");
			
			GD.Print("[PASS] Damage Calculation tests passed!");
		}
		
		private void TestStatusEffects()
		{
			GD.Print("\n[TEST] Status Effects...");
			
			var player = new StsPlayerState();
			
			player.AddStatus(StsStatusEffect.CreateStrength(3));
			Assert(player.GetStatusStacks("strength") == 3, "Strength applied");
			
			player.AddStatus(StsStatusEffect.CreateStrength(2));
			Assert(player.GetStatusStacks("strength") == 5, "Strength stacks: 3+2=5");
			
			player.AddStatus(StsStatusEffect.CreateVulnerable(2));
			Assert(player.HasStatus("vulnerable"), "Vulnerable applied");
			Assert(player.GetStatusStacks("vulnerable") == 2, "Vulnerable stacks: 2");
			
			GD.Print("[PASS] Status Effects tests passed!");
		}
		
		private void TestEnemyAI()
		{
			GD.Print("\n[TEST] Enemy AI...");
			
			var engine = new StsCombatEngine();
			var enemies = new List<StsEnemyData>
			{
				new() { Id = "Cultist", Name = "Cultist", MaxHp = 50, CurrentHp = 50 }
			};
			engine.InitializeCombat(enemies, 12345);
			
			var enemy = engine.Enemies[0];
			Assert(enemy.CurrentIntent != null, "Enemy has intent");
			Assert(!string.IsNullOrEmpty(enemy.CurrentIntent.Description), "Enemy intent has description");
			Assert(!string.IsNullOrEmpty(enemy.CurrentIntent.Icon), "Enemy intent has icon");
			
			GD.Print("[PASS] Enemy AI tests passed!");
		}
		
		private void TestCombatFlow()
		{
			GD.Print("\n[TEST] Combat Flow...");
			
			var engine = new StsCombatEngine();
			var enemies = new List<StsEnemyData>
			{
				new() { Id = "Cultist", Name = "Cultist", MaxHp = 50, CurrentHp = 50 }
			};
			engine.InitializeCombat(enemies, 999);
			
			Assert(engine.Player.Energy == 3, "Initial energy is 3");
			Assert(engine.Player.Hand.Count == 5, "Initial hand size is 5");
			Assert(engine.TurnNumber == 1, "Initial turn is 1");
			
			var card = engine.Player.Hand[0];
			bool canPlay = engine.CanPlayCard(card);
			Assert(canPlay, "Can play card from hand");
			
			var result = engine.PlayCard(card);
			Assert(result.Success, "Card played successfully");
			Assert(engine.Player.Energy == 2, "Energy deducted after play");
			
			GD.Print("[PASS] Combat Flow tests passed!");
		}
		
		private void Assert(bool condition, string message)
		{
			if (!condition)
			{
				GD.PrintErr($"[FAIL] {message}");
				throw new Exception($"Assertion failed: {message}");
			}
			GD.Print($"  ✓ {message}");
		}
	}
}