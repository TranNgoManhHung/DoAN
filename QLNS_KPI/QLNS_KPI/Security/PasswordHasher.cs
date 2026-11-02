using System.Security.Cryptography;
using System.Text;

namespace QLNS_KPI.Security;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
    bool NeedsRehash(string passwordHash);
}

public sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(PreHash(password), workFactor: 12);

    public bool Verify(string password, string passwordHash)
    {
        var storedHash = passwordHash.Trim();
        if (storedHash.StartsWith("$2", StringComparison.Ordinal))
        {
            return BCrypt.Net.BCrypt.Verify(PreHash(password), storedHash);
        }

        if (storedHash.Length != 64)
        {
            return false;
        }

        byte[] expectedHash;
        try
        {
            expectedHash = Convert.FromHexString(storedHash);
        }
        catch (FormatException)
        {
            return false;
        }

        var legacyHash = SHA256.HashData(Encoding.Unicode.GetBytes(password));
        return CryptographicOperations.FixedTimeEquals(legacyHash, expectedHash);
    }

    public bool NeedsRehash(string passwordHash) => !passwordHash.Trim().StartsWith("$2", StringComparison.Ordinal);

    private static string PreHash(string password) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
}
