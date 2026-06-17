using SmartNovelBE.Models;

namespace SmartNovelBE.Services;

public interface ILiveRoomManager
{
    LiveRoom? CreateRoom(string roomId, string novelId, string chapterId, string hostUid);
    LiveRoom? GetRoom(string roomId);
    bool JoinRoom(string roomId, LiveRoomUser user);
    bool LeaveRoom(string roomId, string connectionId);
    bool KickUser(string roomId, string hostUid, string targetUid);
    LiveRoomUser? GetUserByConnectionId(string connectionId);
    IEnumerable<LiveRoom> GetAllRooms();
    void UpdateRoom(LiveRoom room);
}
