using System;
using System.Collections.Generic;
using Starcounter;

namespace Vendigo {
    public interface IRequestProvider {
        int GetNextRequestBatch(out Request[] buffer);
        void SignalBatchFinished();
    }
}