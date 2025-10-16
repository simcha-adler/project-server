var builder = WebApplication.CreateBuilder(args);

// הגדרת מדיניות CORS 
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            // חשוב: זו הכתובת של הקליינט (Angular) – ודא שהיא תואמת לפורט שבו הקליינט ירוץ (4200 כברירת מחדל).
            policy.WithOrigins("http://localhost:4200") 
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// הוספת שירותי Controllers
builder.Services.AddControllers();

// הוספת Swagger (אופציונלי, לבדיקות)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// קונפיגורציה של Pipeline ה-HTTP

// הוספת Swagger אם אנחנו בסביבת פיתוח
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// הפניית מ-HTTP ל-HTTPS (מומלץ)
app.UseHttpsRedirection();

// יישום ה-CORS (חייב להיות לפני MapControllers!)
app.UseCors(MyAllowSpecificOrigins);

// במודל המינימליסטי, שירותי ה-Authorization אינם מוגדרים כברירת מחדל, 
// אך אם נרצה להשתמש בהם בעתיד, נפעיל:
// app.UseAuthorization(); 

// מיפוי הבקרים וה-Endpoints שלנו
app.MapControllers();

app.Run();