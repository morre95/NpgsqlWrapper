using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            MyNpgsql? npgsql = Connect();
            Assert.IsNotNull(npgsql);


            DbParams param = new DbParams("id", 0);

            IEnumerable<Teachers>? teacherList = npgsql.Fetch<Teachers>("SELECT * FROM teachers WHERE id>@id", param);
            Assert.IsNotNull(teacherList);

            Teachers? teachers = npgsql.FetchOne<Teachers>("SELECT COUNT(*) num FROM teachers");

            Assert.IsNotNull(teachers);

            Assert.AreEqual(teachers.Count, teacherList.Count());
        }

        [TestMethod()]
        public void ExecuteTest()
        {
            MyNpgsql? npgsql = Connect();
            Assert.IsNotNull(npgsql);


            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            IEnumerable<MyTemp>? myTemp = npgsql.Execute<MyTemp>(sql);
            Assert.IsNotNull(myTemp);

            Assert.IsFalse(myTemp.Any());
        }

        [TestMethod()]
        public void FetchOneTest()
        {
            MyNpgsql? npgsql = Connect();
            Assert.IsNotNull(npgsql);


            DbParams param = new DbParams("id", 0);

            IEnumerable<Teachers>? teacherList = npgsql.Fetch<Teachers>("SELECT * FROM teachers WHERE id>@id", param);
            Assert.IsNotNull(teacherList);

            Teachers? teachers = npgsql.FetchOne<Teachers>("SELECT COUNT(*) num FROM teachers");

            Assert.IsNotNull(teachers);

            Assert.AreEqual(teacherList.Count(), teachers.Count);
        }

        [TestMethod()]
        public void ExecuteOneTest()
        {
            MyNpgsql? npgsql = Connect();
            Assert.IsNotNull(npgsql);


            DbParams param = new DbParams("id", 0);

            IEnumerable<Teachers>? teacherList = npgsql.Fetch<Teachers>("SELECT * FROM teachers WHERE id>@id", param);
            Assert.IsNotNull(teacherList);

            Teachers? teachers = npgsql.ExecuteOne<Teachers>("SELECT COUNT(*) num FROM teachers");

            Assert.IsNotNull(teachers);

            Assert.AreEqual(teacherList.Count(), teachers.Count);
        }

        [TestMethod()]
        public void InsertTest()
        {
            MyNpgsql? npgsql = Connect();
            Assert.IsNotNull(npgsql);

            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            npgsql.ExecuteNonQuery(sql);

            MyTemp myTemp = new MyTemp();
            myTemp.c = 23;

            int rows = npgsql.Insert(myTemp);
            Assert.AreEqual(1, rows);

            myTemp.c = 42;

            int secondRow = npgsql.Insert(myTemp);
            Assert.AreEqual(1, secondRow);

            IEnumerable<MyTemp>? myTempList = npgsql.Fetch<MyTemp>();
            Assert.IsNotNull(myTempList);

            Assert.AreEqual(2, myTempList.Count());
        }

        [TestMethod()]
        public void InsertReturningTest()
        {
            MyNpgsql? npgsql = Connect();
            Assert.IsNotNull(npgsql);

            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            npgsql.ExecuteNonQuery(sql);

            MyTemp myTemp = new MyTemp();
            myTemp.c = 23;

            MyTemp? myTempReturning = npgsql.InsertReturning(myTemp);
            Assert.IsNotNull(myTempReturning);
            Assert.AreEqual(0, myTempReturning.num);

            Assert.AreEqual(23, myTempReturning.c);
        }

        [TestMethod()]
        public void InsertManyTest()
        {
            MyNpgsql? npgsql = Connect();
            Assert.IsNotNull(npgsql);

            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            npgsql.ExecuteNonQuery(sql);

            MyTemp myTemp = new MyTemp();
            myTemp.c = 23;

            List<MyTemp> tList = new()
            {
                myTemp,
                myTemp,
                myTemp
            };

            int rows = npgsql.InsertMany(tList);
            Assert.AreEqual(3, rows);
        }

        [TestMethod()]
        public void ExecuteNonQueryTest()
        {
            MyNpgsql? npgsql = Connect();
            Assert.IsNotNull(npgsql);

            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            npgsql.ExecuteNonQuery(sql);

            string insertSql = "INSERT INTO mytemp(c) VALUES(697)";

            int insertedRow = npgsql.ExecuteNonQuery(insertSql);

            Assert.AreEqual(1, insertedRow);
        }

        [TestMethod()]
        public void ExecuteNonQueryTest1()
        {
            MyNpgsql? npgsql = Connect();
            Assert.IsNotNull(npgsql);

            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            npgsql.ExecuteNonQuery(sql);

            string insertSql = "INSERT INTO mytemp(c) VALUES(697), (2), (88)";

            int insertedRow = npgsql.ExecuteNonQuery(insertSql);

            Assert.AreEqual(3, insertedRow);
        }

        [TestMethod()]
        public void InsertManyReturningTest()
        {
            MyNpgsql? npgsql = Connect();
            Assert.IsNotNull(npgsql);

            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            npgsql.ExecuteNonQuery(sql);

            MyTemp myTemp = new MyTemp();
            myTemp.c = 23;

            List<MyTemp> tList = new()
            {
                myTemp,
                myTemp,
                myTemp
            };

            IEnumerable<MyTemp>? myTempList = npgsql.InsertManyReturning(tList);
            Assert.IsNotNull(myTempList);
            Assert.AreEqual(3, myTempList.Count());

            MyTemp? tempList = npgsql.FetchOne<MyTemp>("SELECT COUNT(*) num FROM mytemp");
            Assert.IsNotNull(tempList);

            Assert.AreEqual(tempList.num, myTempList.Count());
        }

        [TestMethod()]
        public void UpdateTest()
        {
            MyNpgsql? npgsql = Connect();
            Assert.IsNotNull(npgsql);

            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            npgsql.ExecuteNonQuery(sql);


            MyTemp myTemp = new MyTemp();
            myTemp.c = 23;

            List<MyTemp> tList = new()
            {
                myTemp,
                myTemp,
                myTemp
            };

            myTemp = new();
            myTemp.c = 44;
            tList.Add(myTemp);
            tList.Add(myTemp);

            int rows = npgsql.InsertMany(tList);
            Assert.AreEqual(5, rows);

            DbParams param = new DbParams("temp", 44);

            myTemp.c = 100;

            int num = npgsql.Update(myTemp, "c=@temp", param);

            Assert.AreEqual(2, num);
        }

        [TestMethod()]
        public void DeleteTest()
        {
            MyNpgsql? npgsql = Connect();
            Assert.IsNotNull(npgsql);

            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            npgsql.ExecuteNonQuery(sql);


            MyTemp myTemp = new MyTemp();
            myTemp.c = 23;

            List<MyTemp> tList = new()
            {
                myTemp,
                myTemp,
                myTemp
            };

            myTemp = new();
            myTemp.c = 44;
            tList.Add(myTemp);
            tList.Add(myTemp);

            int rows = npgsql.InsertMany(tList);
            Assert.AreEqual(5, rows);

            int delRows = npgsql.Delete("MyTemp");
            Assert.AreEqual(5, delRows);
        }

        [TestMethod()]
        public void DeleteTest1()
        {
            MyNpgsql? npgsql = Connect();
            Assert.IsNotNull(npgsql);

            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            npgsql.ExecuteNonQuery(sql);


            MyTemp myTemp = new MyTemp();
            myTemp.c = 23;

            List<MyTemp> tList = new()
            {
                myTemp,
                myTemp,
                myTemp
            };

            myTemp = new();
            myTemp.c = 44;
            tList.Add(myTemp);
            tList.Add(myTemp);

            int rows = npgsql.InsertMany(tList);
            Assert.AreEqual(5, rows);

            int delRows = npgsql.Delete("MyTemp");
            Assert.AreEqual(5, delRows);
        }

        [TestMethod()]
        public async Task DeleteAsyncTest3Async()
        {
            MyNpgsql? npgsql = Connect();
            Assert.IsNotNull(npgsql);

            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            npgsql.ExecuteNonQuery(sql);


            MyTemp myTemp = new MyTemp();
            myTemp.c = 23;

            List<MyTemp> tList = new()
            {
                myTemp,
                myTemp,
                myTemp
            };

            myTemp = new();
            myTemp.c = 44;
            tList.Add(myTemp);
            tList.Add(myTemp);

            int rows = npgsql.InsertMany(tList);
            Assert.AreEqual(5, rows);

            DbParams param = new DbParams("temp", 44);
            int delRows = npgsql.Delete<MyTemp>("c=@temp", param);
            Assert.AreEqual(2, delRows);
        }

        [TestMethod()]
        public void DumpTest()
        {
            MyNpgsql? npgsql = Connect();
            Assert.IsNotNull(npgsql);

            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            int num = npgsql.ExecuteNonQuery(sql);

            Assert.AreEqual(-1, num);

            string insertSql = "INSERT INTO mytemp(c) VALUES(697), (2), (88)";


            int insertedRow = npgsql.ExecuteNonQuery(insertSql);

            Assert.AreEqual(3, insertedRow);

            string selectSql = "SELECT c FROM mytemp";

            IEnumerable<Dictionary<string, object>> dict = npgsql.Dump(selectSql, null);

            Assert.IsNotNull(dict);
            Assert.AreEqual(3, dict.Count());

            List<Dictionary<string, object>> expectedResult = new()
            {
                new Dictionary<string, object>{ { "c", 697 } },
                new Dictionary<string, object>{ { "c", 2 } },
                new Dictionary<string, object>{ { "c", 88 } }
            };

            Assert.AreEqual(expectedResult[0].Values.Count, dict.ToList()[0].Values.Count);
        }

        [TestMethod()]
        public void CreateTest()
        {
            MyNpgsql? npgsql = Connect();
            Assert.IsNotNull(npgsql);

            npgsql.Create<Person>(true, true);

            const string firstName = "Test";
            const string lastName = "Last Test";
            Person person = new();
            person.first_name = firstName;
            person.last_name = lastName;

            npgsql.Insert(person);


            Person? p = npgsql.FetchOne<Person>();

            Assert.IsNotNull(p);
            Assert.AreEqual(firstName, p.first_name);
            Assert.AreEqual(lastName, p.last_name);
        }
    }
}