using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Input;
using Starcounter;
using ClientEngine;
using System.Net;
using System.Xml;

namespace PlayersDemoGui
{
    public class InterfaceObject : INotifyPropertyChanged
    {
        #region Properties

        private String _Output;
        public String Output
        {
            get
            {
                return this._Output;
            }

            set
            {
                this._Output = value;
                //this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                //{
                //    this.OnPropertyChanged("Output");
                //}));
            }
        }

        private String _Input;
        public String Input
        {
            get
            {
                return this._Input;
            }

            set
            {
                this._Input = value;
                //this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                //{
                //    this.OnPropertyChanged("Input");
                //}));
            }
        }

        private String _ScServerIp;
        public String ScServerIp
        {
            get
            {
                return this._ScServerIp;
            }

            set
            {
                this._ScServerIp = value;
            }
        }

        private String _ScServerPort;
        public String ScServerPort
        {
            get
            {
                return this._ScServerPort;
            }

            set
            {
                this._ScServerPort = value;
            }
        }

        private String _MsSqlServerIp;
        public String MsSqlServerIp
        {
            get
            {
                return this._MsSqlServerIp;
            }

            set
            {
                this._MsSqlServerIp = value;
            }
        }

        private String _MsSqlServerPort;
        public String MsSqlServerPort
        {
            get
            {
                return this._MsSqlServerPort;
            }

            set
            {
                this._MsSqlServerPort = value;
            }
        }

        private double _Starcounter_Progress;
        public double Starcounter_Progress
        {
            get
            {
                return this._Starcounter_Progress;
            }
            set
            {
                this._Starcounter_Progress = value;
                //this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                //{
                //    this.OnPropertyChanged("Starcounter_Progress");
                //}));
            }
        }

        private double _Starcounter_Responses_Received;
        public double Starcounter_Responses_Received
        {
            get
            {
                return this._Starcounter_Responses_Received;
            }

            set
            {
                this._Starcounter_Responses_Received = value;
                //this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                //{
                //    this.OnPropertyChanged("MSSQL_Transactions");
                //}));
            }
        }

        private double _Starcounter_Requests_Sent;
        public double Starcounter_Requests_Sent
        {
            get
            {
                return this._Starcounter_Requests_Sent;
            }

            set
            {
                this._Starcounter_Requests_Sent = value;
                //this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                //{
                //    this.OnPropertyChanged("MSSQL_Transactions");
                //}));
            }
        }

        private Boolean _Starcounter_MeasureStarted;
        public Boolean Starcounter_MeasureStarted
        {
            get
            {
                return this._Starcounter_MeasureStarted;
            }
            set
            {
                this._Starcounter_MeasureStarted = value;
                //this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                //{
                //    this.OnPropertyChanged("Starcounter_Time");
                //}));
            }
        }

        private volatile Boolean _IsPreparationDone;
        public Boolean IsPreparationDone
        {
            get
            {
                return this._IsPreparationDone;
            }
            set
            {
                this._IsPreparationDone = value;
                //this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                //{
                //    this.OnPropertyChanged("Starcounter_Time");
                //}));
            }
        }

        private volatile Boolean _IsPreparationPhase;
        public Boolean IsPreparationPhase
        {
            get
            {
                return this._IsPreparationPhase;
            }
            set
            {
                this._IsPreparationPhase = value;
                //this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                //{
                //    this.OnPropertyChanged("Starcounter_Time");
                //}));
            }
        }

        private volatile Boolean _IsStarcounterRunning;
        public Boolean IsStarcounterRunning
        {
            get
            {
                return this._IsStarcounterRunning;
            }
            set
            {
                this._IsStarcounterRunning = value;
                //this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                //{
                //    this.OnPropertyChanged("Starcounter_Time");
                //}));
            }
        }

        private DateTime _Starcounter_StartTime;
        public DateTime Starcounter_StartTime
        {
            get
            {
                return this._Starcounter_StartTime;
            }
            set
            {
                this._Starcounter_StartTime = value;
                //this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                //{
                //    this.OnPropertyChanged("Starcounter_Time");
                //}));
            }
        }

        private TimeSpan _Starcounter_Time;
        public TimeSpan Starcounter_Time
        {
            get
            {
                return this._Starcounter_Time;
            }
            set
            {
                this._Starcounter_Time = value;
                //this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                //{
                //    this.OnPropertyChanged("Starcounter_Time");
                //}));
            }
        }

        private double _MSSQL_Progress;
        public double MSSQL_Progress
        {
            get
            {
                return this._MSSQL_Progress;
            }

            set
            {
                this._MSSQL_Progress = value;
                //this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                //{
                //    this.OnPropertyChanged("MSSQL_Progress");
                //}));
            }
        }

        private double _MSSQL_Requests_Sent;
        public double MSSQL_Requests_Sent
        {
            get
            {
                return this._MSSQL_Requests_Sent;
            }

            set
            {
                this._MSSQL_Requests_Sent = value;
                //this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                //{
                //    this.OnPropertyChanged("MSSQL_Transactions");
                //}));
            }
        }

        private double _MSSQL_Responses_Received;
        public double MSSQL_Responses_Received
        {
            get
            {
                return this._MSSQL_Responses_Received;
            }

            set
            {
                this._MSSQL_Responses_Received = value;
                //this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                //{
                //    this.OnPropertyChanged("MSSQL_Transactions");
                //}));
            }
        }

        private Boolean _MSSQL_MeasureStarted;
        public Boolean MSSQL_MeasureStarted
        {
            get
            {
                return this._MSSQL_MeasureStarted;
            }
            set
            {
                this._MSSQL_MeasureStarted = value;
                //this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                //{
                //    this.OnPropertyChanged("MSSQL_Time");
                //}));
            }
        }

        private DateTime _MSSQL_StartTime;
        public DateTime MSSQL_StartTime
        {
            get
            {
                return this._MSSQL_StartTime;
            }
            set
            {
                this._MSSQL_StartTime = value;
                //this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                //{
                //    this.OnPropertyChanged("MSSQL_Time");
                //}));
            }
        }

        private TimeSpan _MSSQL_Time;
        public TimeSpan MSSQL_Time
        {
            get
            {
                return this._MSSQL_Time;
            }
            set
            {
                this._MSSQL_Time = value;
                //this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                //{
                //    this.OnPropertyChanged("MSSQL_Time");
                //}));
            }
        }

        private String _EncodedRequestString;
        public String EncodedRequestString
        {
            get
            {
                return this._EncodedRequestString;
            }

            set
            {
                _EncodedRequestString = value;
            }
        }

        private String _DecodedRequestString;
        public String DecodedRequestString
        {
            get
            {
                return this._DecodedRequestString;
            }

            set
            {
                _DecodedRequestString = value;
            }
        }

        private String _EncodedResponseString;
        public String EncodedResponseString
        {
            get
            {
                return this._EncodedResponseString;
            }

            set
            {
                _EncodedResponseString = value;
            }
        }

        private String _DecodedResponseString;
        public String DecodedResponseString
        {
            get
            {
                return this._DecodedResponseString;
            }

            set
            {
                _DecodedResponseString = value;
            }
        }

        private volatile bool _ResponseUpdateFlag;
        public bool ResponseUpdateFlag
        {
            get
            {
                return this._ResponseUpdateFlag;
            }

            set
            {
                _ResponseUpdateFlag = value;
            }
        }

        private volatile bool _RequestUpdateFlag;
        public bool RequestUpdateFlag
        {
            get
            {
                return this._RequestUpdateFlag;
            }

            set
            {
                _RequestUpdateFlag = value;
            }
        }

        private bool _IsPrepared;
        public bool IsPrepared
        {
            get
            {
                return this._IsPrepared;
            }

            set
            {
                this._IsPrepared = value;
                this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                {
                    // Forces the CommandManager to raise the RequerySuggested event.
                    CommandManager.InvalidateRequerySuggested();
                    this.OnPropertyChanged("IsPrepared");
                }));
            }
        }

        private bool _IsRunning;
        public bool IsRunning
        {
            get
            {
                return this._IsRunning;
            }

            set
            {
                this._IsRunning = value;
                this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                {
                    // Forces the CommandManager to raise the RequerySuggested event.
                    CommandManager.InvalidateRequerySuggested();
                    this.OnPropertyChanged("IsRunning");
                }));
            }
        }

        private volatile bool _IsEncoded;
        public bool IsEncoded
        {
            get
            {
                return this._IsEncoded;
            }

            set
            {
                // Encoding setting has been changed.
                if (value)
                {
                    this._Input = this._EncodedRequestString;
                    this._Output = this._EncodedResponseString;
                }
                else
                {
                    this._Input = this._DecodedRequestString;
                    this._Output = this._DecodedResponseString;
                }

                this._IsEncoded = value;
                this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                {
                    // Forces the CommandManager to raise the RequerySuggested event.
                    CommandManager.InvalidateRequerySuggested();
                    this.OnPropertyChanged("IsEncoded");
                }));
            }
        }

        private volatile bool _IsPaused;
        public bool IsPaused
        {
            get
            {
                return this._IsPaused;
            }

            set
            {
                this._IsPaused = value;
                this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                {
                    // Forces the CommandManager to raise the RequerySuggested event.
                    CommandManager.InvalidateRequerySuggested();
                    this.OnPropertyChanged("IsPaused");
                }));
            }
        }

        private bool _HasScore;
        public bool HasScore
        {
            get
            {
                return this._HasScore;
            }

            set
            {
                this._HasScore = value;
                this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                {
                    // Forces the CommandManager to raise the RequerySuggested event.
                    CommandManager.InvalidateRequerySuggested();
                    this.OnPropertyChanged("HasScore");
                }));
            }
        }

        private double _Score;
        public double Score
        {
            get
            {
                return this._Score;
            }

            set
            {
                this._Score = value;
                this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                {
                    this.OnPropertyChanged("Score");
                }));
            }
        }

        #region Sliders

        private double _Slider_01 = 100000;
        public double Slider_01
        {
            get
            {
                return this._Slider_01;
            }

            set
            {
                this.IsPrepared = false;
                this._Slider_01 = value;
                this.OnPropertyChanged("Slider_01");
            }
        }

        private double _Slider_02 = 1000000;
        public double Slider_02
        {
            get
            {
                return this._Slider_02;
            }

            set
            {
                this.IsPrepared = false;
                this._Slider_02 = value;
                this.OnPropertyChanged("Slider_02");
            }
        }

        private double _Slider_03 = 1000000;
        public double Slider_03
        {
            get
            {
                return this._Slider_03;
            }

            set
            {
                this.IsPrepared = false;
                this._Slider_03 = value;
                this.OnPropertyChanged("Slider_03");
            }
        }

        private double _Slider_04 = 1000000;
        public double Slider_04
        {
            get
            {
                return this._Slider_04;
            }

            set
            {
                this.IsPrepared = false;
                this._Slider_04 = value;
                this.OnPropertyChanged("Slider_04");
            }
        }

        private double _Slider_05 = 1000000;
        public double Slider_05
        {
            get
            {
                return this._Slider_05;
            }

            set
            {
                this.IsPrepared = false;
                this._Slider_05 = value;
                this.OnPropertyChanged("Slider_05");
            }
        }

        private double _Slider_06 = 1000000;
        public double Slider_06
        {
            get
            {
                return this._Slider_06;
            }

            set
            {
                this.IsPrepared = false;
                this._Slider_06 = value;
                this.OnPropertyChanged("Slider_06");
            }
        }

        #endregion

        #endregion

        private Dispatcher _dispatcher;

        public InterfaceObject()
        {
            this._dispatcher = Dispatcher.FromThread(Thread.CurrentThread);

        }
        public void Run()
        {
            if (this.IsRunning == true) throw new InvalidOperationException("Already running");

            this.bAbort = false;

            ThreadPool.QueueUserWorkItem(this.StartDemoThread);
        }

        public bool IsAborted
        {
            get { return bAbort;  }
        }

        volatile bool bAbort = false;
        public void Cancel()
        {
            this.bAbort = true;
            this.IsPrepared = false;
        }

        // Runs Starcounter test.
        void RunStarcounter(RequestProvider reqProvider, ResponseHandler respHandler)
        {
            // Checking if aborted.
            if (bAbort)
                return;

            // Running Starcounter.
            this.IsStarcounterRunning = true;

            // Resetting generated data.
            respHandler.Reset();

            // Closing the connection if any.
            CloseConnection();

            // Starting client engine here.
            clientConn = new Client();

            // Starting client engine threads.
            clientConn.Start(new IPEndPoint(IPAddress.Parse(this.ScServerIp), Int32.Parse(this.ScServerPort)), respHandler, reqProvider);

            // Disabling measurements.
            this.Starcounter_MeasureStarted = false;

            // Processing workers data.
            while (!bAbort)
            {
                // Indicating new Gui update tick.
                this.RequestUpdateFlag = true;
                this.ResponseUpdateFlag = true;

                // Updating the progress bar.
                this.Starcounter_Progress = ((double)respHandler.NumGoodResponses / reqProvider.RequestsCreator.TotalPlannedRequestsNum) * 100;

                // Updating number of requests/responses.
                this.Starcounter_Responses_Received = respHandler.NumGoodResponses;
                this.Starcounter_Requests_Sent = reqProvider.RequestsCreator.TotalRequestsProcessed;

                // Updating time.
                if (this.Starcounter_MeasureStarted)
                    this.Starcounter_Time = DateTime.Now - this.Starcounter_StartTime;

                // Checking if we finished.
                if (respHandler.NumGoodResponses >= reqProvider.RequestsCreator.TotalPlannedRequestsNum)
                    break;

                // Sleeping between Gui updates.
                Thread.Sleep(30);
            }

            // Checking if user aborted execution.
            if (!bAbort)
            {
                // Calculating execution time.
                this.Starcounter_Time = DateTime.Now - this.Starcounter_StartTime;
            }
            else
            {
                ResetGui();
            }
        }

        // Runs MS SQL test.
        void RunMsSql(RequestProvider reqProvider, ResponseHandler respHandler)
        {
            // Checking if aborted.
            if (bAbort)
                return;

            // Running MS SQL.
            this.IsStarcounterRunning = false;

            // Resetting generated data.
            respHandler.Reset();
            
            // Closing the connection if any.
            CloseConnection();

            // Starting client engine here.
            clientConn = new Client();

            // Starting client engine threads.
            clientConn.Start(new IPEndPoint(IPAddress.Parse(this.MsSqlServerIp), Int32.Parse(this.MsSqlServerPort)), respHandler, reqProvider);

            // Disabling measurements.
            this.MSSQL_MeasureStarted = false;

            // Processing workers data.
            while (!bAbort)
            {
                // Indicating new Gui update tick.
                this.RequestUpdateFlag = true;
                this.ResponseUpdateFlag = true;

                // Updating the progress bar.
                this.MSSQL_Progress = ((double)respHandler.NumGoodResponses / reqProvider.RequestsCreator.TotalPlannedRequestsNum) * 100;

                // Updating number of transactions.
                this.MSSQL_Responses_Received = respHandler.NumGoodResponses;
                this.MSSQL_Requests_Sent = reqProvider.RequestsCreator.TotalRequestsProcessed;

                // Updating time.
                if (this.MSSQL_MeasureStarted)
                    this.MSSQL_Time = DateTime.Now - this.MSSQL_StartTime;

                // Checking if we finished.
                if (respHandler.NumGoodResponses >= reqProvider.RequestsCreator.TotalPlannedRequestsNum)
                    break;

                // Sleeping between Gui updates.
                Thread.Sleep(30);
            }

            // Checking if user aborted execution.
            if (!bAbort)
            {
                // Calculating execution time.
                this.MSSQL_Time = DateTime.Now - this.MSSQL_StartTime;
            }
            else
            {
                ResetGui();
            }
        }

        // Prepares the warm run.
        public void PrepareRun()
        {
            // Starting preparation phase.
            this.IsPrepared = false;
            this.IsPreparationPhase = true;

            // Reading configuration.
            ReadDemoConfig();

            // Resetting Gui related stuff.
            ResetGui();

            // Generating random data first.
            GenerateNewRandomData();

            /////////////////////////////////////////////////////

            // Running Starcounter.
            this.IsStarcounterRunning = true;
            this.IsPreparationDone = false;

            // Resetting generated data.
            respHandler.Reset();

            // Closing the connection if any.
            CloseConnection();

            // Starting client engine here.
            clientConn = new Client();

            // Starting client engine threads.
            clientConn.Start(new IPEndPoint(IPAddress.Parse(this.ScServerIp), Int32.Parse(this.ScServerPort)), respHandler, reqProvider);

            // Waiting for cleanup to finish.
            while (!this.IsPreparationDone)
                Thread.Sleep(30);

            //////////////////////////////////////////////////////

            // Running MS SQL.
            this.IsStarcounterRunning = false;
            this.IsPreparationDone = false;

            // Resetting generated data.
            respHandler.Reset();

            // Closing the connection if any.
            CloseConnection();

            // Starting client engine here.
            clientConn = new Client();

            // Starting client engine threads.
            clientConn.Start(new IPEndPoint(IPAddress.Parse(this.MsSqlServerIp), Int32.Parse(this.MsSqlServerPort)), respHandler, reqProvider);

            // Waiting for cleanup to finish.
            while (!this.IsPreparationDone)
                Thread.Sleep(30);

            //////////////////////////////////////////////////////

            // Ending preparation phase.
            this.IsPreparationPhase = false;
            this.IsPrepared = true;
        }

        // Resets most of Gui properties.
        public void ResetGui()
        {
            // Reset values
            this.HasScore = false;
            
            this.Starcounter_Progress = 0;
            this.Starcounter_Responses_Received = 0;
            this.Starcounter_Requests_Sent = 0;
            this.Starcounter_MeasureStarted = false;
            this.Starcounter_Time = TimeSpan.Zero;

            this.MSSQL_Progress = 0;
            this.MSSQL_Responses_Received = 0;
            this.MSSQL_Requests_Sent = 0;
            this.MSSQL_MeasureStarted = false;
            this.MSSQL_Time = TimeSpan.Zero;

            this.DecodedRequestString = "";
            this.EncodedRequestString = "";
            this.EncodedResponseString = "";
            this.DecodedResponseString = "";

            this.Score = -1;
            this.Output = this.Input = string.Empty;

            this.IsStarcounterRunning = false;
        }

        // Reads XML config file.
        void ReadDemoConfig()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("DemoConfig.xml");

            XmlNodeList tmp = xmlDoc.GetElementsByTagName("ScServerIp");
            this.ScServerIp = ((XmlElement)tmp[0]).InnerText; // e.g. 127.0.0.1

            tmp = xmlDoc.GetElementsByTagName("ScServerPort");
            this.ScServerPort = ((XmlElement)tmp[0]).InnerText; // e.g. 8080

            tmp = xmlDoc.GetElementsByTagName("MsSqlServerIp");
            this.MsSqlServerIp = ((XmlElement)tmp[0]).InnerText; // e.g. 127.0.0.1

            tmp = xmlDoc.GetElementsByTagName("MsSqlServerPort");
            this.MsSqlServerPort = ((XmlElement)tmp[0]).InnerText; // e.g. 8081
        }

        // Represents client connection.
        Client clientConn = null;

        // Gracefully closes connection.
        public void CloseConnection()
        {
            // Closing the connection if any.
            if (clientConn != null)
            {
                clientConn.Stop();
                clientConn = null;
            }
        }

        // Request provider.
        RequestProvider reqProvider = null;

        // Response handler.
        ResponseHandler respHandler = null;

        // Generating random data.
        public void GenerateNewRandomData()
        {
            // Request provider.
            reqProvider = new RequestProvider(
                this,
                0,
                (Int32)this.Slider_01,
                (Int32)this.Slider_02,
                (Int32)this.Slider_03,
                (Int32)this.Slider_04,
                (Int32)this.Slider_05,
                (Int32)this.Slider_06);

            // Response handler.
            respHandler = new ResponseHandler(reqProvider, this);
            reqProvider.SetResponseHandler(respHandler);
        }

        // Called when Run button is clicked.
        private void StartDemoThread(object state)
        {
            // Started running the test.
            this.IsRunning = true;

            // Running Starcounter first.
            RunStarcounter(reqProvider, respHandler);

            // Running MS SQL Express.
            RunMsSql(reqProvider, respHandler);

            /*
            int numberOfTransactions = (int)this.Slider_01;

            // Start of simulated values
            #region StarcounterSimulation

            DateTime Starcounter_StartTime = DateTime.Now;
            this.Output = this.Input = string.Empty;

            for (int i = 0; i < numberOfTransactions && bAbort == false; i++)
            {
                this.Starcounter_Progress = (((double)i + 1) / numberOfTransactions) * 100;
                this.Starcounter_Transactions++;
                this.Output = "output " + i;
                this.Input = "Input " + i;
                this.Starcounter_Time = DateTime.Now - Starcounter_StartTime;
                Thread.Sleep(10);
            }

            this.Starcounter_Time = DateTime.Now - Starcounter_StartTime;
            #endregion

            #region MsSqlSimulation

            // MS SQL Express
            DateTime MSSQL_StartTime = DateTime.Now;
            this.Output = this.Input = string.Empty;

            for (int i = 0; i < numberOfTransactions && bAbort == false; i++)
            {
                this.MSSQL_Progress = (((double)i + 1) / numberOfTransactions) * 100;
                this.MSSQL_Progress++;
                this.MSSQL_Transactions++;
                this.Output = "output " + i;
                this.Input = "Input " + i;
                this.MSSQL_Time = DateTime.Now - MSSQL_StartTime;
                Thread.Sleep(10);
            }

            this.MSSQL_Time = DateTime.Now - MSSQL_StartTime;
            #endregion
            */

            // Calculating the score.
            if (!bAbort)
            {
                // Calculate Score
                this.Score = this.MSSQL_Time.TotalMilliseconds / this.Starcounter_Time.TotalMilliseconds;
                this.HasScore = true;
            }

            this.IsRunning = false;
            this.IsPrepared = false;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string fieldName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(fieldName));
            }
        }
        #endregion

    }

}
