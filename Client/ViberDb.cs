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
        private DataTable _dt = new DataTable();
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

        public async Task<BindingSource> WaitNewAccountAsync()
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
                        _dt = new DataTable();
                        var sql = new SQLiteDataAdapter { SelectCommand = sqLiteCommand };
                        sql.Fill(_dt);
                        _bs = new BindingSource { DataSource = _dt };
                    }
                }
                return _bs;
            }
            catch
            {
                return _bs;
            }
        }

        public string CurrentAccounts()
        {
            var result = String.Empty;
            try
            {
                using (var sqLiteConnection = new SQLiteConnection(_connectionString))
                {
                    sqLiteConnection.Open();
                    using (var sqLiteCommand = new SQLiteCommand(sqLiteConnection))
                    {
                        sqLiteCommand.CommandText = "SELECT * FROM Accounts WHERE isDefault = '1'";
                        sqLiteCommand.CommandType = CommandType.Text;
                        var reader = sqLiteCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            result = reader.GetString(0).ToString().Trim();
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }

            return result;
        }

        public BindingSource LoadAccounts()
        {
            try
            {
                var columns = _dt.Columns;
                var column = new DataColumn { AutoIncrementSeed = 1L };
                var num = 1;
                column.AutoIncrement = num != 0;
                var str = "№";
                column.ColumnName = str;
                columns.Add(column);
                var factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite");
                using (var sqLiteConnection = (SQLiteConnection)factory?.CreateConnection())
                {
                    if (sqLiteConnection != null)
                    {
                        sqLiteConnection.ConnectionString = _connectionString;
                        sqLiteConnection.Open();
                        using (var sqLiteCommand = new SQLiteCommand(sqLiteConnection))
                        {
                            sqLiteCommand.CommandText = "SELECT * FROM Accounts";
                            sqLiteCommand.CommandType = CommandType.Text;
                            sqLiteCommand.ExecuteNonQuery();
                            var sql = new SQLiteDataAdapter {SelectCommand = sqLiteCommand};
                            sql.Fill(_dt);
                            _bs.DataSource = _dt;
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }

            return _bs;
        }
    }
}
