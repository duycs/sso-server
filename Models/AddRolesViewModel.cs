using AuthServer.Infrastructure.Constants;

namespace AuthServer.Models
{
    public class AddRolesViewModel
    {
        public string UserName { get; set; }
        public string[] Roles { get; set; }
    }
}
