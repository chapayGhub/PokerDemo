using System;
using Starcounter;
using MongoDB.Driver;

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

            Handle.DELETE(8082, "/all", () => {
                Mongo.Db.GetCollection("Players").RemoveAll();
                Mongo.Db.GetCollection("Accounts").RemoveAll();
                return 200;
            });
        }
    }
}