using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    public class presenceTracker
    {
        private static readonly Dictionary<string, List<string>> OnlineUsers =
                new Dictionary<string, List<string>>();

        public Task<bool> UserConnected(string username, string ConnectionId)
        {
            bool isOnline = false;
            lock (OnlineUsers)
            {
                if (OnlineUsers.ContainsKey(username))
                {
                    OnlineUsers[username].Add(ConnectionId);
                }
                else
                {
                    OnlineUsers.Add(username, new List<string> { ConnectionId });
                    isOnline = true;
                }
            }
            return Task.FromResult(isOnline);
        }
        public Task<bool> userDisConnected(string username, string ConnectionId)
        {
            bool IsOffline = false;
            lock (OnlineUsers)
            {
                //if he DisConnect and no key for his name so nothing will happen
                if (!OnlineUsers.ContainsKey(username))
                {
                    return Task.FromResult(IsOffline);

                }
                //if he DisConnect and there is a key for his name then remove the connectionID
                OnlineUsers[username].Remove(ConnectionId);
                if (OnlineUsers[username].Count == 0)
                {
                    OnlineUsers.Remove(username);
                    IsOffline = true;
                }
            }

            return Task.FromResult(IsOffline);
        }
        public Task<string[]> GetOnlineUsers()
        {
            string[] onlineusers;
            lock (OnlineUsers)
            {
                onlineusers = OnlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
            }
            return Task.FromResult(onlineusers);
        }
        public Task<List<string>> GetConnectionsForUser(string username)
        {
            List<string> ConnectionIds;
            lock (OnlineUsers)
            {
                ConnectionIds = OnlineUsers.GetValueOrDefault(username);

            }
            return Task.FromResult(ConnectionIds);
        }
    }
}