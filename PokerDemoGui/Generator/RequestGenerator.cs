using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Starcounter;

namespace Generator {
    public static class RequestGenerator {
        public const int MaxInitialBalanceOnAccount = 1000;
        public const int MaxAccountsPerPlayer = 3;
        public const int DataTypeLength = 1;

        public static Request Delete() {
            var request = new Request();
            request.Method = "DELETE";
            request.Uri = "/all";
            request.ConstructFromFields();
            return request;
        }

        public static Request Stub() {
            var request = new Request();
            request.Method = "POST";
            request.Uri = "/echotest";
            request.ConstructFromFields();
            return request;
        }

        public static Request GetPlayer(int playerId) {
            var request = new Request();
            request.Method = "GET";
            request.Uri = "/players/" + playerId;
            request.ConstructFromFields();
            return request;
        }

        public static Request GetPlayerAndAccounts(int playerId) {
            var request = new Request();
            request.Method = "GET";
            request.Uri = "/dashboard/" + playerId;
            request.ConstructFromFields();
            return request;
        }

        public static Request GetPlayerByFullName(string fullName) {
            var request = new Request();
            request.Method = "GET";
            request.Uri = "/players?f=" + fullName;
            request.ConstructFromFields();
            return request;
        }

        public static Request PutPlayerAndAccounts(Random rand,
                                                  int playerId,
                                                  string fullName,
                                                  int numAccounts,
                                                  int[] accountsIds,
                                                  int accountsIdsUsed) {
            var paa = new PlayerAndAccounts();
            paa.PlayerId = playerId;
            paa.FullName = fullName;

            // Creating accounts for this player.
            for (int i = 0; i < numAccounts; i++) {
                int accountId = accountsIds[accountsIdsUsed + i];
                var account = paa.Accounts.Add();
                account.AccountId = accountId;
                account.AccountType = i + 1;
                account.Balance = 1;// rand.Next(5000000, 10000000);
            }

            var request = new Request();
            request.Method = "PUT";
            request.Uri = "/players/" + playerId;
            request.BodyBytes = paa.ToJsonUtf8();
            request.ConstructFromFields();
            return request;
        }

        public static Request PostTransfer(Random rand, int accountId1, int accountId2) {
            var request = new Request();
            request.Method = "POST";
            request.Uri = "/transfer?f="
                          + accountId1
                          + "&t="
                          + accountId2
                          + "&x="
                          + (int)rand.Next(1, 50);
            request.ConstructFromFields();
            return request;
        }

        public static Request PostDeposit(Random rand, int accountId) {
            var request = new Request();
            request.Method = "POST";
            request.Uri = "/deposit?a="
                          + accountId
                          + "&x="
                          + (Int32)rand.Next(1, 50);
            request.ConstructFromFields();
            return request;
        }
    }
}