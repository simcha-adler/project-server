using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace sign_in.Services
{
    public static class PasswordService // שירות סטטי לפונקציות Hashing
    {
        // גודל מומלץ למלח (Salt)
        private const int SaltSize = 128 / 8; // 16 bytes
        // גודל מומלץ ל-Hash
        private const int KeySize = 256 / 8; // 32 bytes
        // מספר איטרציות גבוה
        private const int Iterations = 100000; 

        // 1. יצירת Hash לסיסמה
        public static string HashPassword(string password)
        {
            // יצירת מלח רנדומלי (Salt) ייחודי לכל סיסמה
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            // ביצוע Hashing באמצעות PBKDF2
            byte[] hash = KeyDerivation.Pbkdf2(
                password: password!,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: KeySize);

            // שילוב המלח וה-Hash יחד ושמירה כמחרוזת ב-Base64
            // הפורמט: {Iterations}.{Salt (Base64)}.{Hash (Base64)}
            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        // 2. אימות סיסמה נכנסת מול Hash שמור
        public static bool VerifyPassword(string password, string passwordHash)
        {
            // פיצול המחרוזת השמורה (Iterations, Salt, Hash)
            string[] parts = passwordHash.Split('.', 3);
            if (parts.Length != 3)
            {
                return false; // פורמט שגוי
            }

            // חילוץ הנתונים
            int iterations = int.Parse(parts[0]);
            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] storedHash = Convert.FromBase64String(parts[2]);

            // יצירת Hash חדש מהסיסמה שהזין המשתמש ומהמלח השמור
            byte[] computedHash = KeyDerivation.Pbkdf2(
                password: password!,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: iterations,
                numBytesRequested: KeySize);

            // השוואה בטוחה של ה-Hash החדש מול ה-Hash השמור (מונע התקפות Timing)
            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }
    }
}