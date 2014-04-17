using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using PlayersDemoGui;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows;
using Starcounter;
using Generator;

namespace ClientEngine {
    // All supported requests types.
    public enum RequestTypes {
        DELETE_EVERYTHING,
        PUT_PLAYER,
        POST_TRANSFER_MONEY,
        GET_PLAYER_ACCOUNTS_BY_ID,
        POST_DEPOSIT_MONEY,
        GET_PLAYER_BY_ID,
        GET_PLAYER_BY_FULLNAME
    }

    // Basically generates requests and serializes mixed requests.
    public class RequestsCreator {
        // Requests/responses synchronization values.
        const Int32 ScRequestResponsesSync = 100000;
        const Int32 MsSqlRequestResponsesSync = 1000;

        // Number of different request types.
        readonly Int32 numReqTypes = Enum.GetNames(typeof(RequestTypes)).Length;

        // All generated requests data.
        Request[] putPlayer_ = null;
        Request[] getPlayerById_ = null;
        Request[] getPlayerAndAccountsById_ = null;
        Request[] getPlayerByFullName_ = null;
        Request[] transferMoneyBetweenTwoAccounts_ = null;
        Request[] depositMoneyToAccount_ = null;
        Byte[] requestTypesMixed_ = null;

        // Reference to Gui.
        InterfaceObject gui_ = null;

        // Response handler.
        ResponseHandler rh_ = null;

        // Different counters.
        readonly Int32 totalPlannedRequestsNum_ = 0;
        volatile Int32 totalRequestsProcessed_ = 0;
        Int32 numRespToWait_ = 0;

        // Counters for number of requests and responses.
        Int32[] numReqProcessed_ = null;
        Int32[] numReqNeeded_ = null;
        Int32[] numRespsNeeded_ = null;

        // Total number of planned requests.
        public Int32 TotalPlannedRequestsNum {
            get { return totalPlannedRequestsNum_; }
        }

        // Total number of requests processed.
        public Int32 TotalRequestsProcessed {
            get { return totalRequestsProcessed_; }
        }

        // Set response handler.
        public void SetResponseHandler(ResponseHandler rh) {
            rh_ = rh;
        }

        // Constructor.
        public RequestsCreator(
            InterfaceObject gui,
            Int32 initialRandNum,
            Int32 numPutPlayer,
            Int32 numTransferMoneyBetweenTwoAccounts,
            Int32 numGetPlayerAndAccountsById,
            Int32 numDepositMoneyToAccountRequest,
            Int32 numGetPlayerById,
            Int32 numGetPlayerByFullName) {
            gui_ = gui;

            // Generating complete input data.
            DataGenerator dataGen = new DataGenerator();
            dataGen.GenerateInput(
                initialRandNum,
                numPutPlayer,
                numTransferMoneyBetweenTwoAccounts,
                numGetPlayerAndAccountsById,
                numDepositMoneyToAccountRequest,
                numGetPlayerById,
                numGetPlayerByFullName,
                out putPlayer_,
                out transferMoneyBetweenTwoAccounts_,
                out getPlayerAndAccountsById_,
                out depositMoneyToAccount_,
                out getPlayerById_,
                out getPlayerByFullName_,
                out requestTypesMixed_
            );

            // Initiating number of processed requests for each type.
            numReqProcessed_ = new Int32[numReqTypes];
            numReqNeeded_ = new Int32[numReqTypes];
            numRespsNeeded_ = new Int32[numReqTypes];

            // Initializing total number of request of each type.
            numReqNeeded_[(Int32)RequestTypes.DELETE_EVERYTHING] = 1;
            numRespsNeeded_[(Int32)RequestTypes.DELETE_EVERYTHING] = 1;

            numReqNeeded_[(Int32)RequestTypes.PUT_PLAYER] = putPlayer_.Length;
            numRespsNeeded_[(Int32)RequestTypes.PUT_PLAYER] = putPlayer_.Length + 1;

            numReqNeeded_[(Int32)RequestTypes.POST_TRANSFER_MONEY] = transferMoneyBetweenTwoAccounts_.Length;
            numReqNeeded_[(Int32)RequestTypes.GET_PLAYER_ACCOUNTS_BY_ID] = getPlayerAndAccountsById_.Length;
            numReqNeeded_[(Int32)RequestTypes.POST_DEPOSIT_MONEY] = depositMoneyToAccount_.Length;
            numReqNeeded_[(Int32)RequestTypes.GET_PLAYER_BY_ID] = getPlayerById_.Length;
            numReqNeeded_[(Int32)RequestTypes.GET_PLAYER_BY_FULLNAME] = getPlayerByFullName_.Length;

            // Calculating total number of requests.
            totalPlannedRequestsNum_ = requestTypesMixed_.Length;
        }

        // Resetting the requests creator.
        public void Reset() {
            totalRequestsProcessed_ = 0;
            numRespToWait_ = 0;

            for (Int32 i = 0; i < numReqTypes; i++)
                numReqProcessed_[i] = 0;
        }

        // Creates linear requests one type after another.
        public unsafe int CreateLinearRequests(Request[] requestBuffer) {
            UInt32 offset = 0;
            
            // Checking if we are in preparation phase.
            if (gui_.IsPreparationPhase) {
                // Checking if we need to wait for certain amount of responses.
                if (numRespToWait_ > 0) {
                    // Wait until needed number of responses returned.
                    while (rh_.NumGoodResponses < numRespToWait_)
                        Thread.Sleep(30);

                    // Preparation is done.
                    gui_.IsPreparationDone = true;
                    return 0;
                } else {
                    // Sending delete all command.
                    requestBuffer[0] = RequestGenerator.Delete();

                    // Disabling wait on next time.
                    numRespToWait_ = 1;
                    return 1;
                }
            }

            // Checking if aborted.
            if (gui_.IsAborted)
                return 0;

            // Checking if all requests are processed.
            if (totalRequestsProcessed_ >= totalPlannedRequestsNum_)
                return 0;

            // Checking if we need to wait for certain amount of responses.
            if (numRespToWait_ > 0) {
                // Wait until needed number of responses returned.
                while (rh_.NumGoodResponses < numRespToWait_)
                    Thread.Sleep(1);

                // Disabling wait on next time.
                numRespToWait_ = 0;

                // Starting time measurements in Gui.
                if (rh_.NumGoodResponses <= 1) {
                    if (gui_.IsStarcounterRunning) {
                        gui_.Starcounter_StartTime = DateTime.Now;
                        gui_.Starcounter_MeasureStarted = true;
                    } else {
                        gui_.MSSQL_StartTime = DateTime.Now;
                        gui_.MSSQL_MeasureStarted = true;
                    }
                }
            }

            // Synchronizing requests/responses.
            if (gui_.IsStarcounterRunning) {
                /*if ((totalRequestsProcessed_ - rh_.NumGoodResponses) >= ScRequestResponsesSync)
                {
                    while (totalRequestsProcessed_ != rh_.NumGoodResponses)
                        Thread.Sleep(1);
                }*/
            } else {
                if ((totalRequestsProcessed_ - rh_.NumGoodResponses) >= MsSqlRequestResponsesSync) {
                    while (totalRequestsProcessed_ != rh_.NumGoodResponses)
                        Thread.Sleep(1);
                }
            }

            // Repeating until buffer is full.
            int count = 0;

            while (count < requestBuffer.Length) {
                // Getting type index.
                Int32 typeIndex = requestTypesMixed_[totalRequestsProcessed_];
                RequestTypes type = (RequestTypes)typeIndex;
                Int32 curReqTypeIndex = numReqProcessed_[typeIndex];

                Request request = null;

                // Performing request according to type.
                switch (type) {
                    case RequestTypes.DELETE_EVERYTHING:
                        request = RequestGenerator.Delete();
                        break;
                    case RequestTypes.PUT_PLAYER:
                        request = putPlayer_[curReqTypeIndex];
                        break;
                    case RequestTypes.GET_PLAYER_BY_ID:
                        request = getPlayerById_[curReqTypeIndex];
                        break;
                    case RequestTypes.GET_PLAYER_ACCOUNTS_BY_ID:
                        request = getPlayerAndAccountsById_[curReqTypeIndex];
                        break;
                    case RequestTypes.GET_PLAYER_BY_FULLNAME:
                        request = getPlayerByFullName_[curReqTypeIndex];
                        break;
                    case RequestTypes.POST_TRANSFER_MONEY:
                        request = transferMoneyBetweenTwoAccounts_[curReqTypeIndex];
                        break;
                    case RequestTypes.POST_DEPOSIT_MONEY:
                        request = depositMoneyToAccount_[curReqTypeIndex];    
                        break;
                }

                requestBuffer[count] = request;
                count++;

                // Updating the Gui if possible.
                UpdateRequestsGui(type, curReqTypeIndex);

                // Increasing number of processed requests.
                totalRequestsProcessed_++;
                numReqProcessed_[typeIndex]++;

                // Checking if we need to block next time to wait for responses.
                if (numRespsNeeded_[typeIndex] == totalRequestsProcessed_) {
                    numRespToWait_ = numRespsNeeded_[typeIndex];
                    return count;
                }

                // Checking if all requests are processed.
                if (totalRequestsProcessed_ >= totalPlannedRequestsNum_)
                    return count;
            }

            return count;
        }

        // Updates Gui.
        void UpdateRequestsGui(RequestTypes type, Int32 curReqIndex) {
            // Updating the Gui if possible.
            // Checking the paused flag.
            if (!gui_.IsPaused) {
                // Checking if we can update.
                if (gui_.RequestUpdateFlag) {
                    StringBuilder str = new StringBuilder();
                    switch (type) {
                        case RequestTypes.DELETE_EVERYTHING:
                            gui_.EncodedRequestString = "DELETE /all";
                            gui_.DecodedRequestString = "DELETE /all";
                            break;
                        case RequestTypes.PUT_PLAYER:
                            gui_.EncodedRequestString = "PUT_PLAYER";
                            gui_.DecodedRequestString = "PUT_PLAYER";
                            //// Creating encoded request string.
                            //PlayerAndAccountsParam.SerializeToFullEncodedRequest(str, putPlayer_[curReqIndex]);
                            //gui_.EncodedRequestString = str.ToString();
                            //str.Clear();

                            //// Creating decoded request string.
                            //PlayerAndAccountsParam.SerializeToFullDecodedRequest(str, putPlayer_[curReqIndex]);
                            //gui_.DecodedRequestString = str.ToString();
                            //str.Clear();
                            break;
                        case RequestTypes.GET_PLAYER_BY_ID:
                            gui_.EncodedRequestString = "GET_PLAYER_BY_ID";
                            gui_.DecodedRequestString = "GET_PLAYER_BY_ID";

                            //// Creating encoded request string.
                            //PlayerIdWrapper.SerializeToFullEncodedRequest(str, getPlayerById_[curReqIndex]);
                            //gui_.EncodedRequestString = str.ToString();
                            //str.Clear();

                            //// Creating decoded request string.
                            //PlayerIdWrapper.SerializeToFullDecodedRequest(str, getPlayerById_[curReqIndex]);
                            //gui_.DecodedRequestString = str.ToString();
                            //str.Clear();

                            break;

                        case RequestTypes.GET_PLAYER_ACCOUNTS_BY_ID:
                            gui_.EncodedRequestString = "GET_PLAYER_ACCOUNTS_BY_ID";
                            gui_.DecodedRequestString = "GET_PLAYER_ACCOUNTS_BY_ID";

                            //// Creating encoded request string.
                            //PlayerIdWrapper.SerializeToFullEncodedRequest(str, getPlayerAndAccountsById_[curReqIndex]);
                            //gui_.EncodedRequestString = str.ToString();
                            //str.Clear();

                            //// Creating decoded request string.
                            //PlayerIdWrapper.SerializeToFullDecodedRequest(str, getPlayerAndAccountsById_[curReqIndex]);
                            //gui_.DecodedRequestString = str.ToString();
                            //str.Clear();

                            break;
                        case RequestTypes.GET_PLAYER_BY_FULLNAME:
                            gui_.EncodedRequestString = "GET_PLAYER_BY_FULLNAME";
                            gui_.DecodedRequestString = "GET_PLAYER_BY_FULLNAME";
                            
                            //// Creating encoded request string.
                            //FullnameWrapper.SerializeToFullEncodedRequest(str, getPlayerByFullName_[curReqIndex]);
                            //gui_.EncodedRequestString = str.ToString();
                            //str.Clear();

                            //// Creating decoded request string.
                            //FullnameWrapper.SerializeToFullDecodedRequest(str, getPlayerByFullName_[curReqIndex]);
                            //gui_.DecodedRequestString = str.ToString();
                            //str.Clear();

                            break;
                        case RequestTypes.POST_TRANSFER_MONEY:
                            gui_.EncodedRequestString = "POST_TRANSFER_MONEY";
                            gui_.DecodedRequestString = "POST_TRANSFER_MONEY";

                            //// Creating encoded request string.
                            //TransferMoneyBetweenAccountsRequest.SerializeToFullEncodedRequest(str, transferMoneyBetweenTwoAccounts_[curReqIndex]);
                            //gui_.EncodedRequestString = str.ToString();
                            //str.Clear();

                            //// Creating decoded request string.
                            //TransferMoneyBetweenAccountsRequest.SerializeToFullDecodedRequest(str, transferMoneyBetweenTwoAccounts_[curReqIndex]);
                            //gui_.DecodedRequestString = str.ToString();
                            //str.Clear();

                            break;
                        case RequestTypes.POST_DEPOSIT_MONEY:
                            gui_.EncodedRequestString = "POST_DEPOSIT_MONEY";
                            gui_.DecodedRequestString = "POST_DEPOSIT_MONEY";

                            //// Creating encoded request string.
                            //DepositMoneyToAccountRequest.SerializeToFullEncodedRequest(str, depositMoneyToAccount_[curReqIndex]);
                            //gui_.EncodedRequestString = str.ToString();
                            //str.Clear();

                            //// Creating decoded request string.
                            //DepositMoneyToAccountRequest.SerializeToFullDecodedRequest(str, depositMoneyToAccount_[curReqIndex]);
                            //gui_.DecodedRequestString = str.ToString();
                            //str.Clear();

                            break;
                    }

                    // Setting the correct string.
                    if (gui_.IsEncoded)
                        gui_.Input = gui_.EncodedRequestString;
                    else
                        gui_.Input = gui_.DecodedRequestString;

                    // Update string here.
                    gui_.RequestUpdateFlag = false;
                }
            }
        }
    }

}
