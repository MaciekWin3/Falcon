namespace Falcon.Server.Data
{
    public abstract class BaseRepository
    {
        private readonly IConfiguration configuration;
        protected string ConnectionString { get; set; }

        protected BaseRepository(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
    }
}