using ChatServer.WebApi.Areas.Commons;
using ChatServer.WebApi.Extensions;
using ChatServer.WebApi.Hubs;
using Microsoft.AspNetCore.Mvc;

namespace ChatServer.WebApi.Areas.Rooms
{
    public class RoomsController : V1ControllerBase
    {
        private readonly Repository repository;
        private readonly RoomsHub roomsHub;

        public RoomsController(Repository repository, RoomsHub roomsHub)
        {
            this.repository = repository;
            this.roomsHub = roomsHub;
        }

        [HttpPost]
        public async Task<ProposeCallResponse> ProposeCall(ProposeCallRequest request)
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
        public async Task<AcceptCallResponse> AcceptCall(AcceptCallRequest request)
        {
            var currentUserId = this.User.GetUserId();
            var callingUserId = request.CallingUserId;

            var proposition = this.repository.CallPropositions.Single(x => x.CallingUserId == callingUserId && x.ReceivingUserId == currentUserId);

            proposition.IsAccepted = true;

            var room = new Room(callingUserId, request.CallingUserId);
            this.repository.Rooms.Add(room);

            await roomsHub.CallPropositionAccepted(room.Id, request.CallingUserId);
            return new AcceptCallResponse(room.Id);
        }
    }
}
