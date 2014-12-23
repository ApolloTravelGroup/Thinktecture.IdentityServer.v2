namespace Thinktecture.IdentityServer.Models
{
    public class User
    {
        public string UserName { get; set; }
        public bool IsLockedOut { get; set; }
    }
}