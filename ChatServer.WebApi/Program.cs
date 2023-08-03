using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();
builder.Services.AddSignalR();
builder.Services.AddSingleton<ChatHub>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true)
    .AllowCredentials());

app.UseHttpsRedirection();

app.MapHub<ChatHub>("/chatHub");

app.MapPost("Rooms/CreateOffer", async ([FromBody] CreateOfferRequest request) =>
{
    var room = new Room(request.Offer);
    Db.Rooms.Add(room);
    await Task.CompletedTask;
    return new CreateOfferResponse(room.Id);
});

app.MapPost("Rooms/{id}/AddCandidate", async ([FromRoute] Guid id, [FromBody] AddCandidateRequest request, ChatHub hub) =>
{
    var room = Db.Rooms.First(x => x.Id == id);
    room.Candidates.Add(new Candidate(request.Candidate));
    await hub.CandidateAddedToRoom(room.Id, request.Candidate);
});

app.MapPost("Rooms/{id}/AddAnswer", async ([FromRoute] Guid id, [FromBody] AddAnswerRequest request, ChatHub hub) =>
{
    var room = Db.Rooms.First(x => x.Id == id);
    room.Answer = request.Answer;
    await hub.AnswerAddedToRoom(room.Id, request.Answer);
});

app.MapPost("Rooms/{id}/GetOffer", async ([FromRoute] Guid id) =>
{
    var room = Db.Rooms.First(x => x.Id == id);
    await Task.CompletedTask;
    return new GetOfferResponse(room.Offer);
});

app.MapPost("Rooms/GetRooms", async () =>
{
    var roomIds = Db.Rooms.Select(x => x.Id).ToArray();
    await Task.CompletedTask;
    return new GetRoomsResponse(roomIds);
});

app.MapPost("Rooms/{id}/Delete", async ([FromRoute] Guid id) =>
{
    Db.Rooms.Remove(Db.Rooms.First(x => x.Id == id));
    await Task.CompletedTask;
});

app.MapPost("WebRtc/GetIceServers", async () =>
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
    var data = await response.Content.ReadFromJsonAsync<Rootobject>();
    return new GetIceServersResponse(data!.ice_servers!);
});


app.Run();

class ChatHub : Hub
{
    public async Task AnswerAddedToRoom(Guid roomId, SdpData answer)
    {
        await Clients.Others.SendAsync("AnswerAddedToRoom", roomId, answer);
    }

    public async Task CandidateAddedToRoom(Guid roomId, CandidateItem candidate)
    {
        await Clients.Others.SendAsync("CandidateAddedToRoom", roomId, candidate);
    }
}

record CreateOfferRequest(SdpData Offer);
record CreateOfferResponse(Guid RoomId);
record AddCandidateRequest(CandidateItem Candidate);
record AddAnswerRequest(SdpData Answer);
record GetOfferResponse(SdpData offer);
record GetRoomsResponse(Guid[] Ids);

record GetIceServersResponse(Ice_Servers[] IceServers);

static class Db
{
    public static List<Room> Rooms { get; } = new();
}


record Entity { public Guid Id { get; } = Guid.NewGuid(); }

record Candidate(CandidateItem Data) : Entity;

record Room(SdpData Offer) : Entity
{
    public List<Candidate> Candidates { get; } = new();
    public SdpData? Answer { get; set; }
}

record SdpData(string sdp, string type);
record CandidateItem(string candidate, int sdpMLineIndex, string sdpMid, string usernameFragment);



public class Rootobject
{
    public string? username { get; set; }
    public Ice_Servers[]? ice_servers { get; set; }
    public string? date_updated { get; set; }
    public string? account_sid { get; set; }
    public string? ttl { get; set; }
    public string? date_created { get; set; }
    public string? password { get; set; }
}

public class Ice_Servers
{
    public string? url { get; set; }
    public string? urls { get; set; }
    public string? username { get; set; }
    public string? credential { get; set; }
}
