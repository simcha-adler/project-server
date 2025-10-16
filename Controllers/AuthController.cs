using Microsoft.AspNetCore.Mvc;
using sign_in.Models; // ייבוא המודלים שיצרנו

[ApiController]
[Route("api/[controller]")] // יגדיר את נתיב הגישה כ- /api/Auth
public class AuthController : ControllerBase
{
    // 1. מטודה לטיפול בבקשת הרשמה (POST)
    [HttpPost("register")] // הנתיב המלא: /api/Auth/register
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        // בדיקת קלט בסיסית
        if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            // מחזיר 400 Bad Request
            return BadRequest(new { Message = "שם משתמש או סיסמה נדרשים לצורך הרשמה." }); 
        }

        // **לוגיקה זמנית ללא DB:** נניח שהמשתמש נרשם בהצלחה
        Console.WriteLine($"[REGISTER] משתמש נרשם בהצלחה: {request.Username}");

        // מחזיר 200 OK
        return Ok(new { Message = $"המשתמש {request.Username} נרשם בהצלחה." }); 
    }

    // 2. מטודה לטיפול בבקשת התחברות (POST)
    [HttpPost("login")] // הנתיב המלא: /api/Auth/login
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // בדיקת קלט בסיסית
        if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            // מחזיר 400 Bad Request
            return BadRequest(new { Message = "שם משתמש וסיסמה נדרשים להתחברות." });
        }

        // **לוגיקה זמנית ללא DB:** אימות משתמש מדומה
        // רק משתמש "test" עם סיסמה "123456" יצליח להתחבר
        if (request.Username.ToLower() == "test" && request.Password == "123456")
        {
            Console.WriteLine($"[LOGIN] התחברות מוצלחת: {request.Username}");
            // מחזיר 200 OK
            return Ok(new { Message = $"התחברת בהצלחה, {request.Username}!" }); 
        }
        else
        {
            Console.WriteLine($"[LOGIN] ניסיון התחברות כושל: {request.Username}");
            // מחזיר 401 Unauthorized
            return Unauthorized(new { Message = "שם משתמש או סיסמה שגויים." }); 
        }
    }
}