using Falcon.Server.Features.Messages.Models;

namespace Falcon.Server.Features.Messages.Repositories
{
    public interface IMessageRepository
    {
        Task CreateAsync(Message message);
        Task<List<Message>> GetAllAsync();
    }
}