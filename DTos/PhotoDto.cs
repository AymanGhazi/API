namespace API.DTos
{
    public class PhotoDto
    {
        public int id { get; set; }
        public string Url { get; set; }
        public bool IsMain { get; set; }
        public bool isApproved { get; set; }

    }
}