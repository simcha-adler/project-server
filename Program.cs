using Microsoft.AspNetCore.Authentication.JwtBearer; // 1. נדרש ל-JWT
using Microsoft.IdentityModel.Tokens; // 1. נדרש ל-JWT
using System.Text; // 1. נדרש ל-JWT

using sign_in.Data; // 2. נדרש ל-DB Context
using Microsoft.EntityFrameworkCore; // 2. נדרש ל-DB Context

var builder = WebApplication.CreateBuilder(args);

// הגדרת מדיניות CORS 
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") 
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// --- 3. הוספת שירותי DB (EF Core) ---
// שליפת Connection String מ-appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<DataContext>(options =>
{
    // חיבור ל-SQLite
    options.UseSqlite(connectionString);
});


// --- 3. הוספת שירותי JWT (Authentication) ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!); // שליפת המפתח הסודי

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false; // בהפקה צריך להיות true
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true, // אימות החתימה באמצעות המפתח הסודי
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true, // אימות תוקף
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(); // הוספת שירותי הרשאה (Authorization)

// הוספת שירותי Controllers
builder.Services.AddControllers();

// ... (Swagger וכו')

var app = builder.Build();

// קונפיגורציה של Pipeline ה-HTTP

// ... (Swagger, IsDevelopment)

// הפניית מ-HTTP ל-HTTPS (מומלץ)
app.UseHttpsRedirection();

// יישום ה-CORS (חייב להיות לפני UseAuthentication!)
app.UseCors(MyAllowSpecificOrigins);

// --- 4. הפעלת ה-Authentication וה-Authorization ---
// חובה להפעיל אחרי UseCors ולפני MapControllers!
app.UseAuthentication(); 
app.UseAuthorization();  

// מיפוי הבקרים וה-Endpoints שלנו
app.MapControllers();

app.Run();