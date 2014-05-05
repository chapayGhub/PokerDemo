using System;
using System.Collections.Generic;
using Starcounter;

namespace Vendigo {
    public interface IRequestProvider {
        int GetNextRequestBatch(Request[] requestsBatch, out bool completeBatchBeforeGettingMore);
    }
}