using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SDiAF_App
{
    public class Database
    {
        private const string ConString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='C:\Users\Jonathan\SkyDrive\Documents\Uni\Year 3\SDiAF\Assignment\2. SourceCode\Database\coffeeDatabase.mdf';Integrated Security=True;Connect Timeout=30";
        private SqlConnection _dbConnection;

        private bool Connect()
        {
            try
            {
                _dbConnection = new SqlConnection(ConString);
                _dbConnection.Open();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "", MessageBoxButtons.OK);
                return false;
            }
        }

        private void Disconnect()
        {
            try
            {
                _dbConnection.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "", MessageBoxButtons.OK);
            }
        }

        public bool Sanitize(string toClean)
        {
            if (string.IsNullOrEmpty(toClean))
            {
                return true;
            }
            string clean = Regex.Replace(toClean, "-{2,}", "-");
            clean = Regex.Replace(clean, @"[*/]+", string.Empty);
            clean = Regex.Replace(clean,
                @"(;|\s)(exec|execute|select|insert|update|delete|create|alter|drop|rename|truncate|backup|restore)\s",
                string.Empty, RegexOptions.IgnoreCase);

            return clean == toClean;
        }

        public bool CheckInt(string toCheck)
        {
            return !string.IsNullOrEmpty(toCheck) && Regex.IsMatch(toCheck, @"(^[0-9]*$)");
        }

        public bool CheckDouble(string toCheck)
        {
            return !string.IsNullOrEmpty(toCheck) && Regex.IsMatch(toCheck, @"(^[0-9]{1,3}\.[0-9]{2}$)");
        }

        public DataTable Read(string sql)
        {
            DataTable readRes = new DataTable();
            if (!Connect())
            {
                return readRes;
            }
            try
            {
                var readCom = new SqlCommand(sql, _dbConnection);
                readRes.Load(readCom.ExecuteReader());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "", MessageBoxButtons.OK);
            }
            Disconnect();
            return readRes;
        }

        public string Save(string sql)
        {
            string result = "failed";
            if (!Connect())
            {
                return result;
            }
            try
            {
                SqlCommand saveCom = new SqlCommand(sql, _dbConnection);
                saveCom.ExecuteNonQuery();
                result = "success";
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "", MessageBoxButtons.OK);
            }
            Disconnect();
            return result;
        }

        public string SqlBuildInsert(string table, List<string> columns, List<string> values)
        {
            int columnsCount = columns.Count;
            int valuesCount = values.Count;

            if (columns.Count < 1 || values.Count < 1)
            {
                return "error";
            }

            string sql = "INSERT INTO " + table + " ";

            if (columnsCount > 0)
            {
                sql += "(";
                for (int i = 0; i < columnsCount; i++)
                {
                    sql += columns[i] + " ";
                    if (i + 1 < columnsCount)
                    {
                        sql += ", ";
                    }
                }
                sql += ") ";
            }

            sql += "VALUES (";
            for (int i = 0; i < valuesCount; i++)
            {
                sql += values[i] + " ";
                if (i + 1 < valuesCount)
                {
                    sql += ", ";
                }
            }

            sql += ");";

            return sql;
        }

        public string SqlBuildUpdate(string table, List<string> set, List<string> conditions, List<string> exclusions)
        {
            int setCount = set.Count;
            int conditionsCount = conditions.Count;
            int exclusionsCount = exclusions.Count;

            if (setCount < 1)
            {
                return "error";
            }
            string sql = "UPDATE " + table;

            sql += " SET ";

            for (int i = 0; i < setCount; i++)
            {
                sql += set[i] + " ";
                if (i + 1 < setCount)
                {
                    sql += ", ";
                }
            }

            if (conditionsCount > 0 || exclusionsCount > 0)
            {
                sql += "WHERE ";
                if (conditionsCount > 0)
                {

                    for (int i = 0; i < conditionsCount; i++)
                    {
                        sql += conditions[i] + " ";
                        if (i + 1 < conditionsCount || exclusionsCount > 0)
                        {
                            sql += "AND ";
                        }
                    }
                }

                if (exclusionsCount > 0)
                {
                    string excSql = "";
                    for (int i = 0; i < exclusionsCount; i++)
                    {
                        excSql += exclusions[i] + " ";
                        if (i + 1 < exclusionsCount)
                        {
                            excSql += "OR ";
                        }
                    }

                    if (conditionsCount > 0)
                    {
                        excSql = "(" + excSql + ")";
                    }
                    sql += excSql;
                }
            }

            sql += ";";

            return sql;
        }

        public string SqlBuildSelect(List<string> columns, List<string> tables, List<string> conditions, List<string> exclusions, string ordering)
        {
            int columnsCount = columns.Count;
            int tablesCount = tables.Count;
            int conditionsCount = conditions.Count;
            int exclusionsCount = exclusions.Count;
            
            if (columns.Count < 1 || tables.Count < 1)
            {
                return "error";
            }
            string sql = "SELECT ";
            
            for(int i = 0; i < columnsCount; i++)
            {
                sql += columns[i] + " ";
                if (i+1 < columnsCount)
                {
                    sql += ", ";
                }
            }

            sql += "FROM ";

            for (int i = 0; i < tablesCount; i++)
            {
                sql += tables[i] + " ";
                if (i + 1 < tablesCount)
                {
                    sql += ", ";
                }
            }

            if (conditionsCount > 0 || exclusionsCount > 0)
            {
                sql += "WHERE ";
                if (conditionsCount > 0)
                {

                    for (int i = 0; i < conditionsCount; i++)
                    {
                        sql += conditions[i] + " ";
                        if (i + 1 < conditionsCount || exclusionsCount > 0)
                        {
                            sql += "AND ";
                        }
                    }
                }

                if (exclusionsCount > 0)
                {
                    string excSql = "";
                    for (int i = 0; i < exclusionsCount; i++)
                    {
                        excSql += exclusions[i] + " ";
                        if (i + 1 < exclusionsCount)
                        {
                            excSql += "OR ";
                        }
                    }

                    if (conditionsCount > 0)
                    {
                        excSql = "(" + excSql + ")";
                    }
                    sql += excSql;
                }
            }

            if (ordering.Length > 0)
            {
                sql += "ORDER BY " + tables;
            }

            sql += ";";

            return sql;
        }

    }
}
