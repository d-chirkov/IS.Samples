﻿using IS.Models;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Collections.Generic;
using System.Linq;

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

        public static List<User> GetAllUsers()
        {
            using (var connection = ConnectionFactory.GetConnection())
            {
                var db = new QueryFactory(connection, compiler);
                return db.Query(tableName).Get<User>().ToList();
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