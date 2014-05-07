//#define FAST_LOOPBACK
using System;
using System.Collections.Generic;
using System.Threading;
using Starcounter;
using PlayersDemoGui;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Net.Sockets;

namespace Vendigo {
    internal class Sender {

        const Int32 NumWorkers = 1;
        //const Int32 NumWorkers = 3;
        const Int32 DefaultOneSendNumRequests = 5000;
        const Int32 SendBufSizeBytes = 1024 * 1024 * 16;
        const Int32 RecvBufSizeBytes = 1024 * 1024 * 16;
        const Int32 OneRequestsMaxSizeBytes = 256;
        const UInt16 AggregationPort = 9191;

        private readonly IRequestProvider requestProvider_;
        private readonly IResponseHandler responseHandler_;

        private readonly object handlesSync = new object();

        UInt16 serverPort_ = 8080;
        public UInt16 ServerPort { get { return serverPort_; } }

        String serverIp_ = "127.0.0.1";
        public String ServerIp { get { return serverIp_; } }

        [StructLayout(LayoutKind.Sequential)]
        public struct AggregationStruct {
            public UInt64 unique_socket_id_;
            public Int32 size_bytes_;
            public UInt32 socket_info_index_;
            public Int32 unique_aggr_index_;
            public UInt16 port_number_;
        }

        const Int32 AggregationStructSizeBytes = 24;

        public enum AggregationMessageTypes {
            AGGR_CREATE_SOCKET,
            AGGR_DESTROY_SOCKET,
            AGGR_DATA
        };

        class WorkerSettings {
            public readonly CountdownEvent CountdownEvent;
           
            public readonly ManualResetEvent[] HasZeroPendingRequests;

            public WorkerSettings(int numWorkers) {
                CountdownEvent = new CountdownEvent(numWorkers);
                HasZeroPendingRequests = new ManualResetEvent[numWorkers];
                for (var i = 0; i < numWorkers; i++)
                    HasZeroPendingRequests[i] = new ManualResetEvent(true);
            }
        };

        class ConnectionInfo {

            Sender sender_;

            Socket aggrSocket_;
            AggregationStruct agsOrig_;

            Byte[] sendBuf_;
            Int32 roundNumBytesToSend_;
            Int32 roundNumRequestsToSend_;

            Byte[] recvBuf_;
            Int32 receiveOffset_;

            Int64 totalNumProcessedBodyBytes_;
            Int64 totalNumProcessedResponses_;

            Int64 roundCorrectChecksum_;

            Int64 totalCorrectChecksum_;

            void ResetRoundNumbers() {
                roundNumBytesToSend_ = 0;
                roundNumRequestsToSend_ = 0;
                roundCorrectChecksum_ = 0;
            }

            public unsafe ConnectionInfo(Sender sender) {

                sender_ = sender;

                recvBuf_ = new Byte[RecvBufSizeBytes];
                sendBuf_ = new Byte[SendBufSizeBytes];

                aggrSocket_ = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

#if FAST_LOOPBACK
                const int SIO_LOOPBACK_FAST_PATH = (-1744830448);
                Byte[] OptionInValue = BitConverter.GetBytes(1);

                aggrSocket_.IOControl(
                    SIO_LOOPBACK_FAST_PATH,
                    OptionInValue,
                    null);
#endif

                aggrSocket_.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 1 << 19);
                aggrSocket_.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 1 << 19);

                aggrSocket_.Connect(sender_.ServerIp, AggregationPort);

                agsOrig_ = new AggregationStruct() {
                    port_number_ = sender_.ServerPort
                };

                Byte[] sendBuf = new Byte[1024],
                    recvBuf = new Byte[1024];

                fixed (Byte* p = sendBuf) {
                    *p = (Byte)AggregationMessageTypes.AGGR_CREATE_SOCKET;
                    *(AggregationStruct*)(p + 1) = agsOrig_;
                }

                aggrSocket_.Send(sendBuf, AggregationStructSizeBytes + 1, SocketFlags.None);
                Int32 numRecvBytes = aggrSocket_.Receive(recvBuf);

                if (numRecvBytes != AggregationStructSizeBytes)
                    throw new ArgumentOutOfRangeException("Received aggregation structure size.");

                fixed (Byte* p = recvBuf) {
                    agsOrig_ = *(AggregationStruct*)p;
                }
            }

            unsafe void CheckResponses(
                Byte[] buf,
                Int32 numBytes,
                out Int32 restartOffset,
                out Int32 numProcessedResponses,
                out Int64 numProcessedBodyBytes,
                out Int64 outChecksum) {

                Int32 numUnprocessedBytes = numBytes, offset = 0;

                numProcessedResponses = 0;
                numProcessedBodyBytes = 0;
                restartOffset = 0;
                outChecksum = 0;

                fixed (Byte* p = buf) {
                    while (numUnprocessedBytes > 0) {

                        if (numUnprocessedBytes < AggregationStructSizeBytes) {

                            Buffer.BlockCopy(buf, numBytes - numUnprocessedBytes, buf, 0, numUnprocessedBytes);
                            restartOffset = numUnprocessedBytes;
                            return;
                        }

                        AggregationStruct* ags = (AggregationStruct*)(p + offset);
                        if (ags->port_number_ != sender_.ServerPort)
                            throw new ArgumentOutOfRangeException();

                        if (numUnprocessedBytes < (AggregationStructSizeBytes + ags->size_bytes_)) {

                            Buffer.BlockCopy(buf, numBytes - numUnprocessedBytes, buf, 0, numUnprocessedBytes);
                            restartOffset = numUnprocessedBytes;
                            return;
                        }

                        outChecksum += ags->unique_aggr_index_;
                        numProcessedBodyBytes += ags->size_bytes_;
                        numProcessedResponses++;
                        sender_.responseHandler_.ProcessResponse(null);

                        numUnprocessedBytes -= AggregationStructSizeBytes + ags->size_bytes_;

                        offset += AggregationStructSizeBytes + ags->size_bytes_;
                    }
                }
            }

            void DoOneSend() {
                Int32 bytesSent = aggrSocket_.Send(sendBuf_, roundNumBytesToSend_, SocketFlags.None);
                if (bytesSent != roundNumBytesToSend_)
                    throw new IndexOutOfRangeException("bytesSent != numBytesToSend");
            }

            unsafe Boolean GetRequestsPlainBuffer(Int32 maxNumRequests, out bool moreWhenBatchIsCompleted) {
                ResetRoundNumbers();

                Request[] requests = new Request[maxNumRequests];
                Int32 count = sender_.requestProvider_.GetNextRequestBatch(requests, out moreWhenBatchIsCompleted);
                if (count == 0)
                    return true;

                Int64 origChecksum = 0;
                Int32 offset = 0;

                fixed (Byte* p = sendBuf_) {

                    for (Int32 i = 0; i < count; i++) {

                        *(p + offset) = (Byte)AggregationMessageTypes.AGGR_DATA;

                        AggregationStruct a = agsOrig_;
                        a.unique_aggr_index_ = i;
                        a.size_bytes_ = requests[i].CustomBytesLength;

                        origChecksum += a.unique_aggr_index_;

                        *(AggregationStruct*)(p + offset + 1) = a;

                        Marshal.Copy(requests[i].CustomBytes, 0, new IntPtr(p + offset + 1 + AggregationStructSizeBytes), a.size_bytes_);
                        offset += 1 + AggregationStructSizeBytes + a.size_bytes_;
                    }
                }

                roundCorrectChecksum_ = origChecksum;
                totalCorrectChecksum_ += roundCorrectChecksum_;

                roundNumBytesToSend_ = offset;
                roundNumRequestsToSend_ = count;

                return false;
            }

            void DoOneReceive2() {
                Int32 bytesReceived = aggrSocket_.Receive(recvBuf_, receiveOffset_, recvBuf_.Length - receiveOffset_, SocketFlags.None);
                bytesReceived += receiveOffset_;

                Int64 numBodyBytes = 0;
                Int32 numResponses = 0;
                Int64 checksum = 0;

                CheckResponses(recvBuf_, bytesReceived, out receiveOffset_, out numResponses, out numBodyBytes, out checksum);

                totalNumProcessedResponses_ += numResponses;
                totalNumProcessedBodyBytes_ += numBodyBytes;
            }

            public bool DoSendUntilAllReceived2(Int32 workerId, WorkerSettings ws) {
                int totalNumRequestsToSent = 0;

                for (; ; ) {
                    bool moreWhenBatchIsCompleted;
                    if (GetRequestsPlainBuffer(DefaultOneSendNumRequests, out moreWhenBatchIsCompleted)) {
                        // All request sent.

                        while (totalNumProcessedResponses_ != totalNumRequestsToSent) {
                            DoOneReceive2();
                        }

                        ws.HasZeroPendingRequests[workerId].Set();

                        if (!moreWhenBatchIsCompleted) return true;

                        // Wait for all other works to complete current batches before continuing.
                        //
                        // In case of a false positive where a worker has not yet reset its event
                        // but has requests to process still this won't be a problem since we'll
                        // just again get no requests to process and suspend here the next time
                        // around.

                        for (int i = 0; i < ws.HasZeroPendingRequests.Length; i++) {
                            if (i != workerId) ws.HasZeroPendingRequests[i].WaitOne();
                        }

                        continue;
                    }

                    ws.HasZeroPendingRequests[workerId].Reset();

                    totalNumRequestsToSent += roundNumRequestsToSend_;
                    DoOneSend();

                    while ((totalNumRequestsToSent - totalNumProcessedResponses_) > DefaultOneSendNumRequests) {
                        DoOneReceive2();
                    }
                }
            }
        };

        void SenderWorker(Int32 workerId, WorkerSettings ws, ConnectionInfo conn) {
            conn.DoSendUntilAllReceived2(workerId, ws);
            ws.CountdownEvent.Signal();
        }
        
        internal Sender(string serverIp, ushort serverPort, IRequestProvider requestProvider, IResponseHandler responseHandler) {
            serverIp_ = serverIp;
            serverPort_ = serverPort;
            requestProvider_ = requestProvider;
            responseHandler_ = responseHandler;
        }

        internal void ClientSenderThread() {

            ConnectionInfo[] conns = new ConnectionInfo[NumWorkers];
            for (Int32 i = 0; i < NumWorkers; i++) {
                conns[i] = new ConnectionInfo(this);
            }

            WorkerSettings ws = new WorkerSettings(NumWorkers);

            for (Int32 i = 0; i < NumWorkers; i++) {
                Int32 workerId = i;
                var conn = conns[i];
                ThreadStart threadDelegate = new ThreadStart(() => SenderWorker(workerId, ws, conn));
                Thread newThread = new Thread(threadDelegate);
                newThread.Start();
            }

            ws.CountdownEvent.Wait();

            ws.CountdownEvent.Dispose();
            for (int i = 0; i < NumWorkers; i++)
                ws.HasZeroPendingRequests[i].Dispose();
        }
    }
}