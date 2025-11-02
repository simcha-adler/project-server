using System.ComponentModel.DataAnnotations; // נדרש להוספת Key

namespace sign_in.Entities
{
    public class AppUser // זהו מודל ה-DB שלך (Entity)
    {
        // המפתח הראשי
        public int Id { get; set; } 
        
        // השדה בו נשתמש להתחברות
        [Required]
        public required string Email { get; set; } 
        
        // כאן נאחסן את הסיסמה המוצפנת (Hash)
        [Required]
        public required string PasswordHash { get; set; } 
    }
}