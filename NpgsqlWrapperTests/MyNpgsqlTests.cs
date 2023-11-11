using Microsoft.VisualStudio.TestTools.UnitTesting;
using NpgsqlWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace NpgsqlWrapper.Tests
{
    [TestClass()]
    public class MyNpgsqlTests
    {
        private MyNpgsql Connect()
        {
            string configFile = "configTest.json";

            DatabaseConfig config = DatabaseConfig.Load(configFile);

            string host = config.Server + ":" + config.Port;
            string username = config.Username;
            string password = config.Password;
            string database = config.Database;

            MyNpgsql npgsql = new(host, username, password, database);
            npgsql.Connect();
            return npgsql;
        }

        [TestMethod()]
        public void MyNpgsqlTest()
        {
            MyNpgsql npgsql = Connect();
            Assert.IsNotNull(npgsql);
        }

        [TestMethod()]
        public void ConnectTest()
        {
            MyNpgsql npgsql = Connect();
            Assert.IsNotNull(npgsql);
        }

        [TestMethod()]
        public void CloseTest()
        {
            MyNpgsql npgsql = Connect();
            npgsql.Close();
            Assert.ThrowsException<ArgumentNullException>(() => npgsql.Dump("SELECT * FROM teachers"));
        }

        [TestMethod()]
        public void FetchTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FetchTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ExecuteTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FetchOneTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ExecuteOneTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void InsertTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void InsertReturningTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void InsertManyTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ExecuteNonQueryTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ExecuteNonQueryTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void InsertManyReturningTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DeleteTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DeleteTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DumpTest()
        {
            Assert.Fail();
        }
    }
}