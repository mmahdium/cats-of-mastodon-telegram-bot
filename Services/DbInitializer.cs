using MongoDB.Driver;
using mstdnCats.Models;

namespace mstdnCats.Services;

public class DbInitializer
{

    public static Task<IMongoCollection<Post>> SetupDb(string mongoDbConnectionString, string dbName)
    {
        if (mongoDbConnectionString == null) throw new Exception("MongoDb connection string is null");

        try
        {
            var client = new MongoClient(mongoDbConnectionString);
            var database = client.GetDatabase(dbName).GetCollection<Post>("posts");
            return Task.FromResult(database);
        }
        catch (Exception ex)
        {
            throw new Exception("Error while connecting to MongoDB: " + ex.Message);
        }
    }
}