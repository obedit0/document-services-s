using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.Extensions.Configuration;

namespace Domain.Commons;

public static class CryptoCommon
{
    private const string initVector = "pemgail9uzpgzl88";
    private const int keysize = 256;
    private static readonly string passPhrase = "AAED4554C235927C03BAD5BC313E81B0FBEB6FEC5BB60E682C70A7D5C8890B20";

    public static string encryptString(string plainText, bool urlFriendly = false)
    {
        byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
        byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText + "                                                          ");
        PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
        byte[] keyBytes = password.GetBytes(keysize / 8);
        using var symmetricKey = Aes.Create("AesManaged");
        symmetricKey.Mode = CipherMode.CBC;
        ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
        MemoryStream memoryStream = new MemoryStream();
        CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
        cryptoStream.FlushFinalBlock();
        byte[] cipherTextBytes = memoryStream.ToArray();
        memoryStream.Close();
        cryptoStream.Close();
        if (urlFriendly)
        {
            return HttpUtility.UrlEncode(Convert.ToBase64String(cipherTextBytes));
        }
        return Convert.ToBase64String(cipherTextBytes);
    }

    public static string decryptString(string cipherText)
    {
        byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
        try
        {
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(keysize / 8);
            using var symmetricKey = Aes.Create("AesManaged");
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];
            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).Trim();
        }
        catch
        {
            return "";
        }
    }

    public static string ComputeSha256Hash(string cCadena)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(cCadena));

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString().ToUpper();
        }
    }

    public static Dictionary<string, string?> DescryptEnv(IConfiguration baseConfig)
    {
        var plainDict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in baseConfig.AsEnumerable(makePathsRelative: false))
        {
            if (kv.Value is null) continue;

            var v = kv.Value;
            if (v.StartsWith("enc:", StringComparison.OrdinalIgnoreCase))
            {
                var payload = v.Substring("enc:".Length);
                v = decryptString(payload);
            }
            plainDict[kv.Key] = v;
        }
        return plainDict;
    }
}
