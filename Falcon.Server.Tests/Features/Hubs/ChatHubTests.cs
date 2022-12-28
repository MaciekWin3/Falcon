using Castle.Core.Configuration;
using Falcon.Server.Features.Messages.Services;
using Moq;
using NUnit.Framework;
using ILogger = Serilog.ILogger;

namespace Falcon.Server.Tests.Features.Hubs
{
    [TestFixture]
    internal class ChatHubTests
    {
        private const string FALCON_BOT = "Falcon Bot";
        private Mock<IMessageService> messageServiceMock;
        private Mock<IConfiguration> configurationMock;
        private Mock<ILogger> loggerMock;
    }
}