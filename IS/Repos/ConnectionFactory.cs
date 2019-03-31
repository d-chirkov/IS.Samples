using System;
using System.Data;
using System.Data.SQLite;

namespace IS.Repos
{
    public static class ConnectionFactory
    {
        private static string pathToSqliteDb = $"{AppDomain.CurrentDomain.BaseDirectory}is.sqlite";

        public static IDbConnection GetConnection()
        {
            return new SQLiteConnection($"Data Source={ConnectionFactory.pathToSqliteDb};");
        }
    }
}