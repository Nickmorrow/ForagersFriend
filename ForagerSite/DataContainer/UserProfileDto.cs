namespace ForagerSite.DataContainer
{
    public sealed class UserProfileDto
    {
        public Guid UsrId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? UsrName { get; set; }
        public string? UsrBio { get; set; }
        public int? UsrFindsNum { get; set; }
        public int? UsrExpScore { get; set; }
        public DateTime UsrJoinedDate { get; set; }
        public string? UsrCountry { get; set; }
        public string? UsrStateorProvince { get; set; }
        public int? UsrZipCode { get; set; }
    }
}
