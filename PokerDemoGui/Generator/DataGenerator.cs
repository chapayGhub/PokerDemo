using System;
using System.IO;
using System.Text;
using Starcounter;

namespace Generator
{
    public class DataGenerator
    {
        public void SaveGeneratedData(
            Request[] input_AddPlayer,
            Request[] input_TransferMoneyBetweenTwoAccounts,
            Request[] input_GetPlayerAndAccounts,
            Request[] input_DepositMoneyToAccount,
            Request[] input_GetPlayerById,
            Request[] input_GetPlayerByFullName)
        {
            //StringBuilder s = new StringBuilder();

            //// AddPlayer.
            //for (Int32 i = 0; i < input_AddPlayer.Length; i++)
            //{
            //    PlayerAndAccountsParam.SerializeToTxtString(s, input_AddPlayer[i]);
            //    PlayerAndAccountsParam.SerializeToJsonString(s, input_AddPlayer[i]);
            //}

            //File.WriteAllText("AddPlayer.txt", s.ToString());
            //s.Clear();

            //// TransferMoneyBetweenTwoAccounts.
            //for (Int32 i = 0; i < input_TransferMoneyBetweenTwoAccounts.Length; i++)
            //{
            //    TransferMoneyBetweenAccountsRequest.SerializeToTxtString(s, input_TransferMoneyBetweenTwoAccounts[i]);
            //    TransferMoneyBetweenAccountsRequest.SerializeToJsonString(s, input_TransferMoneyBetweenTwoAccounts[i]);
            //}

            //File.WriteAllText("TransferMoneyBetweenTwoAccounts.txt", s.ToString());
            //s.Clear();

            //// GetPlayerAndAccounts.
            //for (Int32 i = 0; i < input_GetPlayerAndAccounts.Length; i++)
            //{
            //    s.Append(input_GetPlayerAndAccounts[i].PlayerId);
            //    s.Append("\n");
            //}

            //File.WriteAllText("GetPlayerAndAccounts.txt", s.ToString());
            //s.Clear();

            //// DepositMoneyToAccount.
            //for (Int32 i = 0; i < input_DepositMoneyToAccount.Length; i++)
            //{
            //    DepositMoneyToAccountRequest.SerializeToTxtString(s, input_DepositMoneyToAccount[i]);
            //    DepositMoneyToAccountRequest.SerializeToJsonString(s, input_DepositMoneyToAccount[i]);
            //}

            //File.WriteAllText("DepositMoneyToAccount.txt", s.ToString());
            //s.Clear();

            //// GetPlayerById.
            //for (Int32 i = 0; i < input_GetPlayerById.Length; i++)
            //{
            //    s.Append(input_GetPlayerById[i].PlayerId);
            //    s.Append("\n");
            //}

            //File.WriteAllText("GetPlayerById.txt", s.ToString());
            //s.Clear();

            //// GetPlayerByFullName.
            //for (Int32 i = 0; i < input_GetPlayerByFullName.Length; i++)
            //{
            //    s.Append(input_GetPlayerByFullName[i].FullName);
            //    s.Append("\n");
            //}

            //File.WriteAllText("GetPlayerByFullName.txt", s.ToString());
            //s.Clear();
        }

        public void GenerateInput(
            Int32 initialRandNum,
            Int32 numAddPlayer,
            Int32 numTransferMoneyBetweenTwoAccounts,
            Int32 numGetPlayerAndAccounts,
            Int32 numDepositMoneyToAccountRequest,
            Int32 numGetPlayerById,
            Int32 numGetPlayerByFullName,
            out Request[] input_AddPlayer,
            out Request[] input_TransferMoneyBetweenTwoAccounts,
            out Request[] input_GetPlayerAndAccounts,
            out Request[] input_DepositMoneyToAccount,
            out Request[] input_GetPlayerById,
            out Request[] input_GetPlayerByFullName,
            out Byte[] requestTypesMixed
            )
        {
            Random rand = new Random(initialRandNum);

            // Obtaining first names.
            String[] allNames = File.ReadAllLines("Names.txt");

            // Obtaining second names.
            String[] allSurnames = File.ReadAllLines("Surnames.txt");

            // All players full names.
            String[] fullNames = new String[numAddPlayer];

            // All players ids.
            Int32[] playedIds = GenerateRandomValuesArray(rand, 0, numAddPlayer);
            Int32 numAccountsMax = numAddPlayer * RequestGenerator.MaxAccountsPerPlayer;

            // All accounts ids.
            Int32[] accountsIds = GenerateRandomValuesArray(rand, 0, numAccountsMax);

            // Global index into accounts offset.
            Int32 accountsIdsUsed = 0;

            // Generating AddPlayer.
            input_AddPlayer = new Request[numAddPlayer];
            for (int i = 0; i < numAddPlayer; i++) {
                // Generating full names that are not necessary unique.
                fullNames[i] = allNames[rand.Next(allNames.Length)] + allSurnames[rand.Next(allSurnames.Length)];

                Int32 numAccounts = rand.Next(1, RequestGenerator.MaxAccountsPerPlayer);
                input_AddPlayer[i] = RequestGenerator.PutPlayerAndAccounts(rand, playedIds[i], fullNames[i], numAccounts, accountsIds, accountsIdsUsed);
                accountsIdsUsed += numAccounts;
            }
          
            // Generating TransferMoneyBetweenTwoAccounts.
            input_TransferMoneyBetweenTwoAccounts = new Request[numTransferMoneyBetweenTwoAccounts];
            for (int i = 0; i < numTransferMoneyBetweenTwoAccounts; i++) {
                int accountId1 = 0;
                int accountId2 = 0;

                // Making sure that accounts are different.
                while (accountId1 == accountId2) {
                    accountId1 = accountsIds[rand.Next(accountsIdsUsed)];
                    accountId2 = accountsIds[rand.Next(accountsIdsUsed)];
                }
                input_TransferMoneyBetweenTwoAccounts[i] = RequestGenerator.PostTransfer(rand, accountId1, accountId2);
            }

            // Generating GetPlayerAndAccounts.
            input_GetPlayerAndAccounts = new Request[numGetPlayerAndAccounts];
            for (int i = 0; i < numGetPlayerAndAccounts; i++) {
                input_GetPlayerAndAccounts[i] = RequestGenerator.GetPlayerAndAccounts(playedIds[rand.Next(numAddPlayer)]);
            }

            // Generating DepositMoneyToAccount.
            input_DepositMoneyToAccount = new Request[numDepositMoneyToAccountRequest];
            for (int i = 0; i < numDepositMoneyToAccountRequest; i++) {
                input_DepositMoneyToAccount[i] = RequestGenerator.PostDeposit(rand, accountsIds[rand.Next(accountsIdsUsed)]);
            }

            // Generating GetPlayerById.
            input_GetPlayerById = new Request[numGetPlayerById];
            for (int i = 0; i < numGetPlayerById; i++) {
                input_GetPlayerById[i] = RequestGenerator.GetPlayer(playedIds[rand.Next(numAddPlayer)]);
            }

            // Generating GetPlayerByFullName.
            input_GetPlayerByFullName = new Request[numGetPlayerByFullName];
            for (int i = 0; i < numGetPlayerByFullName; i++) {
                input_GetPlayerByFullName[i] = RequestGenerator.GetPlayerByFullName(fullNames[rand.Next(numAddPlayer)]);
            }

            // Mixing request types.
            Int32[] typesNums =
            {
                1, // DELETE_EVERYTHING
                numAddPlayer, // PUT_PLAYER
                numTransferMoneyBetweenTwoAccounts, // POST_TRANSFER_MONEY
                numGetPlayerAndAccounts, // GET_PLAYER_ACCOUNTS_BY_ID
                numDepositMoneyToAccountRequest, // POST_DEPOSIT_MONEY
                numGetPlayerById, // GET_PLAYER_BY_ID
                numGetPlayerByFullName // GET_PLAYER_BY_FULLNAME
            };

            // Getting total number of requests.
            Int32 totalReqNum = 0;
            for (Int32 i = 0; i < typesNums.Length; i++)
                totalReqNum += typesNums[i];

            // Creating mixed request types.
            requestTypesMixed = new Byte[totalReqNum];

            // Filling up the array with linear values first.
            Int32 curIndex = 0;
            for (Int32 i = 0; i < typesNums.Length; i++)
            {
                for (Int32 k = 0; k < typesNums[i]; k++)
                {
                    requestTypesMixed[curIndex] = (Byte)i;
                    curIndex++;
                }
            }

            // Mixing randomly the types.
            for (Int32 i = 1 + numAddPlayer; i < totalReqNum; i++)
            {
                // Generating random index within range.
                Int32 randIndex = rand.Next(1 + numAddPlayer, totalReqNum);

                // Exchanging values.
                Byte s = requestTypesMixed[i];
                requestTypesMixed[i] = requestTypesMixed[randIndex];
                requestTypesMixed[randIndex] = s;
            }
        }

        // Generates array of random values within ranges.
        public Int32[] GenerateRandomValuesArray(Random rand, Int32 startValue, Int32 stopValue)
        {
            Int32 numElems = stopValue - startValue;
            Int32[] randArray = new Int32[numElems];

            Int32 k = 0;
            for (Int32 i = startValue; i < stopValue; i++)
            {
                randArray[k] = i;
                k++;
            }

            // Performing permutations.
            for (Int32 i = 0; i < numElems; i++)
            {
                Int32 r = rand.Next(numElems);
                Int32 p = randArray[i];
                randArray[i] = randArray[r];
                randArray[r] = p;
            }

            return randArray;
        }
       
    }
}