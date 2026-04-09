using Godot;
using System;
using System.Collections.Generic;
using RoguelikeGame.Core;
using RoguelikeGame.Audio;

namespace RoguelikeGame.Core
{
	public partial class ResourceInitializer : Node
	{
		public static ResourceInitializer Instance { get; private set; }

		private const string RESOURCES_VERSION = "1.0.0";
		private const string RESOURCES_FLAG_FILE = "user://resources_generated.flag";

		public override void _Ready()
		{
			Instance = this;
			CheckAndGenerateResources();
		}

		private void CheckAndGenerateResources()
		{
			GD.Print("[ResourceInitializer] 检查资源状态...");

			if (ShouldGenerateResources())
			{
				GD.Print("[ResourceInitializer] 开始生成资源...");
				GenerateAllResources();
				MarkResourcesGenerated();
			}
			else
			{
				GD.Print("[ResourceInitializer] 资源已存在，跳过生成");
			}
		}

		private bool ShouldGenerateResources()
		{
			if (Godot.FileAccess.FileExists(RESOURCES_FLAG_FILE))
			{
				using var file = Godot.FileAccess.Open(RESOURCES_FLAG_FILE, Godot.FileAccess.ModeFlags.Read);
				string version = file.GetLine();
				return version != RESOURCES_VERSION;
			}
			return true;
		}

		private void MarkResourcesGenerated()
		{
			using var file = Godot.FileAccess.Open(RESOURCES_FLAG_FILE, Godot.FileAccess.ModeFlags.Write);
			file.StoreLine(RESOURCES_VERSION);
			GD.Print("[ResourceInitializer] 资源生成标记已保存");
		}

		private void GenerateAllResources()
		{
			GD.Print("[ResourceInitializer] ========== 开始生成所有游戏资源 ==========");

			try
			{
				GenerateImageResources();
				GenerateAudioResources();
				ValidateResources();

				GD.Print("[ResourceInitializer] ========== 所有资源生成完成！ ==========");
			}
			catch (Exception e)
			{
				GD.PrintErr($"[ResourceInitializer] 资源生成失败: {e.Message}");
				GD.PrintErr(e.StackTrace);
			}
		}

		private void GenerateImageResources()
		{
			// ⚠️ 已禁用 - 使用 Kenney 风格预生成资源替代程序化生成
			GD.Print("[ResourceInitializer] ✅ 跳过图像生成 - 使用外部 Kenney 风格资源");

			/*
			GD.Print("[ResourceInitializer] --- 生成图像资源 ---");

			var resourceGenerator = GetNode<ResourceGenerator>("/root/ResourceGenerator");
			if (resourceGenerator == null)
			{
				resourceGenerator = new ResourceGenerator();
				AddChild(resourceGenerator);
			}

			resourceGenerator.GenerateAllResources();
			*/
		}

		private void GenerateAudioResources()
		{
			GD.Print("[ResourceInitializer] --- 生成音频资源 ---");

			var audioGenerator = GetNode<AudioGenerator>("/root/AudioGenerator");
			if (audioGenerator == null)
			{
				audioGenerator = new AudioGenerator();
				AddChild(audioGenerator);
			}

			audioGenerator.GenerateAllAudioResources();
		}

		private void ValidateResources()
		{
			GD.Print("[ResourceInitializer] --- 验证资源完整性 ---");

			var requiredImages = new List<string>
			{
				"res://GameModes/base_game/Resources/Images/Characters/Ironclad.png",
				"res://GameModes/base_game/Resources/Images/Characters/Silent.png",
				"res://GameModes/base_game/Resources/Images/Characters/Defect.png",
				"res://GameModes/base_game/Resources/Images/Characters/Watcher.png",
				"res://GameModes/base_game/Resources/Images/Characters/Necromancer.png",
				"res://GameModes/base_game/Resources/Images/Characters/Heir.png",
				"res://GameModes/base_game/Resources/Images/Backgrounds/glory.png",
				"res://GameModes/base_game/Resources/Images/Backgrounds/hive.png",
				"res://GameModes/base_game/Resources/Images/Backgrounds/overgrowth.png",
                "res://GameModes/base_game/Resources/Images/Backgrounds/underdocks.png"
			};

			int missingCount = 0;
			foreach (var path in requiredImages)
			{
				if (!ResourceLoader.Exists(path))
				{
					GD.PrintErr($"[ResourceInitializer] 缺失资源: {path}");
					missingCount++;
				}
			}

			if (missingCount == 0)
			{
				GD.Print("[ResourceInitializer] ✓ 所有核心资源验证通过");
			}
			else
			{
				GD.PrintErr($"[ResourceInitializer] ✗ 缺失 {missingCount} 个核心资源");
			}
		}

		public void ForceRegenerateResources()
		{
			GD.Print("[ResourceInitializer] 强制重新生成所有资源...");
			DeleteFlagFile();
			GenerateAllResources();
			MarkResourcesGenerated();
		}

		private void DeleteFlagFile()
		{
			if (Godot.FileAccess.FileExists(RESOURCES_FLAG_FILE))
			{
				DirAccess.RemoveAbsolute(RESOURCES_FLAG_FILE);
			}
		}

		public Dictionary<string, object> GetResourceStatus()
		{
			var status = new Dictionary<string, object>
			{
				{"version", RESOURCES_VERSION},
				{"flag_exists", Godot.FileAccess.FileExists(RESOURCES_FLAG_FILE)},
				{"images_count", CountFilesInDirectory("res://GameModes/base_game/Resources/Images/")},
				{"icons_count", CountFilesInDirectory("res://GameModes/base_game/Resources/Icons/")},
				{"audio_count", CountFilesInDirectory("res://GameModes/base_game/Resources/Audio/")}
			};

			return status;
		}

		private int CountFilesInDirectory(string path)
		{
			int count = 0;
			var dir = DirAccess.Open(path);

			if (dir != null)
			{
				dir.ListDirBegin();
				string fileName = dir.GetNext();
				while (fileName != "")
				{
					if (!dir.CurrentIsDir())
					{
						count++;
					}
					fileName = dir.GetNext();
				}
				dir.ListDirEnd();
			}

			return count;
		}
	}
}
