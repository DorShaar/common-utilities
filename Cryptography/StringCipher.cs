using System.Text;
using System.Security.Cryptography;

namespace Cryptography;

public static class StringCipher
{
    private const ushort KeySize = 32;
    private const ushort SaltSize = 32;
    private const ushort IVSize = 16; // Initialization Vector.

    // This constant determines the number of iterations for the password bytes generation function.
    private const int DerivationIterations = 1000;

    public static string Encrypt(string plainText, string passPhrase)
    {
        // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
        // so that the same Salt and IV values can be used when decrypting.  
        byte[] saltStringBytes = Generate256BitsOfRandomEntropy(SaltSize);
        byte[] ivStringBytes = Generate256BitsOfRandomEntropy(IVSize);
        byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        using Rfc2898DeriveBytes password = new(passPhrase, saltStringBytes, DerivationIterations, HashAlgorithmName.SHA256);
        byte[] keyBytes = password.GetBytes(KeySize);
            
        using Aes advancedEncryptionStandard = CreateAes();
        using ICryptoTransform encryptor = advancedEncryptionStandard.CreateEncryptor(keyBytes, ivStringBytes);
        using MemoryStream memoryStream = new();
        using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
        cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
        cryptoStream.FlushFinalBlock();
        
        // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
        byte[] cipherTextBytes = saltStringBytes;
        cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
        cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
        memoryStream.Close();
        cryptoStream.Close();
        return Convert.ToBase64String(cipherTextBytes);
    }

    public static string Decrypt(string cipherText, string passPhrase)
    {
        // Get the complete stream of bytes that represent:
        // [32 bytes of Salt] + [16 bytes of IV] + [n bytes of CipherText]
        
        byte[] cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
        
        // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
        byte[] saltStringBytes = cipherTextBytesWithSaltAndIv.Take(SaltSize).ToArray();
        
        // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
        byte[] ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(SaltSize).Take(IVSize).ToArray();
        
        // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
        byte[] cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip(SaltSize + IVSize)
                                                             .Take(cipherTextBytesWithSaltAndIv.Length - (SaltSize + IVSize)).ToArray();

        using Rfc2898DeriveBytes password = new(passPhrase, saltStringBytes, DerivationIterations, HashAlgorithmName.SHA256);
        byte[] keyBytes = password.GetBytes(KeySize);

        using Aes advancedEncryptionStandard = CreateAes();
        using ICryptoTransform decryptor = advancedEncryptionStandard.CreateDecryptor(keyBytes, ivStringBytes);
        using MemoryStream memoryStream = new(cipherTextBytes);
        using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
        using StreamReader streamReader = new(cryptoStream, Encoding.UTF8);
        
        return streamReader.ReadToEnd();
    }

    private static byte[] Generate256BitsOfRandomEntropy(ushort randomBytesSize)
    {
        byte[] randomBytes = new byte[randomBytesSize];
        using RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(randomBytes);
        return randomBytes;
    }

    private static Aes CreateAes()
    {
        Aes advancedEncryptionStandard = Aes.Create();
        advancedEncryptionStandard.BlockSize = 128;
        advancedEncryptionStandard.Mode = CipherMode.CBC;
        advancedEncryptionStandard.Padding = PaddingMode.PKCS7;

        return advancedEncryptionStandard;
    }
}