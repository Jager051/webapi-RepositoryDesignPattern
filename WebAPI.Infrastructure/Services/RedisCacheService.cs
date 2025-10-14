using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Text.Json;
using WebAPI.Core.Interfaces;

namespace WebAPI.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _database;
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _database = _connectionMultiplexer.GetDatabase();
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var value = await _database.StringGetAsync(key);
                if (!value.HasValue)
                    return default(T);

                return JsonSerializer.Deserialize<T>(value!);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var serializedValue = JsonSerializer.Serialize(value);
                await _database.StringSetAsync(key, serializedValue, expiration);
            }
            catch (Exception)
            {
                // Log error if needed
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _database.KeyDeleteAsync(key);
            }
            catch (Exception)
            {
                // Log error if needed
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
                var keys = server.Keys(pattern: pattern);
                await _database.KeyDeleteAsync(keys.ToArray());
            }
            catch (Exception)
            {
                // Log error if needed
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return await _database.KeyExistsAsync(key);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<TimeSpan?> GetExpirationAsync(string key)
        {
            try
            {
                return await _database.KeyTimeToLiveAsync(key);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task SetExpirationAsync(string key, TimeSpan expiration)
        {
            try
            {
                await _database.KeyExpireAsync(key, expiration);
            }
            catch (Exception)
            {
                // Log error if needed
            }
        }
    }
}
