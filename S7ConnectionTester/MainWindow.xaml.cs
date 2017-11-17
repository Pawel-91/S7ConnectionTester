using Microsoft.Win32;
using S7.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace S7ConnectionTester
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// String with interpreter code
        /// </summary>
        public string interpreterString { get; set; }

        /// <summary>
        /// Struct with plc data
        /// </summary>
        IModule PLC;

        DispatcherTimer timer = new DispatcherTimer();
        MainViewModel windowData = new MainViewModel();
        Interpreter interpreter;
        IStoreData dataArchiver;
        bool storeData;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = windowData;
            interpreter = new Interpreter();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 90);
            timer.Tick += Timer_Tick;
            CompleteByHistory();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;

            if (PLC.Module != null && PLC.Module.Pelecek.IsConnected == true)
            {
                bool temp = Convert.ToBoolean(PLC.Module.ReadModule(windowData.CommunicationBit));
                PLC.Module.WriteToPLC(windowData.CommunicationBit, !temp);

                if (!temp == Convert.ToBoolean(PLC.Module.ReadModule(windowData.CommunicationBit)))
                    windowData.ConnectionString = "Connected";
                else
                {
                    windowData.ConnectionString = "Connection lost... Reconnecting.";
                    timer.Stop();
                    connectButton_Click("Reconnect", new RoutedEventArgs());
                    return;
                }
            }


            if (windowData.ExecuteIntProgram)
            {
                interpreter.Interpret(windowData.InterpreterText, PLC);
                windowData.IntResult = interpreter.RLOobtain();
            }

            string fLocation = FileLocationTextBox.Text;

            #region Uploading data to storage location
            Task.Factory.StartNew(() =>
            {
                object[] values = new object[windowData.RVariables.Count];
                IStoreData dataStore = null;
                if (storeData)
                {

                    if (windowData.LocationType == StorageLocation.LocalFile)
                    {
                        dataStore = new LocalFileCommunication(fLocation);

                        if (String.IsNullOrEmpty(fLocation))
                        {
                            startTimeButton_Click("NonFile", new RoutedEventArgs());
                        }
                    }
                    else if (windowData.LocationType == StorageLocation.DataBase)
                    {
                        dataStore = new DBCommunication();
                    }
                }

                List<DataTable> readings = new List<DataTable>();

                for (int i = 0; i < windowData.RVariables.Count; i++)
                {
                    values[i] = PLC.Module.ReadModule(windowData.RVariables[i].Variable);
                    readings.Add(new DataTable()
                    {
                        VariableName = windowData.RVariables[i].Variable,
                        VariableValue = windowData.RVariables[i].Value.ToString() == "ReadError" ? 0 : Convert.ToInt32(windowData.RVariables[i].Value),
                        Time = now

                    });
                }

                if (windowData.IntResult != null && windowData.ExecuteIntProgram)
                {
                    readings.Add(new DataTable()
                    {
                        VariableName = "interpreter",
                        VariableValue = Convert.ToInt32(windowData.IntResult),
                        Time = now
                    });
                }

                dataStore?.StoreData(readings);

                Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    for (int i = 0; i < windowData.RVariables.Count; i++)
                    {
                        windowData.RVariables[i] = new rVariable(windowData.RVariables[i].Variable, values[i]);
                    }
                }));

            });

            #endregion

        }

        private void saveConfigurationButtom_Click(object sender, RoutedEventArgs e)
        {
            if (windowData.ipString == "0.0.0.0" || CpuTypeComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Configuration data is incorrect!");
                return;
            }

            PLC = new PLCstruct(1, new ModuleCommunication() { Pelecek = new Plc((CpuType)CpuTypeComboBox.SelectedValue, windowData.ipString, windowData.rack, windowData.slot) });

            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "\\LoadData.txt", windowData.GetConfigurationDataString());
            MessageBox.Show("Configuration saved!");

        }
        private void CpuTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            windowData.selectedCPU = (CpuType)CpuTypeComboBox.SelectedValue;
        }


        #region Connection attempt
        private void connectButton_Click(object sender, RoutedEventArgs e)
        {

            if (PLC.Module == null)
            {
                MessageBox.Show("Configure device first!");
                return;
            }

            Ping ping = new Ping();
            ping.PingCompleted += Ping_PingCompleted;
            ping.SendPingAsync(windowData.ip);
            Cursor = Cursors.Wait;
        }
        private void Ping_PingCompleted(object sender, PingCompletedEventArgs e)
        {
            PingReply reply = e.Reply;

            windowData.ConnectionString = "Ping result: " + reply.Status.ToString();

            if (PLC.Module.Pelecek.IsConnected)
                PLC.Module.Pelecek.Close();
            PLC.Module.ConnectToPLC(windowData.ip.ToString(),CpuTypeComboBox.SelectedValue.ToString(), windowData.rack, windowData.slot, windowData.CommunicationBit);
            ErrorCode error = PLC.Module.Pelecek.Open();

    //        windowData.ConnectionString = "PLC Connecton result: " + error.ToString();

            //  timer.Start();
            Cursor = Cursors.Arrow;

        }
        #endregion

        #region Interpreter

        private void interpreterTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (windowData.InterpreterText == "Write code to execute")
                windowData.InterpreterText = "";
        }
        private void interpreterTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (windowData.InterpreterText == "")
                windowData.InterpreterText = "Write code to execute";
        }

        #endregion

        #region Reading variables
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (windowData.ReadVariable == "Put var here")
                windowData.ReadVariable = string.Empty;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (windowData.ReadVariable == string.Empty)
                windowData.ReadVariable = "Put var here";
        }

        private void AddVarriableButton_Click(object sender, RoutedEventArgs e)
        {
            if (windowData.ReadVariable == "Put var here" || windowData.RVariables.Any(a => a.Variable == this.VariableNameTextBox.Text) || this.VariableNameTextBox.Text.Length < 3)
                return;

            windowData.RVariables.Add(new rVariable(VariableNameTextBox.Text, 0));
        }
        #endregion

        #region Writing variables to PLC

        private void writeButton_Click(object sender, RoutedEventArgs e)
        {
            if (PLC.Module == null)
            {
                MessageBox.Show("Configure device first!");
                return;
            }

            if (PLC.Module.Pelecek.IsConnected == false)
            {
                MessageBox.Show("Open connection first!");
                return;
            }

            PLC.Module.WriteToPLC(windowData.WriteVariable, windowData.WriteValue);
            if (!windowData.RVariables.Any(a => a.Variable == windowData.WriteVariable))
            {
                windowData.RVariables.Add(new rVariable(windowData.WriteVariable, 0));
            }

        }

        #endregion

        /// <summary>
        /// Completes Plc data panel with saved data
        /// </summary>
        private void CompleteByHistory()
        {
            if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "\\LoadData.txt"))
                return;
            try
            {
                string[] temp = File.ReadAllLines(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "\\LoadData.txt");

                windowData.ipString = temp[0];
                switch (temp[1])
                {
                    case "S7200":
                        CpuTypeComboBox.SelectedValue = CpuType.S7200;
                        break;
                    case "S7300":
                        CpuTypeComboBox.SelectedValue = CpuType.S7300;
                        break;
                    case "S7400":
                        CpuTypeComboBox.SelectedValue = CpuType.S7400;
                        break;
                    case "S71200":
                        CpuTypeComboBox.SelectedValue = CpuType.S71200;
                        break;
                    case "S71500":
                        CpuTypeComboBox.SelectedValue = CpuType.S71500;
                        break;
                }
                windowData.rack = Convert.ToInt16(temp[2]);
                windowData.slot = Convert.ToInt16(temp[3]);
                windowData.CommunicationBit = temp[6];
            }
            catch { }
        }
        private void readDataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (readDataGrid.SelectedIndex < 0)
                return;

            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                windowData.RVariables.RemoveAt(readDataGrid.SelectedIndex);
            }
        }
        private void VariableNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddVarriableButton_Click(sender, e);
                VariableNameTextBox.Text = "";
            }
        }

        private void WValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                writeButton_Click("WValue_KeyDown", new RoutedEventArgs());
            }
        }

        private void ShowButton_Click(object sender, RoutedEventArgs e)
        {
            DataViewWindow dVW = new DataViewWindow();
            dVW.ShowDialog();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            timer.Tick -= Timer_Tick;
            timer.Start();
        }

        private void startTimeButton_Click(object sender, RoutedEventArgs e)
        {
            if (PLC.Module.Connected == false)
            {
                MessageBox.Show("Nie połączono ze sterownikiem!");
                return;
            }

            if (startTimeButton.Content.ToString() == "Start Readings")
            {
                storeData = true;
                startTimeButton.Content = "Stop Readings";
                timer.Start();
            }
            else
            {
                storeData = false;
                startTimeButton.Content = "Start Readings";
                timer.Stop();
            }
        }

        private void NoStorageButton_Click(object sender, RoutedEventArgs e)
        {
            windowData.LocationType = StorageLocation.None;
        }
       
        private void DBRadioButton_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            
            Task.Factory.StartNew(() => {
                bool connected = false;
                connected = DBCommunication.CheckDBConnection();

                Dispatcher.Invoke(() => { 
                if (connected)
                {
                    windowData.LocationType = StorageLocation.DataBase;
                }
                else
                {
                    MessageBox.Show("There is no data base!");
                    windowData.LocationType = StorageLocation.None;
                    this.NoStorageButton.IsChecked = true;
                }
                    Mouse.OverrideCursor = Cursors.Arrow;
                });
            });

            

           
        }

        private void FileRadioButton_Click(object sender, RoutedEventArgs e)
        {
            windowData.LocationType = StorageLocation.LocalFile;
        }

        private void PickFileLocationButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sFD = new SaveFileDialog();
            sFD.DefaultExt = ".txt";
            if (sFD.ShowDialog() == true)
            {
               FileLocationTextBox.Text = sFD.FileName;
            }
        }
    }
}
