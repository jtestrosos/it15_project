using System.Security.Cryptography;
using System.Text;

namespace production_system.Services;

/// <summary>
/// Provides AES-256-CBC symmetric encryption for sensitive data columns (IAS Compliance).
/// The encryption key is read from configuration (Encryption:AesKey) and must be a 
/// Base64-encoded 32-byte key. If no key is configured, a new one is generated at startup
/// and logged as a warning so the developer can persist it.
/// </summary>
public interface IEncryptionService
{
    /// <summary>Encrypt plaintext to a Base64 ciphertext string (IV is prepended).</summary>
    string Encrypt(string plainText);

    /// <summary>Decrypt a Base64 ciphertext string back to plaintext.</summary>
    string Decrypt(string cipherText);
}

public class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly ILogger<AesEncryptionService> _logger;

    public AesEncryptionService(IConfiguration configuration, ILogger<AesEncryptionService> logger)
    {
        _logger = logger;

        var configuredKey = configuration["Encryption:AesKey"];

        try
        {
            if (!string.IsNullOrEmpty(configuredKey))
            {
                _key = Convert.FromBase64String(configuredKey);
                if (_key.Length != 32)
                {
                    throw new InvalidOperationException("Configured AES key must be exactly 32 bytes (256-bit).");
                }
            }
            else
            {
                throw new InvalidOperationException("No AES key found in configuration.");
            }
        }
        catch (Exception ex)
        {
            // Auto-generate an ephemeral key if configured key is missing or invalid (e.g. bad Base64 format)
            _key = RandomNumberGenerator.GetBytes(32); // 256-bit
            _logger.LogWarning(ex,
                "[ENCRYPTION] Missing or invalid AES key in configuration. Generated ephemeral fallback key: {Key}. " +
                "Update your production appsettings.json or Environment Variables with this valid Base64 string under Encryption:AesKey.",
                Convert.ToBase64String(_key));
        }
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Prepend IV to ciphertext for storage
        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;

        try
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Extract IV (first 16 bytes)
            var iv = new byte[16];
            Buffer.BlockCopy(fullCipher, 0, iv, 0, 16);
            aes.IV = iv;

            // Extract ciphertext (rest)
            var cipher = new byte[fullCipher.Length - 16];
            Buffer.BlockCopy(fullCipher, 16, cipher, 0, cipher.Length);

            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt data. Returning raw value.");
            return cipherText; // Return raw if decryption fails (e.g., data was stored unencrypted)
        }
    }
}

/// <summary>
/// Provides RSA asymmetric encryption for demonstration purposes (IAS Compliance).
/// Generates a new 2048-bit RSA keypair per instance for the demo page.
/// </summary>
public interface IRsaDemoService
{
    string PublicKeyPem { get; }
    string PrivateKeyPem { get; }
    string Encrypt(string plainText, string publicKeyPem);
    string Decrypt(string cipherText, string privateKeyPem);
    (string PublicKey, string PrivateKey) GenerateNewKeyPair();
}

public class RsaDemoService : IRsaDemoService
{
    private RSA _rsa;
    
    public string PublicKeyPem { get; private set; }
    public string PrivateKeyPem { get; private set; }

    public RsaDemoService()
    {
        _rsa = RSA.Create(2048);
        PublicKeyPem = ExportPublicKey(_rsa);
        PrivateKeyPem = ExportPrivateKey(_rsa);
    }

    public string Encrypt(string plainText, string publicKeyPem)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encrypted = rsa.Encrypt(plainBytes, RSAEncryptionPadding.OaepSHA256);
        return Convert.ToBase64String(encrypted);
    }

    public string Decrypt(string cipherText, string privateKeyPem)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);
        var cipherBytes = Convert.FromBase64String(cipherText);
        var decrypted = rsa.Decrypt(cipherBytes, RSAEncryptionPadding.OaepSHA256);
        return Encoding.UTF8.GetString(decrypted);
    }

    public (string PublicKey, string PrivateKey) GenerateNewKeyPair()
    {
        _rsa = RSA.Create(2048);
        PublicKeyPem = ExportPublicKey(_rsa);
        PrivateKeyPem = ExportPrivateKey(_rsa);
        return (PublicKeyPem, PrivateKeyPem);
    }

    private static string ExportPublicKey(RSA rsa)
    {
        return new string(System.Text.Encoding.ASCII.GetChars(
            System.Text.Encoding.ASCII.GetBytes(
                "-----BEGIN PUBLIC KEY-----\n" +
                Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo(), Base64FormattingOptions.InsertLineBreaks) +
                "\n-----END PUBLIC KEY-----")));
    }

    private static string ExportPrivateKey(RSA rsa)
    {
        return new string(System.Text.Encoding.ASCII.GetChars(
            System.Text.Encoding.ASCII.GetBytes(
                "-----BEGIN RSA PRIVATE KEY-----\n" +
                Convert.ToBase64String(rsa.ExportRSAPrivateKey(), Base64FormattingOptions.InsertLineBreaks) +
                "\n-----END RSA PRIVATE KEY-----")));
    }
}
