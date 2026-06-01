using Microsoft.AspNetCore.Identity;

namespace Movie_StructureCode.Domain.Entities
{
    public class AppUser : IdentityUser<Guid>
    {
        public ICollection<RefreshToken>? RefreshTokens { get; set; }
        public ICollection<Booking>? Bookings { get; set; }
        public ICollection<Theater_User>? TheaterUsers { get; set; }
    }
}
