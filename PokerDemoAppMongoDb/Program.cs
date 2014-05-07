using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Starcounter;
using System;
using System.Linq;

namespace PokerDemoAppMongoDb {

    class Program {
        static object _lock = new object();

        static void Main() {
            Json.DirtyCheckEnabled = false;
            Mongo.Init();
            CreateIndexes();
            ProcessTransactionsNotDone();

            Handle.GET(8082, "/serverAggregates", () => {
                // Return a string in the form:
                // key1=value1;key2=value2;key[n]=value[n]...
                // This values can then be displayed (by pressing a button)
                // in the client GUI.
                var accounts = Mongo.Db.Collection<Account>();
                var sum = new BsonDocument {
                { "$group", 
                    new BsonDocument { 
                    { "_id", "Balance" }, 
                    { "Total", new BsonDocument { { "$sum", "$Balance" } } } } 
                  } 
                };
                var args = new AggregateArgs() { Pipeline = new[] { sum } };
                var result = accounts.Aggregate(args).FirstOrDefault();
                long total = result != null ? result["Total"].AsInt32 : 0;

                return "AccountBalanceTotal=" + total.ToString();
            });

            Handle.GET(8082, "/players/{?}", (int playerId) => {
                var json = new PlayerJson();
                var query = Query<Player>.EQ(p => p.PlayerId, playerId);
                json.Data = Mongo.Db.Collection<Player>().FindOneAs<Player>(query);
                return new Response() { BodyBytes = json.ToJsonUtf8() };
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
                json.Data = Mongo.Db.Collection<Player>().FindOneAs<Player>(query);
                return new Response() { BodyBytes = json.ToJsonUtf8() };
            });

            Handle.GET(8082, "/players?f={?}", (string fullName) => {
                var json = new PlayerJson();
                var query = Query<Player>.EQ(p => p.FullName, fullName);
                json.Data = Mongo.Db.Collection<Player>().FindOneAs<Player>(query);
                return new Response() { BodyBytes = json.ToJsonUtf8() };
            });

            Handle.POST(8082, "/transfer?f={?}&t={?}&x={?}", (int fromId, int toId, int amount) => {
                lock (_lock) {
                    var accounts = Mongo.Db.Collection<Account>();
                    var query = Query<Account>.EQ(a => a.AccountId, fromId);
                    var source = accounts.FindOneAs<Account>(query);
                    query = Query<Account>.EQ(a => a.AccountId, toId);
                    var target = accounts.FindOneAs<Account>(query);

                    source.Balance -= amount;
                    target.Balance += amount;
                    // The client currently doesn't handle this - the result is a dialog window.
                    // Disabling this until fixed.
                    //if (source.Balance < 0 || target.Balance < 0) {
                    //    throw new Exception("You cannot move money that is not in the account");
                    //}

                    var transactions = Mongo.Db.Collection<AccountBalanceTransaction>();
                    var transaction = new AccountBalanceTransaction() {
                        From = source.Id,
                        To = target.Id,
                        Amount = amount,
                        State = AccountBalanceTransaction.States.Pending  // Skip the "Initial" state, as we don't really differentiate between the two
                    };
                    transactions.Insert(transaction);

                    try {
                        source.AddTransaction(transaction);
                        target.AddTransaction(transaction);
                        accounts.Save(source);
                        accounts.Save(target);
                        transaction.State = AccountBalanceTransaction.States.Committed;
                        transactions.Save(transaction);
                    } catch {
                        TransactionManager.RollbackPending(transaction);
                        throw;
                    }

                    // The transaction is considered committed and the database is
                    // in a consistent state. Clean up the transaction and the
                    // accounts, but never mind trying to do better here. If anything
                    // fails now, a recovery will remedy eventually (and all state
                    // is fine in a business-perspective).

                    TransactionManager.UpdateAsDone(transaction, transactions, accounts, source, target);
                }

                return 200;
            });

            Handle.POST(8082, "/deposit?a={?}&x={?}", (int toId, int amount) => {
                lock (_lock) {
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
                        : Query.And(accountQuery, Query.GTE("Balance", Math.Abs(amount)));

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
                }
            });

            Handle.DELETE(8082, "/all", () => {
                Mongo.Db.Collection<Player>().RemoveAll();
                Mongo.Db.Collection<Account>().RemoveAll();
                Mongo.Db.Collection<AccountBalanceTransaction>().RemoveAll();
                return 200;
            });
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

        static void ProcessTransactionsNotDone() {
            var transactions = Mongo.Db.Collection<AccountBalanceTransaction>();
            var notDone = transactions.Find(Query<AccountBalanceTransaction>.NE(a => a.State, AccountBalanceTransaction.States.Done));
            foreach (var t in notDone) {
                if (t.IsPending) {
                    TransactionManager.RollbackPending(t);
                } else if (t.IsCommitted) {
                    TransactionManager.RecoverCommitted(t);
                } else {
                    // Transaction with a state we don't recognize. We refuse
                    // to try anything automatic - manual help required.
                    throw new Exception(
                        string.Format("Transaction with ID {0} is in a state ({1}) we can not automatically recover.", t.Id, t.State));
                }
            }
        }
    }
}