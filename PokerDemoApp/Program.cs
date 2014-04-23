using System;
using System.Collections.Generic;
using Starcounter;

namespace PokerDemoApp {
    class Program {
        static void Main() {
            CreateIndexes();

            Handle.GET("/players/{?}", (int playerId) => {
                var json = new PlayerAndAccounts();
                json.Data = Db.SQL("SELECT p FROM Player p WHERE PlayerId = ?", playerId).First;
                return json;
            });

            Handle.GET("/dashboard/{?}", (int playerId) => {
                var json = new PlayerAndAccounts();
                json.Data = Db.SQL("SELECT p FROM Player p WHERE PlayerId = ?", playerId).First;
                return json;
            });

            Handle.GET("/players?f={?}", (string fullName) => {
                var json = new PlayerAndAccounts();
                json.Data = Db.SQL("SELECT p FROM Player p WHERE FullName = ?", fullName).First;
                return json;
            });

            Handle.PUT("/players/{?}", (int playerId, PlayerAndAccounts json) => {
                Db.Transaction(() => {
                    var player = new Player { PlayerId = (int)json.PlayerId, FullName = json.FullName };
                    foreach (var a in json.Accounts) {
                        new Account {
                            AccountId = (int)a.AccountId,
                            AccountType = (int)a.AccountType,
                            Balance = (int)a.Balance,
                            Player = player
                        };
                    }
                });
                return 201;
            });

            Handle.POST("/transfer?f={?}&t={?}&x={?}", (int fromId, int toId, int amount) => {
                Db.Transaction(() => {
                    Account source = Db.SQL<Account>("SELECT a FROM Account a WHERE AccountId = ?", fromId).First;
                    Account target = Db.SQL<Account>("SELECT a FROM Account a WHERE AccountId = ?", toId).First;
                    source.Balance -= amount;
                    target.Balance += amount;
                    if (source.Balance < 0 || target.Balance < 0 ) {
                        throw new Exception("You cannot move money that is not in the account");
                    }
                });
                return 200;
            });

            Handle.POST("/deposit?a={?}&x={?}", (int toId, int amount) => {
                Db.Transaction(() => {
                    Account account = Db.SQL<Account>("SELECT a FROM Account a WHERE a.AccountId = ?", toId).First;
                    account.Balance += amount;
                });
                return 200;
            });

            Handle.DELETE("/all", () => {
                Db.Transaction(() => {
                    Db.SlowSQL("DELETE FROM Account");
                    Db.SlowSQL("DELETE FROM Player");
                });
                return 200;
            });
        }

        private static void CreateIndexes() {
            if (Db.SQL("SELECT i FROM MATERIALIZED_INDEX i WHERE Name = ?", "AccountIdIndex").First == null)
                Db.SQL("CREATE UNIQUE INDEX AccountIdIndex ON Account (AccountId asc)");

            if (Db.SQL("SELECT i FROM MATERIALIZED_INDEX i WHERE Name = ?", "PlayerIdIndex").First == null)
                Db.SQL("CREATE UNIQUE INDEX PlayerIdIndex ON Player (PlayerId asc)");

            if (Db.SQL("SELECT i FROM MATERIALIZED_INDEX i WHERE Name = ?", "FullNameIndex").First == null)
                Db.SQL("CREATE INDEX FullNameIndex ON Player (FullName asc)");

            if (Db.SQL("SELECT i FROM MATERIALIZED_INDEX i WHERE Name = ?", "PlayerIndex").First == null)
                Db.SQL("CREATE INDEX PlayerIndex ON Account (Player, AccountId asc)");
        }
    }

    [Database]
    public class Player { // Represents a person with an account (card player, casino player etc.)
        public int PlayerId; // Public identifier
        public string FullName;
        public IEnumerable<Account> Accounts { get { return Db.SQL<Account>("SELECT a FROM Account a WHERE a.Player=?", this); } }
    }

    [Database]
    public class Account { // Represents an account for a specific person
        public int AccountId; // Public identifier
        public int AccountType; // Type of account (i.e. poker, bingo etc.)
        public int Balance; // Money in account.
        public Player Player; // To which player this account belongs.
    }
}
