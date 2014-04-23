
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
                return Mongo.Db.GetCollection("Accounts").FindAs<Account>(query);
            }
        }
    }

    public class Account {
        public ObjectId Id { get; set; }
        public int AccountId { get; set; }
        public int AccountType { get; set; }
        public int Balance { get; set; }
        public ObjectId PlayerObjectId { get; set; }

        [BsonIgnore]
        public Player Player {
            get { return Mongo.Db.GetCollection("Players").FindOneByIdAs<Player>(PlayerObjectId); }
            set { PlayerObjectId = value.Id; }
        }
    }
}