using System;
using Starcounter;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System.Collections.Generic;

namespace PokerDemoAppMongoDb {

    #region Helper class
    internal static class Mongo {
        static Dictionary<Type, string> collectionNames = new Dictionary<Type, string>();
        public static MongoDatabase Db;
        public static void Init() {
            var db = new MongoClient("mongodb://localhost").GetServer().GetDatabase("pokerdemo");
            Mongo.Db = db;
            collectionNames.Add(typeof(Player), "Players");
            collectionNames.Add(typeof(Account), "Accounts");
        }

        public static void CreateIndexes() {
            var players = Mongo.Db.Collection<Player>();
            var accounts = Mongo.Db.Collection<Account>();

            var name = "PlayerIdIndex";
            var keys = IndexKeys.Ascending("PlayerId");
            var options = IndexOptions.SetUnique(true).SetName(name);
            if (!players.IndexExistsByName(name)) {
                players.CreateIndex(keys, options);
            }

            name = "FullNameIndex";
            keys = IndexKeys.Ascending("FullName");
            options = IndexOptions.SetName(name);
            if (!players.IndexExistsByName(name)) {
                players.CreateIndex(keys, options);
            }

            name = "AccountIdIndex";
            keys = IndexKeys.Ascending("AccountId");
            options = IndexOptions.SetUnique(true).SetName(name);
            if (!accounts.IndexExistsByName(name)) {
                accounts.CreateIndex(keys, options);
            }

            name = "PlayerObjectIdIndex";
            keys = IndexKeys.Ascending("PlayerObjectId");
            options = IndexOptions.SetName(name);
            if (!accounts.IndexExistsByName(name)) {
                accounts.CreateIndex(keys, options);
            }
        }

        public static MongoCollection<TDomainClass> Collection<TDomainClass>(this MongoDatabase db) {
            return Mongo.Db.GetCollection<TDomainClass>(collectionNames[typeof(TDomainClass)]);
        }
    }

    #endregion

    class Program {
        static void Main() {
            Mongo.Init();
            Mongo.CreateIndexes();

            Handle.GET(8082, "/players/{?}", (int playerId) => {
                var json = new PlayerAndAccounts();
                var query = Query<Player>.EQ(p => p.PlayerId, playerId);
                var player = Mongo.Db.Collection<Player>().FindOneAs<Player>(query);
                json.PlayerId = player.PlayerId;
                json.FullName = player.FullName;
                return json;
            });

            Handle.PUT(8082, "/players/{?}", (int playerId, PlayerAndAccounts json) => {
                var player = new Player() { PlayerId = playerId, FullName = json.FullName };
                Mongo.Db.Collection<Player>().Insert(player);
                foreach (var a in json.Accounts) {
                    var account = new Account {
                        AccountId = (int)a.AccountId,
                        AccountType = (int)a.AccountType,
                        Balance = (int)a.Balance,
                        Player = player
                    };
                    Mongo.Db.Collection<Account>().Insert(account);
                }
                return 201;
            });

            Handle.GET(8082, "/dashboard/{?}", (int playerId) => {
                var json = new PlayerAndAccounts();
                var query = Query<Player>.EQ(p => p.PlayerId, playerId);
                var player = Mongo.Db.Collection<Player>().FindOneAs<Player>(query);
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
                var player = Mongo.Db.Collection<Player>().FindOneAs<Player>(query);
                json.PlayerId = player.PlayerId;
                json.FullName = player.FullName;
                return json;
            });

            Handle.POST(8082, "/transfer?f={?}&t={?}&x={?}", (int fromId, int toId, int amount) => {
                var accounts = Mongo.Db.Collection<Account>();
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

            Handle.POST(8082, "/deposit?a={?}&x={?}", (int toId, int amount) => {
                // We want to deposit [amount] to the balance of the given
                // account, failing either if the account does not exist or if
                // the result is a negative balance.
                // We can use FindAndModify, that allows an atomic find/write.
                // We create a query where we allow the increase if the amount
                // is either positive or if the balance is at least as big as
                // the amount if negative.

                IMongoQuery query;
                var accountQuery = Query<Account>.EQ(a => a.AccountId, toId);
                query = amount >= 0 
                    ? accountQuery 
                    : Query.And(accountQuery, Query.GTE("Balance", amount));

                var accounts = Mongo.Db.Collection<Account>();
                var args = new FindAndModifyArgs() {
                     Query = query,
                     Update = Update.Inc("Balance", amount),
                     Upsert = false,
                     VersionReturned = FindAndModifyDocumentVersion.Modified
                };
                var result = accounts.FindAndModify(args);
                if (result.Ok && result.ModifiedDocument != null) {
                    return 200;
                }

                throw new Exception(
                    string.Format("Unable to deposit {0} to account with ID {1}: {2}", amount, toId, result.ErrorMessage ?? string.Empty));
            });

            Handle.DELETE(8082, "/all", () => {
                Mongo.Db.Collection<Player>().RemoveAll();
                Mongo.Db.Collection<Account>().RemoveAll();
                return 200;
            });
        }
    }
}