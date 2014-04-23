using System;
using System.Collections.Generic;
using Starcounter;

namespace Vendigo {
    public interface IResponseHandler {
        void ProcessResponse(Response response);
    }
}