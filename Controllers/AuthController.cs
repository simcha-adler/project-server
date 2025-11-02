using Microsoft.AspNetCore.Mvc;
using sign_in.Models; 
using sign_in.Data; 
using sign_in.Entities;
using sign_in.Services; 
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // נדרש לתגית [Authorize]
using System.Security.Claims; // נדרש ל-Claims בטוקן
using System.IdentityModel.Tokens.Jwt; // נדרש לעבודה עם JWT
using Microsoft.IdentityModel.Tokens; // נדרש ל-SigningCredentials
using System.Text; // נדרש לקידוד המפתח

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IConfiguration _config; // הזרקת ה-Configuration

    // עדכון ה-Constructor לקבלת IConfiguration
    public AuthController(DataContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }
    
    // ----------------------------------------------------------------------
    // 1. מטודה לטיפול בבקשת הרשמה (POST)
    // ----------------------------------------------------------------------
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // 1. בדיקת קלט בסיסית
        if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { Message = "אימייל וסיסמה נדרשים." }); 
        }
        
        // 2. בדיקה אם האימייל כבר קיים ב-DB
        if (await _context.AppUsers.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest(new { Message = "כתובת האימייל כבר רשומה במערכת." });
        }

        // 3. יצירת Hash מאובטח לסיסמה
        var passwordHash = PasswordService.HashPassword(request.Password); 

        // 4. יצירת אובייקט משתמש והוספה ל-DB
        var user = new AppUser
        {
            Email = request.Email,
            PasswordHash = passwordHash,
        };

        _context.AppUsers.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { Message = $"ההרשמה הצליחה! משתמש: {user.Email}" }); 
    }

    // ----------------------------------------------------------------------
    // 2. מטודה לטיפול בבקשת התחברות (POST)
    // ----------------------------------------------------------------------
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // 1. חיפוש המשתמש ב-DB לפי האימייל
        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return Unauthorized(new { Message = "אימייל או סיסמה שגויים." });
        }

        // 2. אימות הסיסמה באמצעות ה-Hash השמור
        if (!PasswordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { Message = "אימייל או סיסמה שגויים." });
        }

        // 3. הצלחה! יצירת טוקן JWT
        var tokenString = GenerateJwtToken(user);

        // 4. החזרת הטוקן וה-DisplayName לקליינט
        return Ok(new 
        { 
            Token = tokenString, 
            Email = user.Email
        }); 
    }

    // ----------------------------------------------------------------------
    // 3. מטודה מאובטחת לבדיקת האימות (GET)
    // ----------------------------------------------------------------------
    [Authorize] // דורש טוקן JWT חוקי
    [HttpGet("me")] 
    public IActionResult GetCurrentUser()
    {
        // אם הגענו לכאן, המשתמש מאומת!
        // שליפת ה-Claims מתוך הטוקן
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

        return Ok(new
        {
            Id = userId,
            Email = userEmail,
            Message = "קיבלת גישה לנתונים מאובטחים!"
        });
    }

    // ----------------------------------------------------------------------
    // 4. פונקציית עזר ליצירת הטוקן
    // ----------------------------------------------------------------------
    private string GenerateJwtToken(AppUser user)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings["Secret"]!));

        // הוספת Claims (נתוני זיהוי) לטוקן
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7), // הטוקן תקף לשבוע
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}