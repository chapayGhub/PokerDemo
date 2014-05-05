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

        const Int32 NumAggregationSockets = 1;
        const Int32 NumWorkers = 1;
        const Int32 DefaultOneSendNumRequests = 5000;
        const Int32 SendBufSizeBytes = 1024 * 1024 * 16;
        const Int32 RecvBufSizeBytes = 1024 * 1024 * 16;
        const Int32 OneRequestsMaxSizeBytes = 256;
        const UInt16 AggregationPort = 9191;

        private readonly IRequestProvider requestProvider_;
        private readonly IResponseHandler responseHandler_;

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
            public CountdownEvent CountdownEvent;
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
            Int32 roundNumProcessedResponses_;

            Int64 totalNumProcessedBodyBytes_;
            Int64 totalNumProcessedResponses_;

            Int64 roundCorrectChecksum_;
            Int64 roundResponsesChecksum_;

            Int64 totalCorrectChecksum_;
            Int64 totalResponsesChecksum_;

            void ResetRoundNumbers() {
                roundNumBytesToSend_ = 0;
                roundNumRequestsToSend_ = 0;
                roundNumProcessedResponses_ = 0;
                roundCorrectChecksum_ = 0;
                roundResponsesChecksum_ = 0;
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

            bool DoOneReceive() {
                Int32 bytesReceived = aggrSocket_.Receive(recvBuf_, receiveOffset_, recvBuf_.Length - receiveOffset_, SocketFlags.None);
                bytesReceived += receiveOffset_;

                Int64 numBodyBytes = 0;
                Int32 numResponses = 0;
                Int64 checksum = 0;

                CheckResponses(recvBuf_, bytesReceived, out receiveOffset_, out numResponses, out numBodyBytes, out checksum);

                roundNumProcessedResponses_ += numResponses;
                totalNumProcessedResponses_ += numResponses;
                totalNumProcessedBodyBytes_ += numBodyBytes;

                roundResponsesChecksum_ += checksum;
                totalResponsesChecksum_ += checksum;

                if (roundNumProcessedResponses_ == roundNumRequestsToSend_) {
                    if (roundResponsesChecksum_ != roundCorrectChecksum_)
                        throw new ArgumentOutOfRangeException("CurrentResponsesChecksum != CorrectChecksum");

                    return true;
                }

                return false;
            }

            unsafe Boolean GetRequestsPlainBuffer(Int32 maxNumRequests, out bool completeBatchBeforeGettingMore) {
                ResetRoundNumbers();

                Request[] requests = new Request[maxNumRequests];
                Int32 count = sender_.requestProvider_.GetNextRequestBatch(requests, out completeBatchBeforeGettingMore);
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

            public Boolean DoSendUntilAllReceived() {
                bool completeBatchBeforeGettingMore;
                if (GetRequestsPlainBuffer(DefaultOneSendNumRequests, out completeBatchBeforeGettingMore)) {
                    // All request sent.
                    return true;
                }

                DoOneSend();

                while (!DoOneReceive()) ;

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

            public bool DoSendUntilAllReceived2() {
                int totalNumRequestsToSent = 0;

                for (; ; ) {
                    bool completeBatchBeforeGettingMore;
                    if (GetRequestsPlainBuffer(DefaultOneSendNumRequests, out completeBatchBeforeGettingMore)) {
                        // All request sent.

                        while (totalNumProcessedResponses_ != totalNumRequestsToSent) {
                            DoOneReceive2();
                        }
                        return true;
                    }

                    totalNumRequestsToSent += roundNumRequestsToSend_;
                    DoOneSend();

                    if (!completeBatchBeforeGettingMore) {
                        while ((totalNumRequestsToSent - totalNumProcessedResponses_) > DefaultOneSendNumRequests) {
                            DoOneReceive2();
                        }
                    }
                    else {
                        while (totalNumRequestsToSent != totalNumProcessedResponses_) {
                            DoOneReceive2();
                        }
                    }
                }
            }
        };

        unsafe void SenderWorker(Int32 workerId, WorkerSettings ws, ConnectionInfo[] conns) {

            // Processing until all responses are received.
            while (true) {
                for (Int32 i = 0; i < conns.Length; i++) {
//                    if (conns[i].DoSendUntilAllReceived()) {
                    if (conns[i].DoSendUntilAllReceived2()) {
                        ws.CountdownEvent.Signal();
                        return;
                    }
                }
            }
        }
        
        internal Sender(string serverIp, ushort serverPort, IRequestProvider requestProvider, IResponseHandler responseHandler) {
            serverIp_ = serverIp;
            serverPort_ = serverPort;
            requestProvider_ = requestProvider;
            responseHandler_ = responseHandler;
        }

        internal void ClientSenderThread() {

            ConnectionInfo[] conns = new ConnectionInfo[NumAggregationSockets];
            for (Int32 i = 0; i < NumAggregationSockets; i++) {
                conns[i] = new ConnectionInfo(this);
            }

            WorkerSettings ws = new WorkerSettings() {
                CountdownEvent = new CountdownEvent(NumWorkers)
            };

            for (Int32 i = 0; i < NumWorkers; i++) {
                Int32 workerId = i;
                ThreadStart threadDelegate = new ThreadStart(() => SenderWorker(workerId, ws, conns));
                Thread newThread = new Thread(threadDelegate);
                newThread.Start();
            }

            ws.CountdownEvent.Wait();
        }
    }
}