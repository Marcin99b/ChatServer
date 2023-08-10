using ChatServer.WebApi.Areas.Commons;

namespace ChatServer.WebApi.Areas.WebRtc
{
    public record SdpData(
        string sdp,
        string type);

    public record Candidate(
        string candidate,
        int sdpMLineIndex,
        string sdpMid,
        string usernameFragment);

    public record WebRtcRoom(Guid RoomId, SdpData Offer, Candidate[] OfferCandidates) : Entity
    {
        public Candidate[]? AnswerCandidates { get; private set; }
        public SdpData? Answer { get; private set; }

        public void SetAnswer(SdpData answer, Candidate[] answerCandidates)
        {
            Answer = answer;
            AnswerCandidates = answerCandidates;
        }
    }

    public record TwillioIceServersResponse(
        string username,
        Ice_Servers[] ice_servers,
        string date_updated,
        string account_sid,
        string ttl,
        string date_created,
        string password);

    public record Ice_Servers(
        string url,
        string urls,
        string username,
        string credential);

    public record GetIceServersRequest(Guid RoomId);
    public record GetIceServersResponse(Ice_Servers[] IceServers);

    public record CreateRoomRequest(Guid RoomId, SdpData Offer, Candidate[] Candidates);
    public record CreateRoomResponse(WebRtcRoom RtcRoom);

    public record AddCandidateRequest(Guid WebRtcRoomId, Candidate Candidate);
    public record AddCandidateResponse;

    public record SetAnswerRequest(Guid RoomId, SdpData Answer, Candidate[] Candidates);
    public record SetAnswerResponse;

    public record NotifyCallerAboutRoomConfiguredRequest(Guid RoomId);
    public record NotifyCallerAboutRoomConfiguredResponse;

    public record NotifyReceiverAboutRoomConfiguredRequest(Guid RoomId);
    public record NotifyReceiverAboutRoomConfiguredResponse;
}
