using Falcon.Server.Utils;
using NUnit.Framework;

namespace Falcon.Server.Tests.Utils
{
    public class CryptographyTests
    {
        private const int CRYPTOGRAPHY_KEY = 10;

        [Test]
        public void ShouldEncryptAndDecryptText()
        {
            // Arrage
            string text = "Hello, world!";
            // Act
            string encryptedText = Cryptography.EncryptDecrypt(text, CRYPTOGRAPHY_KEY);
            string decryptedText = Cryptography.EncryptDecrypt(encryptedText, CRYPTOGRAPHY_KEY);
            // Assert
            Assert.AreNotEqual(encryptedText, decryptedText);
            Assert.AreEqual(text, decryptedText);
        }
    }
}