using System;
using System.Collections.Generic;
using System.Threading;
using Starcounter;

namespace Vendigo {
    internal class Sender {
        private Node node_;
        private readonly IRequestProvider requestProvider_;
        private readonly IResponseHandler responseHandler_;
        private Response[] responses = new Response[256];
        int numResponses = 0;
        int totalCount;
        
        internal Sender(Node node, IRequestProvider requestProvider, IResponseHandler responseHandler) {
            node_ = node;
            requestProvider_ = requestProvider;
            responseHandler_ = responseHandler;
        }

        internal void Main() {
            int count;
            Request[] buffer;

            try {
                for (; ;) {
                    count = requestProvider_.GetNextRequestBatch(out buffer);
                    
                    if (count == 0)
                        break;
                    numResponses = count;
                    totalCount = count;
                    for (int i = 0; i < count; i++) {
                        var req = buffer[i];
                        node_.CustomRESTRequest(req.Method, req.Uri, req.BodyBytes, null, i, HandleResponse);
                    }
                }
            }
            catch {
                // TODO:
                throw;
            }            
        }

        private void HandleResponse(Response response, object state) {
            responses[(int)state] = response;

            if (Interlocked.Decrement(ref numResponses) <= 0) {
                responseHandler_.ProcessResponseBatch(responses, totalCount);
                requestProvider_.SignalBatchFinished();
            }
        }
    }
}