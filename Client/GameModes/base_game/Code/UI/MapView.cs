using Godot;
using System.Collections.Generic;
using RoguelikeGame.Core;
using RoguelikeGame.Generation;
using RoguelikeGame.Audio;

namespace RoguelikeGame.UI
{
	public partial class MapView : Control, IUIScreen
	{
		private Control _nodeLayer;
		private Node2D _connectionLayer;
		private Label _floorLabel;
		private Label _goldLabel;
		private Button _backButton;
		private readonly List<MapNodeUI> _nodeUIs = new();
		private FloorMap _currentMap;
		private HashSet<int> _visitedNodeIds = new();
		private HashSet<int> _reachableNodeIds = new();

		private static HashSet<int> _persistentVisited = new();
		private static HashSet<int> _persistentReachable = new();

		public static void ResetPersistentState()
		{
			_persistentVisited.Clear();
			_persistentReachable.Clear();
			GD.Print("[MapView] Persistent state reset for new game");
		}

		public event System.Action<MapNode> NodeSelected;

		public override void _Ready()
		{
			GD.Print("========== [MapView] _Ready() START ==========");

			SetupNodeReferences();
			SetupSignals();

			GD.Print("[MapView] Creating immediate test visuals...");
			CreateTestVisuals();

			GD.Print("[MapView] Scheduling map load via CallDeferred...");
			CallDeferred(nameof(LoadMapFromGameManager));

			GD.Print("========== [MapView] _Ready() END ==========");
		}

		private void CreateTestVisuals()
		{
			if (_nodeLayer == null)
			{
				GD.PushError("[MapView] CreateTestVisuals: _nodeLayer is NULL!");
				return;
			}

			var testRect = new ColorRect
			{
				Name = "TestDebugRect",
				Color = new Color(0f, 1f, 0f, 0.5f),
				Position = new Vector2(100, 200),
				Size = new Vector2(80, 80),
				MouseFilter = MouseFilterEnum.Ignore
			};
			_nodeLayer.AddChild(testRect);
			GD.Print($"[MapView] Test rect added at {testRect.Position}, size {testRect.Size}, parent: {_nodeLayer.Name}");

			var testLabel = new Label
			{
				Text = "MAP NODES HERE",
				Position = new Vector2(80, 180),
				ZIndex = 100,
				MouseFilter = MouseFilterEnum.Ignore
			};
			testLabel.AddThemeFontSizeOverride("font_size", 16);
			testLabel.Modulate = Colors.Yellow;
			_nodeLayer.AddChild(testLabel);
		}

		private void SetupNodeReferences()
		{
			var mapContainer = GetNodeOrNull<Control>("MapContainer");

			if (mapContainer != null)
			{
				GD.Print($"[MapView] MapContainer FOUND: {mapContainer.Name}, size={mapContainer.Size}");

				_nodeLayer = mapContainer.GetNodeOrNull<Control>("NodeLayer");
				if (_nodeLayer == null)
				{
					GD.PushWarning("[MapView] NodeLayer not found, creating...");
					_nodeLayer = new Control { Name = "NodeLayer", MouseFilter = MouseFilterEnum.Ignore };
					mapContainer.AddChild(_nodeLayer);
				}
				_nodeLayer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
				GD.Print($"[MapView] NodeLayer ready: {_nodeLayer.Name}, size will fill MapContainer");

				_connectionLayer = mapContainer.GetNodeOrNull<Node2D>("ConnectionLayer");
				if (_connectionLayer == null)
				{
					_connectionLayer = new Node2D { Name = "ConnectionLayer" };
					mapContainer.AddChild(_connectionLayer);
				}
				GD.Print($"[MapView] ConnectionLayer ready: {_connectionLayer.GetType().Name}");
			}
			else
			{
				GD.PushError("[MapView] CRITICAL: MapContainer NOT found!");
				_nodeLayer = new Control { Name = "NodeLayer", MouseFilter = MouseFilterEnum.Ignore };
				_connectionLayer = new Node2D { Name = "ConnectionLayer" };
				AddChild(_nodeLayer);
				AddChild(_connectionLayer);
			}

			_floorLabel = GetNodeOrNull<Label>("HeaderBar/FloorLabel");
			_goldLabel = GetNodeOrNull<Label>("HeaderBar/GoldLabel");
			_backButton = GetNodeOrNull<Button>("HeaderBar/BackButton");

			if (_backButton != null)
				_backButton.Pressed += OnBackPressed;

			var headerBar = GetNodeOrNull<Control>("HeaderBar");
			if (headerBar != null && headerBar.GetNodeOrNull<Button>("SaveButton") == null)
			{
				var saveBtn = new Button
				{
					Name = "SaveButton",
					Text = "💾 存档",
					CustomMinimumSize = new Vector2(90, 36),
					MouseFilter = MouseFilterEnum.Stop,
					Position = new Vector2(400, 5)
				};
				saveBtn.Pressed += OnSavePressed;
				headerBar.AddChild(saveBtn);
				GD.Print("[MapView] SaveButton added to HeaderBar");
			}

			GD.Print($"[MapView] Setup done - Layer:{_nodeLayer != null} BackBtn:{_backButton != null}");
		}

		private void OnSavePressed()
		{
			GD.Print("[MapView] Quick save pressed");
			var run = GameManager.Instance?.CurrentRun;
			if (run != null)
			{
				RoguelikeGame.Systems.EnhancedSaveSystem.Instance?.SaveGame(1, run);
				FloatingText.ShowStatus(this, "💾 已保存!", new Vector2(640, 30));
			}
		}

		private void SetupSignals() { }

		private void OnBackPressed()
		{
			GD.Print("[MapView] Back pressed");
			Main.Instance?.GoToMainMenu();
		}

		private void LoadMapFromGameManager()
		{
			GD.Print("---------- [MapView] LoadMapFromGameManager() START ----------");

			try
			{
				var gm = GameManager.Instance;
				GD.Print($"[MapView] GameManager.Instance: {gm != null}");

				FloorMap map = null;

				if (gm != null)
				{
					map = gm.CurrentMap;
					GD.Print($"[MapView] CurrentMap: {map != null}");
					if (map != null)
						GD.Print($"[MapView] Nodes count: {map.Nodes?.Count ?? 0}");
				}

				if (map != null && map.Nodes != null && map.Nodes.Count > 0)
				{
					GD.Print($"[MapView] Using GameManager map with {map.Nodes.Count} nodes");
					SetMap(map);
					UpdateFloor(gm.CurrentRun?.CurrentFloor ?? 1);
					UpdateGold(gm.CurrentRun?.Gold ?? 0);
				}
				else
				{
					GD.Print("[MapView] No valid map from GameManager, generating DEMO map");
					GenerateDemoMap();
				}
			}
			catch (System.Exception ex)
			{
				GD.PushError($"[MapView] EXCEPTION in LoadMapFromGameManager: {ex.Message}\n{ex.StackTrace}");
				GenerateDemoMap();
			}

			GD.Print("---------- [MapView] LoadMapFromGameManager() END ----------");
		}

		private void GenerateDemoMap()
		{
			GD.Print("[MapView] GenerateDemoMap() start");

			var demoMap = new FloorMap
			{
				FloorNumber = 1,
				FloorName = "The Exordium",
				Width = 800,
				Height = 500
			};

			int id = 0;

			var startNode = new MapNode
			{
				Id = id++, Position = new Vector2I(40, 250), Type = NodeType.Unknown,
				Status = MapNodeStatus.Available, ConnectedNodes = new List<int> { 1, 2 }
			};
			demoMap.Nodes.Add(startNode);
			demoMap.StartNodeId = 0;

			string[] enemies = { "Cultist", "JawWorm", "Louse", "Gremlin_Nob", "Cultist", "JawWorm" };

			var layer1 = new[]
			{
				new { X = 160, Y = 120, Type = NodeType.Monster },
				new { X = 160, Y = 350, Type = NodeType.Event },
			};

			var layer2 = new[]
			{
				new { X = 300, Y = 80, Type = NodeType.Monster },
				new { X = 300, Y = 250, Type = NodeType.Shop },
				new { X = 300, Y = 400, Type = NodeType.Monster },
			};

			var layer3 = new[]
			{
				new { X = 440, Y = 150, Type = NodeType.Elite },
				new { X = 440, Y = 350, Type = NodeType.Rest },
			};

			var layer4 = new[]
			{
				new { X = 580, Y = 100, Type = NodeType.Treasure },
				new { X = 580, Y = 300, Type = NodeType.Monster },
			};

			var bossNode = new { X = 720, Y = 250, Type = NodeType.Boss };

			int enemyIdx = 0;

			foreach (var n in layer1)
			{
				demoMap.Nodes.Add(new MapNode
				{
					Id = id++, Position = new Vector2I(n.X, n.Y), Type = n.Type,
					Status = MapNodeStatus.Locked,
					EnemyEncounterId = n.Type == NodeType.Monster ? enemies[enemyIdx++ % enemies.Length] : "",
					ConnectedNodes = new List<int>()
				});
			}

			foreach (var n in layer2)
			{
				demoMap.Nodes.Add(new MapNode
				{
					Id = id++, Position = new Vector2I(n.X, n.Y), Type = n.Type,
					Status = MapNodeStatus.Locked,
					EnemyEncounterId = n.Type == NodeType.Monster ? enemies[enemyIdx++ % enemies.Length] :
									   n.Type == NodeType.Elite ? "Gremlin_Nob" : "",
					ConnectedNodes = new List<int>()
				});
			}

			foreach (var n in layer3)
			{
				demoMap.Nodes.Add(new MapNode
				{
					Id = id++, Position = new Vector2I(n.X, n.Y), Type = n.Type,
					Status = MapNodeStatus.Locked,
					EnemyEncounterId = n.Type == NodeType.Elite ? "Gremlin_Nob" : "",
					ConnectedNodes = new List<int>()
				});
			}

			foreach (var n in layer4)
			{
				demoMap.Nodes.Add(new MapNode
				{
					Id = id++, Position = new Vector2I(n.X, n.Y), Type = n.Type,
					Status = MapNodeStatus.Locked,
					EnemyEncounterId = n.Type == NodeType.Monster ? enemies[enemyIdx++ % enemies.Length] : "",
					ConnectedNodes = new List<int>()
				});
			}

			demoMap.Nodes.Add(new MapNode
			{
				Id = id++, Position = new Vector2I(bossNode.X, bossNode.Y), Type = NodeType.Boss,
				Status = MapNodeStatus.Locked,
				EnemyEncounterId = "The_Guardian",
				ConnectedNodes = new List<int>()
			});
			demoMap.BossNodeId = id - 1;

			startNode.ConnectedNodes = new List<int> { 1, 2 };

			demoMap.Nodes[1].ConnectedNodes = new List<int> { 0, 3, 4 };
			demoMap.Nodes[2].ConnectedNodes = new List<int> { 0, 4, 5 };

			demoMap.Nodes[3].ConnectedNodes = new List<int> { 1, 6 };
			demoMap.Nodes[4].ConnectedNodes = new List<int> { 1, 2, 6, 7 };
			demoMap.Nodes[5].ConnectedNodes = new List<int> { 2, 7 };

			demoMap.Nodes[6].ConnectedNodes = new List<int> { 3, 4, 8 };
			demoMap.Nodes[7].ConnectedNodes = new List<int> { 4, 5, 9 };

			demoMap.Nodes[8].ConnectedNodes = new List<int> { 6, 10 };
			demoMap.Nodes[9].ConnectedNodes = new List<int> { 7, 10 };

			demoMap.Nodes[10].ConnectedNodes = new List<int> { 8, 9 };

			SetMap(demoMap);
			UpdateFloor(1);
			UpdateGold(99);

			GD.Print($"[MapView] Demo map created with {demoMap.Nodes.Count} nodes, layers: start→L1→L2→L3→L4→boss");
		}

		public void SetMap(FloorMap map)
		{
			GD.Print($"[MapView] SetMap() called, nodes: {map?.Nodes?.Count ?? 0}");

			_currentMap = map;
			ClearNodes();

			if (map?.Nodes == null || map.Nodes.Count == 0)
			{
				GD.PushWarning("[MapView] SetMap: empty/null node list!");
				return;
			}

			bool hasPersistentState = _persistentVisited.Count > 0;

			if (hasPersistentState)
			{
				_visitedNodeIds = new HashSet<int>(_persistentVisited);
				_reachableNodeIds = new HashSet<int>(_persistentReachable);

				var newlyReachable = new List<int>();
				foreach (var vid in _visitedNodeIds)
				{
					var vNode = map.Nodes.Find(n => n.Id == vid);
					if (vNode != null)
					{
						vNode.IsVisited = true;
						vNode.Status = MapNodeStatus.Completed;
						foreach (var cid in vNode.ConnectedNodes)
						{
							if (!_visitedNodeIds.Contains(cid) && !_reachableNodeIds.Contains(cid))
								newlyReachable.Add(cid);
						}
					}
				}
				foreach (var nid in newlyReachable)
				{
					_reachableNodeIds.Add(nid);
					_persistentReachable.Add(nid);
					var node = map.Nodes.Find(n => n.Id == nid);
					if (node != null) node.Status = MapNodeStatus.Available;
				}

				GD.Print($"[MapView] Restored from persistent: visited=[{string.Join(",", _visitedNodeIds)}] reachable=[{string.Join(",", _reachableNodeIds)}]");
			}
			else
			{
				_visitedNodeIds.Clear();
				_reachableNodeIds.Clear();
				_persistentVisited.Clear();
				_persistentReachable.Clear();

				var startNode = map.Nodes.Find(n => n.Id == map.StartNodeId);
				if (startNode != null)
				{
					_visitedNodeIds.Add(startNode.Id);
					_persistentVisited.Add(startNode.Id);
					startNode.IsVisited = true;
					startNode.Status = MapNodeStatus.Completed;
					foreach (var cid in startNode.ConnectedNodes)
					{
						_reachableNodeIds.Add(cid);
						_persistentReachable.Add(cid);
						var connected = map.Nodes.Find(n => n.Id == cid);
						if (connected != null) connected.Status = MapNodeStatus.Available;
					}
					GD.Print($"[MapView] Fresh init: start={startNode.Id}, reachable=[{string.Join(",", _reachableNodeIds)}]");
				}
			}

			foreach (var node in map.Nodes)
			{
				try
				{
					bool isReachable = _reachableNodeIds.Contains(node.Id);
					bool isVisited = _visitedNodeIds.Contains(node.Id);
					var nodeUI = new MapNodeUI(node, isReachable, isVisited);
					nodeUI.NodeClicked += OnNodeClicked;
					_nodeUIs.Add(nodeUI);
					_nodeLayer.AddChild(nodeUI);
					GD.Print($"[MapView] +Node {node.Id} ({node.Type}) at ({node.Position.X},{node.Position.Y}) reachable={isReachable} visited={isVisited}");
				}
				catch (System.Exception ex)
				{
					GD.PushError($"[MapView] Failed creating node {node.Id}: {ex.Message}");
				}
			}

			DrawConnections();
			GD.Print($"[MapView] SetMap complete: {_nodeUIs.Count} nodes rendered");
		}

		private void ClearNodes()
		{
			foreach (var n in _nodeUIs) n.QueueFree();
			_nodeUIs.Clear();
			if (_connectionLayer != null)
				foreach (var c in _connectionLayer.GetChildren()) c.QueueFree();
		}

		private void DrawConnections()
		{
			if (_connectionLayer == null) return;
			foreach (var node in _nodeUIs)
			{
				if (node.NodeData.ConnectedNodes == null) continue;
				foreach (var cid in node.NodeData.ConnectedNodes)
				{
					var target = _nodeUIs.Find(n => n.NodeData.Id == cid);
					if (target != null)
					{
						var line = new Line2D { Width = 3f, DefaultColor = new Color(0.6f, 0.55f, 0.45f, 0.8f), ZIndex = 0 };
						line.AddPoint(node.GetCenterPosition());
						line.AddPoint(target.GetCenterPosition());
						_connectionLayer.AddChild(line);
					}
				}
			}
		}

		private void OnNodeClicked(MapNodeUI nodeUI)
		{
			var nodeData = nodeUI.NodeData;
			GD.Print($"[MapView] Clicked: {nodeData.Type} id={nodeData.Id} reachable={_reachableNodeIds.Contains(nodeData.Id)} visited={_visitedNodeIds.Contains(nodeData.Id)}");

			if (_visitedNodeIds.Contains(nodeData.Id))
			{
				GD.Print("[MapView] Node already visited, ignoring");
				return;
			}

			if (!_reachableNodeIds.Contains(nodeData.Id))
			{
				GD.Print("[MapView] Node not reachable, ignoring");
				return;
			}

			AudioManager.Instance?.PlayButtonClick();

			_visitedNodeIds.Add(nodeData.Id);
			_persistentVisited.Add(nodeData.Id);
			_reachableNodeIds.Remove(nodeData.Id);
			_persistentReachable.Remove(nodeData.Id);
			nodeData.IsVisited = true;
			nodeData.Status = MapNodeStatus.Completed;

			foreach (var cid in nodeData.ConnectedNodes)
			{
				if (!_visitedNodeIds.Contains(cid))
				{
					_reachableNodeIds.Add(cid);
					_persistentReachable.Add(cid);
					var connected = _currentMap?.Nodes.Find(n => n.Id == cid);
					if (connected != null && connected.Status == MapNodeStatus.Locked)
						connected.Status = MapNodeStatus.Available;
				}
			}

			UpdateNodeVisuals();

			foreach (var n in _nodeUIs) n.Highlighted = (n == nodeUI);
			NodeSelected?.Invoke(nodeData);

			GameManager.Instance?.VisitNode(nodeData);

			var t = nodeData.Type;
			Main.Instance?.SetLastClickedNodeType(t);

			switch (t)
			{
				case NodeType.Monster:
				case NodeType.Elite:
				case NodeType.Boss:
					Main.Instance?.GoToCombat(nodeData.EnemyEncounterId);
					break;
				case NodeType.Shop:
					Main.Instance?.GoToShop();
					break;
				case NodeType.Rest:
					Main.Instance?.GoToRest();
					break;
				case NodeType.Event:
					Main.Instance?.GoToEvent();
					break;
				case NodeType.Treasure:
					Main.Instance?.GoToTreasure();
					break;
				default:
					GD.Print($"[MapView] Node type {t} not implemented yet");
					break;
			}
		}

		private void UpdateNodeVisuals()
		{
			foreach (var n in _nodeUIs)
			{
				bool isReachable = _reachableNodeIds.Contains(n.NodeData.Id);
				bool isVisited = _visitedNodeIds.Contains(n.NodeData.Id);
				n.SetReachableState(isReachable, isVisited);
			}
		}

		public void UpdateFloor(int f) { if (_floorLabel != null) _floorLabel.Text = $"第 {f} 层"; }
		public void UpdateGold(int g) { if (_goldLabel != null) _goldLabel.Text = $"💰 {g}"; }
		public void VisitNode(MapNode n) { var ui = _nodeUIs.Find(x => x.NodeData == n); if (ui != null) ui.Visited = true; }
		public void OnShow() { Visible = true; CallDeferred(nameof(LoadMapFromGameManager)); AudioManager.Instance?.PlayBGM("map_ambient"); }
		public void OnHide() { Visible = false; }
	}

	public partial class MapNodeUI : Control
	{
		public MapNode NodeData { get; }
		public event System.Action<MapNodeUI> NodeClicked;

		public bool Visited { set { _visited = value; UpdateAppearance(); } }
		public bool Highlighted
		{
			set
			{
				_highlighted = value;
				var s = value ? 1.25f : (_visited ? 0.9f : 1.0f);
				Scale = new Vector2(s, s);
			}
		}

		private bool _visited, _highlighted, _reachable;
		private ColorRect _bgRect;
		private Label _iconLabel;
		private Label _tooltip;

		public MapNodeUI(MapNode data, bool reachable = false, bool visited = false)
		{
			NodeData = data;
			_reachable = reachable;
			_visited = visited;
		}

		public override void _Ready()
		{
			MouseFilter = MouseFilterEnum.Stop;

			float x = NodeData.Position.X;
			float y = NodeData.Position.Y;
			Position = new Vector2(x, y);
			Size = new Vector2(56, 56);

			Color nodeColor = GetColor(NodeData.Type);
			Color borderColor = nodeColor.Lightened(0.3f);

			_bgRect = new ColorRect
			{
				Name = "BG",
				Color = new Color(0.15f, 0.13f, 0.10f, 0.95f),
				MouseFilter = MouseFilterEnum.Ignore
			};
			_bgRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			AddChild(_bgRect);

			var borderStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.15f, 0.13f, 0.10f, 0.95f),
				CornerRadiusTopLeft = 12,
				CornerRadiusTopRight = 12,
				CornerRadiusBottomLeft = 12,
				CornerRadiusBottomRight = 12,
				BorderWidthLeft = 3,
				BorderWidthRight = 3,
				BorderWidthTop = 3,
				BorderWidthBottom = 3,
				BorderColor = borderColor
			};
			AddThemeStyleboxOverride("panel", borderStyle);

			_iconLabel = new Label
			{
				Text = GetIconChar(NodeData.Type),
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				MouseFilter = MouseFilterEnum.Ignore
			};
			_iconLabel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			_iconLabel.AddThemeFontSizeOverride("font_size", 22);
			_iconLabel.Modulate = nodeColor;
			AddChild(_iconLabel);

			_tooltip = new Label
			{
				Text = GetTooltip(NodeData.Type),
				Visible = false,
				ZIndex = 200,
				Position = new Vector2(28, -50),
				MouseFilter = MouseFilterEnum.Ignore
			};
			_tooltip.AddThemeFontSizeOverride("font_size", 14);
			AddChild(_tooltip);

			GuiInput += OnGuiInput;
			MouseEntered += OnMouseEnter;
			MouseExited += OnMouseExit;

			ApplyInitialState();

			GD.Print($"[MapNodeUI] Ready: type={NodeData.Type} pos=({x},{y}) color={nodeColor} reachable={_reachable} visited={_visited}");
		}

		private void ApplyInitialState()
		{
			if (_visited)
			{
				UpdateAppearance();
			}
			else if (_reachable)
			{
				Modulate = new Color(1f, 1f, 1f, 1f);
			}
			else
			{
				Modulate = new Color(0.4f, 0.4f, 0.4f, 0.6f);
				MouseFilter = MouseFilterEnum.Ignore;
			}
		}

		public void SetReachableState(bool reachable, bool visited)
		{
			_reachable = reachable;
			_visited = visited;

			if (visited)
			{
				UpdateAppearance();
			}
			else if (reachable)
			{
				Modulate = new Color(1f, 1f, 1f, 1f);
				MouseFilter = MouseFilterEnum.Stop;
				Scale = Vector2.One;
			}
			else
			{
				Modulate = new Color(0.4f, 0.4f, 0.4f, 0.6f);
				MouseFilter = MouseFilterEnum.Ignore;
				Scale = Vector2.One;
			}
		}

		private string GetIconChar(NodeType t) => t switch
		{
			NodeType.Monster => "⚔",
			NodeType.Elite => "★",
			NodeType.Boss => "👑",
			NodeType.Event => "?",
			NodeType.Shop => "$",
			NodeType.Rest => "♥",
			NodeType.Treasure => "◆",
			_ => "●"
		};

		private Color GetColor(NodeType t) => t switch
		{
			NodeType.Monster => new Color(0.9f, 0.35f, 0.35f),
			NodeType.Elite => new Color(1f, 0.65f, 0.2f),
			NodeType.Boss => new Color(0.95f, 0.2f, 0.2f),
			NodeType.Event => new Color(0.55f, 0.8f, 0.35f),
			NodeType.Shop => new Color(0.4f, 0.7f, 1f),
			NodeType.Rest => new Color(0.35f, 0.6f, 0.35f),
			NodeType.Treasure => new Color(1f, 0.88f, 0.3f),
			_ => new Color(0.6f, 0.6f, 0.6f)
		};

		private string GetTooltip(NodeType t) => t switch
		{
			NodeType.Monster => "普通敌人", NodeType.Elite => "精英敌人", NodeType.Boss => "Boss",
			NodeType.Event => "事件", NodeType.Shop => "商店", NodeType.Rest => "休息点",
			NodeType.Treasure => "宝箱", _ => "起点"
		};

		private void UpdateAppearance()
		{
			if (!_visited) return;
			var s = new StyleBoxFlat
			{
				BgColor = new Color(0.15f, 0.22f, 0.15f, 0.8f),
				CornerRadiusTopLeft = 12, CornerRadiusTopRight = 12,
				CornerRadiusBottomLeft = 12, CornerRadiusBottomRight = 12,
				BorderWidthLeft = 2, BorderWidthRight = 2, BorderWidthTop = 2, BorderWidthBottom = 2,
				BorderColor = new Color(0.35f, 0.5f, 0.35f, 0.7f)
			};
			AddThemeStyleboxOverride("panel", s);
			Scale = new Vector2(0.9f, 0.9f);
			Modulate = new Color(0.8f, 0.8f, 0.8f, 0.85f);
		}

		private void OnMouseEnter()
		{
			if (!_reachable && !_visited) return;
			_tooltip.Visible = true;
			if (!_highlighted && !_visited)
				CreateTween().TweenProperty(this, "scale", new Vector2(1.15f, 1.15f), 0.1f);
		}

		private void OnMouseExit()
		{
			_tooltip.Visible = false;
			if (!_highlighted && !_visited && _reachable)
				CreateTween().TweenProperty(this, "scale", Vector2.One, 0.1f);
		}

		private void OnGuiInput(InputEvent ev)
		{
			if (ev is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
				NodeClicked?.Invoke(this);
		}

		public Vector2 GetCenterPosition() => Position + Size / 2;
	}
}
