using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Starcounter;
using Vendigo;
using System.Threading;

namespace PlayersDemoGui {
    public class ResponseHandler : IResponseHandler {
        // Number of correct responses.
        Int32 numGoodResps_ = 0;

        // Reference to Gui.
        InterfaceObject gui_ = null;

        // Reference to requests provider.
        RequestProvider reqProvider_ = null;

        // Gets number of good responses.
        public Int32 NumGoodResponses {
            get { return numGoodResps_; }
        }

        // Constructor.
        public ResponseHandler(RequestProvider rp, InterfaceObject gui) {
            gui_ = gui;
            reqProvider_ = rp;
        }

        // Resetting the requests creator.
        public void Reset() {
            numGoodResps_ = 0;
            reqProvider_.Reset();
        }

        // Batched response processor.
        void IResponseHandler.ProcessResponse(Response response) {
            Interlocked.Increment(ref numGoodResps_);
        }
    }
}
