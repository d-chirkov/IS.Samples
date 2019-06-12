namespace IdSrv.Account.WebApi.IntegrationTests
{
    using System;
    using System.IO;

    static class TestHelper
    {
        public static string GetPathToTestDb() => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Databases\test_compact_db.sdf");
    }
}
