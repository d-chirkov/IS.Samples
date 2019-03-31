﻿using System.Linq;
using SqlKata.Execution;
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

        public static bool SetClient(string id, string name, string secret, string uri)
        {
            using (var connection = ConnectionFactory.GetConnection())
            {
                try
                {
                    var db = new QueryFactory(connection, compiler);
                    return db.Query(tableName).Insert(new { id, name, secret, uri }) == 1;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}