using S7.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Windows;

namespace S7ConnectionTester
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Fields
        string interpreterText = "Write code to execute";
        public IPAddress ip = IPAddress.Parse("0.0.0.0");
        string readVariable = "Put var here";
        string connectionString = "Click button to connect";
        string writeVariable;
        string writeValue;
        object intResult;
        public CpuType selectedCPU;
        //      Dictionary<string, string> rVariables = new Dictionary<string, string>();
        ObservableCollection<rVariable> rVariables = new ObservableCollection<rVariable>();

        StorageLocation locationType;
        #endregion

        #region Propreties
        public string InterpreterText
        {
            get { return interpreterText; }
            set
            {
                interpreterText = value;
                NotifyPropertyChanged("InterpreterText");
            }
        }
        public string ConnectionString
        {
            get { return connectionString; }
            set
            {
                this.connectionString = value;
                NotifyPropertyChanged("ConnectionString");
            }
        }
        public string ReadVariable
        {
            get
            {
                return readVariable;
            }

            set
            {
                readVariable = value;
                NotifyPropertyChanged("ReadVariable");
            }
        }
        public string WriteVariable
        {
            get
            {
                return writeVariable;
            }

            set
            {
                writeVariable = value;
                NotifyPropertyChanged("WriteVariable");
            }
        }
        public string WriteValue
        {
            get
            {
                return writeValue;
            }

            set
            {
                writeValue = value;
                NotifyPropertyChanged("WriteValue");
            }
        }
        public string ipString
        {
            get
            {
                return ip.ToString();
            }

            set
            {
                if (!IPAddress.TryParse(value, out ip))
                {
                    MessageBox.Show("Podano niepoprawny adres IP!");
                    ip = IPAddress.Parse("0.0.0.0");
                    NotifyPropertyChanged("ipString");
                }
            }
        }
        public object IntResult
        {
            get
            {
                return intResult;
            }

            set
            {
                intResult = value;
                NotifyPropertyChanged("IntResult");
            }
        }


        public short rack { get; set; }
        public short slot { get; set; }
        public string CommunicationBit { get; set; }
        public bool ExecuteIntProgram { get; set; } = false;
        public Dictionary<string, CpuType> cpuType { get; set; } = new Dictionary<string, CpuType>();
        public ObservableCollection<rVariable> RVariables
        {
            get
            {
                return rVariables;
            }

            set
            {
                rVariables = value;
                NotifyPropertyChanged("RVariables");
            }
        }
        public StorageLocation LocationType
        {
            get
            {
                return locationType;
            }

            set
            {
                locationType = value;
            }
        }

        #endregion

        public string GetConfigurationDataString()
        {
            return ip.ToString() + Environment.NewLine + selectedCPU.ToString() + Environment.NewLine + rack.ToString() + Environment.NewLine + slot.ToString()
                + Environment.NewLine + CommunicationBit;
        }

        //Constructor
        public MainViewModel()
        {
            cpuType.Add("S7 200", CpuType.S7200);
            cpuType.Add("S7 300", CpuType.S7300);
            cpuType.Add("S7 400", CpuType.S7400);
            cpuType.Add("S7 1200", CpuType.S71200);
            cpuType.Add("S7 1500", CpuType.S71500);

        }

        //Methods
        private void NotifyPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }

    public struct rVariable
    {
        public string Variable { get; set; }
        public object Value { get; set; }

        public rVariable(string VarName, object VarValue)
        {
            Variable = VarName;
            Value = VarValue;
        }
    }
}
