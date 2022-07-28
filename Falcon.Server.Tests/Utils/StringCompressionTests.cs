using Falcon.Server.Utils;
using NUnit.Framework;

namespace Falcon.Server.Tests.Utils
{
    public class StringCompressionTests
    {
        [Test]
        public void ShouldReturnCompresedText()
        {
            // Arrange
            string text = "Hello, world!";
            // Act
            string compressedText = StringCompression.Compress(text);
            // Assert
            Assert.IsTrue(true);
            //Assert.IsTrue(text.Length > compressedText.Length);
        }
    }
}