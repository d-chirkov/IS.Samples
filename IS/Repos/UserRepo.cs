using System.Linq;
using SqlKata.Execution;
using System.Data.SQLite;
using System.Data;
using SqlKata.Compilers;
using IS.Models;

namespace IS.Repos
{
    class UserRepo : IUserRepo
    {
        private string pathToSqliteDb;
        private Compiler compiler;

        public UserRepo(string pathToSqliteDb)
        {
            this.pathToSqliteDb = pathToSqliteDb;
            this.compiler = new SqliteCompiler();
        }

        public User GetUser(string name, string password)
        {
            using (var connection = this.GetConnection())
            {
                var db = new QueryFactory(connection, this.compiler);
                return db.Query("users").Where(new { name, password }).FirstOrDefault<User>();
            }
        }

        public User GetUser(int id)
        {
            using (var connection = this.GetConnection())
            {
                var db = new QueryFactory(connection, this.compiler);
                return db.Query("users").Where(new { id }).FirstOrDefault<User>();
            }
        }

        public bool SetUser(string name, string password)
        {
            using (var connection = this.GetConnection())
            {
                try
                {
                    var db = new QueryFactory(connection, this.compiler);
                    return db.Query("users").Insert(new { name, password }) == 1;
                }
                catch
                {
                    return false;
                }
            }
        }

        private IDbConnection GetConnection()
        {
            return new SQLiteConnection($"Data Source={this.pathToSqliteDb};");
        }
    }
}