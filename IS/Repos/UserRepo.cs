using System.Linq;
using SqlKata.Execution;
using SqlKata.Compilers;
using IS.Models;

namespace IS.Repos
{
    public static class UserRepo
    {
        private static string tableName = "users";
        private static Compiler compiler = new SqliteCompiler();
        
        public static User GetUser(string name, string password)
        {
            using (var connection = ConnectionFactory.GetConnection())
            {
                var db = new QueryFactory(connection, compiler);
                return db.Query(tableName).Where(new { name, password }).FirstOrDefault<User>();
            }
        }

        public static User GetUser(int id)
        {
            using (var connection = ConnectionFactory.GetConnection())
            {
                var db = new QueryFactory(connection, compiler);
                return db.Query(tableName).Where(new { id }).FirstOrDefault<User>();
            }
        }

        public static bool SetUser(string name, string password)
        {
            using (var connection = ConnectionFactory.GetConnection())
            {
                try
                {
                    var db = new QueryFactory(connection, compiler);
                    return db.Query(tableName).Insert(new { name, password }) == 1;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}