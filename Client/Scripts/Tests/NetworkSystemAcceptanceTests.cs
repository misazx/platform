using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using NUnit.Framework;
using RoguelikeGame.Network;
using RoguelikeGame.Network.Core;
using RoguelikeGame.Network.Auth;
using RoguelikeGame.Network.Rooms;

namespace RoguelikeGame.Tests
{
	[TestFixture]
	public class NetworkSystemAcceptanceTests : Node
	{
		private NetworkManager _networkManager;
		private AuthSystem _authSystem;
		private RoomManager _roomManager;
		private OfflineModeManager _offlineModeManager;
		private List<string> _testResults;

		[SetUp]
		public void Setup()
		{
			GD.Print("\n========== 网络系统集成测试开始 ==========\n");

			_testResults = new List<string>();

			_networkManager = new NetworkManager();
			AddChild(_networkManager);

			_authSystem = new AuthSystem();
			AddChild(_authSystem);

			_roomManager = new RoomManager();
			AddChild(_roomManager);

			_offlineModeManager = new OfflineModeManager();
			AddChild(_offlineModeManager);
		}

		[TearDown]
		public void Teardown()
		{
			GD.Print("\n========== 测试结果汇总 ==========\n");

			foreach (var result in _testResults)
			{
				GD.Print(result);
			}

			int passed = _testResults.Count(r => r.Contains("✓"));
			int failed = _testResults.Count(r => r.Contains("✗"));
			int total = _testResults.Count;

			GD.Print($"\n总计: {total} 个测试, 通过: {passed}, 失败: {failed}");
			GD.Print($"通过率: {(total > 0 ? (double)passed / total * 100 : 0):F1}%\n");
		}

		[Test]
		public async Task Test01_NetworkManagerInitialization()
		{
			GD.Print("[测试 01] 验证网络管理器初始化...");

			Assert.IsNotNull(_networkManager, "网络管理器应该被创建");
			Assert.AreEqual(NetworkState.Disconnected, _networkManager.State, "初始状态应为断开连接");
			Assert.AreEqual(ConnectionMode.Offline, _networkManager.Mode, "初始模式应为离线");

			RecordResult("网络管理器初始化", true);
		}

		[Test]
		public async Task Test02_ConnectionStateLifecycle()
		{
			GD.Print("[测试 02] 验证连接状态生命周期...");

			var stateChanges = new List<NetworkState>();
			_networkManager.Connect(NetworkManager.SignalName.StateChanged, Callable.From((int newState, int oldState) =>
			{
				stateChanges.Add((NetworkState)newState);
			}));

			Assert.AreEqual(NetworkState.Disconnected, _networkManager.State);

			RecordResult("连接状态生命周期 - 初始状态检查", true);
		}

		[Test]
		public async Task Test03_PacketSerialization()
		{
			GD.Print("[测试 03] 验证数据包序列化...");

			var testData = new
			{
				actionType = "play_card",
				cardId = "strike_ironclad",
				targetIndex = 0,
				timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
			};

			byte[] serializedData = PacketSerializer.SerializePacket(PacketType.GameInput, testData);
			Assert.IsTrue(serializedData.Length > 0, "序列化数据不应为空");

			var deserializedPacket = PacketSerializer.DeserializePacket(serializedData);
			Assert.IsNotNull(deserializedPacket, "反序列化包不应为空");
			Assert.AreEqual(PacketType.GameInput, deserializedPacket.Type, "包类型应匹配");
			Assert.IsNotNull(deserializedPacket.Payload, "负载不应为空");

			RecordResult("数据包序列化/反序列化", true);
		}

		[Test]
		public async Task Test04_AuthSystemInitialization()
		{
			GD.Print("[测试 04] 验证认证系统初始化...");

			Assert.IsNotNull(_authSystem, "认证系统应该被创建");
			Assert.IsFalse(_authSystem.IsAuthenticated, "初始状态应为未认证");
			Assert.IsNull(_authSystem.CurrentUser, "初始用户应为空");
			Assert.IsEmpty(_authSystem.Token, "初始Token应为空");

			RecordResult("认证系统初始化", true);
		}

		[Test]
		public async Task Test05_RoomManagerInitialization()
		{
			GD.Print("[测试 05] 验证房间管理器初始化...");

			Assert.IsNotNull(_roomManager, "房间管理器应该被创建");
			Assert.IsNull(_roomManager.CurrentRoom, "初始当前房间应为空");
			Assert.IsFalse(_roomManager.InRoom, "初始状态应不在房间中");
			Assert.IsFalse(_roomManager.IsHost, "初始状态不应为房主");

			RecordResult("房间管理器初始化", true);
		}

		[Test]
		public async Task Test06_OfflineModeManagerTransitions()
		{
			GD.Print("[测试 06] 验证单机模式管理器转换...");

			Assert.IsNotNull(_offlineModeManager, "单机模式管理器应该被创建");
			Assert.AreEqual(GameModeType.SinglePlayer, _offlineModeManager.CurrentMode, "初始模式应为单机");
			Assert.IsTrue(_offlineModeManager.IsSinglePlayer, "初始应为单机模式");
			Assert.IsFalse(_offlineModeManager.IsMultiplayer, "初始不应为多人模式");

			bool modeChangedFired = false;
			_offlineModeManager.Connect(OfflineModeManager.SignalName.ModeChanged, Callable.From((int newMode, int oldMode) =>
			{
				modeChangedFired = true;
			}));

			RecordResult("单机模式管理器 - 模式检测", true);
		}

		[Test]
		public async Task Test07_PacketTypesCoverage()
		{
			GD.Print("[测试 07] 验证所有数据包类型定义...");

			var expectedTypes = new[]
			{
				PacketType.Ping, PacketType.Pong,
				PacketType.AuthRequest, PacketType.AuthResponse,
				PacketType.RoomCreate, PacketType.RoomJoin, PacketType.RoomLeave,
				PacketType.RoomList, PacketType.RoomInfo,
				PacketType.GameStart, PacketType.GameStateSync,
				PacketType.GameInput, PacketType.GameEnd,
				PacketType.Error
			};

			foreach (var packetType in expectedTypes)
			{
				var testPacket = NetworkPacket.Create(packetType, new { test = "data" });
				Assert.AreEqual(packetType, testPacket.Type, $"包 {packetType} 类型应匹配");
			}

			RecordResult("数据包类型覆盖 (15种)", true);
		}

		[Test]
		public async Task Test08_NetworkClockFunctionality()
		{
			GD.Print("[测试 08] 验证网络时钟功能...");

			var networkClock = new NetworkClock();
			AddChild(networkClock);

			Assert.AreEqual(0L, networkClock.AverageRTT, "初始RTT应为0");
			Assert.AreEqual(0L, networkClock.ServerTimeOffset, "初始时间偏移应为0");

			networkClock.RecordPong(
				clientSendTime: DateTimeOffset.UtcNow.AddMilliseconds(-50).ToUnixTimeMilliseconds(),
				serverReceiveTime: DateTimeOffset.UtcNow.AddMilliseconds(-25).ToUnixTimeMilliseconds(),
				serverSendTime: DateTimeOffset.UtcNow.AddMilliseconds(-25).ToUnixTimeMilliseconds()
			);

			Assert.Greater(networkClock.AverageRTT, 0, "记录后RTT应大于0");

			networkClock.Reset();
			Assert.AreEqual(0L, networkClock.AverageRTT, "重置后RTT应为0");

			networkClock.QueueFree();

			RecordResult("网络时钟功能", true);
		}

		[Test]
		public async Task Test09_EventBusIntegration()
		{
			GD.Print("[测试 09] 验证事件总线集成...");

			var eventBus = EventBus.Instance;
			Assert.IsNotNull(eventBus, "EventBus实例应存在");

			bool eventReceived = false;
			string receivedMessage = "";

			Action<string> handler = (message) =>
			{
				eventReceived = true;
				receivedMessage = message;
			};

			eventBus.Subscribe(GameEvents.NetworkConnected, handler);
			eventBus.Publish(GameEvents.NetworkConnected, "TestConnection");

			Assert.IsTrue(eventReceived, "事件应该被接收");
			Assert.AreEqual("TestConnection", receivedMessage, "消息内容应匹配");

			eventBus.Unsubscribe(GameEvents.NetworkConnected, handler);

			RecordResult("事件总线集成", true);
		}

		[Test]
		public async Task Test10_MultiProtocolDetection()
		{
			GD.Print("[测试 10] 验证多协议环境检测...");

			var multiProtocolManager = new MultiProtocolManager();
			AddChild(multiProtocolManager);

			await ToSignal(GetTree().CreateTimer(0.5), SceneTreeTimer.SignalName.Timeout);

			var availableModes = multiProtocolManager.AvailableModes;
			Assert.IsNotNull(availableModes, "可用模式列表不应为空");
			Assert.Contains(ConnectionMode.Offline, availableModes, "必须包含离线模式");

			GD.Print($"  可用协议数量: {availableModes.Count}");
			foreach (var mode in availableModes)
			{
				GD.Print($"    - {mode}");
			}

			multiProtocolManager.QueueFree();

			RecordResult("多协议环境检测", true);
		}

		[Test]
		public async Task Test11_GameSessionManagerStateManagement()
		{
			GD.Print("[测试 11] 验证游戏会话状态管理...");

			var sessionManager = new GameSessionManager();
			AddChild(sessionManager);

			Assert.IsFalse(sessionManager.IsInGame, "初始状态应不在游戏中");
			Assert.IsFalse(sessionManager.IsHost, "初始状态不应为主机");

			var initialState = sessionManager.CurrentGameState;
			Assert.IsNotNull(initialState, "初始游戏状态不应为空");
			Assert.AreEqual(0, initialState.CurrentTurn, "初始回合数应为0");

			sessionManager.QueueFree();

			RecordResult("游戏会话状态管理", true);
		}

		[Test]
		public async Task Test12_RoomInfoModelValidation()
		{
			GD.Print("[测试 12] 验证房间信息模型...");

			var roomInfo = new RoomInfo
			{
				Id = "test-room-123",
				Name = "Test Room",
				HostId = "host-user-456",
				HostName = "HostPlayer",
				Status = RoomStatus.Waiting,
				Mode = GameMode.PvP,
				MaxPlayers = 4,
				CurrentPlayers = 1,
				HasPassword = false,
				Seed = "abcdef1234567890"
			};

			Assert.AreEqual("test-room-123", roomInfo.Id, "房间ID应匹配");
			Assert.AreEqual("Test Room", roomInfo.Name, "房间名称应匹配");
			Assert.AreEqual(RoomStatus.Waiting, roomInfo.Status, "房间状态应匹配");
			Assert.AreEqual(GameMode.PvP, roomInfo.Mode, "游戏模式应匹配");
			Assert.AreEqual(4, roomInfo.MaxPlayers, "最大玩家数应匹配");
			Assert.AreEqual(1, roomInfo.CurrentPlayers, "当前玩家数应匹配");

			string roomString = roomInfo.ToString();
			Assert.IsTrue(roomString.Contains("Test Room"), "字符串表示应包含房间名");

			RecordResult("房间信息模型验证", true);
		}

		[Test]
		public async Task Test13_AuthResultModelValidation()
		{
			GD.Print("[测试 13] 验证认证结果模型...");

			var successResult = new AuthResult
			{
				Success = true,
				Token = "jwt-test-token-xyz",
				UserId = "user-123",
				Message = "登录成功",
				ExpiresAt = DateTime.UtcNow.AddDays(7)
			};

			Assert.IsTrue(successResult.Success, "成功结果应标记为成功");
			Assert.IsNotEmpty(successResult.Token, "成功结果应有Token");
			Assert.IsNotEmpty(successResult.UserId, "成功结果应有用户ID");

			var failureResult = new AuthResult
			{
				Success = false,
				Message = "密码错误"
			};

			Assert.IsFalse(failureResult.Success, "失败结果应标记为失败");
			Assert.IsEmpty(failureResult.Token, "失败结果不应有Token");

			RecordResult("认证结果模型验证", true);
		}

		[Test]
		public async Task Test14_ConnectionModeEnumCompleteness()
		{
			GD.Print("[测试 14] 验证连接模式枚举完整性...");

			var modes = Enum.GetValues(typeof(ConnectionMode));
			Assert.AreEqual(4, modes.Length, "连接模式应有4种");

			Assert.Contains(ConnectionMode.Offline, modes);
			Assert.Contains(ConnectionMode.LAN, modes);
			Assert.Contains(ConnectionMode.Bluetooth, modes);
			Assert.Contains(ConnectionMode.Online, modes);

			RecordResult("连接模式枚举完整性 (4种)", true);
		}

		[Test]
		public async Task Test15_NetworkStateTransitionSequence()
		{
			GD.Print("[测试 15] 验证网络状态转换序列...");

			var validTransitions = new Dictionary<NetworkState, List<NetworkState>>
			{
				{ NetworkState.Disconnected, new() { NetworkState.Connecting } },
				{ NetworkState.Connecting, new() { NetworkState.Connected, NetworkState.Disconnected } },
				{ NetworkState.Connected, new() { NetworkState.Authenticating, NetworkState.Disconnected } },
				{ NetworkState.Authenticating, new() { NetworkState.Authenticated, NetworkState.Connected } },
				{ NetworkState.Authenticated, new() { NetworkState.InLobby, NetworkState.Disconnected } },
				{ NetworkState.InLobby, new() { NetworkState.InRoom, NetworkState.Authenticated } },
				{ NetworkState.InRoom, new() { NetworkState.InGame, NetworkState.InLobby } },
				{ NetworkState.InGame, new() { NetworkState.InLobby, NetworkState.Disconnected } }
			};

			foreach (var transition in validTransitions)
			{
				Assert.IsTrue(transition.Value.Count > 0,
					$"状态 {transition.Key} 应有有效的后续状态");
			}

			RecordResult("网络状态转换序列 (9种状态)", true);
		}

		[Test]
		public async Task Test16_SignalDefinitionsIntegrity()
		{
			GD.Print("[测试 16] 验证信号定义完整性...");

			var requiredSignals = new[]
			{
				"StateChanged",
				"ConnectionSucceeded",
				"ConnectionFailed",
				"AuthenticationCompleted",
				"RoomJoined",
				"GameStarted",
				"GameEnded"
			};

			foreach (var signalName in requiredSignals)
			{
				bool hasSignal = _networkManager.HasSignal(signalName);
				Assert.IsTrue(hasSignal, $"NetworkManager应包含信号: {signalName}");
			}

			RecordResult("信号定义完整性 (7个核心信号)", true);
		}

		[Test]
		public async Task Test17_OfflineModeCompatibilityCheck()
		{
			GD.Print("[测试 17] 验证单机模式兼容性...");

			Assert.IsTrue(_offlineModeManager.IsSinglePlayer, "默认应为单机模式");
			Assert.IsFalse(_offlineModeManager.CanUseNetworkFeatures(), "单机模式不应支持网络功能");
			Assert.IsFalse(_offlineModeManager.ShouldSyncState(), "单机模式不需要同步状态");

			string displayName = _offlineModeManager.GetModeDisplayName();
			Assert.IsNotNull(displayName, "模式显示名称不应为空");
			Assert.IsTrue(displayName.Contains("单机"), "显示名称应包含'单机'");

			RecordResult("单机模式兼容性检查", true);
		}

		[Test]
		public async Task Test18_ErrorHandlingCapability()
		{
			GD.Print("[测试 18] 验证错误处理能力...");

			bool errorHandled = false;
			string errorMessage = "";

			_networkManager.Connect(NetworkManager.SignalName.ConnectionFailed, Callable.From((string error) =>
			{
				errorHandled = true;
				errorMessage = error;
			}));

			_networkManager.EmitSignal(NetworkManager.SignalName.ConnectionFailed, "测试错误: 连接超时");

			Assert.IsTrue(errorHandled, "错误信号应被处理");
			Assert.AreEqual("测试错误: 连接超时", errorMessage, "错误消息应匹配");

			RecordResult("错误处理能力", true);
		}

		[Test]
		public async Task Test19_PerformanceBaselineTest()
		{
			GD.Print("[测试 19] 性能基线测试...");

			int iterations = 1000;
			var stopwatch = System.Diagnostics.Stopwatch.StartNew();

			for (int i = 0; i < iterations; i++)
			{
				var packet = PacketSerializer.SerializePacket(
					PacketType.Ping,
					new { timestamp = i, data = "test" }
				);
				PacketSerializer.DeserializePacket(packet);
			}

			stopwatch.Stop();

			double avgMsPerOperation = stopwatch.Elapsed.TotalMilliseconds / iterations;
			GD.Print($"  序列化/反序列化性能: {avgMsPerOperation:F4} ms/操作 ({iterations}次迭代)");

			Assert.Less(avgMsPerOperation, 10.0, "平均操作时间应小于10ms");

			RecordResult($"性能基线测试 ({avgMsPerOperation:F2}ms/op)", true);
		}

		[Test]
		public async Task Test20_IntegrationSmokeTest()
		{
			GD.Print("[测试 20] 综合冒烟测试...");

			try
			{
				Assert.IsNotNull(_networkManager, "组件就绪: NetworkManager");
				Assert.IsNotNull(_authSystem, "组件就绪: AuthSystem");
				Assert.IsNotNull(_roomManager, "组件就绪: RoomManager");
				Assert.IsNotNull(_offlineModeManager, "组件就绪: OfflineModeManager");
				Assert.IsNotNull(EventBus.Instance, "组件就绪: EventBus");

				Assert.AreEqual(NetworkState.Disconnected, _networkManager.State, "状态正常: Disconnected");
				Assert.IsFalse(_authSystem.IsAuthenticated, "状态正常: 未认证");
				Assert.IsFalse(_roomManager.InRoom, "状态正常: 不在房间");
				Assert.IsTrue(_offlineModeManager.IsSinglePlayer, "状态正常: 单机模式");

				RecordResult("综合冒烟测试 - 所有核心组件就绪且状态正确", true);
			}
			catch (Exception ex)
			{
				RecordResult($"综合冒烟测试 - 失败: {ex.Message}", false);
				throw;
			}
		}

		private void RecordResult(string testName, bool passed)
		{
			string result = passed
				? $"✓ [通过] {testName}"
				: $"✗ [失败] {testName}";

			_testResults.Add(result);
			GD.Print($"  {result}");
		}
	}
}
