using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Web.Script.Serialization;

namespace ClientePisee.Domain
{
    public class DataSource
    {
        public static string ExecuteNonQuery(SqlCommand command, string connectionString)
        {
            using ( SqlConnection cn = new SqlConnection(connectionString) )
            {
                string response = "";
                command.Connection = cn;
                cn.Open();
                try
                {
                    command.ExecuteNonQuery();
                    response = "OK";
                }
                catch ( Exception ex )
                {
                    Log.TextLog("ERROR al ejecutar Query : " + command.CommandText, ex.ToString());
                    response = "ERROR";
                }
                finally
                {
                    cn.Close();
                }

                return response;
            }
        }

        public static string ExecuteNonQuery(string Sql, string connectionString)
        {
            using ( SqlConnection cn = new SqlConnection(connectionString) )
            {
                string response = "";
                SqlCommand command = new SqlCommand(Sql, cn);
                cn.Open();
                try
                {
                    command.ExecuteNonQuery();
                    response = "OK";
                }
                catch ( Exception ex )
                {
                    Log.TextLog("ERROR al ejecutar Query : " + Sql, ex.ToString());
                    response = "ERROR";
                }
                finally
                {
                    cn.Close();
                }
                return response;
            }
        }

        public static DataTable ExecuteReader(string Sql, string connectionString)
        {

            DataTable table = new DataTable();
            using ( var conn = new SqlConnection(connectionString) )
            {

                var command = new SqlCommand(Sql, conn);
                conn.Open();

                try
                {
                    SqlDataReader reader = command.ExecuteReader();
                    if ( reader.HasRows )
                    {
                        table.Load(reader);
                    }
                }
                catch ( Exception ex )
                {
                    Log.TextLog("Error al ejecutar Query SQL : " + Sql + " ", ex.ToString());
                }


            }
            return table;
        }

        //Serialize Datetime//


        public class DateTimeConverter : JavaScriptConverter
        {
            public override IEnumerable<Type> SupportedTypes
            {
                get { return new List<Type>() { typeof(DateTime), typeof(DateTime?) }; }
            }

            public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
            {
                Dictionary<string, object> result = new Dictionary<string, object>();
                if ( obj == null )
                {
                    result["DateTime1"] = "";
                    result["DateTime2"] = "";
                    result["DateTime3"] = "";
                }
                else
                {
                    result["DateTime1"] = ((DateTime)obj).ToString("D", CultureInfo.CreateSpecificCulture("es-ES"));
                    result["DateTime2"] = ((DateTime)obj).ToString("yyyy-MM-dd");
                    result["DateTime3"] = ((DateTime)obj).ToString("yyyy-MM-dd HH.mm.ss");
                    result["DateTime4"] = ((DateTime)obj).ToString("dd-MM-yyyy");
                    result["DateTime5"] = ((DateTime)obj).ToString("dd-MM-yyyy HH.mm.ss");
                }

                return result;
            }

            public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
            {
                if ( dictionary.ContainsKey("DateTime") )
                    return new DateTime(long.Parse(dictionary["DateTime"].ToString()), DateTimeKind.Unspecified);
                return null;
            }
        }

        public static string ExecuteReaderJson(string Sql, string connectionString)
        {
            string Json = "";
            DataTable table = new DataTable();
            using ( var conn = new SqlConnection(connectionString) )
            {
                var command = new SqlCommand(Sql, conn);
                conn.Open();

                try
                {
                    SqlDataReader reader = command.ExecuteReader();
                    if ( reader.HasRows )
                    {
                        table.Load(reader);
                    }
                }
                catch ( Exception ex )
                {
                    Log.TextLog("Error al ejecutar Query SQL : " + Sql + " ", ex.ToString());
                }

            }
            var list = new List<Dictionary<string, object>>();

            foreach ( DataRow row in table.Rows )
            {
                var dict = new Dictionary<string, object>();

                foreach ( DataColumn col in table.Columns )
                {
                    if ( row[col].ToString() != "null" && row[col].ToString() != "NULL" && row[col].ToString() != null && row[col] != null )
                    {
                        if ( col.DataType == typeof(string) )
                        {
                            dict[col.ColumnName] = row[col].ToString().Trim();
                        }
                        else
                        {
                            dict[col.ColumnName] = row[col];
                        }

                    }
                    else
                    {
                        dict[col.ColumnName] = "";
                    }


                }
                list.Add(dict);
            }



            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new JavaScriptConverter[] { new DateTimeConverter() });

            Json = serializer.Serialize(list);
            return Json;



        }

        public static DataTable ExecuteReaderOleDB(string Sql, string connectionString)
        {
            DataTable table = new DataTable();
            using ( var conn = new OleDbConnection(connectionString) )
            {
                OleDbCommand comm = new OleDbCommand(Sql, conn);

                conn.Open();
                try
                {
                    OleDbDataReader reader = comm.ExecuteReader();

                    if ( reader.HasRows )
                    {
                        table.Load(reader);
                    }
                }
                catch ( Exception ex )
                {
                    Log.TextLog("Error al ejecutar Query OLEDB: " + Sql + " ", ex.ToString());
                }
            }



            return table;
        }

    }
}
