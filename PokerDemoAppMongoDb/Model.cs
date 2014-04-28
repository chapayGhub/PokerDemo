
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
        [BsonRepresentation(BsonType.Array)]
        private List<ObjectId> PendingTransactions { get; set; }

        [BsonIgnore]
        public Player Player {
            get { return Mongo.Db.Collection<Player>().FindOneByIdAs<Player>(PlayerObjectId); }
            set { PlayerObjectId = value.Id; }
        }

        public void AddTransaction(AccountBalanceTransaction transaction) {
            var pending = PendingTransactions;
            if (pending == null) {
                pending = PendingTransactions = new List<ObjectId>();
            }
            pending.Add(transaction.Id);
        }

        public bool RemoveTransaction(AccountBalanceTransaction transaction) {
            var pending = PendingTransactions;
            return pending == null ? false : pending.Remove(transaction.Id);
        }

        public bool ContainsTransaction(AccountBalanceTransaction transaction) {
            return PendingTransactions == null ? false : PendingTransactions.Contains(transaction.Id);
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

        public bool IsPending {
            get {
                return State.Equals(States.Pending);
            }
        }

        public bool IsCommitted {
            get {
                return State.Equals(States.Committed);
            }
        }

        public bool IsDone {
            get {
                return State.Equals(States.Done);
            }
        }

        public ObjectId Id { get; set; }
        public ObjectId From { get; set; }
        public ObjectId To { get; set; }
        public int Amount { get; set; }
        public string State { get; set; }
    }
}