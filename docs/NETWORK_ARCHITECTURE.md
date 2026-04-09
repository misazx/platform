# RoguelikeGame 网络系统架构设计文档

## 📋 项目概述

### 目标
为现有的 Godot 4 C# Roguelike 卡牌游戏构建完整的多人联网系统，支持：
- **局域网 (LAN)**: 低延迟本地对战
- **蓝牙 (Bluetooth)**: 移动设备近距离联机
- **外网 (Internet)**: 全球在线多人游戏
- **单机模式**: 保持现有功能完全兼容

### 技术栈
- **引擎**: Godot 4.6.1
- **语言**: C# (.NET 8.0)
- **网络协议**:
  - ENet (UDP) - 局域网/外网主协议
  - WebSocket (TCP) - Web端/备用通道
  - WebRTC (P2P) - 蓝牙/NAT穿透
  - 自定义信令服务器 - 房间匹配/中继

---

## 🏗️ 架构设计

### 整体架构图

```
┌─────────────────────────────────────────────────────────────┐
│                      客户端 (Client)                         │
│  ┌───────────┐ ┌──────────┐ ┌───────────┐ ┌──────────────┐ │
│  │ GameUI    │ │ GameManager│ │NetworkMgr │ │OfflineMode  │ │
│  │ (现有)    │ │(扩展)     │ │(新增)     │ │(新增)       │ │
│  └─────┬─────┘ └────┬─────┘ └─────┬─────┘ └──────┬───────┘ │
│        │            │             │               │         │
│        └────────────┴─────────────┴───────────────┘         │
│                            │                                │
│              ┌─────────────▼─────────────┐                  │
│              │   ConnectionManager      │                  │
│              │  (多协议适配器)           │                  │
│              └─────────────┬─────────────┘                  │
│                            │                                │
│  ┌─────────────────────────┼───────────────────────────┐   │
│  │         Protocol Adapters                           │   │
│  │  ┌──────────┐ ┌──────────┐ ┌──────────┐            │   │
│  │  │LANAdapter│ │BTAdapter │ │NetAdapter│            │   │
│  │  │(ENet)    │ │(WebRTC)  │ │(ENet+WS) │            │   │
│  │  └──────────┘ └──────────┘ └──────────┘            │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            │
                    ┌───────▼────────┐
                    │   信令服务器    │
                    │ (Signaling)    │
                    │  - 认证        │
                    │  - 匹配        │
                    │  - 房间管理    │
                    │  - 中继(可选)  │
                    └───────┬────────┘
                            │
              ┌─────────────┼─────────────┐
              │             │             │
     ┌────────▼──────┐ ┌───▼────┐ ┌──────▼──────┐
     │  游戏服务器    │ │ LAN    │ │ P2P直连     │
     │  (专用服务器)  │ │ 直连   │ │ (WebRTC)    │
     │               │ │        │ │             │
     │ - 战斗同步     │ │        │ │ - 蓝牙      │
     │ - 状态验证     │ │        │ │ - NAT穿透   │
     └───────────────┘ └────────┘ └─────────────┘
```

### 核心组件设计

#### 1. NetworkManager (网络管理器) - 单例
```csharp
// Scripts/Network/NetworkManager.cs
public partial class NetworkManager : SingletonBase<NetworkManager>
{
    // 连接状态
    public enum ConnectionMode { Offline, LAN, Bluetooth, Online }
    public enum NetworkState { Disconnected, Connecting, Connected, InRoom, InGame }

    public ConnectionMode CurrentMode { get; private set; }
    public NetworkState State { get; private set; }

    // 核心组件
    private ConnectionManager _connectionManager;
    private AuthSystem _authSystem;
    private RoomManager _roomManager;
    private GameSessionManager _sessionManager;

    // 事件
    [Signal] public delegate void ConnectionStateChangedEventHandler(NetworkState newState);
    [Signal] public delegate void AuthenticationCompletedEventHandler(bool success, string userId);
    [Signal] public delegate void RoomJoinedEventHandler(RoomInfo room);
}
```

#### 2. ConnectionManager (连接管理器)
```csharp
// Scripts/Network/Core/ConnectionManager.cs
public class ConnectionManager : Node
{
    // 协议适配器接口
    public interface IConnectionAdapter
    {
        Task<bool> ConnectAsync(string address, int port);
        Task DisconnectAsync();
        Task SendAsync(byte[] data, DeliveryMode mode);
        event Action<byte[]> OnDataReceived;
        event Action OnConnected;
        event Action OnDisconnected;
        event Action<string> OnError;
    }

    // 具体实现
    private ENetConnectionAdapter _enetAdapter;      // 局域网/外网
    private WebSocketConnectionAdapter _wsAdapter;    // Web端
    private WebRTCConnectionAdapter _webrtcAdapter;  // 蓝牙/P2P

    public IConnectionMode ActiveAdapter { get; private set; }

    // 根据模式选择适配器
    public async Task<bool> ConnectAsync(ConnectionMode mode, string target)
    {
        switch (mode)
        {
            case ConnectionMode.LAN:
                ActiveAdapter = _enetAdapter;
                break;
            case ConnectionMode.Bluetooth:
                ActiveAdapter = _webrtcAdapter;
                break;
            case ConnectionMode.Online:
                // 智能选择：优先ENet，降级到WebSocket
                ActiveAdapter = await TryENetFirst(target) ?? _wsAdapter;
                break;
        }
        return await ActiveAdapter.ConnectAsync(target);
    }
}
```

#### 3. 信令服务器架构 (Server-Side)

##### 技术选型
- **框架**: ASP.NET Core 8.0 (与客户端统一技术栈)
- **通信**: SignalR (实时双向通信) + REST API
- **数据库**: SQLite (轻量部署) / PostgreSQL (生产环境)
- **部署**: Docker 容器化，支持本地/云部署

##### 服务器模块结构
```
Server/
├── RoguelikeGame.Server/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Controllers/
│   │   ├── AuthController.cs          # 认证API
│   │   ├── RoomController.cs          # 房间管理API
│   │   ├── MatchmakingController.cs   # 匹配系统
│   │   └── HealthController.cs        # 健康检查
│   ├── Hubs/
│   │   ├── GameHub.cs                 # SignalR实时通信
│   │   └── LobbyHub.cs                # 大厅聊天/通知
│   ├── Services/
│   │   ├── AuthService.cs             # 认证服务(JWT)
│   │   ├── RoomService.cs             # 房间逻辑
│   │   ├── MatchmakingService.cs      # ELO匹配算法
│   │   └── RelayService.cs            # 数据中继(可选)
│   ├── Models/
│   │   ├── User.cs
│   │   ├── Room.cs
│   │   └── GameSession.cs
│   └── Middleware/
│       ├── RateLimitingMiddleware.cs
│       └── AuthenticationMiddleware.cs
├── Dockerfile
├── docker-compose.yml
└── README.md
```

---

## 🔌 协议详细设计

### 1. 局域网 (LAN) - ENet UDP

**特点**:
- 超低延迟 (< 10ms)
- 无需互联网
- 适合本地聚会/电竞馆

**流程**:
```
主机A (Server)          主机B (Client)
    │                        │
    ├─ 广播: "我是主机" ──────►│
    │                        ├─ 发现主机列表
    │◄─ 请求加入房间 ─────────┤
    ├─ 接受连接 ──────────────►│
    │                        │
    ▼                        ▼
  开始P2P对战 (ENet直连)
```

**实现要点**:
- 使用 UDP 广播发现局域网内的主机
- 主机同时充当服务器角色
- 支持最多 4 人本地联机

### 2. 蓝牙 (Bluetooth) - WebRTC

**特点**:
- 无需WiFi/互联网
- 移动设备友好
- 适合户外/通勤场景

**流程**:
```
设备A                     设备B
  │                          │
  ├─ Bluetooth广播 ─────────►│
  │                          ├─ 扫描发现
  │◄─ 配对请求 ──────────────┤
  ├─ 建立WebRTC DataChannel ►│
  │                          │
  ▼                          ▼
      P2P数据通道建立
```

**实现要点**:
- 使用 WebRTC DataChannel API (Godot 4 支持)
- 需要信令服务器交换 SDP/ICE 候选
- 限制 1v1 对战（蓝牙带宽有限）

### 3. 外网 (Internet) - 混合协议

**特点**:
- 全球互联
- 需要账号系统
- 支持大规模匹配

**架构选项**:

#### 方案A: 专用服务器 (推荐用于正式版)
```
Client ──► 信令服务器 ──► 游戏服务器集群
                │
                ├─ 认证
                ├─ 匹配
                ├─ 房间分配
                └─ 状态同步
```

**优势**:
- 防作弊 (Server-Authoritative)
- 稳定的游戏体验
- 可扩展

#### 方案B: P2P + 中继 (适合独立开发阶段)
```
Client A ◄──► 信令服务器(中继) ◄──► Client B
                │
                ├─ NAT穿透协助
                ├─ 失败时数据中继
                └─ 最小化服务器成本
```

**优势**:
- 服务器成本低
- 开发快速
- 适合小规模测试

**推荐**: 初期使用方案B快速验证，后期迁移至方案A

---

## 🔄 核心业务流程

### 1. 用户认证流程

```
┌──────────┐    ┌──────────┐    ┌──────────┐
│  Client  │    │ Signaling│    │ Database │
└────┬─────┘    └────┬─────┘    └────┬─────┘
     │               │               │
     │── 注册/登录 ──►│               │
     │               │── 查询用户 ───►│
     │               │◄─ 用户数据 ───┤
     │◄─ JWT Token ──│               │
     │               │               │
     ▼               ▼               ▼
```

**API设计**:
```csharp
POST /api/auth/register
{
  "username": "player123",
  "password": "hashed_password",
  "email": "optional@email.com"
}
→ Response: { "token": "jwt...", "userId": "uuid" }

POST /api/auth/login
{
  "username": "player123",
  "password": "hashed_password"
}
→ Response: { "token": "jwt...", "userId": "uuid" }
```

### 2. 房间管理流程

```
创建房间:
Client ──POST /api/rooms/create──► Server
         { "name": "我的房间", "maxPlayers": 4, "mode": "pvp" }
         ◄── Response: { "roomId": "abc123", "hostToken": "..." }

加入房间:
Client B ──POST /api/rooms/join──► Server
           { "roomId": "abc123" }
           ◄── Response: { "success": true, "players": [...] }

房间列表:
Client ──GET /api/rooms/list?mode=pvp&page=1──► Server
         ◄── Response: { "rooms": [...], "total": 42 }
```

**房间状态机**:
```
Waiting ──► Full(满员) ──► Ready(所有玩家就绪) ──► Playing ──► Finished
   │                                                           │
   └──────────────── 取消/解散 ────────────────────────────────┘
```

### 3. 游戏会话流程

```
开始游戏:
1. Host 发送 START_GAME 信号
2. 所有客户端加载战斗场景
3. 同步随机种子 (确保确定性)
4. 开始 tick-based 同步循环

游戏进行中 (每帧/每tick):
Client A ──[Input: 打出攻击卡]──► Server
                                      │
                                      ├─ 验证合法性
                                      ├─ 计算结果
                                      └─ 广播状态更新
                                   │
Client B ◄──[StateSync: HP-20]─────┘
Client C ◄──[StateSync: HP-20]─────┘

结束游戏:
1. 检测胜利/失败条件
2. 广播 GAME_OVER + 结果数据
3. 更新统计数据
4. 返回大厅或断开连接
```

**状态同步策略**:
- **关键状态**: 服务器权威 (HP、金币、卡组)
- **输入**: 客户端发送，服务器验证 (出牌顺序、技能使用)
- **动画**: 客户端插值预测 (减少感知延迟)

---

## 🎮 单机模式兼容性设计

### 设计原则
**网络功能作为可选层，不影响核心游戏逻辑**

### 实现方式

```csharp
// 扩展 GameManager
public partial class GameManager : Node
{
    private bool _isOnlineMode = false;

    public bool IsOnlineMode => _isOnlineMode;

    // 原有方法保持不变
    public void StartNewRun(string characterId, uint seed = 0)
    {
        // 单机逻辑不变
        _currentRun = new RunData { Seed = seed, CharacterId = characterId };
        // ...
    }

    // 新增网络模式入口
    public void StartOnlineRun(string characterId, string roomId, uint syncSeed)
    {
        _isOnlineMode = true;
        _currentRun = new RunData
        {
            Seed = syncSeed,  // 使用服务器下发的同步种子
            CharacterId = characterId,
            CustomData = { ["RoomId"] = roomId }
        };

        // 注册网络回调
        NetworkManager.Instance.RegisterGameStateSyncHandler(OnNetworkStateUpdate);
    }

    private void OnNetworkStateUpdate(GameState state)
    {
        // 仅在网络模式下处理远程状态更新
        if (_isOnlineMode)
        {
            ApplyRemoteState(state);
        }
    }
}
```

### 模式切换机制

```csharp
// MainMenu 扩展
public partial class EnhancedMainMenu : Control
{
    private void _OnSinglePlayerPressed()
    {
        NetworkManager.Instance.SetMode(ConnectionMode.Offline);
        GetTree().ChangeSceneToFile("res://Scenes/CharacterSelect.tscn");
    }

    private void _OnMultiplayerPressed()
    {
        ShowMultiplayerPanel(); // 显示局域网/在线/蓝牙选项
    }

    private async void _OnLANPressed()
    {
        var success = await NetworkManager.Instance.ConnectLANAsync();
        if (success)
        {
            ShowLobbyScene(); // 显示局域网大厅
        }
    }
}
```

---

## 📁 项目文件结构

### 客户端新增文件
```
Scripts/
├── Network/                          # 新增网络模块
│   ├── NetworkManager.cs            # 网络管理器(单例)
│   ├── Core/
│   │   ├── ConnectionManager.cs     # 连接管理器
│   │   ├── ProtocolAdapters/
│   │   │   ├── IConnectionAdapter.cs
│   │   │   ├── ENetConnectionAdapter.cs
│   │   │   ├── WebSocketConnectionAdapter.cs
│   │   │   └── WebRTCConnectionAdapter.cs
│   │   ├── PacketSerializer.cs      # 数据包序列化
│   │   └── NetworkClock.cs          # 网络时钟同步
│   ├── Auth/
│   │   └── AuthSystem.cs            # 认证系统
│   ├── Rooms/
│   │   ├── RoomManager.cs           # 房间管理器
│   │   └── Models/
│   │       └── RoomInfo.cs
│   ├── Session/
│   │   ├── GameSessionManager.cs    # 游戏会话管理
│   │   └── StateSync.cs             # 状态同步器
│   └── Discovery/
│       ├── LANDiscoveryService.cs   # 局域网发现
│       └── BluetoothDiscoveryService.cs
│
├── UI/Panels/                       # 新增UI面板
│   ├── MultiplayerPanel.cs          # 多人模式选择面板
│   ├── LobbyPanel.cs               # 大厅界面
│   ├── CreateRoomPanel.cs          # 创建房间面板
│   └── RoomListPanel.cs            # 房间列表
│
└── Core/
    └── GameManager.cs               # (扩展) 添加网络支持
```

### 服务器工程 (独立解决方案)
```
Server/
├── RoguelikeGame.Server.sln
├── RoguelikeGame.Server/
│   ├── RoguelikeGame.Server.csproj
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Controllers/
│   ├── Hubs/
│   ├── Services/
│   ├── Models/
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   └── Migrations/
│   └── Properties/
├── tests/
│   └── RoguelikeGame.Server.Tests/
├── Dockerfile
├── docker-compose.yml
└── README.md
```

---

## 🔐 安全性设计

### 1. 认证安全
- JWT Token (有效期 24小时)
- 密码 bcrypt 加密存储
- Rate Limiting 防暴力破解
- HTTPS 强制 (生产环境)

### 2. 游戏安全
- Server-Authoritative 模式
- 输入验证 (防止非法操作)
- 反作弊检测 (异常数据包频率)
- 关键操作日志记录

### 3. 网络安全
- 数据包加密 (AES-256, 可选)
- 防DDoS攻击 (限流 + 黑名单)
- CORS 配置 (仅允许信任域名)

---

## ⚡ 性能优化

### 1. 带宽优化
- **增量同步**: 只传输变化的状态字段
- **压缩**: 使用 MessagePack 或 Protobuf 替代 JSON
- **优先级队列**: 关键数据优先传输
- **批量发送**: 合并多个小数据包

### 2. 延迟优化
- **客户端预测**: 本地立即响应，服务器校正
- **插值平滑**: 其他玩家位置插值显示
- **Tick速率**: 动态调整 (30Hz 正常, 60Hz 战斗中)
- **预加载**: 提前加载下一个房间的资源

### 3. 服务器性能
- **连接池**: 数据库连接复用
- **缓存**: Redis 缓存热门房间信息
- **水平扩展**: 微服务架构 (后期)
- **CDN加速**: 静态资源分发

---

## 🚀 部署方案

### 本地开发
```bash
# 启动信令服务器
cd Server
dotnet run --launch-profile "https"

# 启动客户端 (Godot Editor)
# F5 运行游戏
```

### 生产环境 (Docker)
```yaml
# docker-compose.yml
version: '3.8'
services:
  signaling-server:
    build: ./Server
    ports:
      - "5000:5000"    # HTTP API
      - "5001:5001"    # HTTPS
    environment:
      - ConnectionStrings__DefaultConnection=...
      - JWT__Secret=...
    depends_on:
      - db

  db:
    image: postgres:15-alpine
    volumes:
      - postgres_data:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine

volumes:
  postgres_data:
```

### 云部署选项
- **AWS**: ECS + RDS + ElastiCache
- **Azure**: App Service + Azure SQL + Redis Cache
- **阿里云**: ACK + RDS + Redis
- **低成本方案**: Railway / Render / Fly.io

---

## 📊 测试策略

### 单元测试
- 协议适配器单元测试
- 数据包序列化/反序列化测试
- 房间状态机测试
- 认证逻辑测试

### 集成测试
- 客户端-服务器完整流程测试
- 多客户端并发连接测试
- 断线重连机制测试
- 状态同步一致性测试

### 性能测试
- 延迟模拟 (0ms ~ 500ms)
- 丢包率测试 (0% ~ 20%)
- 最大并发连接数测试
- 带宽占用测试

### 手动测试用例
- [ ] 局域网创建/加入房间
- [ ] 蓝牙配对和对战
- [ ] 外网注册/登录
- [ ] 在线匹配和游戏
- [ ] 断线重连恢复
- [ ] 单机模式正常工作
- [ ] 不同网络切换无缝过渡

---

## 📈 开发路线图

### Phase 1: 基础框架 (第1-2周)
- [x] 架构设计和文档
- [ ] 实现 NetworkManager 单例
- [ ] 实现 ConnectionManager + ENet适配器
- [ ] 搭建信令服务器基础框架
- [ ] 实现基本的连接/断开功能

### Phase 2: 核心功能 (第3-4周)
- [ ] 实现认证系统 (JWT)
- [ ] 实现房间 CRUD 操作
- [ ] 局域网发现和直连
- [ ] 基础 UI 面板 (大厅/房间列表)

### Phase 3: 多协议支持 (第5-6周)
- [ ] WebSocket 适配器 (Web端支持)
- [ ] WebRTC 适配器 (蓝牙/P2P)
- [ ] 外网连接和中继逻辑
- [ ] 匹配系统基础版

### Phase 4: 游戏会话 (第7-8周)
- [ ] 状态同步机制
- [ ] 战斗场景网络化
- [ ] 输入验证和反作弊
- [ ] 断线重连

### Phase 5: 优化和完善 (第9-10周)
- [ ] 性能优化 (压缩/批处理)
- [ ] 安全加固
- [ ] UI/UX打磨
- [ ] 文档和测试完善

---

## 💡 关键决策记录

### 决策1: 为什么选择 Client-Server 而非纯P2P?
**理由**:
- 防作弊需求 (卡牌游戏需要严格的状态验证)
- 便于实现匹配系统和排行榜
- 后期可扩展为 MMO 或锦标赛模式
- 便于数据统计和分析

**权衡**:
- 服务器成本增加
- 延略高于纯P2P (可通过边缘节点缓解)

### 决策2: 为什么使用混合协议?
**理由**:
- 不同场景有不同需求 (局域网需要低延迟，外网需要可靠性)
- Godot 4 原生支持多种协议，切换成本低
- 为未来平台扩展留有余地 (Web端需要WebSocket)

### 决策3: 为什么保持单机模式?
**理由**:
- 降低新玩家门槛 (无需联网即可体验)
- 离线场景支持 (飞机、地铁等)
- 开发和调试便利
- 尊重原有用户群体

---

## 📚 参考资源

- [Godot 4 官方网络文档](https://docs.godotengine.org/en/4.3/tutorials/networking/high_level_multiplayer.html)
- [Nebula Networking Framework](https://github.com/Heavenlode/Nebula) (参考其Tick-based同步设计)
- [ASP.NET Core SignalR 文档](https://docs.microsoft.com/aspnet/core/signalr)
- [WebRTC for Godot](https://github.com/godotengine/godot-demo-projects/tree/master/networking/webrtc_signaling)

---

**文档版本**: v1.0
**最后更新**: 2026-04-10
**作者**: AI Assistant
