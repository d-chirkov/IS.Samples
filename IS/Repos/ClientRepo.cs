using System.Linq;
using SqlKata.Execution;
using System.Data.SQLite;
using System.Data;
using SqlKata.Compilers;
using IS.Models;

namespace IS.Repos
{
    public static class ClientRepo
    {
        private static string tableName = "clients";
        private static Compiler compiler = new SqliteCompiler();

        public static Client GetClient(string id)
        {
            using (var connection = ConnectionFactory.GetConnection())
            {
                var db = new QueryFactory(connection, compiler);
                return db.Query(tableName).Where(new { id }).FirstOrDefault<Client>();
            }
        }
    }
}