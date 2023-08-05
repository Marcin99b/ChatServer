using ChatServer.WebApi.Areas.Commons;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace ChatServer.WebApi.Areas.WebRtc
{
    public class WebRtcController : V1ControllerBase
    {
        private readonly Repository repository;

        public WebRtcController(Repository repository)
        {
            this.repository = repository;
        }

        [HttpPost]
        public async Task<GetIceServersResponse> GetIceServers([FromBody] GetIceServersRequest request)
        {
            using var client = new HttpClient();
            var userId = "AC465f954cc0d4e0325e12d9fc875620a8";
            var token = "1b4ce6482eccb875302f34816daf8de6";
            var authenticationString = $"{userId}:{token}";
            var base64String = Convert.ToBase64String(
           System.Text.Encoding.ASCII.GetBytes(authenticationString));
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"https://api.twilio.com/2010-04-01/Accounts/{userId}/Tokens.json");
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
            return new CreateRoomResponse();
        }

        [HttpPost]
        public async Task<AddCandidateResponse> AddCandidate([FromBody] AddCandidateRequest request)
        {
            return new AddCandidateResponse();
        }

        [HttpPost]
        public async Task<SetAnswerResponse> SetAnswer([FromBody] SetAnswerRequest request)
        {
            return new SetAnswerResponse();
        }
    }
}
