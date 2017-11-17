using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace S7ConnectionTester
{
    class DBCommunication : IStoreData
    {
        public IEnumerable<DataTable> GetData()
        {
            using (mgrDBEntities entity = new mgrDBEntities())
            {
                var query = entity.DataTable.ToList();
                return query;
            }
        }

        public void StoreData(IEnumerable<DataTable> table)
        {
            using (mgrDBEntities entity = new mgrDBEntities())
            {
                entity.DataTable.AddRange(table);
                entity.SaveChanges();
            }
        }

        public void StoreData(DataTable table)
        {
            using (mgrDBEntities entity = new mgrDBEntities())
            {
                entity.DataTable.Add(table);
                entity.SaveChanges();

            }
        }

        public static bool CheckDBConnection()
        {
            bool connected;
            using (DbContext dBC = new DbContext("mgrDBEntities"))
            {
                connected = dBC.Database.Exists();
            }

            return connected;
        }
    }
}
