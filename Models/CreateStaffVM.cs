namespace AuthServer.Models
{
    public class CreateStaffVM
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Account { get; set; }
        public string Email { get; set; }
        public int[]? RoleIds { get; set; }
        public int[]? ProductLevelIds { get; set; }
        public int[]? GroupIds { get; set; }
    }
}
