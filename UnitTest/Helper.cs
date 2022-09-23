using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BizTalkComponents.ExtensionObjects.DBLookupHelper.UnitTests
{
    public class Helper
    {
        public static void CreateDatabase()
        {
            var cnxn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=true;");
            var cmd = cnxn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cnxn.Open();
            cmd.CommandText = Properties.Resources.Create_TestDatabase;
            cmd.ExecuteNonQuery();
            cnxn.ChangeDatabase("Sales");
            cmd.CommandText = Properties.Resources.CreateTable;
            cmd.ExecuteNonQuery();
            cmd.CommandText = Properties.Resources.AddRecords;
            cmd.ExecuteNonQuery();
            cnxn.Close();
        }
    }
}

