using System.Collections.Generic;
using Godot;

namespace RoguelikeGame.Packages
{
	public interface IPackageExtension
	{
		string PackageId { get; }
		PackageData PackageInfo { get; }

		void OnInitialize();
		void OnInstalled();
		void OnUninstalling();
		void OnLaunch();
		void OnUpdate();

		void RegisterCustomCards();
		void RegisterCustomCharacters();
		void RegisterCustomEnemies();
		void RegisterCustomEvents();
		void RegisterCustomRelics();
		void RegisterCustomPotions();

		Dictionary<string, object> GetSaveData();
		void LoadSaveData(Dictionary<string, object> data);
		void ClearSaveData();

		bool ValidateDependencies();
		List<string> GetConflictingPackages();
	}

	public abstract class PackageExtensionBase : IPackageExtension
	{
		public abstract string PackageId { get; }
		protected PackageData _packageInfo;

		public virtual PackageData PackageInfo => _packageInfo;

		public virtual void OnInitialize()
		{
			GD.Print($"[Package:{PackageId}] Extension initialized");
		}

		public virtual void OnInstalled()
		{
			GD.Print($"[Package:{PackageId}] Installed successfully");
			RegisterContent();
		}

		public virtual void OnUninstalling()
		{
			GD.Print($"[Package:{PackageId}] Uninstalling...");
			UnregisterContent();
			ClearSaveData();
		}

		public virtual void OnLaunch()
		{
			GD.Print($"[Package:{PackageId}] Launching package");
		}

		public virtual void OnUpdate()
		{
			GD.Print($"[Package:{PackageId}] Updating package content");
		}

		public virtual void RegisterCustomCards() { }
		public virtual void RegisterCustomCharacters() { }
		public virtual void RegisterCustomEnemies() { }
		public virtual void RegisterCustomEvents() { }
		public virtual void RegisterCustomRelics() { }
		public virtual void RegisterCustomPotions() { }

		protected virtual void RegisterContent()
		{
			RegisterCustomCards();
			RegisterCustomCharacters();
			RegisterCustomEnemies();
			RegisterCustomEvents();
			RegisterCustomRelics();
			RegisterCustomPotions();
		}

		protected virtual void UnregisterContent()
		{
			// TODO: 实现内容注销逻辑
		}

		public virtual Dictionary<string, object> GetSaveData()
		{
			return new Dictionary<string, object>();
		}

		public virtual void LoadSaveData(Dictionary<string, object> data)
		{
			// 子类实现具体的存档加载逻辑
		}

		public virtual void ClearSaveData()
		{
			GD.Print($"[Package:{PackageId}] Save data cleared");
		}

		public virtual bool ValidateDependencies()
		{
			var package = PackageManager.Instance?.GetPackage(PackageId);
			if (package == null) return false;

			foreach (var dep in package.Dependencies)
			{
				if (!PackageManager.Instance.IsPackageInstalled(dep))
				{
					return false;
				}
			}
			return true;
		}

		public virtual List<string> GetConflictingPackages()
		{
			return new List<string>();
		}
	}

	public class BaseGameExtension : PackageExtensionBase
	{
		public override string PackageId => "base_game";

		public override void OnInitialize()
		{
			base.OnInitialize();
			_packageInfo = PackageManager.Instance?.GetPackage("base_game");
		}

		public override void RegisterCustomCards()
		{
			// 基础游戏卡牌已在ConfigLoader中加载
			GD.Print("[BaseGame] Core cards already loaded via ConfigLoader");
		}

		public override void RegisterCustomCharacters()
		{
			// 基础游戏角色已注册
			GD.Print("[BaseGame] Core characters registered");
		}
	}
}
