using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace gamemaster
{
    public class MongoStore
    {
        private readonly MongoClient _mc;

        public MongoStore(IOptions<MongoConfig> cfg)
        {
            var cs = MongoClientSettings.FromConnectionString(cfg.Value.ConnectionString);
            cs.ConnectTimeout = TimeSpan.FromSeconds(2);
            _mc = new MongoClient(cs);
            Db = _mc.GetDatabase(cfg.Value.DbName ?? "gamemaster");
            Bright = GetCollection<Action>("actions");
            Journal = GetCollection<JournalRecord>("journal");
            Ops = GetCollection<OperationDescription>("ops");
            Players = GetCollection<Player>("players");
            App = GetCollection<AppState>("appState");
        }

        public IMongoCollection<OperationDescription> Ops { get; set; }

        public IMongoDatabase Db { get; set; }

        public IMongoCollection<JournalRecord> Journal { get; }
        public IMongoCollection<AppState> App { get; set; }
        public IMongoCollection<Player> Players { get; set; }
        public IMongoCollection<Action> Bright { get; set; }

        private IMongoCollection<T> GetCollection<T>(string col)
        {
            return Db.GetCollection<T>(col);
        }

        public async Task<IClientSessionHandle> StartSessionAsync()
        {
            return await Db.Client.StartSessionAsync();
        }
    }
}