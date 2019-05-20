namespace SharedLib.IS
{
    using System;
    using System.Data;
    using System.Data.SQLite;
    using System.IO;

    public static class ConnectionFactory
    {
        private static string pathToSqliteDb = Path.Combine(
            new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName,
            "SharedResources",
            "is.sqlite");

        public static IDbConnection GetConnection()
        {
            return new SQLiteConnection($"Data Source={ConnectionFactory.pathToSqliteDb};");
        }
    }
}