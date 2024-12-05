using System.Security.Cryptography;
using System.Text;

namespace HrCommonApi.Authorization;

public class PasswordManager
{
    /// <summary>
    /// Public function to hash a password. Can be changed to use a different hashing algorithm.
    /// </summary>
    public string HashPassword(string password) => ApplySha256Hash(password);

    /// <summary>
    /// Public function to verify a password. Can be changed to use a different hashing algorithm.
    /// </summary>
    public bool VerifyPassword(string hashedPassword, string password) => hashedPassword == ApplySha256Hash(password);

    /// <summary>
    /// Private function to hash a password using SHA256.
    /// </summary>
    private static string ApplySha256Hash(string value) =>
        string.Concat(SHA256.HashData(Encoding.UTF8.GetBytes(value)).Select(item => item.ToString("x2")));
}
