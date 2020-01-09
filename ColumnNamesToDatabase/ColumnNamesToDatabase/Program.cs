using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace ColumnNamesToDatabase
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //Read CSV or EXCEL files/./
            string path = @"C:\Users\Public\Documents\AHVB";
            string[] fileEntries = Directory.GetFiles(@"C:\Users\Public\Documents\AHVB\", "*.CSV");
            var cnnStr = string.Format(@"Provider=Microsoft.Jet.OleDb.4.0; Data Source=" + path + @";Extended Properties='Text;HDR=YES;FMT=Delimited(,)';");
            //string csvData = File.ReadAllText(csvPath);
            DataTable dt = new DataTable();
            string fileName = null;
            foreach (var pathName in fileEntries)
            {
                fileName = Path.GetFileName(pathName).ToUpper();
                Console.WriteLine("#$#$%#%#$ {0}", fileName);

                try
                {
                    using (var conn = new OleDbConnection(cnnStr))
                    {

                        conn.Open();
                        string sql = string.Format("select * from [{0}]", new FileInfo(fileName).Name);
                        using (OleDbDataAdapter adapter = new OleDbDataAdapter(sql, conn))
                        {
                            adapter.Fill(dt);
                            //CsvCreator(csvOutputFile, fileName, dt);
                            foreach (DataColumn col in dt.Columns)
                            {
                                Console.WriteLine(col);
                                await PushToDBAsync(fileName, col.ToString());
                                ////Console.ReadLine();
                            }
                        }
                        conn.Close();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);

                }
            }
            
            
            //await PushToDBAsync();
        }

        //Push data to Azure SQL DB
        public static async Task PushToDBAsync(string filename, string columnName)
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

                builder.DataSource = "glbbirptsqlsvrdev.database.windows.net";
                builder.UserID = "DLDRSSOHR";
                builder.Password = "@_ObsEn321";
                builder.InitialCatalog = "SSO_HR_HAVB_VBA";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    Console.WriteLine("\nQuery data example:");
                    Console.WriteLine("=========================================\n");

                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Select * from [SRC_AHVB_SourceFilename]");
                    //sb.Append("SELECT TOP 20 pc.Name as CategoryName, p.name as ProductName ");
                    //sb.Append("FROM [SalesLT].[ProductCategory] pc ");
                    //sb.Append("JOIN [SalesLT].[Product] p ");
                    //sb.Append("ON pc.productcategoryid = p.productcategoryid;");
                    String sql = sb.ToString();
                    string a = "avcd";
                    string b = "Asdf";
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"Insert into SRC_AHVB_SourceFilename (Filename, Filename_ColumnName) VALUES (@n1, @n2)";
                        command.Parameters.AddWithValue(@"n1", filename);
                        command.Parameters.AddWithValue(@"n2", columnName);
                        int rowCount = await command.ExecuteNonQueryAsync();
                        Console.WriteLine(String.Format("Number of rows inserted={0}", rowCount));
                        //using (SqlDataReader reader = command.ExecuteReader())
                        //{
                        //    while (reader.Read())
                        //    {
                        //        //Console.WriteLine(reader.GetString(2));
                        //        Console.WriteLine("{0} {1}", reader.GetString(0), reader.GetString(2));
                        //    }
                        //}
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("\nDone. Press enter.");
            //try
            //{
            //    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            //    builder.DataSource = "glbbirptsqlsvrdev.database.windows.net";
            //    builder.UserID = "DLDRSSOHR";
            //    builder.Password = "@_ObsEn321";
            //    builder.InitialCatalog = "SSO_HR_HAVB_VBA";

            //    using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            //    {
            //        Console.WriteLine("\nQuery data example:");
            //        Console.WriteLine("=========================================\n");

            //        connection.Open();
            //        StringBuilder sb = new StringBuilder();
            //        sb.Append("Select * from [SRC_AHVB_SourceFilename]");
            //        //sb.Append("SELECT TOP 20 pc.Name as CategoryName, p.name as ProductName ");
            //        //sb.Append("FROM [SalesLT].[ProductCategory] pc ");
            //        //sb.Append("JOIN [SalesLT].[Product] p ");
            //        //sb.Append("ON pc.productcategoryid = p.productcategoryid;");
            //        string query = sb.ToString();
            //        //using (SqlCommand command1 = new SqlCommand(query, connection))
            //        //{
            //        //    using (SqlDataReader reader = command1.ExecuteReader())
            //        //    {
            //        //        while (reader.Read())
            //        //        {
            //        //            //Console.WriteLine(reader.GetString(2));
            //        //            Console.WriteLine("{0} {1}", reader.GetString(0), reader.GetString(2));
            //        //        }
            //        //    }
            //        //}  
            //        using (SqlCommand command = connection.CreateCommand())
            //        {

            //            command.CommandText = @"Insert into SRC_AHVB_SourceFilename (Filename, Filename_ColumnName) VALUES (@n1, @n2)";
            //            command.Parameters.AddWithValue(@"n1", "sdf");
            //            command.Parameters.AddWithValue(@"n2", "asdf");
            //            int rowCount = await command.ExecuteNonQueryAsync();
            //            Console.WriteLine(String.Format("Number of rows inserted={0}", rowCount));

            //        }
            //    }
            //}
            //catch (SqlException e)
            //{
            //    Console.WriteLine(e.ToString());
            //}
            //Console.WriteLine("\nDone. Press enter.");
        }
    }
}
