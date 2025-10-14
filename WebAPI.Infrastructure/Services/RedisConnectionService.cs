using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace WebAPI.Infrastructure.Services
{
    public class RedisConnectionService
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public RedisConnectionService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            _connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
        }

        public IConnectionMultiplexer GetConnection()
        {
            return _connectionMultiplexer;
        }
    }
}

