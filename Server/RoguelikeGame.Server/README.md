# Roguelike Game Server

多人联机卡牌游戏服务器端工程

## 📋 技术栈

- **框架**: ASP.NET Core 8.0
- **实时通信**: SignalR
- **数据库**: SQLite (开发) / PostgreSQL (生产)
- **认证**: JWT Bearer Token
- **容器化**: Docker + Docker Compose
- **日志**: Serilog

## 🚀 快速开始

### 前置要求

- .NET 8.0 SDK
- Docker (可选，用于容器化部署)

### 本地开发模式

```bash
# 1. 进入服务器目录
cd Server/RoguelikeGame.Server

# 2. 还原依赖
dotnet restore

# 3. 运行服务器 (开发模式，自动创建SQLite数据库)
dotnet run --launch-profile "https"

# 4. 服务器将在 https://localhost:5001 启动
# Swagger UI: https://localhost:5001/swagger
```

### Docker 部署

```bash
# 1. 构建并启动容器
cd Server
docker-compose up -d --build

# 2. 查看日志
docker-compose logs -f server

# 3. 停止服务
docker-compose down
```

## 📡 API 端点

### 认证 API

| 方法 | 路径 | 描述 | 认证 |
|------|------|------|------|
| POST | `/api/auth/register` | 用户注册 | ❌ |
| POST | `/api/auth/login` | 用户登录 | ❌ |
| GET | `/api/auth/me` | 获取当前用户信息 | ✅ |

### 房间 API

| 方法 | 路径 | 描述 | 认证 |
|------|------|------|------|
| POST | `/api/rooms/create` | 创建房间 | ✅ |
| POST | `/api/rooms/{id}/join` | 加入房间 | ✅ |
| POST | `/api/rooms/{id}/leave` | 离开房间 | ✅ |
| GET | `/api/rooms/list` | 获取公开房间列表 | ✅ |
| GET | `/api/rooms/{id}` | 获取房间详情 | ✅ |
| POST | `/api/rooms/{id}/ready` | 设置准备状态 | ✅ |
| POST | `/api/rooms/{id}/start` | 开始游戏 | ✅ |

### SignalR Hubs

- **LobbyHub**: `https://localhost:5001/hubs/lobby`
  - 大厅聊天、用户在线状态、房间通知

- **GameHub**: `https://localhost:5001/hubs/game`
  - 游戏内实时通信（待实现）

## 🔧 配置说明

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=roguelike.db"  // 数据库连接字符串
  },
  "Jwt": {
    "Issuer": "RoguelikeGameServer",     // JWT签发者
    "Audience": "RoguelikeGameClient",   // JWT接收者
    "Key": "your-secret-key",            // JWT签名密钥(至少32字符)
    "ExpireDays": 7                      // Token有效期(天)
  },
  "ServerSettings": {
    "MaxRooms": 100,                     // 最大房间数
    "MaxPlayersPerRoom": 4,              // 每个房间最大玩家数
    "HeartbeatInterval": 30,             // 心跳间隔(秒)
    "MatchmakingTimeout": 60             // 匹配超时(秒)
  }
}
```

### 环境变量 (Docker部署)

```bash
export JWT_SECRET_KEY="your-production-secret-key-at-least-32-characters-long"
docker-compose up -d
```

## 🏗️ 项目结构

```
Server/RoguelikeGame.Server/
├── Controllers/
│   ├── AuthController.cs        # 认证API (注册/登录)
│   └── RoomController.cs         # 房间管理API
├── Hubs/
│   ├── GameHub.cs               # 游戏实时通信 (待实现)
│   └── LobbyHub.cs              # 大厅聊天和通知
├── Services/
│   ├── AuthService.cs           # 认证业务逻辑
│   ├── RoomService.cs           # 房间管理逻辑
│   └── MatchmakingService.cs    # 匹配系统 (待实现)
├── Models/
│   ├── User.cs                  # 用户数据模型
│   └── Room.cs                  # 房间数据模型
├── Data/
│   └── ApplicationDbContext.cs   # EF Core数据库上下文
├── Program.cs                   # 应用程序入口
├── appsettings.json             # 配置文件
├── Dockerfile                   # Docker构建文件
└── README.md                    # 本文档
```

## 🔐 安全特性

- ✅ JWT Token 认证
- ✅ 密码 BCrypt 加密存储
- ✅ CORS 配置 (开发环境允许所有来源)
- ✅ API 授权验证
- ⚠️ 生产环境需要配置 HTTPS 和限制 CORS

## 🧪 测试

```bash
# 运行单元测试 (待添加)
dotnet test

# 使用 curl 测试 API:

# 注册用户
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","password":"123456","email":"test@example.com"}'

# 登录
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","password":"123456"}'

# 创建房间 (需要替换 YOUR_TOKEN)
curl -X POST http://localhost:5000/api/rooms/create \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{"name":"TestRoom","mode":"PvP","maxPlayers":4}'
```

## 📊 监控和日志

- 日志文件位置: `logs/server-{date}.log`
- Docker 日志: `docker-compose logs -f server`
- Swagger UI: `https://localhost:5001/swagger` (仅开发环境)

## 🚢 生产部署建议

1. **修改配置**:
   - 更改 JWT 密钥为强随机密钥
   - 配置 PostgreSQL 连接字符串
   - 设置 HTTPS 证书
   - 限制 CORS 来源

2. **扩展性**:
   - 使用 Redis 缓存热门房间
   - 水平扩展多个服务器实例
   - 使用负载均衡器

3. **监控**:
   - 集成 Prometheus + Grafana
   - 设置告警规则
   - 监控关键指标 (连接数、延迟、错误率)

## 📄 License

MIT License

## 👥 贡献

欢迎提交 Issue 和 Pull Request！
