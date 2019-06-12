namespace IdSrv.Account.WebApi.IntegrationTests
{
    using System;
    using System.IO;

    internal static class TestHelper
    {
        public static string GetPathToTestDb()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Databases\test_compact_db.sdf");
        }
    }
}
