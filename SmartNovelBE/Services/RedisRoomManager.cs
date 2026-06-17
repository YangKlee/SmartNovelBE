using StackExchange.Redis;
using System.Text.Json;
using SmartNovelBE.Models;

namespace SmartNovelBE.Services;

public class RedisRoomManager : ILiveRoomManager
{
    private readonly IDatabase _db;
    private readonly IServer _server;
    private readonly string _redisPrefix = "liveroom:";

    public RedisRoomManager(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
        // Just get the first endpoint for scanning keys (simplification)
        _server = redis.GetServer(redis.GetEndPoints().First());
    }

    private string GetRoomKey(string roomId) => $"{_redisPrefix}{roomId}";

    public LiveRoom? CreateRoom(string roomId, string novelId, string chapterId, string hostUid)
    {
        var key = GetRoomKey(roomId);
        if (_db.KeyExists(key))
        {
            return GetRoom(roomId);
        }

        var room = new LiveRoom
        {
            RoomId = roomId,
            NovelId = novelId,
            ChapterId = chapterId,
            HostUid = hostUid
        };

        SaveRoom(room);
        return room;
    }

    public LiveRoom? GetRoom(string roomId)
    {
        var key = GetRoomKey(roomId);
        var value = _db.StringGet(key);
        if (value.IsNullOrEmpty) return null;

        return JsonSerializer.Deserialize<LiveRoom>((string)value!);
    }

    private void SaveRoom(LiveRoom room)
    {
        var key = GetRoomKey(room.RoomId);
        _db.StringSet(key, JsonSerializer.Serialize(room), TimeSpan.FromHours(24)); // Auto expire after 24h
    }

    public bool JoinRoom(string roomId, LiveRoomUser user)
    {
        var room = GetRoom(roomId);
        if (room == null || room.KickedUids.Contains(user.Uid))
        {
            return false;
        }

        // Thread-safety in a distributed environment would require Redis Transactions or Lua scripts.
        // For this refactoring, we use a basic read-modify-write pattern.
        var existingUser = room.Users.FirstOrDefault(u => u.Uid == user.Uid);
        if (existingUser == null)
        {
            room.Users.Add(user);
        }
        else
        {
            existingUser.ConnectionId = user.ConnectionId;
        }

        SaveRoom(room);
        return true;
    }

    public bool LeaveRoom(string roomId, string connectionId)
    {
        var room = GetRoom(roomId);
        if (room == null) return false;

        var user = room.Users.FirstOrDefault(u => u.ConnectionId == connectionId);
        if (user != null)
        {
            room.Users.Remove(user);
            if (room.Users.Count == 0)
            {
                _db.KeyDelete(GetRoomKey(roomId));
            }
            else
            {
                SaveRoom(room);
            }
            return true;
        }

        return false;
    }

    public bool KickUser(string roomId, string hostUid, string targetUid)
    {
        var room = GetRoom(roomId);
        if (room != null && room.HostUid == hostUid)
        {
            var user = room.Users.FirstOrDefault(u => u.Uid == targetUid);
            if (user != null && user.Uid != hostUid)
            {
                room.KickedUids.Add(targetUid);
                room.Users.Remove(user);
                SaveRoom(room);
                return true;
            }
        }
        return false;
    }

    public LiveRoomUser? GetUserByConnectionId(string connectionId)
    {
        // This operation is expensive in Redis as it requires scanning all rooms.
        // In a true Enterprise system, we would maintain a secondary index mapping connectionId -> roomId
        foreach (var room in GetAllRooms())
        {
            var user = room.Users.FirstOrDefault(u => u.ConnectionId == connectionId);
            if (user != null)
            {
                return user;
            }
        }
        return null;
    }

    public IEnumerable<LiveRoom> GetAllRooms()
    {
        var rooms = new List<LiveRoom>();
        var keys = _server.Keys(pattern: $"{_redisPrefix}*").ToArray();
        foreach (var key in keys)
        {
            var value = _db.StringGet(key);
            if (!value.IsNullOrEmpty)
            {
                var room = JsonSerializer.Deserialize<LiveRoom>((string)value!);
                if (room != null) rooms.Add(room);
            }
        }
        return rooms;
    }

    public void UpdateRoom(LiveRoom room)
    {
        SaveRoom(room);
    }
}
