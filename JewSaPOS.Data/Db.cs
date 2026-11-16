using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using JewSaPOS.Domain;

namespace JewSaPOS.Data
{
    /// <summary>All database access goes through stored procedures with parameters (no string-built SQL).</summary>
    public static class Db
    {
        public static string ConnectionString
        {
            get
            {
                var cs = ConfigurationManager.ConnectionStrings["PosDb"];
                if (cs == null) throw new PosException("Connection string 'PosDb' is missing in the .exe.config file.");
                return cs.ConnectionString;
            }
        }

        public static SqlParameter P(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        public static DataTable Query(string proc, params SqlParameter[] ps)
        {
            try
            {
                using (var c = new SqlConnection(ConnectionString))
                using (var cmd = Build(c, proc, ps))
                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
            catch (SqlException ex) { throw Translate(ex); }
        }

        public static object Scalar(string proc, params SqlParameter[] ps)
        {
            try
            {
                using (var c = new SqlConnection(ConnectionString))
                using (var cmd = Build(c, proc, ps))
                {
                    c.Open();
                    return cmd.ExecuteScalar();
                }
            }
            catch (SqlException ex) { throw Translate(ex); }
        }

        public static void NonQuery(string proc, params SqlParameter[] ps)
        {
            try
            {
                using (var c = new SqlConnection(ConnectionString))
                using (var cmd = Build(c, proc, ps))
                {
                    c.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex) { throw Translate(ex); }
        }

        private static SqlCommand Build(SqlConnection c, string proc, SqlParameter[] ps)
        {
            var cmd = new SqlCommand(proc, c) { CommandType = CommandType.StoredProcedure };
            if (ps != null && ps.Length > 0) cmd.Parameters.AddRange(ps);
            return cmd;
        }

        private static PosException Translate(SqlException ex)
        {
            switch (ex.Number)
            {
                case 2627:
                case 2601:
                    return new PosException("Duplicate value: that SKU or barcode already exists.");
                case 547:
                    return new PosException("The change breaks a database rule (for example an invalid category).");
                case 515:
                    return new PosException("A required value is missing.");
                case -1:
                case 2:
                case 53:
                case 4060:
                case 18456:
                    return new PosException("Cannot connect to the database. Check the connection string in the .exe.config file and make sure SQL Server is running.\n\nDetails: " + ex.Message);
            }
            if (ex.Number >= 50000) return new PosException(ex.Message);
            return new PosException("Database error: " + ex.Message);
        }
    }
}
