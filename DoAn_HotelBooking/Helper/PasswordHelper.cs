using System.Security.Cryptography;

namespace DoAn_HotelBooking.Helper
{
    public class PasswordHelper
    {
        private const int SaltSize = 16;  // 128 bit
        private const int KeySize = 32;   // 256 bit
        private const int Iterations = 100000;

        // Hash mật khẩu với PBKDF2 + Salt
        public static string HashPassword(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] salt = new byte[SaltSize];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(KeySize);

            // Lưu dạng: iterations:salt:hash
            return $"{Iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        // Kiểm tra mật khẩu nhập vào
        public static bool VerifyPassword(string inputPassword, string storedHash)
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 3) return false;

            int iterations = int.Parse(parts[0]);
            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] storedKey = Convert.FromBase64String(parts[2]);

            using var pbkdf2 = new Rfc2898DeriveBytes(inputPassword, salt, iterations, HashAlgorithmName.SHA256);
            byte[] key = pbkdf2.GetBytes(KeySize);

            return CryptographicOperations.FixedTimeEquals(key, storedKey);
        }
    }
}
