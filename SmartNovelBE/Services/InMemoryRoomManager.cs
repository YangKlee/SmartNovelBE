using System.Collections.Concurrent;
using SmartNovelBE.Models;

namespace SmartNovelBE.Services;

public class InMemoryRoomManager : ILiveRoomManager
{
    private readonly ConcurrentDictionary<string, LiveRoom> _rooms = new();

    public LiveRoom? CreateRoom(string roomId, string novelId, string chapterId, string hostUid)
    {
        // If room already exists, don't recreate it
        if (_rooms.TryGetValue(roomId, out var existingRoom))
        {
            return existingRoom;
        }

        var room = new LiveRoom
        {
            RoomId = roomId,
            NovelId = novelId,
            ChapterId = chapterId,
            HostUid = hostUid
        };
        _rooms.TryAdd(roomId, room);
        return room;
    }

    public LiveRoom? GetRoom(string roomId)
    {
        _rooms.TryGetValue(roomId, out var room);
        return room;
    }

    public bool JoinRoom(string roomId, LiveRoomUser user)
    {
        if (_rooms.TryGetValue(roomId, out var room))
        {
            if (room.KickedUids.Contains(user.Uid))
            {
                return false; // User is banned
            }

            lock (room.Users)
            {
                if (!room.Users.Any(u => u.Uid == user.Uid))
                {
                    room.Users.Add(user);
                }
                else
                {
                    // Update connection ID if user re-joins
                    var existingUser = room.Users.First(u => u.Uid == user.Uid);
                    existingUser.ConnectionId = user.ConnectionId;
                }
            }
            return true;
        }
        return false;
    }

    public bool LeaveRoom(string roomId, string connectionId)
    {
        if (_rooms.TryGetValue(roomId, out var room))
        {
            lock (room.Users)
            {
                var user = room.Users.FirstOrDefault(u => u.ConnectionId == connectionId);
                if (user != null)
                {
                    room.Users.Remove(user);
                    
                    // If room is empty, we could remove it, but let's keep it until host formally closes or timeout
                    if (room.Users.Count == 0)
                    {
                        _rooms.TryRemove(roomId, out _);
                    }
                    return true;
                }
            }
        }
        return false;
    }

    public bool KickUser(string roomId, string hostUid, string targetUid)
    {
        if (_rooms.TryGetValue(roomId, out var room))
        {
            if (room.HostUid == hostUid)
            {
                lock (room.Users)
                {
                    var user = room.Users.FirstOrDefault(u => u.Uid == targetUid);
                    if (user != null && user.Uid != hostUid) // Cannot kick self
                    {
                        room.KickedUids.Add(targetUid);
                        room.Users.Remove(user);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public LiveRoomUser? GetUserByConnectionId(string connectionId)
    {
        foreach (var room in _rooms.Values)
        {
            lock (room.Users)
            {
                var user = room.Users.FirstOrDefault(u => u.ConnectionId == connectionId);
                if (user != null)
                {
                    return user;
                }
            }
        }
        return null;
    }

    public IEnumerable<LiveRoom> GetAllRooms()
    {
        return _rooms.Values;
    }

    public void UpdateRoom(LiveRoom room)
    {
        // For in-memory, the reference is already updated
    }
}
