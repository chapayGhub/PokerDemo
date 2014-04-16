using System;
using Starcounter;

namespace Vendigo {
    public interface IRequestProvider {
        Request[] GetNextRequestBatch();
    }
}