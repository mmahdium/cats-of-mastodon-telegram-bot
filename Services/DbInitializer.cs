using JsonFlatFileDataStore;
using mstdnCats.Models;

namespace mstdnCats.Services
{
    public class DbInitializer
    {
        public static Task<IDocumentCollection<Post>> SetupDb(string _dbname)
        {
            // Setup DB
            IDocumentCollection<Post>? collection = null;

            try
            {
                // Initialize DB
                var store = new DataStore($"{_dbname}.json", minifyJson: false);
                collection = store.GetCollection<Post>();
            }
            catch
            {
                return Task.FromResult<IDocumentCollection<Post>>(null);
            }

            // Return collection
            return Task.FromResult(collection);
        }
    }
}