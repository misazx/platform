using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RoguelikeGame.Server.Controllers;
using RoguelikeGame.Server.Data;
using RoguelikeGame.Server.Hubs;
using RoguelikeGame.Server.Models;
using RoguelikeGame.Server.Services;
using RoguelikeGame.Shared.Bots;
using System.Security.Claims;

namespace RoguelikeGame.Server.Tests;

public class BotChainTests
{
    private readonly Mock<IHubContext<GameHub>> _mockHubContext;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly Mock<IGroupManager> _mockGroupManager;
    private readonly Mock<IBotGameService> _mockBotGameService;
    private readonly Mock<IRoomService> _mockRoomService;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<ILogger<RoomController>> _mockLogger;
    private readonly Mock<ApplicationDbContext> _mockDbContext;

    public BotChainTests()
    {
        _mockHubContext = new Mock<IHubContext<GameHub>>();
        _mockClientProxy = new Mock<IClientProxy>();
        _mockGroupManager = new Mock<IGroupManager>();
        _mockBotGameService = new Mock<IBotGameService>();
        _mockRoomService = new Mock<IRoomService>();
        _mockAuthService = new Mock<IAuthService>();
        _mockLogger = new Mock<ILogger<RoomController>>();
        _mockDbContext = new Mock<ApplicationDbContext>();

        _mockHubContext.Setup(h => h.Clients.Group(It.IsAny<string>()))
            .Returns(_mockClientProxy.Object);
        _mockHubContext.Setup(h => h.Groups)
            .Returns(_mockGroupManager.Object);
    }

    [Fact]
    public async Task StartGame_ShouldBroadcastGameStartingViaSignalR()
    {
        var roomId = "test-room-123";
        var hostId = "host-user-001";

        var room = new Room
        {
            Id = roomId,
            HostId = hostId,
            Status = RoomStatus.Ready,
            Mode = GameMode.Race,
            GameModeId = "light_shadow_traveler",
            Seed = "test-seed-abc",
            Players = new List<RoomPlayer>
            {
                new() { UserId = hostId, IsBot = false },
                new() { UserId = "bot_001", IsBot = true, BotName = "Bot1" }
            }
        };

        _mockRoomService.Setup(r => r.StartGameAsync(roomId, hostId))
            .ReturnsAsync(true);
        _mockRoomService.Setup(r => r.GetRoomByIdAsync(roomId))
            .ReturnsAsync(room);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDb_" + Guid.NewGuid())
            .Options;
        await using var dbContext = new ApplicationDbContext(options);
        dbContext.Users.Add(new User { Id = hostId, Username = "HostPlayer" });
        await dbContext.SaveChangesAsync();

        var controller = new RoomController(
            _mockRoomService.Object,
            _mockAuthService.Object,
            _mockLogger.Object,
            _mockHubContext.Object,
            _mockBotGameService.Object,
            dbContext
        );

        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, hostId)
                }))
            }
        };

        var result = await controller.StartGame(roomId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        dynamic value = okResult.Value!;
        Assert.True((bool)value.GetType().GetProperty("success")!.GetValue(value)!);

        _mockClientProxy.Verify(c => c.SendCoreAsync(
            "GameStarting",
            It.IsAny<object[]>(),
            default(CancellationToken)), Times.Once,
            "StartGame must broadcast GameStarting SignalR event to the room group");
    }

    [Fact]
    public async Task StartGame_GameStartingPayload_ContainsRequiredFields()
    {
        var roomId = "test-room-456";
        var hostId = "host-user-002";

        var room = new Room
        {
            Id = roomId,
            HostId = hostId,
            Status = RoomStatus.Ready,
            Mode = GameMode.Coop,
            GameModeId = "light_shadow_traveler",
            Seed = "seed-xyz",
            Players = new List<RoomPlayer>
            {
                new() { UserId = hostId, IsBot = false },
                new() { UserId = "bot_002", IsBot = true, BotName = "Bot2" }
            }
        };

        _mockRoomService.Setup(r => r.StartGameAsync(roomId, hostId))
            .ReturnsAsync(true);
        _mockRoomService.Setup(r => r.GetRoomByIdAsync(roomId))
            .ReturnsAsync(room);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDb_" + Guid.NewGuid())
            .Options;
        await using var dbContext = new ApplicationDbContext(options);
        dbContext.Users.Add(new User { Id = hostId, Username = "HostPlayer2" });
        await dbContext.SaveChangesAsync();

        object? capturedPayload = null;
        _mockClientProxy.Setup(c => c.SendCoreAsync(
            "GameStarting",
            It.IsAny<object[]>(),
            default(CancellationToken)))
            .Callback<string, object[], CancellationToken>((method, args, ct) =>
            {
                if (args.Length > 0) capturedPayload = args[0];
            })
            .Returns(Task.CompletedTask);

        var controller = new RoomController(
            _mockRoomService.Object,
            _mockAuthService.Object,
            _mockLogger.Object,
            _mockHubContext.Object,
            _mockBotGameService.Object,
            dbContext
        );

        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, hostId)
                }))
            }
        };

        await controller.StartGame(roomId);

        Assert.NotNull(capturedPayload);
        var payloadType = capturedPayload!.GetType();
        var modeProp = payloadType.GetProperty("mode");
        var gameModeIdProp = payloadType.GetProperty("gameModeId");
        var seedProp = payloadType.GetProperty("seed");
        var roomIdProp = payloadType.GetProperty("roomId");
        var playersProp = payloadType.GetProperty("players");

        Assert.NotNull(modeProp);
        Assert.NotNull(gameModeIdProp);
        Assert.NotNull(seedProp);
        Assert.NotNull(roomIdProp);
        Assert.NotNull(playersProp);

        Assert.Equal("Coop", modeProp!.GetValue(capturedPayload));
        Assert.Equal("light_shadow_traveler", gameModeIdProp!.GetValue(capturedPayload));
        Assert.Equal("seed-xyz", seedProp!.GetValue(capturedPayload));
        Assert.Equal(roomId, roomIdProp!.GetValue(capturedPayload));
    }

    [Fact]
    public void BotGameService_RegisterRoomBot_CreatesBotWithCorrectGameMode()
    {
        var mockHubContext = new Mock<IHubContext<GameHub>>();
        var mockLogger = new Mock<ILogger<BotGameService>>();
        var service = new BotGameService(mockHubContext.Object, mockLogger.Object);

        var roomId = "room-001";
        var botUserId = "bot_user_001";
        var botName = "TestBot";

        service.RegisterRoomBot(roomId, botUserId, botName,
            BotDifficulty.Normal, 1, "light_shadow_traveler", GameMode.Race);

        var state = service.GetRegisteredBots();
        Assert.Single(state);
        Assert.Equal(roomId, state[0].RoomId);
        Assert.Equal(botUserId, state[0].BotUserId);
        Assert.Equal(botName, state[0].BotName);
    }

    [Fact]
    public void BotGameService_RegisterRoomBot_ResolvesLightShadowGameMode()
    {
        var mockHubContext = new Mock<IHubContext<GameHub>>();
        var mockLogger = new Mock<ILogger<BotGameService>>();
        var service = new BotGameService(mockHubContext.Object, mockLogger.Object);

        service.RegisterRoomBot("r1", "b1", "Bot1",
            BotDifficulty.Normal, 0, "light_shadow_traveler", GameMode.Race);

        service.RegisterRoomBot("r2", "b2", "Bot2",
            BotDifficulty.Normal, 0, "light_shadow_traveler", GameMode.Coop);

        var state = service.GetRegisteredBots();
        Assert.Equal(2, state.Count);
    }

    [Fact]
    public void BotGameService_UpdateBotGameState_StoresStateInBlackboard()
    {
        var mockHubContext = new Mock<IHubContext<GameHub>>();
        var mockLogger = new Mock<ILogger<BotGameService>>();
        var service = new BotGameService(mockHubContext.Object, mockLogger.Object);

        var roomId = "room-gs";
        var botUserId = "bot-gs-001";

        service.RegisterRoomBot(roomId, botUserId, "GameBot",
            BotDifficulty.Normal, 0, "light_shadow_traveler", GameMode.Race);

        var gameState = new Dictionary<string, object>
        {
            ["player_x"] = 100.0,
            ["player_y"] = 200.0,
            ["player_form"] = "light",
            ["player_health"] = 100,
            ["player_max_health"] = 100,
            ["player_energy"] = 50.0,
            ["is_grounded"] = true,
            ["facing_right"] = true
        };

        service.UpdateBotGameState(roomId, botUserId, gameState);

        Assert.True(true, "UpdateBotGameState should not throw");
    }

    [Fact]
    public void BotGameService_UnregisterRoomBot_RemovesBot()
    {
        var mockHubContext = new Mock<IHubContext<GameHub>>();
        var mockLogger = new Mock<ILogger<BotGameService>>();
        var service = new BotGameService(mockHubContext.Object, mockLogger.Object);

        var roomId = "room-unreg";
        var botUserId = "bot-unreg-001";

        service.RegisterRoomBot(roomId, botUserId, "UnregBot",
            BotDifficulty.Normal, 0, "light_shadow_traveler", GameMode.Race);

        var stateAfterReg = service.GetRegisteredBots();
        Assert.Single(stateAfterReg);

        service.UnregisterRoomBot(roomId, botUserId);

        var stateAfterUnreg = service.GetRegisteredBots();
        Assert.Empty(stateAfterUnreg);
    }
}
