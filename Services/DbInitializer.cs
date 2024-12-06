using JsonFlatFileDataStore;
using MongoDB.Driver;
using mstdnCats.Models;

namespace mstdnCats.Services
{
    public class DbInitializer
    {
        public static Task<IDocumentCollection<Post>> SetupJsonDb(string _dbname)
        {
            // Setup DB
            IDocumentCollection<Post>? collection = null;

            try
            {
                // Initialize Backup DB
                var store = new DataStore($"{_dbname + "_BK"}.json", minifyJson: true);
                collection = store.GetCollection<Post>();
            }
            catch
            {
                return Task.FromResult<IDocumentCollection<Post>>(null);
            }

            // Return collection
            return Task.FromResult(collection);
        }

        public static Task<IMongoDatabase> SetupDb(string mongoDbConnectionString, string dbName)
        {
            if (mongoDbConnectionString == null)
            {
                throw new Exception("MongoDb connection string is null");
            }

            try
            {
                var client = new MongoClient(mongoDbConnectionString);
                var database = client.GetDatabase(dbName);
                return Task.FromResult(database);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while connecting to MongoDB: " + ex.Message);
            }
        }
    }
}