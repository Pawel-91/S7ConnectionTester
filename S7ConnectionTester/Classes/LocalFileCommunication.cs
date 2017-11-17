using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace S7ConnectionTester
{
    class LocalFileCommunication : IStoreData
    {
        private string fileLocation;
        public LocalFileCommunication(string fileLocation)
        {
            this.fileLocation = fileLocation;
        }

        public IEnumerable<DataTable> GetData()
        {
            if (fileLocation == string.Empty)
            {
                MessageBox.Show("Nie wskazano pliku z danymi!", "Błąd pobierania danych", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            if (File.Exists(this.fileLocation) == false)
            {
                MessageBox.Show("Podana lokalizacja nie istnieje!", "Błąd pobierania danych", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            var readings = File.ReadAllLines(this.fileLocation);
            List<DataTable> dataFromFile = new List<DataTable>();

            foreach (var line in readings)
            {
                try
                {

                    string[] fields = line.Split(';');

                    if (fields.Length < 3)
                    {
                        continue;
                    }

                    int id = dataFromFile.Count + 1;
                    int indexOfMiliseconds = fields[2].LastIndexOf(':');
                    DateTime time = DateTime.Parse(fields[2].Remove(indexOfMiliseconds));
                    time = time.AddMilliseconds(double.Parse(fields[2].Substring(indexOfMiliseconds + 1)));
                    DataTable dt = new DataTable()
                    {
                        ID = id,
                        VariableName = fields[0],
                        VariableValue = int.Parse(fields[1]),
                        Time = time
                    };
                    dataFromFile.Add(dt);
                }
                catch
                {
                    continue;
                }
            }

            return dataFromFile;

        }

        public void StoreData(IEnumerable<DataTable> table)
        {
            StringBuilder sB = new StringBuilder();

            foreach (var tab in table)
            {
                sB.AppendLine($"{tab.VariableName};{tab.VariableValue};{tab.Time}:{tab.Time.Millisecond}");
            }

            File.AppendAllText(this.fileLocation, sB.ToString());
        }

        /// <summary>
        /// Stores new line of data in the file
        /// </summary>
        /// <param name="table"></param>
        public void StoreData(DataTable table)
        {
            string data = $"{table.VariableName};{table.VariableValue};{table.Time}:{table.Time.Millisecond}{Environment.NewLine}";

            File.AppendAllText(this.fileLocation, data);

        }
    }
}
