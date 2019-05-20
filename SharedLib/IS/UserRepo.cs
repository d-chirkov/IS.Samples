namespace SharedLib.IS
{
    using SqlKata.Compilers;
    using SqlKata.Execution;
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

        public static ISUser GetUser(string name)
        {
            using (var connection = ConnectionFactory.GetConnection())
            {
                var db = new QueryFactory(connection, compiler);
                return db.Query(tableName).Where(new { name }).FirstOrDefault<ISUser>();
            }
        }

        public static ISUser GetUser(int id)
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

        public static int? SetUser(string name, string password)
        {
            using (var connection = ConnectionFactory.GetConnection())
            {
                try
                {
                    var db = new QueryFactory(connection, compiler);
                    bool added = db.Query(tableName).Insert(new { name, password }) == 1;
                    // Костыль, но для примера так просто, InsertGetId всегда нули возвращает
                    if (added)
                    {
                        return int.Parse(GetUser(name, password).Id);
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