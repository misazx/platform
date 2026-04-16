using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoguelikeGame.Server.Data;
using RoguelikeGame.Server.Models;

namespace RoguelikeGame.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FriendsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FriendsController> _logger;

        public FriendsController(ApplicationDbContext context, ILogger<FriendsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetFriendsList()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized(new { success = false, message = "未登录" });

            try
            {
                var friendships = await _context.Friendships
                    .Where(f => f.Status == FriendshipStatus.Accepted &&
                                (f.RequesterId == userId || f.AddresseeId == userId))
                    .ToListAsync();

                var friendIds = friendships
                    .Select(f => f.RequesterId == userId ? f.AddresseeId : f.RequesterId)
                    .Distinct()
                    .ToList();

                var friends = await _context.Users
                    .Where(u => friendIds.Contains(u.Id))
                    .Select(u => new
                    {
                        id = u.Id,
                        username = u.Username,
                        level = u.Level,
                        isOnline = u.IsOnline
                    })
                    .ToListAsync();

                var pendingRequests = await _context.Friendships
                    .Where(f => f.AddresseeId == userId && f.Status == FriendshipStatus.Pending)
                    .Join(_context.Users, f => f.RequesterId, u => u.Id, (f, u) => new
                    {
                        id = f.Id,
                        requesterId = u.Id,
                        username = u.Username,
                        level = u.Level
                    })
                    .ToListAsync();

                return Ok(new { success = true, friends, pendingRequests });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取好友列表失败");
                return StatusCode(500, new { success = false, message = "服务器内部错误" });
            }
        }

        [HttpPost("request")]
        public async Task<IActionResult> SendFriendRequest([FromBody] SendFriendRequestRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized(new { success = false, message = "未登录" });

            try
            {
                var targetUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == request.TargetUsername || u.Id == request.TargetUsername);

                if (targetUser == null)
                    return NotFound(new { success = false, message = "用户不存在" });

                if (targetUser.Id == userId)
                    return BadRequest(new { success = false, message = "不能添加自己为好友" });

                var existing = await _context.Friendships
                    .FirstOrDefaultAsync(f =>
                        (f.RequesterId == userId && f.AddresseeId == targetUser.Id) ||
                        (f.RequesterId == targetUser.Id && f.AddresseeId == userId));

                if (existing != null)
                {
                    if (existing.Status == FriendshipStatus.Accepted)
                        return BadRequest(new { success = false, message = "已经是好友" });
                    if (existing.Status == FriendshipStatus.Pending)
                        return BadRequest(new { success = false, message = "好友请求已发送" });
                }

                _context.Friendships.Add(new Friendship
                {
                    RequesterId = userId,
                    AddresseeId = targetUser.Id,
                    Status = FriendshipStatus.Pending
                });

                await _context.SaveChangesAsync();
                _logger.LogInformation("好友请求: {UserId} -> {TargetId}", userId, targetUser.Id);

                return Ok(new { success = true, message = "好友请求已发送" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送好友请求失败");
                return StatusCode(500, new { success = false, message = "服务器内部错误" });
            }
        }

        [HttpPost("{requestId}/accept")]
        public async Task<IActionResult> AcceptFriendRequest(string requestId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized(new { success = false, message = "未登录" });

            try
            {
                var friendship = await _context.Friendships.FindAsync(requestId);
                if (friendship == null)
                    return NotFound(new { success = false, message = "请求不存在" });

                if (friendship.AddresseeId != userId)
                    return Forbid();

                if (friendship.Status != FriendshipStatus.Pending)
                    return BadRequest(new { success = false, message = "请求已处理" });

                friendship.Status = FriendshipStatus.Accepted;
                friendship.AcceptedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("好友请求已接受: {RequestId}", requestId);

                return Ok(new { success = true, message = "已接受好友请求" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "接受好友请求失败");
                return StatusCode(500, new { success = false, message = "服务器内部错误" });
            }
        }

        [HttpDelete("{friendId}")]
        public async Task<IActionResult> RemoveFriend(string friendId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized(new { success = false, message = "未登录" });

            try
            {
                var friendship = await _context.Friendships
                    .FirstOrDefaultAsync(f =>
                        f.Status == FriendshipStatus.Accepted &&
                        ((f.RequesterId == userId && f.AddresseeId == friendId) ||
                         (f.RequesterId == friendId && f.AddresseeId == userId)));

                if (friendship == null)
                    return NotFound(new { success = false, message = "好友关系不存在" });

                _context.Friendships.Remove(friendship);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "已移除好友" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除好友失败");
                return StatusCode(500, new { success = false, message = "服务器内部错误" });
            }
        }

        [HttpPost("invite")]
        public async Task<IActionResult> InviteToRoom([FromBody] InviteToRoomRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized(new { success = false, message = "未登录" });

            try
            {
                var isFriend = await _context.Friendships
                    .AnyAsync(f => f.Status == FriendshipStatus.Accepted &&
                                   ((f.RequesterId == userId && f.AddresseeId == request.FriendId) ||
                                    (f.RequesterId == request.FriendId && f.AddresseeId == userId)));

                if (!isFriend)
                    return BadRequest(new { success = false, message = "不是好友关系" });

                _logger.LogInformation("房间邀请: {UserId} -> {FriendId}, 房间: {RoomId}", userId, request.FriendId, request.RoomId);

                return Ok(new { success = true, message = "邀请已发送" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "邀请好友失败");
                return StatusCode(500, new { success = false, message = "服务器内部错误" });
            }
        }
    }

    public class SendFriendRequestRequest
    {
        [Required]
        public string TargetUsername { get; set; } = "";
    }

    public class InviteToRoomRequest
    {
        [Required]
        public string FriendId { get; set; } = "";

        [Required]
        public string RoomId { get; set; } = "";
    }
}
