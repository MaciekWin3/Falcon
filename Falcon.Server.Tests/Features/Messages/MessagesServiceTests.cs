using Falcon.Server.Features.Messages.Models;
using Falcon.Server.Features.Messages.Repositories;
using Falcon.Server.Features.Messages.Services;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Falcon.Server.Tests.Features.Messages
{
    [TestFixture]
    internal class MessagesServiceTests
    {
        private Mock<IMessageRepository> messageRepositoryMock;

        [SetUp]
        public void Setup()
        {
            messageRepositoryMock = new Mock<IMessageRepository>();
        }

        [Test]
        public async Task ShouldCreateMessage()
        {
            // Arrange
            var message = new Message();
            var messageService = CreateMessageService();

            // Act
            messageRepositoryMock.Setup(x => x.CreateAsync(message)).Returns(Task.FromResult(message));

            // Assert
            var result = await messageService.CreateAsync(message);
            result.Should().NotBeNull();
        }

        private MessageService CreateMessageService()
        {
            return new MessageService(messageRepositoryMock.Object);
        }
    }
}