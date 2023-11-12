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
            Assert.IsNotNull(npgsql);

            npgsql.Close();
            Assert.ThrowsException<ArgumentNullException>(() => npgsql.Close());
        }

        [TestMethod()]
        public void FetchTest()
        {
            MyNpgsql? npgsql = Connect();
            Assert.IsNotNull(npgsql);

            IEnumerable<Teachers>? teacherList = npgsql.Fetch<Teachers>();
            Assert.IsNotNull(teacherList);

            Teachers? teachers = npgsql.FetchOne<Teachers>("SELECT COUNT(*) num FROM teachers");

            Assert.IsNotNull(teachers);

            Assert.AreEqual(teachers.Count, teacherList.Count());
        }

        [TestMethod()]
        public void FetchTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ExecuteTest()
        {
            MyNpgsql? npgsql = Connect();
            Assert.IsNotNull(npgsql);


            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            IEnumerable<MyTemp>? myTemp = npgsql.Execute<MyTemp>(sql);
            Assert.IsNotNull(myTemp);

            Assert.IsTrue(myTemp.Any());
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