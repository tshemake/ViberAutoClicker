using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
        private readonly string _connectionString;

        public ViberDb(string configDbPath)
        {
            _connectionString = "Data Source = " + configDbPath;
        }

        public void OffAccounts()
        {
            try
            {
                using (var sqLiteConnection = new SQLiteConnection(_connectionString))
                {
                    sqLiteConnection.Open();
                    using (var sqLiteCommand = new SQLiteCommand(sqLiteConnection))
                    {
                        sqLiteCommand.CommandText = "UPDATE 'Accounts' SET 'IsValid'='0'";
                        sqLiteCommand.CommandType = CommandType.Text;
                        sqLiteCommand.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        public async Task<BindingSource> WaitNewAAccountAsync()
        {
            try
            {
                using (var sqLiteConnection = new SQLiteConnection(_connectionString))
                {
                    sqLiteConnection.Open();
                    using (var sqLiteCommand = new SQLiteCommand(sqLiteConnection))
                    {
                        sqLiteCommand.CommandText = "SELECT COUNT(*) FROM 'Accounts'";
                        sqLiteCommand.CommandType = CommandType.Text;
                        var i = Convert.ToInt32(sqLiteCommand.ExecuteScalar());
                        while (Viber.IsOpen())
                        {
                            await Task.Delay(100);
                            if (i < Convert.ToInt32(sqLiteCommand.ExecuteScalar()))
                                break;
                        }

                        sqLiteCommand.CommandText = "UPDATE 'Accounts' SET 'IsValid'='1'";
                        sqLiteCommand.CommandType = CommandType.Text;
                        sqLiteCommand.ExecuteNonQuery();

                        sqLiteCommand.CommandText = "SELECT * FROM Accounts";
                        sqLiteCommand.CommandType = CommandType.Text;
                        sqLiteCommand.ExecuteNonQuery();
                        var dt = new DataTable();
                        var sql = new SQLiteDataAdapter { SelectCommand = sqLiteCommand };
                        sql.Fill(dt);
                        _bs = new BindingSource { DataSource = dt };
                    }
                }
                return _bs;
            }
            catch
            {
                return _bs;
            }
        }
    }
}
