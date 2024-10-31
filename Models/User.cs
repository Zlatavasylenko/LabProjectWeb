using Microsoft.AspNetCore.Identity;

namespace LabProject
{
    public class User : IdentityUser
    {
        public int Year { get; set; }
    }
}

