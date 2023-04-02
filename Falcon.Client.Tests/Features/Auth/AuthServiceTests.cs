using Falcon.Client.Features.Auth;
using Moq;
using NUnit.Framework;

namespace Falcon.Client.Tests.Features.Auth
{
    [TestFixture]
    internal class AuthServiceTests
    {
        private Mock<IHttpClientFactory> httpClientFactoryMock;
        private Mock<HttpClient> httpClientMock;
        private readonly string password = "password";

        [SetUp]
        public void Setup()

        {
            httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientMock = new Mock<HttpClient>();
        }

        [Test]
        public async Task ShouldLoginUserSuccessfully()
        {
            // Arrange
            var authService = CreateAuthService();
            //var result = await authService.LoginAsync(username, password);
            //result.Should().NotBeNullOrEmpty();
        }

        private AuthService CreateAuthService()
        {
            return new AuthService(httpClientFactoryMock.Object);
        }
    }
}