using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace PokerDemoAppMongoDb {
    /// <summary>
    /// Utility methods to connect to server and work with collections.
    /// </summary>
    internal static class Mongo {
        static Dictionary<Type, string> collectionNames = new Dictionary<Type, string>();
        public static MongoDatabase Db;
        public static void Init() {
            var db = new MongoClient("mongodb://localhost").GetServer().GetDatabase("pokerdemo");
            Mongo.Db = db;
            collectionNames.Add(typeof(Player), "Players");
            collectionNames.Add(typeof(Account), "Accounts");
            collectionNames.Add(typeof(AccountBalanceTransaction), "Transactions");
        }

        public static MongoCollection<TDomainClass> Collection<TDomainClass>(this MongoDatabase db) {
            return Mongo.Db.GetCollection<TDomainClass>(collectionNames[typeof(TDomainClass)]);
        }
    }
}
