using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace gamemaster
{
    public class MongoStore
    {
        private readonly MongoClient _mc;

        public MongoStore(IOptions<MongoConfig> cfg)
        {
            _mc = new MongoClient(cfg.Value.ConnectionString);
            Db = _mc.GetDatabase(cfg.Value.DbName ?? "gamemaster");
            Bright = GetCollection<Action>("actions");
            Journal = GetCollection<JournalRecord>("journal");
            Players = GetCollection<Player>("players");
            App = GetCollection<AppState>("appState");
        }

        public IMongoDatabase Db { get; set; }

        public IMongoCollection<JournalRecord> Journal { get; }
        public IMongoCollection<AppState> App { get; set; }
        public IMongoCollection<Player> Players { get; set; }
        public IMongoCollection<Action> Bright { get; set; }

        private IMongoCollection<T> GetCollection<T>(string col)
        {
            return Db.GetCollection<T>(col);
        }
    }
}