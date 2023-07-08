using System.Security.Cryptography;
using System.Text;

namespace DS3.Models
{
    static class HashingClass
    {
        public static string HashingSHA256(string password, string salt)
        {
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);

            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000);

            byte[] keyBytes = pbkdf2.GetBytes(32);

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] hashBytes = sha256Hash.ComputeHash(keyBytes);

                StringBuilder stringBuilder = new StringBuilder();

                for (int i = 0; i < hashBytes.Length; i++)
                {
                    stringBuilder.Append(hashBytes[i].ToString("x2"));
                }

                return stringBuilder.ToString();
            }
        }
    }
}
