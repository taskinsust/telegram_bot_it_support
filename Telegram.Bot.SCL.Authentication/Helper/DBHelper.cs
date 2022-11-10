using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace BOTAuthentication.Helper
{
    public static class DbHelper
    {
        public static SqlConnection OpenDbConnection()
        {
            SqlConnection con;
          
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile($"appsettings.json", true, true);
            var Configuration = builder.Build();
            string connectionString = Configuration["Settings:connString"];

            con = new SqlConnection(connectionString);
            con.Open();
            return con;
        }

    }

}
