namespace SharedLib.IS
{
    using SqlKata.Compilers;
    using SqlKata.Execution;
    using System.Collections.Generic;
    using System.Linq;

    public class ClientRepo
    {
        public static string TableName { get; set; } = "clients";

        private static Compiler compiler = new SqliteCompiler();

        public static ISClient GetClient(string id)
        {
            using (var connection = ConnectionFactory.GetConnection())
            {
                var db = new QueryFactory(connection, compiler);
                return db.Query(TableName).Where(new { id }).FirstOrDefault<ISClient>();
            }
        }

        public static List<ISClient> GetAllClients()
        {
            using (var connection = ConnectionFactory.GetConnection())
            {
                var db = new QueryFactory(connection, compiler);
                return db.Query(TableName).Get<ISClient>().ToList();
            }
        }

        public static bool SetClient(string id, string name, string secret, string uri)
        {
            using (var connection = ConnectionFactory.GetConnection())
            {
                try
                {
                    var db = new QueryFactory(connection, compiler);
                    return db.Query(TableName).Insert(new { id, name, secret, uri }) == 1;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static bool UpdateClient(string id, string name, string secret, string uri)
        {
            using (var connection = ConnectionFactory.GetConnection())
            {
                try
                {
                    var db = new QueryFactory(connection, compiler);
                    object updateObject = secret == null ? (object)new { name, uri } : new { name, uri, secret };
                    return db.Query(TableName).Where(new { id }).Update(updateObject) == 1;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static bool DeleteClient(string id)
        {
            using (var connection = ConnectionFactory.GetConnection())
            {
                try
                {
                    var db = new QueryFactory(connection, compiler);
                    return db.Query(TableName).Where(new { id }).Delete() == 1;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static List<string> GetAllUris()
        {
            using (var connection = ConnectionFactory.GetConnection())
            {
                try
                {
                    var db = new QueryFactory(connection, compiler);
                    return db.Query(TableName).Select("Uri").Get<string>().ToList();
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}