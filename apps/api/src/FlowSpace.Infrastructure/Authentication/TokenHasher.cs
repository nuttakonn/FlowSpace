using System.Security.Cryptography;
using System.Text;

namespace FlowSpace.Infrastructure.Authentication;

public static class TokenHasher
{
    public static string HashToken(string token)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashBytes);
    }
}
