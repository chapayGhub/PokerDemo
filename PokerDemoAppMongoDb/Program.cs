using System;
using Starcounter;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace PokerDemoAppMongoDb {

    #region Helper class
    internal static class Mongo {
        public static MongoDatabase Db;
        public static void Init() {
            var db = new MongoClient("mongodb://localhost").GetServer().GetDatabase("pokerdemo");
            Mongo.Db = db;
        }
    }

    #endregion

    class Program {
        static void Main() {
            Mongo.Init();

            Handle.GET(8082, "/players/{?}", (int playerId) => {
                var json = new PlayerAndAccounts();
                var query = Query<Player>.EQ(p => p.PlayerId, playerId);
                var player = Mongo.Db.GetCollection("Players").FindOneAs<Player>(query);
                json.PlayerId = player.PlayerId;
                json.FullName = player.FullName;
                return json;
            });

            Handle.PUT(8082, "/players/{?}", (int playerId, PlayerAndAccounts json) => {
                var player = new Player() { PlayerId = playerId, FullName = json.FullName };
                Mongo.Db.GetCollection("Players").Insert(player);
                foreach (var a in json.Accounts) {
                    var account = new Account {
                        AccountId = (int)a.AccountId,
                        AccountType = (int)a.AccountType,
                        Balance = (int)a.Balance,
                        Player = player
                    };
                    Mongo.Db.GetCollection("Accounts").Insert(account);
                }
                return 201;
            });

            Handle.GET(8082, "/dashboard/{?}", (int playerId) => {
                var json = new PlayerAndAccounts();
                var query = Query<Player>.EQ(p => p.PlayerId, playerId);
                var player = Mongo.Db.GetCollection("Players").FindOneAs<Player>(query);
                json.PlayerId = player.PlayerId;
                json.FullName = player.FullName;
                foreach (Account account in player.Accounts) {
                    var a = json.Accounts.Add();
                    a.AccountId = account.AccountId;
                    a.Balance = account.Balance;
                }

                return json;
            });

            Handle.GET(8082, "/players?f={?}", (string fullName) => {
                var json = new PlayerAndAccounts();
                var query = Query<Player>.EQ(p => p.FullName, fullName);
                var player = Mongo.Db.GetCollection("Players").FindOneAs<Player>(query);
                json.PlayerId = player.PlayerId;
                json.FullName = player.FullName;
                return json;
            });

            Handle.POST(8082, "/transfer?f={?}&t={?}&x={?}", (int fromId, int toId, int amount) => {
                var accounts = Mongo.Db.GetCollection("Accounts");
                var query = Query<Account>.EQ(a => a.AccountId, fromId);
                Account source = accounts.FindOneAs<Account>(query);
                query = Query<Account>.EQ(a => a.AccountId, toId);
                Account target = accounts.FindOneAs<Account>(query);
                source.Balance -= amount;
                target.Balance += amount;
                accounts.Save(source);
                accounts.Save(target);
                return 200;
            });

            Handle.DELETE(8082, "/all", () => {
                Mongo.Db.GetCollection("Players").RemoveAll();
                Mongo.Db.GetCollection("Accounts").RemoveAll();
                return 200;
            });
        }
    }
}