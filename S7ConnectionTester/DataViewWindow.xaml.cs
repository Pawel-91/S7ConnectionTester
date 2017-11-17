using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace S7ConnectionTester
{
    /// <summary>
    /// Interaction logic for DataViewWindow.xaml
    /// </summary>
    public partial class DataViewWindow : Window
    {
        DataViewViewModel dataViewModel;
        public DataViewWindow()
        {
            InitializeComponent();
            this.dataViewModel = new DataViewViewModel();
            this.DataContext = this.dataViewModel;
            dataViewModel.Location = StorageLocation.LocalFile;
        }


        private void DBRadioButton_Click(object sender, RoutedEventArgs e)
        {
            dataViewModel.Location = StorageLocation.DataBase;
        }

        private void FileRadioButton_Click(object sender, RoutedEventArgs e)
        {
            dataViewModel.Location = StorageLocation.LocalFile;
        }

        private void GetDataButton_Click(object sender, RoutedEventArgs e)
        {
            IStoreData dataReader;

            if (dataViewModel.Location == StorageLocation.LocalFile)
            {
                dataReader = new LocalFileCommunication(this.dataViewModel.FileLocation);
            }
            else
            {
                dataReader = new DBCommunication();
            }

            this.dataViewModel.AllData = new ObservableCollection<DataTable>(dataReader.GetData());
            this.dataViewModel.ShownData = new ObservableCollection<DataTable>(this.dataViewModel.AllData).ToGridCollection();

        }

        private void pickLocation_Click(object sender, RoutedEventArgs e)
        {

            Task.Factory.StartNew(() =>
            {
                OpenFileDialog oFD = new OpenFileDialog();
                if (oFD.ShowDialog() == true)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        dataViewModel.FileLocation = oFD.FileName;
                    }));
                }
            });
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<DataTable> filterData = dataViewModel.AllData;

            if (dataViewModel.StartTime.HasValue)
            {
                filterData = filterData.Where(a => a.Time > dataViewModel.StartTime.Value);
            }

            if (dataViewModel.EndTime.HasValue)
            {
                filterData = filterData.Where(a => a.Time < dataViewModel.EndTime);
            }

            if (string.IsNullOrEmpty(dataViewModel.FilterVariable) == false)
            {
                filterData = filterData.Where(a => a.VariableName == dataViewModel.FilterVariable);
            }

            dataViewModel.ShownData = filterData.ToGridCollection();
        }
    }
}
