using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace PlayersDemoGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        #region Properties
        public bool IsMaximied
        {
            get
            {
                return this.WindowState == System.Windows.WindowState.Maximized && this.WindowStyle == System.Windows.WindowStyle.None;
            }

        }

        private bool _IsRunning;
        public bool IsRunning
        {
            get
            {
                return this._IsRunning;
            }
        }


        #endregion

        #region Commands


        #region Prepare Command

        public static RoutedCommand Prepare_RoutedCommand = new RoutedCommand();

        private void CanExecute_Prepare_Command(object sender, CanExecuteRoutedEventArgs e)
        {
            InterfaceObject interfaceObject = this.DataContext as InterfaceObject;
            if (interfaceObject != null)
            {
                e.Handled = true;
                e.CanExecute = interfaceObject.IsRunning == false && interfaceObject.IsPrepared == false;
            }

        }
        private void Executed_Prepare_Command(object sender, ExecutedRoutedEventArgs e)
        {
            InterfaceObject interfaceObject = this.DataContext as InterfaceObject;
            if (interfaceObject != null)
            {
                interfaceObject.PrepareRun();
                e.Handled = true;
            }
        }

        #endregion
        #region Run Command

        public static RoutedCommand Run_RoutedCommand = new RoutedCommand();

        private void CanExecute_Run_Command(object sender, CanExecuteRoutedEventArgs e)
        {
            InterfaceObject interfaceObject = this.DataContext as InterfaceObject;
            if (interfaceObject != null)
            {
                e.Handled = true;
                e.CanExecute = interfaceObject.IsRunning == false;
            }

        }
        private void Executed_Run_Command(object sender, ExecutedRoutedEventArgs e)
        {


            InterfaceObject interfaceObject = this.DataContext as InterfaceObject;
            if (interfaceObject != null)
            {

                if (interfaceObject.IsPrepared == false)
                {
                    MessageBox.Show("Before running the demo, you need to remove all existing data and generate some random data. The random data will be used to create simulated user transactions." + Environment.NewLine + Environment.NewLine + "Press the button marked 'Delete old data/prepare random data'", "PlayersDemo", MessageBoxButton.OK, MessageBoxImage.Information); // TODO:

                }
                else
                {
                    interfaceObject.Run();
                }
                e.Handled = true;
            }

        }

        #endregion

        #region Cancel Command

        public static RoutedCommand Cancel_RoutedCommand = new RoutedCommand();

        private void CanExecute_Cancel_Command(object sender, CanExecuteRoutedEventArgs e)
        {
            InterfaceObject interfaceObject = this.DataContext as InterfaceObject;
            if (interfaceObject != null)
            {
                e.Handled = true;
                e.CanExecute = interfaceObject.IsRunning == true;
            }


        }
        private void Executed_Cancel_Command(object sender, ExecutedRoutedEventArgs e)
        {
            InterfaceObject interfaceObject = this.DataContext as InterfaceObject;
            if (interfaceObject != null)
            {
                interfaceObject.Cancel();
                // TODO: Cancel operation
                e.Handled = true;
            }

        }

        #endregion

        #region ToggleFullScreen Command

        public static RoutedCommand ToggleFullScreen_RoutedCommand = new RoutedCommand();

        private void CanExecute_ToggleFullScreen_Command(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = true;
        }
        private void Executed_ToggleFullScreen_Command(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.IsMaximied)
            {
                this.WindowState = System.Windows.WindowState.Normal;
                this.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
            }
            else
            {
                this.WindowState = System.Windows.WindowState.Maximized;
                this.WindowStyle = System.Windows.WindowStyle.None;
            }
            e.Handled = true;

        }

        #endregion

        #endregion



        EventHandler CompositionTargetEventHandler;

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new InterfaceObject();

            this.CompositionTargetEventHandler = new EventHandler(CompositionTarget_Rendering);
            CompositionTarget.Rendering += this.CompositionTargetEventHandler;

        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            InterfaceObject co = this.DataContext as InterfaceObject;
            if (co == null) return;


            // time formating
            string starcounter_Time_ms = string.Format("{0:000}", co.Starcounter_Time.Milliseconds);
            string mssql_time_ms = string.Format("{0:000}", co.MSSQL_Time.Milliseconds);
            this.tb_starcounter_time.Text = string.Format("{0}:{1}.{2}", co.Starcounter_Time.Minutes.ToString(), co.Starcounter_Time.Seconds, starcounter_Time_ms.Substring(0, 3));
            this.tb_mssql_time.Text = string.Format("{0}:{1}.{2}", co.MSSQL_Time.Minutes.ToString(), co.MSSQL_Time.Seconds, mssql_time_ms.Substring(0, 3));

            // Starcounter
            this.tb_starcounter_transations.Text = string.Format("{0}/{1}", co.Starcounter_Requests_Sent, co.Starcounter_Responses_Received);
            this.starcounter_progress.Value = co.Starcounter_Progress;

            // MS SQL
            this.tb_mssql_transations.Text = string.Format("{0}/{1}", co.MSSQL_Requests_Sent, co.MSSQL_Responses_Received);
            this.mssql_progress.Value = co.MSSQL_Progress;



        }

        // Overriding the process closure.
        protected override void OnClosed(EventArgs e)
        //protected override void OnClosing(CancelEventArgs e)
        {
            // Closing connection if any.
            ((InterfaceObject)this.DataContext).CloseConnection();

            // Killing the whole process.
            Environment.Exit(0);
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

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
