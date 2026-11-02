using System.Security.Cryptography;

namespace QLNS_KPI.Security;

public static class TemporaryPasswordGenerator
{
    public static string Generate() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(24))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
