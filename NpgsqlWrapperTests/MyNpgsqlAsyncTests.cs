using Microsoft.VisualStudio.TestTools.UnitTesting;
using NpgsqlWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NpgsqlWrapper.Tests
{
    [TestClass()]
    public class MyNpgsqlAsyncTests
    {
        private async Task<MyNpgsqlAsync?> ConnectAsync()
        {
            string configFile = "configTest.json";

            DatabaseConfig config = DatabaseConfig.Load(configFile);

            string host = config.Server + ":" + config.Port;
            string username = config.Username;
            string password = config.Password;
            string database = config.Database;

            MyNpgsqlAsync npgsql = new(host, username, password, database);
            await npgsql.ConnectAsync();
            return npgsql;
        }

        [TestMethod()]
        public async Task MyNpgsqlAsyncTest()
        {
            MyNpgsqlAsync? npgsql = await ConnectAsync();
            Assert.IsNotNull(npgsql);
        }

        [TestMethod()]
        public void ConnectAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CloseAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FetchAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FetchAsyncTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FetchEnumerableAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FetchOneAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FetchOneAsyncTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void InsertAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void InsertReturningAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void InsertManyAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void InsertManyReturningAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DeleteAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DeleteAsyncTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ExecuteNonQueryAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ExecuteNonQueryAsyncTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ExecuteOneAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ExecuteAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DumpAsyncTest()
        {
            Assert.Fail();
        }
    }
}