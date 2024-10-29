using StackExchange.Redis;

namespace Order.API.RedisServices;

public class RedisService
{
    private readonly ConnectionMultiplexer _connectionMultiplexer;

    public RedisService(IConfiguration configuration)
    {
        var host = configuration.GetSection("Redis")["Host"];
        var port = configuration.GetSection("Redis")["Port"];

        var config = $"{host}:{port}";
        _connectionMultiplexer = ConnectionMultiplexer.Connect(config);
    }

    public ConnectionMultiplexer GetConnectionMultiplexer()
    {
        return _connectionMultiplexer;
    }

    public IDatabase GetdB(int db)
    {
        return _connectionMultiplexer.GetDatabase(db);
    }
    
}