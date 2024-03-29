﻿using System;
using System.Collections.Generic;
using System.Transactions;
using System.Linq;
using Starcounter;
using System.Data.Entity;

namespace PokerDemoAppMsSql {
    class Program {
        #region Transaction management
        private static TransactionOptions transopt = new TransactionOptions() {
//            IsolationLevel = System.Transactions.IsolationLevel.Snapshot
        };

        public static void RunTransaction(Action act) {
            for (int retry = 0; retry < 100; retry++) {
                using (var trans = new TransactionScope(TransactionScopeOption.RequiresNew, transopt)) {
                    try {
                        act();
                        trans.Complete();
                        break;
                    } catch (System.Data.Entity.Infrastructure.DbUpdateException) { }
                }
            }
        }
        #endregion

        static void Main(string[] args) {
            Json.DirtyCheckEnabled = false;

            if (args != null) {
                if (args.Length == 1)
                    PlayersDemoDb.SetConnection(args[0]);
                if (args.Length == 2)
                    PlayersDemoDb.SetConnection(args[0], args[1]);
            }
            InitDb();

            Handle.GET(8081, "/players/{?}", (int playerId) => {
                var json = new PlayerJson();
                using (var db = new PlayersDemoDb())
                    RunTransaction(delegate {
                        json.Data = db.Players.Find(playerId);
                    });
                return new Response() { BodyBytes = json.ToJsonUtf8() };
            });

            Handle.GET(8081, "/dashboard/{?}", (int playerId) => {
                var json = new PlayerAndAccounts();
                using (var db = new PlayersDemoDb())
                    RunTransaction(delegate {
                        json.Data = db.Players.Find(playerId);
                    });
                return new Response() { BodyBytes = json.ToJsonUtf8() };
            });

            Handle.GET(8081, "/players?f={?}", (string fullName) => {
                var json = new PlayerJson();
                using (var db = new PlayersDemoDb())
                    RunTransaction(delegate {
                        json.Data = db.Players.Where(p => p.FullName == fullName).First<Player>();
                    });
                return new Response() { BodyBytes = json.ToJsonUtf8() };
            });

            Handle.PUT(8081, "/players/{?}", (int playerId, PlayerAndAccounts json) => {
                using (var db = new PlayersDemoDb())
                    RunTransaction(delegate {
                        Player player = new Player { PlayerId = playerId, FullName = json.FullName };
                        db.Players.Add(player);
                        foreach (var a in json.Accounts) {
                            db.Accounts.Add(new Account {
                                AccountId = (int)a.AccountId,
                                AccountType = (int)a.AccountType,
                                Balance = (int)a.Balance,
                                Player = player
                            });
                        }
                        db.SaveChanges();
                    });
                return 201;
            });

            Handle.POST(8081, "/transfer?f={?}&t={?}&x={?}", (int fromId, int toId, int amount) => {
                using (var db = new PlayersDemoDb())
                    RunTransaction(delegate {
                        Account source = db.Accounts.Find(fromId);
                        Account target = db.Accounts.Find(toId);
                        source.Balance -= amount;
                        target.Balance += amount;
                        db.SaveChanges();
                    });
                return 200;
            });

            Handle.POST(8081, "/deposit?a={?}&x={?}", (int toId, int amount) => {
                using (var db = new PlayersDemoDb())
                    RunTransaction(delegate {
                        Account account = db.Accounts.Find(toId);
                        account.Balance += amount;
                        db.SaveChanges();
                    });
                return 200;
            });

            Handle.DELETE(8081, "/all", () => {
                using (var db = new PlayersDemoDb())
                    RunTransaction(delegate {
                        db.Database.ExecuteSqlCommand("DELETE FROM dbo.Accounts");
                        db.Database.ExecuteSqlCommand("DELETE FROM dbo.Players");
                    });
                return 200;
            });
        }

        // Init indexes and snapshot isolation
        private static void InitDb() {
            using (var db = new PlayersDemoDb()) {
                //Reference: Set TransactionalBehavior.DoNotEnsureTransaction in ExecuteSqlCommand http://www.danbartram.com/entity-framework-6-and-executesqlcommand/
                db.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, "ALTER DATABASE \"PlayersDemoDb\" SET ALLOW_SNAPSHOT_ISOLATION ON");
                try {
                    db.Database.ExecuteSqlCommand("CREATE INDEX Account_fk ON dbo.Accounts(Player_PlayerId,AccountId)");
                    db.Database.ExecuteSqlCommand("CREATE INDEX PlayerNameIndx ON dbo.Players(FullName)");
                } catch (System.Data.SqlClient.SqlException) { };
            }
        }
    }
}