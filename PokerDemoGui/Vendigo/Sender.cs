using Starcounter;

namespace Vendigo {
    internal class Sender {
        private Node node_;
        private readonly IRequestProvider requestProvider_;
        private readonly IResponseHandler responseHandler_;

        internal Sender(Node node, IRequestProvider requestProvider, IResponseHandler responseHandler) {
            node_ = node;
            requestProvider_ = requestProvider;
            responseHandler_ = responseHandler;
        }

        internal void Main() {
            try {
                for (; ;) {
                    Request[] batch = requestProvider_.GetNextRequestBatch();

                    if (batch == null || batch.Length == 0)
                        break;

                    Response[] responses = new Response[batch.Length];
                    for (int i = 0; i < batch.Length; i++) {
//                        responses[i] = node_.CustomRESTRequest(batch[i]);

                        var req = batch[i];
                        responses[i] = node_.CustomRESTRequest(req.Method, req.Uri, req.BodyBytes, null);
                        responses[i].Request = batch[i];
                    }
                    responseHandler_.ProcessResponseBatch(responses);
                }
            }
            catch {
                // TODO:
                throw;
            }
            
        }
    }
}