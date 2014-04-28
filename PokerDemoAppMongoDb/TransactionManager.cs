using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerDemoAppMongoDb {
    /// <summary>
    /// Since MongoDb does not support transactions, we need to implement
    /// custom, application-level transaction-like management for the case
    /// where we want to move money between two accounts. This class contains
    /// such functionality, used with the special-purpose AccountBalanceTransaction
    /// domain model class. This class does not provide full ACID transactions,
    /// but it support atomicy for the case where money is moved between accounts.
    /// Consistency is still jeopardized and isolation is not supported.
    /// </summary>
    /// <remarks>
    /// The solution is inspired by this article:
    /// http://docs.mongodb.org/manual/tutorial/perform-two-phase-commits
    /// </remarks>
    internal static class TransactionManager {

        public static void RollbackPending(AccountBalanceTransaction transaction) {
            throw new NotImplementedException();
        }

        public static void RecoverCommitted(AccountBalanceTransaction transaction) {
            throw new NotImplementedException();
        }

        public static void UpdateAsDone(
            AccountBalanceTransaction transaction,
            MongoCollection<AccountBalanceTransaction> transactions,
            MongoCollection<Account> accounts,
            Account sourceAccount,
            Account targetAccount) {
            throw new NotImplementedException();
        }
    }
}