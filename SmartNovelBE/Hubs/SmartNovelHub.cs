using Microsoft.AspNetCore.SignalR;
using SmartNovelBE.Models;
using SmartNovelBE.Services;
using System.Security.Claims;

namespace SmartNovelBE.Hubs;

public class SmartNovelHub : Hub
{
    private readonly ILiveRoomManager _roomManager;

    public SmartNovelHub(ILiveRoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    public async Task<string?> CreateRoom(string novelId, string chapterId, string uid)
    {
        string roomId = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        var room = _roomManager.CreateRoom(roomId, novelId, chapterId, uid);
        
        if (room != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, room.RoomId);
            return room.RoomId;
        }
        return null;
    }

    public async Task ChangeChapter(string roomId, string newChapterId)
    {
        var room = _roomManager.GetRoom(roomId);
        var user = _roomManager.GetUserByConnectionId(Context.ConnectionId);

        if (room != null && user != null)
        {
            room.ChapterId = newChapterId;
            _roomManager.UpdateRoom(room);
            await Clients.Group(roomId).SendAsync("HostChangedChapter", newChapterId);

            // Push comments for the new chapter
            var chapterComments = room.Comments.Where(c => c.ChapterId == newChapterId).ToList();
            await Clients.Group(roomId).SendAsync("LoadRoomComments", chapterComments);
        }
    }


    public async Task<bool> JoinRoom(string roomId, string uid, string displayName, string avatarUrl)
    {
        var room = _roomManager.GetRoom(roomId);
        if (room == null) return false;
        
        var effectiveUid = Context.UserIdentifier ?? uid; // Enterprise: Trust JWT token over client payload

        var user = new LiveRoomUser
        {
            ConnectionId = Context.ConnectionId,
            Uid = effectiveUid,
            DisplayName = displayName,
            AvatarUrl = avatarUrl,
            IsHost = room.HostUid == effectiveUid
        };

        if (_roomManager.JoinRoom(roomId, user))
        {
            // BẮT BUỘC: Lấy lại state mới nhất từ Redis vì _roomManager.JoinRoom đã modify một instance khác
            room = _roomManager.GetRoom(roomId);
            if (room == null) return false;

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            
            // Send to everyone else that a new user joined
            await Clients.GroupExcept(roomId, Context.ConnectionId).SendAsync("UserJoined", user);
            await Clients.GroupExcept(roomId, Context.ConnectionId).SendAsync("UpdateUsersList", room.Users);
            
            // Explicitly send to Caller to guarantee they don't miss the initial state
            await Clients.Caller.SendAsync("UpdateUsersList", room.Users);

            // Send existing comments in the room to the new user, filtered by current chapter
            var currentComments = room.Comments.Where(c => c.ChapterId == room.ChapterId).ToList();
            await Clients.Caller.SendAsync("LoadRoomComments", currentComments);

            // Send existing chat history
            await Clients.Caller.SendAsync("LoadRoomChatHistory", room.ChatMessages);

            // Send current chapter state
            await Clients.Caller.SendAsync("RoomState", new { NovelId = room.NovelId, ChapterId = room.ChapterId, HostUid = room.HostUid });
            
            return true;
        }
        return false;
    }

    public async Task LeaveRoom(string roomId)
    {
        var user = _roomManager.GetUserByConnectionId(Context.ConnectionId);
        if (user != null && _roomManager.LeaveRoom(roomId, Context.ConnectionId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("UserLeft", user.Uid);
            
            var room = _roomManager.GetRoom(roomId);
            if (room != null)
            {
                await Clients.Group(roomId).SendAsync("UpdateUsersList", room.Users);
            }
        }
    }

    public async Task SendChatMessage(string roomId, string message)
    {
        var user = _roomManager.GetUserByConnectionId(Context.ConnectionId);
        var room = _roomManager.GetRoom(roomId);
        if (user != null && room != null)
        {
            var timestamp = DateTime.UtcNow;
            lock (room.ChatMessages)
            {
                room.ChatMessages.Add(new LiveRoomChatMessage
                {
                    User = user,
                    Message = message,
                    Timestamp = timestamp
                });
                if (room.ChatMessages.Count > 100)
                {
                    room.ChatMessages.RemoveAt(0);
                }
            }
            _roomManager.UpdateRoom(room);
            await Clients.Group(roomId).SendAsync("ReceiveChatMessage", user, message, timestamp);
        }
    }

    public async Task SyncScroll(string roomId, double scrollPercentage)
    {
        var user = _roomManager.GetUserByConnectionId(Context.ConnectionId);
        var room = _roomManager.GetRoom(roomId);
        
        // Only allow Host to sync scroll
        if (user != null && room != null && room.HostUid == user.Uid)
        {
            await Clients.GroupExcept(roomId, Context.ConnectionId).SendAsync("ReceiveScrollSync", scrollPercentage);
        }
    }

    public async Task KickUser(string roomId, string targetUid)
    {
        var user = _roomManager.GetUserByConnectionId(Context.ConnectionId);
        var room = _roomManager.GetRoom(roomId);

        if (user != null && room != null && room.HostUid == user.Uid)
        {
            var targetUser = room.Users.FirstOrDefault(u => u.Uid == targetUid);
            if (targetUser != null && _roomManager.KickUser(roomId, user.Uid, targetUid))
            {
                await Groups.RemoveFromGroupAsync(targetUser.ConnectionId, roomId);
                await Clients.Client(targetUser.ConnectionId).SendAsync("KickedFromRoom");
                await Clients.Group(roomId).SendAsync("UserLeft", targetUid);

                // Lấy lại state mới nhất sau khi kick
                room = _roomManager.GetRoom(roomId);
                if (room != null)
                {
                    await Clients.Group(roomId).SendAsync("UpdateUsersList", room.Users);
                }
            }
        }
    }

    public async Task SendParagraphComment(string roomId, ParagraphComment comment)
    {
        var room = _roomManager.GetRoom(roomId);
        if (room != null)
        {
            lock (room.Comments)
            {
                room.Comments.Add(comment);
            }
            _roomManager.UpdateRoom(room);
        }

        // Broadcast the comment to everyone ELSE in the room
        await Clients.GroupExcept(roomId, Context.ConnectionId).SendAsync("ReceiveParagraphComment", comment);
    }

    public async Task DeleteParagraphComment(string roomId, string commentId)
    {
        var room = _roomManager.GetRoom(roomId);
        var user = _roomManager.GetUserByConnectionId(Context.ConnectionId);

        if (room != null && user != null)
        {
            ParagraphComment? commentToRemove = null;
            lock (room.Comments)
            {
                commentToRemove = room.Comments.FirstOrDefault(c => c.Id == commentId);
                // Allow ANYONE to delete the comment
                if (commentToRemove != null)
                {
                    room.Comments.Remove(commentToRemove);
                }
            }

            if (commentToRemove != null)
            {
                _roomManager.UpdateRoom(room);
                // Broadcast deletion to EVERYONE in the room (including the caller)
                await Clients.Group(roomId).SendAsync("CommentDeleted", commentId);
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = _roomManager.GetUserByConnectionId(Context.ConnectionId);
        if (user != null)
        {
            // Find which room they were in and leave
            foreach (var room in _roomManager.GetAllRooms())
            {
                if (room.Users.Any(u => u.ConnectionId == Context.ConnectionId))
                {
                    await LeaveRoom(room.RoomId);
                }
            }
        }
        await base.OnDisconnectedAsync(exception);
    }
}
