using System.Collections.Generic;


namespace S7ConnectionTester
{
    interface IStoreData
    {
        void StoreData(DataTable table);
        void StoreData(IEnumerable<DataTable> table);
        IEnumerable<DataTable> GetData();

    }
}
