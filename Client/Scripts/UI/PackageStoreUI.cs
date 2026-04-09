using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using RoguelikeGame.Packages;

namespace RoguelikeGame.UI
{
	public partial class PackageStoreUI : Control
	{
		private ScrollContainer _scrollContainer;
		private VBoxContainer _packageList;
		private LineEdit _searchBar;
		private GridContainer _categoryTabs;
		private Label _titleLabel;
		private Button _refreshButton;
		private PanelContainer _detailPanel;

		private string _selectedCategory = "all";
		private string _searchQuery = "";
		private PackageData _selectedPackage;

		public event System.Action<PackageData> PackageSelected;
		public event System.Action<PackageData> PackageLaunchRequested;
		public event System.Action<PackageData> DownloadRequested;

		public override void _Ready()
		{
			SetupUI();
			ConnectSignals();
			RefreshPackageList();
		}

		private void SetupUI()
		{
			SetAnchorsPreset(Control.LayoutPreset.FullRect);

			var mainVBox = new VBoxContainer();
			mainVBox.AddThemeConstantOverride("separation", 15);
			AddChild(mainVBox);

			CreateHeader(mainVBox);
			CreateSearchAndCategories(mainVBox);
			CreatePackageGrid(mainVBox);
			CreateDetailPanel();
		}

		private void CreateHeader(VBoxContainer parent)
		{
			var headerHBox = new HBoxContainer();
			headerHBox.AddThemeConstantOverride("separation", 20);
			parent.AddChild(headerHBox);

			_titleLabel = new Label
			{
				Text = "🎮 游戏包商店",
				HorizontalAlignment = HorizontalAlignment.Center
			};
			_titleLabel.AddThemeFontSizeOverride("font_size", 32);
			headerHBox.AddChild(_titleLabel);

			var spacer = new Control();
			spacer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			headerHBox.AddChild(spacer);

			_refreshButton = new Button
			{
				Text = "🔄 刷新"
			};
			_refreshButton.CustomMinimumSize = new Vector2(100, 40);
			headerHBox.AddChild(_refreshButton);
		}

		private void CreateSearchAndCategories(VBoxContainer parent)
		{
			var searchHBox = new HBoxContainer();
			searchHBox.AddThemeConstantOverride("separation", 10);
			parent.AddChild(searchHBox);

			_searchBar = new LineEdit
			{
				PlaceholderText = "搜索游戏包...",
				CustomMinimumSize = new Vector2(400, 35)
			};
			searchHBox.AddChild(_searchBar);

			_categoryTabs = new GridContainer
			{
				Columns = -1
			};
			_categoryTabs.AddThemeConstantOverride("h_separation", 10);
			searchHBox.AddChild(_categoryTabs);

			CreateCategoryButton("全部", "all", true);
			CreateCategoryButton("官方", "official");
			CreateCategoryButton("社区", "community");
			CreateCategoryButton("DLC", "dlc");
		}

		private void CreateCategoryButton(string label, string categoryId, bool selected = false)
		{
			var btn = new Button { Text = label };
			btn.Pressed += () => OnCategorySelected(categoryId);
			btn.ToggleMode = true;
			btn.ButtonPressed = selected;
			_categoryTabs.AddChild(btn);
		}

		private void CreatePackageGrid(VBoxContainer parent)
		{
			_scrollContainer = new ScrollContainer
			{
				SizeFlagsVertical = Control.SizeFlags.ExpandFill
			};
			parent.AddChild(_scrollContainer);

			_packageList = new VBoxContainer
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
			};
			_packageList.AddThemeConstantOverride("separation", 12);
			_scrollContainer.AddChild(_packageList);
		}

		private void CreateDetailPanel()
		{
			_detailPanel = new PanelContainer
			{
				Visible = false,
				ZIndex = 100,
				MouseFilter = MouseFilterEnum.Stop
			};

			var style = new StyleBoxFlat
			{
				BgColor = new Color(0.1f, 0.1f, 0.15f, 0.98f),
				CornerRadiusTopLeft = 20,
				CornerRadiusTopRight = 20,
				CornerRadiusBottomLeft = 20,
				CornerRadiusBottomRight = 20,
				BorderWidthLeft = 3,
				BorderWidthRight = 3,
				BorderWidthTop = 3,
				BorderWidthBottom = 3,
				BorderColor = new Color(0.4f, 0.6f, 0.8f, 0.9f),
				ContentMarginTop = 25,
				ContentMarginBottom = 25,
				ContentMarginLeft = 25,
				ContentMarginRight = 25
			};
			_detailPanel.AddThemeStyleboxOverride("panel", style);

			AddChild(_detailPanel);
		}

		private void ConnectSignals()
		{
			_searchBar.TextChanged += OnSearchTextChanged;
			_refreshButton.Pressed += OnRefreshPressed;

			if (PackageManager.Instance != null)
			{
				PackageManager.Instance.PackageListUpdated += RefreshPackageList;
				PackageManager.Instance.PackageDownloadProgress += OnDownloadProgress;
				PackageManager.Instance.PackageInstalled += OnPackageInstalled;
				PackageManager.Instance.PackageError += OnPackageError;
			}
		}

		public void RefreshPackageList()
		{
			foreach (var child in _packageList.GetChildren())
			{
				child.QueueFree();
			}

			List<PackageData> packagesToShow;

			if (_selectedCategory == "all")
			{
				packagesToShow = new List<PackageData>(PackageManager.Instance.AvailablePackages.Values);
			}
			else if (_selectedCategory == "featured")
			{
				packagesToShow = PackageManager.Instance.GetFeaturedPackages();
			}
			else
			{
				packagesToShow = PackageManager.Instance.GetPackagesByCategory(_selectedCategory);
			}

			if (!string.IsNullOrEmpty(_searchQuery))
			{
				var searchResults = PackageManager.Instance.SearchPackages(_searchQuery);
				packagesToShow = packagesToShow.Intersect(searchResults).ToList();
			}

			foreach (var package in packagesToShow)
			{
				var card = CreatePackageCard(package);
				_packageList.AddChild(card);
			}

			GD.Print($"[PackageStoreUI] Displayed {packagesToShow.Count} packages");
		}

		private Control CreatePackageCard(PackageData package)
		{
			var state = PackageManager.Instance.GetPackageState(package.Id);
			var isInstalled = PackageManager.Instance.IsPackageInstalled(package.Id);
			var canLaunch = PackageManager.Instance.CanLaunchPackage(package.Id);

			var card = new PanelContainer
			{
				CustomMinimumSize = new Vector2(750, 140)
			};

			var cardStyle = new StyleBoxFlat
			{
				BgColor = isInstalled ? new Color(0.15f, 0.2f, 0.15f, 0.95f) : new Color(0.12f, 0.12f, 0.18f, 0.95f),
				CornerRadiusTopLeft = 12,
				CornerRadiusTopRight = 12,
				CornerRadiusBottomLeft = 12,
				CornerRadiusBottomRight = 12,
				BorderWidthLeft = 2,
				BorderWidthRight = 2,
				BorderWidthTop = 2,
				BorderWidthBottom = 2,
				BorderColor = canLaunch ? new Color(0.3f, 0.7f, 0.3f, 0.8f) : new Color(0.4f, 0.5f, 0.6f, 0.6f),
				ContentMarginTop = 15,
				ContentMarginBottom = 15,
				ContentMarginLeft = 15,
				ContentMarginRight = 15
			};
			card.AddThemeStyleboxOverride("panel", cardStyle);

			var hBox = new HBoxContainer();
			hBox.AddThemeConstantOverride("separation", 15);
			card.AddChild(hBox);

			var iconTexture = TryLoadIcon(package.IconPath ?? package.ThumbnailPath);
			var icon = new TextureRect
			{
				Texture = iconTexture,
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
				CustomMinimumSize = new Vector2(90, 90),
				MouseFilter = MouseFilterEnum.Ignore
			};
			hBox.AddChild(icon);

			var infoVBox = new VBoxContainer
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				MouseFilter = MouseFilterEnum.Ignore
			};
			infoVBox.AddThemeConstantOverride("separation", 5);
			hBox.AddChild(infoVBox);

			var nameLabel = new Label
			{
				Text = $"{package.Name} {(isInstalled ? "✓" : "")}",
				MouseFilter = MouseFilterEnum.Ignore
			};
			nameLabel.AddThemeFontSizeOverride("font_size", 20);
			infoVBox.AddChild(nameLabel);

			var descLabel = new Label
			{
				Text = package.Description,
				AutowrapMode = TextServer.AutowrapWordSmart,
				MouseFilter = MouseFilterEnum.Ignore,
				CustomMinimumSize = new Vector2(400, 30)
			};
			descLabel.AddThemeFontSizeOverride("font_size", 13);
			infoVBox.AddChild(descLabel);

			var tagsHBox = new HBoxContainer
			{
				MouseFilter = MouseFilterEnum.Ignore
			};
			tagsHBox.AddThemeConstantOverride("separation", 8);
			infoVBox.AddChild(tagsHBox);

			foreach (var tag in package.Tags.Take(3))
			{
				var tagLabel = new Label
				{
					Text = $"#{tag}",
					MouseFilter = MouseFilterEnum.Ignore
				};
				tagLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.7f, 0.9f));
				tagLabel.AddThemeFontSizeOverride("font_size", 11);
				tagsHBox.AddChild(tagLabel);
			}

			var actionVBox = new VBoxContainer
			{
				Alignment = BoxContainer.AlignmentMode.Center
			};
			actionVBox.AddThemeConstantOverride("separation", 10);
			hBox.AddChild(actionVBox);

			Button actionButton;

			if (canLaunch)
			{
				actionButton = new Button { Text = "▶ 启动" };
				actionButton.Pressed += () => OnLaunchPackage(package);
			}
			else if (state?.Status == PackageStatus.Downloading)
			{
				actionButton = new Button { Text = $"{state.DownloadProgress:P0}%" };
				actionButton.Disabled = true;
			}
			else if (isInstalled)
			{
				actionButton = new Button { Text = "✓ 已安装" };
				actionButton.Disabled = true;
			}
			else if (package.IsFree)
			{
				actionButton = new Button { Text = "免费下载" };
				actionButton.Pressed += () => OnDownloadPackage(package);
			}
			else
			{
				actionButton = new Button { Text = $"¥{package.Price:F2}" };
				actionButton.Pressed += () => OnDownloadPackage(package);
			}

			actionButton.CustomMinimumSize = new Vector2(120, 38);
			actionVBox.AddChild(actionButton);

			var detailBtn = new Button { Text = "详情" };
			detailBtn.CustomMinimumSize = new Vector2(80, 28);
			detailBtn.Pressed += () => ShowPackageDetail(package);
			actionVBox.AddChild(detailBtn);

			card.GuiInput += @event =>
			{
				if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
				{
					PackageSelected?.Invoke(package);
				}
			};

			return card;
		}

		private Texture2D TryLoadIcon(string path)
		{
			try
			{
				if (!string.IsNullOrEmpty(path) && ResourceLoader.Exists(path))
				{
					return GD.Load<Texture2D>(path);
				}
			}
			catch { }

			return null;
		}

		private void ShowPackageDetail(PackageData package)
		{
			_selectedPackage = package;

			foreach (var child in _detailPanel.GetChildren())
			{
				child.QueueFree();
			}

			var vbox = new VBoxContainer
			{
				CustomMinimumSize = new Vector2(600, 500)
			};
			vbox.AddThemeConstantOverride("separation", 15);
			_detailPanel.AddChild(vbox);

			var closeBtn = new Button { Text = "✕ 关闭" };
			closeBtn.Pressed += () => _detailPanel.Visible = false;
			vbox.AddChild(closeBtn);

			var title = new Label
			{
				Text = package.Name,
				MouseFilter = MouseFilterEnum.Ignore
			};
			title.AddThemeFontSizeOverride("font_size", 28);
			vbox.AddChild(title);

			var info = new RichTextLabel
			{
				MouseFilter = MouseFilterEnum.Ignore,
				FitContent = true
			};
			info.AppendText($"[b]版本:[/b] {package.Version}\n");
			info.AppendText($"[b]作者:[/b] {package.Author}\n");
			info.AppendText($"[b]大小:[/b] {FormatFileSize(package.FileSize)}\n");
			info.AppendText($"[b]评分:[/b] {package.Rating:F1}/5.0 ⭐ ({package.DownloadCount} 次下载)\n\n");
			info.AppendText($"[b]描述:[/b]\n{package.Description}\n\n");

			if (package.Features.Count > 0)
			{
				info.AppendText("[b]特性:[/b]\n");
				foreach (var feature in package.Features)
				{
					info.AppendText($"• {feature}\n");
				}
			}

			vbox.AddChild(info);

			_detailPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
			_detailPanel.Visible = true;
		}

		private string FormatFileSize(long bytes)
		{
			string[] sizes = { "B", "KB", "MB", "GB" };
			int order = 0;
			double size = bytes;
			while (size >= 1024 && order < sizes.Length - 1)
			{
				order++;
				size /= 1024;
			}
			return $"{size:0.##} {sizes[order]}";
		}

		private void OnCategorySelected(string category)
		{
			_selectedCategory = category;

			foreach (Button btn in _categoryTabs.GetChildren())
			{
				btn.ButtonPressed = (btn.Text switch
				{
					"全部" => category == "all",
					"官方" => category == "official",
					"社区" => category == "community",
					"DLC" => category == "dlc",
					_ => false
				});
			}

			RefreshPackageList();
		}

		private void OnSearchTextChanged(string text)
		{
			_searchQuery = text;
			RefreshPackageList();
		}

		private async void OnRefreshPressed()
		{
			_refreshButton.Disabled = true;
			_refreshButton.Text = "⏳ 刷新中...";

			await PackageManager.Instance.RefreshPackageListAsync();

			_refreshButton.Disabled = false;
			_refreshButton.Text = "🔄 刷新";
		}

		private void OnLaunchPackage(PackageData package)
		{
			PackageLaunchRequested?.Invoke(package);
			PackageManager.Instance.LaunchPackage(package.Id);
		}

		private void OnDownloadPackage(PackageData package)
		{
			DownloadRequested?.Invoke(package);
			PackageManager.Instance.DownloadPackageAsync(package.Id);
		}

		private void OnDownloadProgress(string packageId, float progress)
		{
			RefreshPackageList();
		}

		private void OnPackageInstalled(string packageId)
		{
			RefreshPackageList();
			GD.Print($"[PackageStoreUI] Package installed: {packageId}");
		}

		private void OnPackageError(string packageId, string error)
		{
			GD.PrintErr($"[PackageStoreUI] Error with package {packageId}: {error}");
		}
	}
}
