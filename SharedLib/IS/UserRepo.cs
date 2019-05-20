namespace SharedLib.IS
{
    using SqlKata.Compilers;
    using SqlKata.Execution;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class UserRepo
    {
        private static string tableName = "users";
        private static Compiler compiler = new SqliteCompiler();

        public static ISUser GetUser(string name, string password)
        {
            using (var connection = ConnectionFactory.GetConnection())
            {
                var db = new QueryFactory(connection, compiler);
                return db.Query(tableName).Where(new { name, password }).FirstOrDefault<ISUser>();
            }
        }

        public static ISUser GetUserByName(string name)
        {
            using (var connection = ConnectionFactory.GetConnection())
            {
                var db = new QueryFactory(connection, compiler);
                return db.Query(tableName).Where(new { name }).FirstOrDefault<ISUser>();
            }
        }

        public static ISUser GetUserById(string id)
        {
            using (var connection = ConnectionFactory.GetConnection())
            {
                var db = new QueryFactory(connection, compiler);
                return db.Query(tableName).Where(new { id }).FirstOrDefault<ISUser>();
            }
        }

        public static List<ISUser> GetAllUsers()
        {
            using (var connection = ConnectionFactory.GetConnection())
            {
                var db = new QueryFactory(connection, compiler);
                return db.Query(tableName).Get<ISUser>().ToList();
            }
        }

        public static string SetUser(string name, string password)
        {
            using (var connection = ConnectionFactory.GetConnection())
            {
                try
                {
                    string selectedGuid = string.Empty;
                    var db = new QueryFactory(connection, compiler);
                    // Костыль, этот цикл вечный, чисто для дебага с sqlite (он не умеет сам назначать guid-ы)
                    do
                    {
                        selectedGuid = Guid.NewGuid().ToString();
                    }
                    while (db.Query(tableName).Where(new { id = selectedGuid }).Get<ISUser>().Count() == 0);
                    
                    bool added = db.Query(tableName).Insert(new { id = selectedGuid, name, password }) == 1;
                    // Костыль, но для примера так просто, InsertGetId всегда нули возвращает
                    if (added)
                    {
                        return GetUser(name, password).Id;
                    }
                }
                catch
                {
                }
                return null;
            }
        }
    }
}