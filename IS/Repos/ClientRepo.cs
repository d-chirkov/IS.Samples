using System.Linq;
using SqlKata.Execution;
using System.Data.SQLite;
using System.Data;
using SqlKata.Compilers;
using IS.Models;

namespace IS.Repos
{
    public class ClientRepo : IClientRepo
    {
        private string pathToSqliteDb;
        private string tableName;
        private Compiler compiler;

        public ClientRepo(string pathToSqliteDb, string tableName)
        {
            this.pathToSqliteDb = pathToSqliteDb;
            this.tableName = tableName;
            this.compiler = new SqliteCompiler();
        }

        public Client GetClient(string id)
        {
            using (var connection = this.GetConnection())
            {
                var db = new QueryFactory(connection, this.compiler);
                return db.Query(this.tableName).Where(new { id }).FirstOrDefault<Client>();
            }
        }

        private IDbConnection GetConnection()
        {
            return new SQLiteConnection($"Data Source={this.pathToSqliteDb};");
        }
    }
}