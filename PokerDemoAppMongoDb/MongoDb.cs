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
            var client = new MongoClient("mongodb://localhost/?maxPoolSize=5000");
            var db = client.GetServer().GetDatabase("pokerdemo");
            Mongo.Db = db;
            Console.WriteLine(client.Settings);

            collectionNames.Add(typeof(Player), "Players");
            collectionNames.Add(typeof(Account), "Accounts");
            collectionNames.Add(typeof(AccountBalanceTransaction), "Transactions");
        }

        public static MongoCollection<TDomainClass> Collection<TDomainClass>(this MongoDatabase db) {
            return Mongo.Db.GetCollection<TDomainClass>(collectionNames[typeof(TDomainClass)]);
        }
    }
}
