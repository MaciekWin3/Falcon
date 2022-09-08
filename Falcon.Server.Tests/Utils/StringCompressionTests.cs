using Bogus;
using Falcon.Server.Utils;
using NUnit.Framework;

namespace Falcon.Server.Tests.Utils
{
    public class StringCompressionTests
    {
        [Test]
        public void ShouldReturnCompressedText()
        {
            // Arrange
            string text = "Hello, world!";
            // Act
            string compressedText = StringCompression.Compress(text);
            // Assert
            Assert.IsTrue(text.Length > compressedText.Length);
        }

        [Test]
        public void ShouldReturnCompressedTextLorem()
        {
            // Arrange
            var faker = new Faker();
            string text = string.Join(" ", faker.Lorem.Sentences(10));
            // Act
            string compressedText = StringCompression.Compress(text);
            // Assert
            Assert.IsTrue(text.Length > compressedText.Length);
        }

        [Test]
        public void ShouldDecompressCompressedString()
        {
            // Arrange
            string text = "Lorem ipsum";
            // Act
            string compressedText = StringCompression.Compress(text);
            string decompressedText = StringCompression.Decompress(compressedText);
            // Assert
            Assert.AreEqual(text, decompressedText);
        }
    }
}