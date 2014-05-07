using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Starcounter;

namespace Vendigo {
    public class Client {
        private Thread sendThread_;

        public void Start(string serverIp, ushort serverPort, IResponseHandler responseHandler, IRequestProvider requestProvider, int requestsPerBatch) {
            Console.WriteLine("Connection established.");

            sendThread_ = new Thread(new ThreadStart(new Sender(serverIp, serverPort, requestProvider, responseHandler, requestsPerBatch).ClientSenderThread));
            sendThread_.Start();
        }

        public void Stop() {
            sendThread_.Abort();
        }

        public void Join() {
            sendThread_.Join();
        }
    }
}
