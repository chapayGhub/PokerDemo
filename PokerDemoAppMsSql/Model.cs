﻿using System;
using System.Linq;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace PokerDemoAppMsSql {
    // Represents any type of player: poker, domino, darts, etc.
    public class Player {
        // Unique player identifier.
//        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Int32 PlayerId { get; set; }

        // Player's full name. Limit according to UK standard (stackoverflow).
        [MaxLength(70)]
        public String FullName { get; set; }

        // Recommended or necessary for EF code first approach to have a reference
        public virtual ICollection<Account> Accounts { get; private set; }
    }

    // Represents one of player's accounts.
    public class Account {
        // Unique account identifier.
//        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Int32 AccountId { get; set; }

        // Type of account.
        public Int32 AccountType { get; set; }

        // Balance of money on account.
        public Int32 Balance { get; set; }

        // To which player this account belongs.
        public virtual Player Player { get; set; }
    }

    public class PlayersDemoDb : DbContext {
        public DbSet<Player> Players { get; set; }
        public DbSet<Account> Accounts { get; set; }

        private static string _connection = "Server=chrhol\\SQLEXPRESS;Initial Catalog=PlayersDemoDb;Integrated Security=SSPI";//"Server=STARCOUNTER3\\SQLEXPRESS;Initial Catalog=PlayersDemoDb;Integrated Security=SSPI";
        public static string SetConnection(string SrvName) {
            _connection = "Server=" + SrvName + ";Initial Catalog=PlayersDemoDb;Integrated Security=SSPI";
            return _connection;
        }
        public static string SetConnection(string CompName, string SrvName) {
            _connection = "Server=" + CompName + "\\" + SrvName + ";Initial Catalog=PlayersDemoDb;Integrated Security=SSPI";
            return _connection;
        }
        public PlayersDemoDb() : base(_connection) { }
    }
}
