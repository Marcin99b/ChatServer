using ChatServer.WebApi.Areas.Commons;
using ChatServer.WebApi.Extensions;
using ChatServer.WebApi.Hubs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace ChatServer.WebApi.Areas.WebRtc
{
    public class WebRtcController : V1ControllerBase
    {
        private readonly Repository repository;
        private readonly RoomsHub hub;

        public WebRtcController(Repository repository, RoomsHub hub)
        {
            this.repository = repository;
            this.hub = hub;
        }

        [HttpPost]
        public async Task<GetIceServersResponse> GetIceServers([FromBody] GetIceServersRequest request)
        {
            var userId = this.User.GetUserId();
            var room = this.repository.Rooms.First(x => x.Id == request.RoomId);
            if(room.CallingUserId != userId && room.ReceivingUserId != userId)
            {
                throw new ArgumentException("User is not in room");
            }
            using var client = new HttpClient();
            var twillioUserId = "AC465f954cc0d4e0325e12d9fc875620a8";
            var token = "1b4ce6482eccb875302f34816daf8de6";
            var authenticationString = $"{twillioUserId}:{token}";
            var base64String = Convert.ToBase64String(
           System.Text.Encoding.ASCII.GetBytes(authenticationString));
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"https://api.twilio.com/2010-04-01/Accounts/{twillioUserId}/Tokens.json");
            requestMessage.Headers.Authorization =
               new AuthenticationHeaderValue("Basic", base64String);

            var response = await client.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<TwillioIceServersResponse>();
            return new GetIceServersResponse(data!.ice_servers!);
        }

        [HttpPost]
        public async Task<CreateRoomResponse> CreateRoom([FromBody] CreateRoomRequest request)
        {
            var userId = this.User.GetUserId();
            var room = this.repository.Rooms.First(x => x.Id == request.RoomId);
            if(userId != room.CallingUserId) 
            {
                throw new Exception("Room must be created by receiving user");
            }
            var rtcRoom = new WebRtcRoom(room.Id, request.Offer);
            this.repository.WebRtcRooms.Add(rtcRoom);
            return new CreateRoomResponse();
        }

        [HttpPost]
        public async Task<AddCandidateResponse> AddCandidate([FromBody] AddCandidateRequest request)
        {
            var userId = this.User.GetUserId();

            var rtcRoom = this.repository.WebRtcRooms.First(x => x.Id == request.WebRtcRoomId);
            var room = this.repository.Rooms.First(x => x.Id == rtcRoom.RoomId);
            if(room.CallingUserId == userId)
            {
                rtcRoom.OfferCandidates.Add(request.Candidate);
            }
            else if(room.ReceivingUserId == userId)
            {
                rtcRoom.AnswerCandidates.Add(request.Candidate);
                await hub.CandidateAddedToRoom(room.Id, request.Candidate);
            }
            else
            {
                throw new ArgumentException("User is not in room");
            }
            return new AddCandidateResponse();
        }

        [HttpPost]
        public async Task<SetAnswerResponse> SetAnswer([FromBody] SetAnswerRequest request)
        {
            var userId = this.User.GetUserId();

            var rtcRoom = this.repository.WebRtcRooms.First(x => x.Id == request.WebRtcRoomId);
            var room = this.repository.Rooms.First(x => x.Id == rtcRoom.RoomId);

            if (userId != room.ReceivingUserId)
            {
                throw new Exception("User must be caller to set answer");
            }

            rtcRoom.Answer = request.Answer;
            await hub.AnswerAddedToRoom(room.Id, request.Answer);
            return new SetAnswerResponse();
        }
    }
}
