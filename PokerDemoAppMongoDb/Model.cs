
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace PokerDemoAppMongoDb {

    public class Player {
        public ObjectId Id { get; set; }
        public int PlayerId { get; set; }
        public string FullName { get; set; }
        
        [BsonIgnore]
        public ICollection<Account> Accounts {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
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