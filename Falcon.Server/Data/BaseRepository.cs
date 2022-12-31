using Falcon.Server.Data;

namespace Falcon.Server
{
    public class BaseRepository
    {
        protected readonly FalconDbContext context;

        public BaseRepository(FalconDbContext context)
        {
            this.context = context;
        }
    }
}