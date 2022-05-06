namespace API.DTos
{
    public class PhotoForApprovalDto
    {
        //Response Dto
        public int Id { get; set; }
        public string Url { get; set; }
        public string Username { get; set; }
        public bool IsApprovedStatus { get; set; }
    }
}