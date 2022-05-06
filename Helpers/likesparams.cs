namespace API.Helpers
{
    public class likesparams : paginationParams
    {
        public int userID { get; set; }
        public string predicate { get; set; }
    }
}