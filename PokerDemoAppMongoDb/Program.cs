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

            Handle.DELETE("/all", () => {
                Mongo.Db.GetCollection("Players").RemoveAll();
                Mongo.Db.GetCollection("Accounts").RemoveAll();
                return 200;
            });
        }
    }
}