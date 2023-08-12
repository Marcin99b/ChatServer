using ChatServer.WebApi.Areas.Commons;
using ChatServer.WebApi.Areas.Users;
using ChatServer.WebApi.Extensions;
using ChatServer.WebApi.Hubs;
using Microsoft.AspNetCore.Mvc;

namespace ChatServer.WebApi.Areas.Rooms
{
    public class RoomsController : V1ControllerBase
    {
        private readonly Repository repository;
        private readonly RoomsHub roomsHub;
        private readonly RoomsService roomsService;

        public RoomsController(Repository repository, RoomsHub roomsHub, RoomsService roomsService)
        {
            this.repository = repository;
            this.roomsHub = roomsHub;
            this.roomsService = roomsService;
        }

        [HttpPost]
        public async Task<ProposeCallResponse> ProposeCall([FromBody] ProposeCallRequest request)
        {
            var currentUserId = this.User.GetUserId();

            var found = this.repository.CallPropositions.FirstOrDefault(x => x.CallingUserId == currentUserId && x.ReceivingUserId == request.ReceivingUserId);
            if(found != null) 
            {
                throw new ArgumentException("Call proposition already exist");
            }

            var proposition = new CallProposition(currentUserId, request.ReceivingUserId);

            this.repository.CallPropositions.Add(proposition);
            await roomsHub.WaitingForCallAccept(currentUserId, request.ReceivingUserId);
            return new ProposeCallResponse();
        }

        [HttpPost]
        public async Task<AcceptCallResponse> AcceptCall([FromBody]AcceptCallRequest request)
        {
            var receivingUser = this.User.GetUserId();
            var callingUserId = request.CallingUserId;

            var proposition = this.repository.CallPropositions.Single(x => x.CallingUserId == callingUserId && x.ReceivingUserId == receivingUser);

            proposition.IsAccepted = true;

            var room = new Room(request.CallingUserId, receivingUser);
            this.repository.Rooms.Add(room);

            await roomsHub.CallPropositionAccepted(room.Id, request.CallingUserId);
            this.repository.CallPropositions.Remove(proposition);
            return new AcceptCallResponse(room.Id);
        }

        [HttpPost]
        public async Task<GetRoomResponse> GetRoom([FromBody] GetRoomRequest request)
        {
            var userId = this.User.GetUserId();
            var room = this.repository.Rooms.First(x => x.Id == request.RoomId);
            if (userId != room.CallingUserId && userId != room.ReceivingUserId)
            {
                throw new Exception("User must be in room");
            }
            await Task.CompletedTask;
            return new GetRoomResponse(room);
        }

        [HttpPost]
        public async Task<LeaveResponse> Leave([FromBody] LeaveRequest request)
        {
            var userId = this.User.GetUserId();
            var roomFound = repository.Rooms.Single(x => x.Id == request.RoomId && (x.ReceivingUserId == userId || x.CallingUserId == userId));
            var rtcRoom = repository.WebRtcRooms.Single(x => x.RoomId == roomFound.Id);
            repository.WebRtcRooms.Remove(rtcRoom);
            repository.Rooms.Remove(roomFound);

            await roomsHub.RoomDeleted(roomFound.Id, roomFound.CallingUserId == userId ? roomFound.ReceivingUserId : roomFound.CallingUserId);
            return new LeaveResponse();
        }
    }
}
