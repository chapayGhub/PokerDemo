using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Starcounter;

namespace Vendigo {
    public class Client {
        private Thread sendThread_;

        public void Start(string hostName, ushort port, IResponseHandler responseHandler, IRequestProvider requestProvider) {
            Console.WriteLine("Connection established.");

            sendThread_ = new Thread(new ThreadStart(new Sender(requestProvider, responseHandler).ClientSenderThread));
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
