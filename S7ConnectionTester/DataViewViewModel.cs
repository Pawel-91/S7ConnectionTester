using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace S7ConnectionTester
{
    public class DataViewViewModel : INotifyPropertyChanged
    {

        #region fields

        StorageLocation location;
        string fileLocation;
        DateTime? startTime, endTime;
        string filterVariable;
        ObservableCollection<DataTable> allData;
        ObservableCollection<GridData> shownData;

        #endregion

        public StorageLocation Location
        {
            get
            {
                return location;
            }

            set
            {
                location = value;

            }
        }

        public string FileLocation
        {
            get
            {
                return fileLocation;
            }

            set
            {
                fileLocation = value;
                this.NotifyPropertyChanged("FileLocation");
            }
        }

        public string FilterVariable
        {
            get
            {
                return filterVariable;
            }

            set
            {
                filterVariable = value;
                this.NotifyPropertyChanged("FilterVariable");
            }
        }

        public DateTime? EndTime
        {
            get
            {
                return endTime;
            }

            set
            {
                endTime = value;
                this.NotifyPropertyChanged("EndTime");
            }
        }
        public ObservableCollection<GridData> ShownData
        {
            get
            {
                return shownData;
            }

            set
            {
                shownData = value;
                this.NotifyPropertyChanged("ShownData");
            }
        }
        public ObservableCollection<DataTable> AllData
        {
            get
            {
                return allData;
            }

            set
            {
                allData = value;
            }
        }

        public DateTime? StartTime
        {
            get
            {
                return startTime;
            }

            set
            {
                startTime = value;
                this.NotifyPropertyChanged("StartTime");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }

            // this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public static class Extensions
    {
        public static ObservableCollection<GridData> ToGridCollection(this IEnumerable<DataTable> inputData)
        {
            ObservableCollection<GridData> returnDataCollection = new ObservableCollection<GridData>();

            foreach (var item in inputData)
            {
                string time = item.Time.ToString("dd:MM:yyyy HH:mm:ss:fff");
                returnDataCollection.Add(new GridData(item.VariableName, item.VariableValue, time));
            }

            return returnDataCollection;
        }
    }


    public struct GridData
    {
        string variable;
        long value;
        string time;

        public string Variable
        {
            get
            {
                return variable;
            }

            set
            {
                variable = value;
            }
        }
        public long Value
        {
            get
            {
                return value;
            }

            set
            {
                this.value = value;
            }
        }
        public string Time
        {
            get
            {
                return time;
            }

            set
            {
                time = value;
            }
        }

        public GridData(string variable, long value, string time)
        {
            this.variable = variable;
            this.value = value;
            this.time = time;
        }
    }

}
