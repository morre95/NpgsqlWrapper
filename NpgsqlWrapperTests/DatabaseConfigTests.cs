using Microsoft.VisualStudio.TestTools.UnitTesting;
using NpgsqlWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NpgsqlWrapper.Tests
{
    [TestClass()]
    public class DatabaseConfigTests
    {
        [TestMethod()]
        public void LoadTest()
        {
            const string dbServer = "localhost";
            const int dbPort = 5432;
            const string dbUsername = "Username";
            const string dbPassword = "Passwords_12345öäå%@£$&hlkjkloiwui2345¤%#&";
            const string dbDatabase = "Databaseöäå#¤%";

            DatabaseConfig config = new DatabaseConfig
            {
                Server = dbServer,
                Port = dbPort,
                Username = dbUsername,
                Password = dbPassword,
                Database = dbDatabase
            };

            string configFile = "configTestLoad.json";

            DatabaseConfig.Save(configFile, config);

            DatabaseConfig configLoad = DatabaseConfig.Load(configFile);

            string host = configLoad.Server + ":" + configLoad.Port;
            string username = configLoad.Username;
            string password = configLoad.Password;
            string database = configLoad.Database;

            Assert.AreEqual(dbServer + ":" + dbPort, host);
            Assert.AreEqual(dbUsername, username);
            Assert.AreEqual(dbPassword, password);
            Assert.AreEqual(dbDatabase, database);
        }

        [TestMethod()]
        public void SaveTest()
        {
            const string dbServer = "localhost";
            const int dbPort = 5432;
            const string dbUsername = "Username";
            const string dbPassword = "Passwords_12345öäå%@£$&hlkjkloiwui2345¤%#&";
            const string dbDatabase = "Databaseöäå#¤%";

            DatabaseConfig config = new DatabaseConfig
            {
                Server = dbServer,
                Port = dbPort,
                Username = dbUsername,
                Password = dbPassword,
                Database = dbDatabase
            };

            string configFile = "configTestLoad.json";

            DatabaseConfig.Save(configFile, config);

            Assert.IsNull(config.Server);
            Assert.IsNull(config.Port);
            Assert.IsNull(config.Username);
            Assert.IsNull(config.Password);
            Assert.IsNull(config.Database);
        }
    }
}