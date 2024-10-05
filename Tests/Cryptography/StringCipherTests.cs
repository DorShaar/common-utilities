using System.Security.Cryptography;
using Cryptography;
using Xunit;

namespace Tests.Cryptography;

public class StringCipherTests
{
	[Fact]
	public void EncryptAndDecrypt_WithSamePassword_PlainTextAndDecryptedTextAreEqual()
	{
		string password = Guid.NewGuid().ToString();
		const string plainText = "Hello World!";
		
		string encryptedText = StringCipher.Encrypt(plainText, password);
		string decryptedText = StringCipher.Decrypt(encryptedText, password);
		
		Assert.Equal(plainText, decryptedText);
	}
	
	[Fact]
	public void EncryptAndDecrypt_WithDifferentPasswords_PlainTextAndDecryptedTextAreNotEqual()
	{
		string password = Guid.NewGuid().ToString();
		string otherPassword = Guid.NewGuid().ToString();
		const string plainText = "Hello World!";
		
		string encryptedText = StringCipher.Encrypt(plainText, password); 
		Assert.Throws<CryptographicException>(() => StringCipher.Decrypt(encryptedText, otherPassword));
	}
}