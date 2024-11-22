using Falcon.Server.Features.Messages.Models;
using Falcon.Server.Features.Messages.Repositories;
using Falcon.Server.Features.Messages.Services;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Falcon.Server.Tests.Features.Messages
{
    [TestFixture]
    internal class MessagesServiceTests
    {
        private IMessageRepository messageRepositoryMock = null!;

        [SetUp]
        public void Setup()
        {
            messageRepositoryMock = Substitute.For<IMessageRepository>();
        }

        [Test]
        public async Task ShouldCreateMessage()
        {
            // Arrange
            var message = new Message();
            var messageService = CreateMessageService();

            // Act
            messageRepositoryMock
                .CreateAsync(message)
                .Returns(Task.FromResult(message));

            // Assert
            var result = await messageService.CreateAsync(message);
            result.Should().NotBeNull();
        }

        private MessageService CreateMessageService()
        {
            return new MessageService(messageRepositoryMock);
        }
    }
}