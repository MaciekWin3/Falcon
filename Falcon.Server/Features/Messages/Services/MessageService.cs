using Falcon.Server.Features.Messages.Repositories;
using Falcon.Server.Models;
using Falcon.Server.Utils;

namespace Falcon.Server.Features.Messages.Services
{
    public class MessageService
    {
        private readonly MessageRepository messageRepository;

        public MessageService(MessageRepository messageRepository)
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