namespace API.Entities
{
    public class UserLike
    {
        public AppUser sourceUser { get; set; }
        public int SourceUserID { get; set; }
        public AppUser LikedUser { get; set; }
        public int LikedUserID { get; set; }
    }
}
