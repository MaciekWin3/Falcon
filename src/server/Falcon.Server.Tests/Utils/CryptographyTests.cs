using Falcon.Server.Utils;
using FluentAssertions;
using NUnit.Framework;

namespace Falcon.Server.Tests.Utils
{
    [TestFixture]
    public class CryptographyTests
    {
        private const int CRYPTOGRAPHY_KEY = 10;

        [Test]
        public void ShouldEncryptAndDecryptText()
        {
            // Arrage
            string text = "Hello, world!";
            // Act
            string encryptedText = Cryptography.XorEncrypt(text, CRYPTOGRAPHY_KEY);
            string decryptedText = Cryptography.XorDecrypt(encryptedText, CRYPTOGRAPHY_KEY);

            // Assert
            encryptedText.Should().NotBe(decryptedText);
            text.Should().Be(decryptedText);
        }
    }
}