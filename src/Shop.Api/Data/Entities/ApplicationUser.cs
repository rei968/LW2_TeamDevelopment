using Microsoft.AspNetCore.Identity;

namespace Shop.Api.Data.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
