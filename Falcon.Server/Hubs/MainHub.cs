using Microsoft.AspNetCore.SignalR;

namespace Falcon.Server.Hubs
{
    public class MainHub : Hub
    {
        public async IAsyncEnumerable<DateTime> Streaming(CancellationToken cancellationToken)
        {
            while (true)
            {
                yield return DateTime.UtcNow;
                await Task.Delay(1000, cancellationToken);
            }
        }
    }
}
