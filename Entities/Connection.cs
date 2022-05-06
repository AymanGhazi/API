namespace API.Entities
{
    public class Connection
    {
        public Connection()
        {
        }

        public Connection(string ConnectionId, string Username)
        {
            this.ConnectionId = ConnectionId;
            this.Username = Username;
        }
        public string ConnectionId { get; set; }
        public string Username { get; set; }
    }
}