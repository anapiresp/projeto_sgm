using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SeaRise.Services.Database
{
    public class MongoService
    {
        private readonly MongoClient _client;
        private readonly IMongoDatabase _database;

        public MongoService(IOptions<MongoDBSettings> options)
        {
            var settings = options.Value;

            // Build MongoClientSettings with ServerApi V1 (recommended for Atlas)
            var clientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
            clientSettings.ServerApi = new ServerApi(ServerApiVersion.V1);

            _client = new MongoClient(clientSettings);
            _database = _client.GetDatabase(settings.DatabaseName);
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }

        // Ping the server to confirm connectivity. Returns the ping result document.
        public async Task<BsonDocument> PingAsync(CancellationToken cancellation = default)
        {
            // Ping against the admin database as in the MongoDB example
            var adminDb = _client.GetDatabase("admin");
            var command = new BsonDocument("ping", 1);
            var result = await adminDb.RunCommandAsync<BsonDocument>(command, cancellationToken: cancellation);
            return result;
        }
    }
}