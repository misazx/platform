using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Godot;

namespace RoguelikeGame.Packages
{
	public enum PackageType
	{
		BaseGame,
		Expansion,
		CustomMap,
		Mod
	}

	public enum PackageStatus
	{
		Available,
		Downloading,
		Downloaded,
		Installing,
		Installed,
		UpdateAvailable,
		Error
	}

	public class PackageData
	{
		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("description")]
		public string Description { get; set; }

		[JsonPropertyName("version")]
		public string Version { get; set; } = "1.0.0";

		[JsonPropertyName("type")]
		public PackageType Type { get; set; } = PackageType.Expansion;

		[JsonPropertyName("author")]
		public string Author { get; set; }

		[JsonPropertyName("iconPath")]
		public string IconPath { get; set; }

		[JsonPropertyName("thumbnailPath")]
		public string ThumbnailPath { get; set; }

		[JsonPropertyName("downloadUrl")]
		public string DownloadUrl { get; set; }

		[JsonPropertyName("fileSize")]
		public long FileSize { get; set; }

		[JsonPropertyName("requiredBaseVersion")]
		public string RequiredBaseVersion { get; set; } = "1.0.0";

		[JsonPropertyName("dependencies")]
		public List<string> Dependencies { get; set; } = new();

		[JsonPropertyName("tags")]
		public List<string> Tags { get; set; } = new();

		[JsonPropertyName("features")]
		public List<string> Features { get; set; } = new();

		[JsonPropertyName("releaseDate")]
		public DateTime ReleaseDate { get; set; } = DateTime.Now;

		[JsonPropertyName("lastUpdated")]
		public DateTime LastUpdated { get; set; } = DateTime.Now;

		[JsonPropertyName("isFree")]
		public bool IsFree { get; set; } = true;

		[JsonPropertyName("price")]
		public decimal Price { get; set; } = 0;

		[JsonPropertyName("rating")]
		public double Rating { get; set; } = 0.0;

		[JsonPropertyName("downloadCount")]
		public int DownloadCount { get; set; } = 0;

		[JsonPropertyName("customData")]
		public Dictionary<string, object> CustomData { get; set; } = new();

		[JsonPropertyName("entryScene")]
		public string EntryScene { get; set; }

		[JsonPropertyName("configFile")]
		public string ConfigFile { get; set; }
	}

	public class PackageInstallState
	{
		public string PackageId { get; set; }
		public PackageStatus Status { get; set; } = PackageStatus.Available;
		public string InstalledVersion { get; set; }
		public string InstalledPath { get; set; }
		public DateTime InstallDate { get; set; }
		public DateTime LastPlayed { get; set; }
		public float DownloadProgress { get; set; }
		public string ErrorMessage { get; set; }
		public Dictionary<string, object> SaveData { get; set; } = new();
	}

	public class PackageRegistry
	{
		[JsonPropertyName("version")]
		public string Version { get; set; } = "1.0.0";

		[JsonPropertyName("packages")]
		public List<PackageData> Packages { get; set; } = new();

		[JsonPropertyName("featuredPackages")]
		public List<string> FeaturedPackages { get; set; } = new();

		[JsonPropertyName("categories")]
		public List<PackageCategory> Categories { get; set; } = new();
	}

	public class PackageCategory
	{
		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("description")]
		public string Description { get; set; }

		[JsonPropertyName("icon")]
		public string Icon { get; set; }

		[JsonPropertyName("packageIds")]
		public List<string> PackageIds { get; set; } = new();
	}
}
