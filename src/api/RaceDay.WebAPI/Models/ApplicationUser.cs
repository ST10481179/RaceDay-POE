using Microsoft.AspNetCore.Identity;

namespace RaceDay.WebAPI.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int? Age { get; set; }
}