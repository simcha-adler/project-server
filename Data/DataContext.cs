using Microsoft.EntityFrameworkCore;
using sign_in.Entities; // שינוי הייבוא לתיקיית Entities

namespace sign_in.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        
        // עדכון השם ל-AppUser והרבים ל-AppUsers
        public DbSet<AppUser> AppUsers { get; set; } = null!;
    }
}