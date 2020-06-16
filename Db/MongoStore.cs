using System;
using System.Threading.Tasks;
using gamemaster.Config;
using gamemaster.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Action = gamemaster.Models.Action;

namespace gamemaster.Db
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
            Bright = GetCollection<SlackInteractionAction>("actions");
            Journal = GetCollection<JournalRecord>("journal");
            Ops = GetCollection<OperationDescription>("ops");
            Players = GetCollection<Player>("players");
            Totes = GetCollection<Tote>("totes");
            ToteReports = GetCollection<ToteReport>("toteReports");
            App = GetCollection<AppState>("appState");
        }

        public IMongoCollection<OperationDescription> Ops { get; set; }

        public IMongoDatabase Db { get; set; }

        public IMongoCollection<JournalRecord> Journal { get; }
        public IMongoCollection<AppState> App { get; set; }
        public IMongoCollection<Player> Players { get; set; }
        public IMongoCollection<Tote> Totes { get; set; }
        public IMongoCollection<ToteReport> ToteReports { get; set; }
        public IMongoCollection<SlackInteractionAction> Bright { get; set; }

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