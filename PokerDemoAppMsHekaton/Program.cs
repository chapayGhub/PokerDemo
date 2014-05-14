using System;
using System.Collections.Generic;
using System.Linq;
using Starcounter;
using System.Data.Entity;
using System.Data.SqlClient;

namespace PokerDemoAppMsSqlHekaton
{
    class Program
    {
        static void Main(string[] args)
        {
            Json.DirtyCheckEnabled = false;
            if (args != null)
            {
                if (args.Length == 1)
                    PlayersDemoDb.SetConnection(args[0]);
                if (args.Length == 2)
                    PlayersDemoDb.SetConnection(args[0], args[1]);
            }
            InitDb();

            Handle.GET(8081, "/players/{?}", (int playerId) =>
            {
                var json = new PlayerAndAccounts();
                using (var db = new PlayersDemoDb())
                {
                    Player player = db.Players.SqlQuery("dbo.GetPlayer @PlayerId",
                        new SqlParameter("PlayerId", playerId)).First<Player>();
                    json.PlayerId = player.PlayerId;
                    json.FullName = player.FullName;
                }
                return json;
            });

            Handle.GET(8081, "/dashboard/{?}", (int playerId) =>
            {
                var json = new PlayerAndAccounts();
                using (var db = new PlayersDemoDb())
                {
                    List<Account> playerAccounts = db.Accounts.SqlQuery("dbo.GetPlayerAccounts @PlayerId",
                        new SqlParameter("PlayerId", playerId)).ToList();

                    Player player = db.Players.SqlQuery("dbo.GetPlayer @PlayerId",
                        new SqlParameter("PlayerId", playerId)).First<Player>();

                    json.PlayerId = player.PlayerId;
                    json.FullName = player.FullName;

                    foreach (Account account in playerAccounts)
                    {
                        var a = json.Accounts.Add();
                        a.AccountId = account.AccountId;
                        a.Balance = account.Balance;
                    }
                }
                return json;
            });

            Handle.GET(8081, "/players?f={?}", (string fullName) =>
            {
                var json = new PlayerAndAccounts();
                using (var db = new PlayersDemoDb())
                {
                    Player player = db.Players.SqlQuery("dbo.GetPlayerByFullName @FullName",
                        new SqlParameter("FullName", fullName)).First<Player>();
                    json.PlayerId = player.PlayerId;
                    json.FullName = player.FullName;
                }
                return json;
            });

            Handle.PUT(8081, "/players/{?}", (int playerId, PlayerAndAccounts json) =>
            {
                using (var db = new PlayersDemoDb())
                {
                    Player player = new Player { PlayerId = playerId, FullName = json.FullName };
                    db.Database.ExecuteSqlCommand("dbo.AddPlayer @PlayerId = {0}, @FullName = {1}", playerId, json.FullName);
                    foreach (var a in json.Accounts)
                    {
                        db.Database.ExecuteSqlCommand("dbo.AddPlayerAccount @AccountId = {0}, @AccountType = {1}, @Balance = {2}, @PlayerRefId = {3}",
                                                            (int)a.AccountId, (int)a.AccountType, (int)a.Balance, playerId);
                    }

                }
                return 201;
            });

            Handle.POST(8081, "/transfer?f={?}&t={?}&x={?}", (int fromId, int toId, int amount) =>
            {
                using (var db = new PlayersDemoDb())
                    db.Database.ExecuteSqlCommand("dbo.AccountTransfers @FromId = {0}, @ToId = {1}, @Amount = {2}", fromId, toId, amount);
                return 200;
            });

            Handle.POST(8081, "/deposit?a={?}&x={?}", (int toId, int amount) =>
            {
                using (var db = new PlayersDemoDb())
                    db.Database.ExecuteSqlCommand("dbo.DepositAccount @ToId = {0}, @Amount = {1}", toId, amount);
                return 200;
            });

            Handle.DELETE(8081, "/all", () =>
            {
                using (var db = new PlayersDemoDb())
                    db.Database.ExecuteSqlCommand("dbo.DeleteAccountsAndPlayers");
                return 200;
            });
        }

        // Init indexes
        private static void InitDb()
        {
            using (var db = new PlayersDemoDb())
            {
                try
                {
                    db.Database.ExecuteSqlCommand("CREATE INDEX Account_fk ON dbo.Accounts(Player_PlayerId,AccountId)");
                    db.Database.ExecuteSqlCommand("CREATE INDEX PlayerNameIndx ON dbo.Players(FullName)");
                }
                catch (System.Data.SqlClient.SqlException) { };
            }
        }
    }
}