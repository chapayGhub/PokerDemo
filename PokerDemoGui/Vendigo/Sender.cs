using System;
using System.Collections.Generic;
using System.Threading;
using Starcounter;
using PlayersDemoGui;

namespace Vendigo {
    internal class Sender {
        private Node node_;
        private readonly IRequestProvider requestProvider_;
        private readonly IResponseHandler responseHandler_;
        private Response[] responses = new Response[RequestProvider.MaxRequestsInBatch];
        
        internal Sender(Node node, IRequestProvider requestProvider, IResponseHandler responseHandler) {
            node_ = node;
            requestProvider_ = requestProvider;
            responseHandler_ = responseHandler;
        }

        internal void ClientSenderThread() {
            int count;
            Request[] requestsBatch;

            try {
                for (;;) {
                    count = requestProvider_.GetNextRequestBatch(out requestsBatch);
                    
                    if (count == 0)
                        break;

                    for (int i = 0; i < count; i++) {
                        var req = requestsBatch[i];
                        node_.CustomRESTRequest(req, 0, HandleResponse);
                    }
                }
            }
            catch {
                throw;
            }            
        }

        private void HandleResponse(Response response, object state) {
            responseHandler_.ProcessResponse(response);
        }
    }
}