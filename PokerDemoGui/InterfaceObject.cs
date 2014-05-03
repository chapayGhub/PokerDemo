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
using Vendigo;

namespace PlayersDemoGui {
    public class InterfaceObject : INotifyPropertyChanged {
        private volatile bool bAbort = false;
        private Client clientConn;
        private RequestProvider reqProvider = null;
        private ResponseHandler respHandler = null;
        public event PropertyChangedEventHandler PropertyChanged;

        private bool runMsSql = false;

        #region Fields

        private bool _MSSQL_IsDone;
        private bool _Starcounter_IsDone;
        private volatile bool _ResponseUpdateFlag;
        private bool _IsPrepared;
        private bool _IsRunning;
        private volatile bool _IsEncoded;
        private volatile bool _IsPaused;
        private bool _HasScore;
        private double _Score;
        private Dispatcher _dispatcher;

        #region Sliders
        private double _Slider_01 = 10000;
        private double _Slider_02 = 100000;
        private double _Slider_03 = 100000;
        private double _Slider_04 = 100000;
        private double _Slider_05 = 100000;
        private double _Slider_06 = 100000;
        #endregion

        #endregion

        #region Properties

        public String Output { get; set; }
        public String Input { get; set; }
        public String ScServerIp { get; set; }
        public String ScServerPort { get; set; }
        public String MsSqlServerIp { get; set; }
        public String MsSqlServerPort { get; set; }
        public double Starcounter_Progress { get; set; }
        public double Starcounter_Responses_Received { get; set; }
        public double Starcounter_Requests_Sent { get; set; }
        public Boolean Starcounter_MeasureStarted { get; set; }
        public Boolean IsPreparationDone { get; set; }
        public Boolean IsPreparationPhase { get; set; }
        public Boolean IsStarcounterRunning { get; set; }
        public DateTime Starcounter_StartTime { get; set; }
        public TimeSpan Starcounter_Time { get; set; }
        public bool Starcounter_EstimatedTime { get; set; }
        public double MSSQL_Progress { get; set; }
        public double MSSQL_Requests_Sent { get; set; }
        public double MSSQL_Responses_Received { get; set; }
        public Boolean MSSQL_MeasureStarted { get; set; }
        public DateTime MSSQL_StartTime { get; set; }
        public TimeSpan MSSQL_Time { get; set; }
        public TimeSpan MSSQL_EstimatedTime { get; set; }
        public String EncodedRequestString { get; set; }
        public String DecodedRequestString { get; set; }
        public String EncodedResponseString { get; set; }
        public String DecodedResponseString { get; set; }

        public bool Starcounter_IsDone {
            get {
                return this._Starcounter_IsDone;
            }
            set {
                this._Starcounter_IsDone = value;
                this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate {
                    // Forces the CommandManager to raise the RequerySuggested event.
                    CommandManager.InvalidateRequerySuggested();
                    this.OnPropertyChanged("Starcounter_IsDone");
                }));
            }
        }
        
        public bool MSSQL_IsDone {
            get {
                return this._MSSQL_IsDone;
            }
            set {
                this._MSSQL_IsDone = value;
                this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate {
                    // Forces the CommandManager to raise the RequerySuggested event.
                    CommandManager.InvalidateRequerySuggested();
                    this.OnPropertyChanged("MSSQL_IsDone");
                }));
            }
        }

        public bool ResponseUpdateFlag { 
            get {
                return this._ResponseUpdateFlag;
            }
            set {
                _ResponseUpdateFlag = value;
            }
        }

        public bool RequestUpdateFlag { get; set; }
        
        
        public bool IsPrepared {
            get {
                return this._IsPrepared;
            }
            set {
                this._IsPrepared = value;
                this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                {
                    // Forces the CommandManager to raise the RequerySuggested event.
                    CommandManager.InvalidateRequerySuggested();
                    this.OnPropertyChanged("IsPrepared");
                }));
            }
        }

        
        public bool IsRunning {
            get {
                return this._IsRunning;
            }
            set {
                this._IsRunning = value;
                this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate {
                    // Forces the CommandManager to raise the RequerySuggested event.
                    CommandManager.InvalidateRequerySuggested();
                    this.OnPropertyChanged("IsRunning");
                }));
            }
        }

        
        public bool IsEncoded {
            get {
                return this._IsEncoded;
            }
            set {
                // Encoding setting has been changed.
                if (value) {
                    this.Input = this.EncodedRequestString;
                    this.Output = this.EncodedResponseString;
                }
                else {
                    this.Input = this.DecodedRequestString;
                    this.Output = this.DecodedResponseString;
                }

                this._IsEncoded = value;
                this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate {
                    // Forces the CommandManager to raise the RequerySuggested event.
                    CommandManager.InvalidateRequerySuggested();
                    this.OnPropertyChanged("IsEncoded");
                }));
            }
        }

        string _competitorName;
        public string CompetitorName {
            get { return _competitorName; }
            set {
                this._competitorName = value;
                this.OnPropertyChanged("CompetitorName");
            }
        }

        public string RaceTitle {
            get { return "Starcounter vs. " + CompetitorName; }
        }

        public bool IsPaused {
            get {
                return this._IsPaused;
            }
            set {
                this._IsPaused = value;
                this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate {
                    // Forces the CommandManager to raise the RequerySuggested event.
                    CommandManager.InvalidateRequerySuggested();
                    this.OnPropertyChanged("IsPaused");
                }));
            }
        }

        
        public bool HasScore {
            get {
                return this._HasScore;
            }
            set {
                this._HasScore = value;
                this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate {
                    // Forces the CommandManager to raise the RequerySuggested event.
                    CommandManager.InvalidateRequerySuggested();
                    this.OnPropertyChanged("HasScore");
                }));
            }
        }

        public double Score {
            get {
                return this._Score;
            }
            set {
                this._Score = value;
                this._dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate {
                    this.OnPropertyChanged("Score");
                }));
            }
        }

        #region Sliders

        public double Slider_01 {
            get {
                return this._Slider_01;
            }
            set {
                this.IsPrepared = false;
                this._Slider_01 = value;
                this.OnPropertyChanged("Slider_01");
            }
        }

        public double Slider_02 {
            get {
                return this._Slider_02;
            }
            set {
                this.IsPrepared = false;
                this._Slider_02 = value;
                this.OnPropertyChanged("Slider_02");
            }
        }

        public double Slider_03 {
            get {
                return this._Slider_03;
            }
            set {
                this.IsPrepared = false;
                this._Slider_03 = value;
                this.OnPropertyChanged("Slider_03");
            }
        }

        public double Slider_04 {
            get {
                return this._Slider_04;
            }
            set {
                this.IsPrepared = false;
                this._Slider_04 = value;
                this.OnPropertyChanged("Slider_04");
            }
        }

        public double Slider_05 {
            get {
                return this._Slider_05;
            }
            set {
                this.IsPrepared = false;
                this._Slider_05 = value;
                this.OnPropertyChanged("Slider_05");
            }
        }

        public double Slider_06 {
            get {
                return this._Slider_06;
            }
            set {
                this.IsPrepared = false;
                this._Slider_06 = value;
                this.OnPropertyChanged("Slider_06");
            }
        }

        #endregion

        #endregion

        public InterfaceObject() {
            this._dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
            ReadDemoConfig();
        }

        public void Run() {
            if (this.IsRunning == true) throw new InvalidOperationException("Already running");

            this.bAbort = false;
            ThreadPool.QueueUserWorkItem(this.StartDemoThread);
        }

        public bool IsAborted {
            get { return bAbort;  }
        }

        public void Cancel() {
            this.bAbort = true;
            this.IsPrepared = false;
        }

        // Runs Starcounter test.
        void RunStarcounter(RequestProvider reqProvider, ResponseHandler respHandler) {
            // Checking if aborted.
            if (bAbort)
                return;

            // Resetting test time.
            this.Starcounter_Time = new TimeSpan(0);    // ***
            this.Starcounter_IsDone = false;        // ***

            // Running Starcounter.
            this.IsStarcounterRunning = true;

            // Resetting generated data.
            respHandler.Reset();

            // Closing the connection if any.
            CloseConnection();

            // Starting client engine here.
            clientConn = new Client();

            // Starting client engine threads.
            clientConn.Start(this.ScServerIp, ushort.Parse(this.ScServerPort), respHandler, reqProvider);

            // Disabling measurements.
            this.Starcounter_MeasureStarted = false;

            // Processing workers data.
            while (!bAbort) {
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
                this.Starcounter_IsDone = true; // ***
                this.Starcounter_Time = DateTime.Now - this.Starcounter_StartTime;
            }
            else
            {
                //ResetGui();
            }
        }

        // Runs MS SQL test.
        void RunMsSql(RequestProvider reqProvider, ResponseHandler respHandler)
        {
            // Checking if aborted.
            if (bAbort)
                return;

            if (!runMsSql)
                return;

            // Resetting test time.
            this.MSSQL_Time = new TimeSpan(0);   // ***
            this.MSSQL_IsDone = false;  // ***

            // Running MS SQL.
            this.IsStarcounterRunning = false;

            // Resetting generated data.
            respHandler.Reset();

            // Closing the connection if any.
            CloseConnection();

            // Starting client engine here.
            clientConn = new Client();

            // Starting client engine threads.
            clientConn.Start(this.MsSqlServerIp, ushort.Parse(this.MsSqlServerPort), respHandler, reqProvider);

            // Disabling measurements.
            this.MSSQL_MeasureStarted = false;

            // Processing workers data.
            while (!bAbort) {
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
            if (!bAbort) {
                // Calculating execution time.
                this.MSSQL_IsDone = true; // ***
                this.MSSQL_Time = DateTime.Now - this.MSSQL_StartTime;
            } else {
                //ResetGui();
            }
        }

        // Prepares the warm run.
        public void PrepareRun() {
            // Starting preparation phase.
            this.IsPrepared = false;
            this.IsPreparationPhase = true;

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

            clientConn.Start(this.ScServerIp, ushort.Parse(this.ScServerPort), respHandler, reqProvider);

            // Waiting for cleanup to finish.
            while (!this.IsPreparationDone)
                Thread.Sleep(30);

            //////////////////////////////////////////////////////

            if (runMsSql) {
                // Running MS SQL.
                this.IsStarcounterRunning = false;
                this.IsPreparationDone = false;

                // Resetting generated data.
                respHandler.Reset();

                // Closing the connection if any.
                CloseConnection();

                // Starting client engine here.
                clientConn = new Client();

                clientConn.Start(this.MsSqlServerIp, ushort.Parse(this.MsSqlServerPort), respHandler, reqProvider);

                // Waiting for cleanup to finish.
                while (!this.IsPreparationDone)
                    Thread.Sleep(30);
            }
            //////////////////////////////////////////////////////

            // Ending preparation phase.
            this.IsPreparationPhase = false;
            this.IsPrepared = true;
        }

        // Resets most of Gui properties.
        public void ResetGui() {
            // Reset values
            this.HasScore = false;

            this.Starcounter_IsDone = false;        // ***
            this.Starcounter_Progress = 0;
            this.Starcounter_Responses_Received = 0;
            this.Starcounter_Requests_Sent = 0;
            this.Starcounter_MeasureStarted = false;
            this.Starcounter_Time = TimeSpan.Zero;

            this.MSSQL_IsDone = false;        // ***
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

            tmp = xmlDoc.GetElementsByTagName("CompetitorServerIp");
            this.MsSqlServerIp = ((XmlElement)tmp[0]).InnerText; // e.g. 127.0.0.1

            tmp = xmlDoc.GetElementsByTagName("CompetitorServerPort");
            this.MsSqlServerPort = ((XmlElement)tmp[0]).InnerText; // e.g. 8081

            tmp = xmlDoc.GetElementsByTagName("CompetitorServerPort");
            this.MsSqlServerPort = ((XmlElement)tmp[0]).InnerText; // e.g. 8081

            tmp = xmlDoc.GetElementsByTagName("CompetitorName");
            this.CompetitorName = ((XmlElement)tmp[0]).InnerText; // e.g. Microsoft SQL Server
        }

        // Gracefully closes connection.
        public void CloseConnection() {
            // Closing the connection if any.
            if (clientConn != null) {
                clientConn.Stop();
                clientConn = null;
            }
        }

        // Generating random data.
        public void GenerateNewRandomData() {
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
        private void StartDemoThread(object state) {
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
            if (!bAbort) {
                // Calculate Score
                this.Score = this.MSSQL_Time.TotalMilliseconds / this.Starcounter_Time.TotalMilliseconds;
                this.HasScore = true;
            }

            this.IsRunning = false;
            this.IsPrepared = false;
        }

        protected virtual void OnPropertyChanged(string fieldName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(fieldName));
            }
        }
    }
}
