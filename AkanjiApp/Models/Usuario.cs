using Microsoft.AspNetCore.Identity;

namespace AkanjiApp.Models
{
    public class Usuario : IdentityUser
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string ZenodoToken { get; set; }


    }
}


