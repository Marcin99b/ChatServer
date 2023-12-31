﻿using ChatServer.WebApi.Areas.Commons;
using ChatServer.WebApi.Areas.WebRtc;
using ChatServer.WebApi.Extensions;
using Microsoft.AspNetCore.SignalR;

namespace ChatServer.WebApi.Hubs
{
    public record UserConnection(string SignalRConnectionId, Guid UserId);

    public class RoomsHub : Hub
    {
        private readonly Repository repository;

        public RoomsHub(Repository repository)
        {
            this.repository = repository;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            if(Context.User == null)
            {
                return;
            }
            var userId = Context.User.GetUserId();
            var foundConnections = repository.Connections.Where(c => c.UserId == userId);
            foreach( var connection in foundConnections) 
            {
                repository.Connections.Remove(connection);
            }
            repository.Connections.Add(new UserConnection(Context.ConnectionId, userId));
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
            if(Context.User == null)
            {
                return;
            }

            var userId = Context.User.GetUserId();
            var foundConnections = repository.Connections.Where(c => c.UserId == userId);
            foreach (var connection in foundConnections)
            {
                repository.Connections.Remove(connection);
            }

            var roomFound = repository.Rooms.FirstOrDefault(x => x.ReceivingUserId == userId || x.CallingUserId == userId);
            if(roomFound == null)
            {
                return;
            }
            var rtcRoom = repository.WebRtcRooms.Single(x => x.RoomId == roomFound.Id);
            repository.WebRtcRooms.Remove(rtcRoom);
            
            repository.Rooms.Remove(roomFound);
            await this.RoomDeleted(roomFound.Id, roomFound.CallingUserId == userId ? roomFound.ReceivingUserId : roomFound.CallingUserId);
        }

        public async Task WaitingForCallAccept(Guid callingUser, Guid receiverUser)
        {
            var receiverConnectionId = repository.Connections
                .FirstOrDefault(x => x.UserId == receiverUser)
                ?.SignalRConnectionId;
            if (receiverConnectionId == null)
            {
                return;
            }
            await Clients.User(receiverUser.ToString()).SendAsync("WaitingForCallAccept", callingUser);
        }

        public async Task CallPropositionAccepted(Guid createdRoomId, Guid callingUser)
        {
            var receiverConnectionId = repository.Connections
                .FirstOrDefault(x => x.UserId == callingUser)
                ?.SignalRConnectionId;
            if (receiverConnectionId == null)
            {
                return;
            }
            await Clients.User(callingUser.ToString()).SendAsync("CallPropositionAccepted", createdRoomId);
        }

        public async Task RoomConfiguredByReceiver(WebRtcRoom rtcRoom, Guid callingUserId)
        {
            await Clients.User(callingUserId.ToString()).SendAsync("RoomConfiguredByReceiver", rtcRoom);
        }

        public async Task RoomConfiguredByCaller(WebRtcRoom rtcRoom, Guid receivingUserId)
        {
            await Clients.User(receivingUserId.ToString()).SendAsync("RoomConfiguredByCaller", rtcRoom);
        }

        public async Task RoomDeleted(Guid roomId, Guid anotherUserId)
        {
            await Clients.User(anotherUserId.ToString()).SendAsync("RoomDeleted", roomId);
        }
    }
}
