using Falcon.Server.Features.Messages.Models;
using Falcon.Server.Features.Messages.Repositories;
using Falcon.Server.Utils;

namespace Falcon.Server.Features.Messages.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository messageRepository;

        public MessageService(IMessageRepository messageRepository)
        {
            this.messageRepository = messageRepository;
        }

        public async Task<Result> CreateAsync(Message message)
        {
            await messageRepository.CreateAsync(message);
            return Result.Ok();
        }
    }
}