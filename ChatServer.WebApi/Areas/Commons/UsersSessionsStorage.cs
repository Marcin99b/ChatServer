namespace ChatServer.WebApi.Areas.Commons
{
    public record UserSession(Guid UserId, string Token, DateTime CreatedAt, string IpAddress, string UserAgent);

    public interface IUsersSessionsStorage
    {
        void Add(UserSession session);
        UserSession? Get(string token);
        void RemoveByToken(string token);
    }

    public class UsersSessionsStorage : IUsersSessionsStorage
    {
        private readonly CacheService cacheService;

        public UsersSessionsStorage(CacheService cacheService)
        {
            this.cacheService = cacheService;
        }

        public void Add(UserSession session) => cacheService.SaveValue(session);
        public UserSession? Get(string token) => this.cacheService.GetValue(token);
        public void RemoveByToken(string token)
        {
            cacheService.RemoveValue(token);
        }
    }

    public class CacheService
    {
        private readonly Repository repository;

        public CacheService(Repository repository)
        {
            this.repository = repository;
        }

        public UserSession? GetValue(string key)
        {
            var item = repository.UserSessions.FirstOrDefault(x => x.Token == key);
            return item;
        }

        public void SaveValue(UserSession value)
        {
            repository.UserSessions.Add(value);
        }

        public void RemoveValue(string key)
        {
            var value = repository.UserSessions.FirstOrDefault(x => x.Token == key);
            if(value == null)
            {
                return;
            }
            repository.UserSessions.Remove(value);
        }
    }
}
