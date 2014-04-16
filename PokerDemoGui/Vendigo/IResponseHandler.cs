using System;
using Starcounter;

namespace Vendigo {
    public interface IResponseHandler {
        void ProcessResponseBatch(Response[] responseBatch);
    }
}