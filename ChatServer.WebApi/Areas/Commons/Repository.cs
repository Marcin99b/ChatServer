using ChatServer.WebApi.Areas.Rooms;
using ChatServer.WebApi.Areas.Users;
using ChatServer.WebApi.Areas.WebRtc;
using ChatServer.WebApi.Hubs;

namespace ChatServer.WebApi.Areas.Commons
{
    public class Repository
    {
        public List<Room> Rooms { get; } = new();
        public List<CallProposition> CallPropositions { get; } = new();
        public List<WebRtcRoom> WebRtcRooms { get; } = new();
        public List<User> Users { get; } = new();
        public List<UserConnection> Connections { get; } = new();
        public List<UserSession> UserSessions { get; } = new();
    }
}
