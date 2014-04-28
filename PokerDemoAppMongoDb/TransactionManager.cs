using MongoDB.Driver;
using MongoDB.Driver.Builders;
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
            var accounts = Mongo.Db.Collection<Account>();
            var transactions = Mongo.Db.Collection<AccountBalanceTransaction>();

            var source = accounts.FindOneByIdAs<Account>(transaction.From);
            var target = accounts.FindOneByIdAs<Account>(transaction.To);
            if (source == null || target == null) {
                throw new Exception(
                    string.Format("Transaction with ID {0} in {1} can not be recovered - at least one of the accounts are missing.",
                    transaction.Id, transaction.State)
                    );
            }

            if (source.ContainsTransaction(transaction)) {
                source.Balance += transaction.Amount;
                source.RemoveTransaction(transaction);
                accounts.Save(source);
            }

            if (target.ContainsTransaction(transaction)) {
                target.Balance -= transaction.Amount;
                target.RemoveTransaction(transaction);
                accounts.Save(target);
            }

            var q = Query<AccountBalanceTransaction>.EQ(t => t.Id, transaction.Id);
            transactions.Remove(q, RemoveFlags.Single);
        }

        public static void RecoverCommitted(AccountBalanceTransaction transaction) {
            var accounts = Mongo.Db.Collection<Account>();
            var transactions = Mongo.Db.Collection<AccountBalanceTransaction>();
            var source = accounts.FindOneByIdAs<Account>(transaction.From);
            var target = accounts.FindOneByIdAs<Account>(transaction.To);
            if (source == null || target == null) {
                throw new Exception(
                    string.Format("Transaction with ID {0} in {1} can not be recovered - at least one of the accounts are missing.", 
                    transaction.Id, transaction.State)
                    );
            }

            UpdateAsDone(transaction, transactions, accounts, source, target);
        }

        public static void UpdateAsDone(
            AccountBalanceTransaction transaction,
            MongoCollection<AccountBalanceTransaction> transactions,
            MongoCollection<Account> accounts,
            Account sourceAccount,
            Account targetAccount) {
            sourceAccount.RemoveTransaction(transaction);
            targetAccount.RemoveTransaction(transaction);
            transaction.State = AccountBalanceTransaction.States.Done;
            accounts.Save(sourceAccount);
            accounts.Save(targetAccount);
            transactions.Save(transaction);
        }
    }
}