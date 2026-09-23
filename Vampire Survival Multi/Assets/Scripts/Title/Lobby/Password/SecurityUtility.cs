using System;
using System.Security.Cryptography;
using System.Text;

public static class SecurityUtility
{
    /// <summary>
    /// 비밀번호와 Salt(방 ID)를 조합하여 SHA-256 해시 생성
    /// </summary>
    public static string GetPasswordHash(string password, string salt)
    {
        if (string.IsNullOrEmpty(password)) return string.Empty;

        // 비밀번호와 ID 결합
        string combined = $"{password}-{salt}";
        using var sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
        return Convert.ToBase64String(bytes);
    }
}