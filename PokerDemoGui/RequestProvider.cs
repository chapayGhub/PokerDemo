using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientEngine;
using Starcounter;
using Vendigo;

namespace PlayersDemoGui {
    public class RequestProvider : IRequestProvider {

        // Maximum accumulative buffer size for requests batch (at least two MaxRequestBytes).
        public const Int32 MaxAccumBufferBytes = 50000;
        public const Int32 MaxRequestBytes = 1000;
        public const Int32 MaxRequestsInBatch = 256;

        // Request generator.
        RequestsCreator requestsCreator_ = null;

        // Reference to Gui.
        InterfaceObject gui_ = null;

        // Response handler.
        ResponseHandler respHandler_ = null;

        // Request generator.
        public RequestsCreator RequestsCreator {
            get { return requestsCreator_; }
        }

        // Set response handler.
        public void SetResponseHandler(ResponseHandler rh) {
            requestsCreator_.SetResponseHandler(rh);
            respHandler_ = rh;
        }

        // Constructor.
        public RequestProvider(
            InterfaceObject gui,
            Int32 initialRandNum,
            Int32 numAddPlayer,
            Int32 numTransferMoneyBetweenTwoAccounts,
            Int32 numGetPlayerAndAccounts,
            Int32 numDepositMoneyToAccountRequest,
            Int32 numGetPlayerById,
            Int32 numGetPlayerByFullName) {
            // Generating all needed data.
            requestsCreator_ = new RequestsCreator(
                gui,
                initialRandNum,
                numAddPlayer,
                numTransferMoneyBetweenTwoAccounts,
                numGetPlayerAndAccounts,
                numDepositMoneyToAccountRequest,
                numGetPlayerById,
                numGetPlayerByFullName
                );

            gui_ = gui;
        }

        // Resetting the requests creator.
        public void Reset() {
            requestsCreator_.Reset();
        }

        // Gets next request batch.
        public int GetNextRequestBatch(Request[] requestsBatch, out bool completeBatchBeforeGettingMore) {
            return requestsCreator_.CreateLinearRequests(requestsBatch, out completeBatchBeforeGettingMore);
        }
    }
}
