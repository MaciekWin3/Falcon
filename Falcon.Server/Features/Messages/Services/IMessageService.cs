using Falcon.Server.Features.Messages.Models;
using Falcon.Server.Utils;

namespace Falcon.Server.Features.Messages.Services
{
    public interface IMessageService
    {
        Task<Result> CreateAsync(Message message);
    }
}