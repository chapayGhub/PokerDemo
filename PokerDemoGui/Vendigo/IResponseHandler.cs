using System;
using System.Collections.Generic;
using Starcounter;

namespace Vendigo {
    public interface IResponseHandler {
        void ProcessResponseBatch(Response[] responseBatch, int count);
    }
}