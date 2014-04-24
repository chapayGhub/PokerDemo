
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;

namespace PokerDemoAppMongoDb {

    public class Player {
        public ObjectId Id { get; set; }
        public int PlayerId { get; set; }
        public string FullName { get; set; }
        
        [BsonIgnore]
        public IEnumerable<Account> Accounts {
            get {
                var query = Query<Account>.EQ(a => a.PlayerObjectId, Id);
                return Mongo.Db.Collection<Account>().FindAs<Account>(query);
            }
        }
    }

    public class Account {
        public ObjectId Id { get; set; }
        public int AccountId { get; set; }
        public int AccountType { get; set; }
        public int Balance { get; set; }
        public ObjectId PlayerObjectId { get; set; }
        public ObjectId[] PendingTransactions { get; set; }

        [BsonIgnore]
        public Player Player {
            get { return Mongo.Db.Collection<Player>().FindOneByIdAs<Player>(PlayerObjectId); }
            set { PlayerObjectId = value.Id; }
        }
    }

    public class AccountBalanceTransaction {
        public static class States {
            public const string Initial = "Initial";
            public const string Pending = "Pending";
            public const string Committed = "Committed";
            public const string Done = "Done";
            public const string Cancelling = "Cancelling";
        }

        public ObjectId Id { get; set; }
        public ObjectId From { get; set; }
        public ObjectId To { get; set; }
        public int Amount { get; set; }
        public string State { get; set; }
    }
}