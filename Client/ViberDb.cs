using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public class ViberDb
    {
        private BindingSource _bs = new BindingSource();
        private readonly string _configDbPath;

        public ViberDb(string configDbPath)
        {
            _configDbPath = configDbPath;
        }

        public void OffAccounts()
        {
            try
            {
                var connection = (SQLiteConnection)null;
                connection.ConnectionString = "Data Source = " + _configDbPath;
                connection.Open();
                SQLiteCommand sqLiteCommand = new SQLiteCommand(connection)
                {
                    CommandText = "UPDATE 'Accounts' SET 'IsValid'='0'",
                    CommandType = CommandType.Text
                };
                sqLiteCommand.ExecuteNonQuery();
                connection.Close();
            }
            catch
            {
            }
        }

        public async Task<BindingSource> WaitNewAAccountAsync()
        {
            try
            {
                var connection = (SQLiteConnection)null;
                connection.ConnectionString = "Data Source = " + _configDbPath;
                connection.Open();
                var command = new SQLiteCommand(connection)
                {
                    CommandText = "SELECT COUNT(*) FROM 'Accounts'",
                    CommandType = CommandType.Text
                };
                var i = Convert.ToInt32(command.ExecuteScalar());
                while (Viber.IsOpen())
                {
                    await Task.Delay(100);
                    if (i < Convert.ToInt32(command.ExecuteScalar()))
                        break;
                }
                command.CommandText = "UPDATE 'Accounts' SET 'IsValid'='1'";
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
                command.CommandText = "SELECT * FROM Accounts";
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
                var dt = new DataTable();
                var sql = new SQLiteDataAdapter { SelectCommand = command };
                sql.Fill(dt);
                _bs = new BindingSource { DataSource = dt };
                connection.Close();
                return _bs;
            }
            catch
            {
                return _bs;
            }
        }
    }
}
