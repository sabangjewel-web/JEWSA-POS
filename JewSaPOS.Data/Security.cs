using System.Security.Cryptography;
using System.Text;

namespace JewSaPOS.Data
{
    /// <summary>Matches the SQL seed: SHA-256 of (Salt + Password), uppercase hex.</summary>
    public static class PasswordHasher
    {
        public static string Hash(string salt, string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.ASCII.GetBytes(salt + password));
                var sb = new StringBuilder(bytes.Length * 2);
                foreach (var b in bytes) sb.Append(b.ToString("X2"));
                return sb.ToString();
            }
        }

        public static bool Verify(string salt, string password, string expectedHash)
        {
            var actual = Hash(salt, password);
            if (expectedHash == null || actual.Length != expectedHash.Length) return false;
            int diff = 0;   // constant-time comparison
            for (int i = 0; i < actual.Length; i++)
                diff |= char.ToUpperInvariant(actual[i]) ^ char.ToUpperInvariant(expectedHash[i]);
            return diff == 0;
        }
    }
}
