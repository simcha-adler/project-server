using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using sign_in.Data;

// מיישמת את הממשק הנדרש ליצירת DbContext בזמן עיצוב
public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        // זוהי הדרך שבה כלי ה-EF Core יכולים לקבל את ה-DbContextOptions
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
        
        // הגדרת ה-Connection String באופן ידני לצורך ה-Migration בלבד.
        // הדרך המקובלת היא להשתמש ב-Configuration Builder
        // עבור SQLite, הנתיב הוא שם הקובץ:
        
        // **הגדרת ה-Connection String כמו בקובץ appsettings.json**
        optionsBuilder.UseSqlite("Data Source=AppData.db");

        return new DataContext(optionsBuilder.Options);
    }
}