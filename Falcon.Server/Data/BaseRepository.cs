namespace Falcon.Server.Data
{
    public abstract class BaseRepository
    {
        protected readonly FalconDbContext context;

        protected BaseRepository(FalconDbContext context)
        {
            this.context = context;
        }
    }
}