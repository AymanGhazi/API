namespace API.Helpers
{
    public class MessageParams : paginationParams
    {
        public string UserName { get; set; }
        public string container { get; set; } = "unread";
    }
}