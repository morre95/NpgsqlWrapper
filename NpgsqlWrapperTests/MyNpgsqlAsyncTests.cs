﻿using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace NpgsqlWrapper.Tests
{

    [TableName("person")]
    public class Person
    {
        [Field("person_id", "serial", true, true)]
        public int? PersonId { get; set; }

        public string first_name { get; set; } = "FirstName";

        public string last_name { get; set; }

        [Field("new_address", "varchar(25)")]
        public string AddressLine { get; set; } = "Banglore";

        [Field("my_int", "int", true)]
        public int MyInt { get; set; }

        [Field("price", "int")]
        public int? Price { get; set; }

        [Field("my_numeric", "numeric(15, 2) CHECK (price >= 0)")]
        public decimal MyNumric { get; set; } = 55.11m;
    }

    public class Sales
    {
        [Field("sale_id", "serial", true, true)]
        public int? SalesId {  get; set; }

        [Field("date", "date")]
        public DateTime? Date { get; set; }

        [Field("customer_id", "int")]
        public int? CustomerId { get; set; }

        [Field("product_id", "int")]
        public int? ProductId { get; set; }

        [Field("quantity", "integer CHECK (quantity >= 0)")]
        public int? Quantity { get; set; }

        [Field("total_price", "numeric(15, 2) CHECK(total_price >= 0)")]
        public decimal? TotalPrice { get; set; }

        [Field("num", "int64")]
        public Int64 Count { get; set; }
    }

    public class MyTemp
    {
        public int? c { get; set; }

        [UpdateIgnore]
        [InsertIgnore]
        public Int64 num { get; set; }
    }

    [TestClass()]
    public class MyNpgsqlAsyncTests
    {

        private const string TempSalesTable = "CREATE TEMP TABLE sales_temp"+
                                        "("+
                                            "sale_id serial NOT NULL,"+
                                            "date date,"+
                                            "customer_id integer,"+
                                            "product_id integer,"+
                                            "quantity integer, "+
                                            "total_price numeric(15, 2),"+
                                            "PRIMARY KEY (sale_id)"+
                                        ");";

        private const string TempSalesInsert = "INSERT INTO sales (date, customer_id, product_id, quantity, total_price) VALUES" +
            "('2023-11-10', 1, 1, 3, 32.97)," +
            "('2023-11-11', 2, 2, 2, 30.98)," +
            "('2023-11-12', 4, 3, 4, 51.96)," +
            "('2023-11-13', 5, 4, 5, 64.95)," +
            "('2023-11-14', 6, 5, 2, 16.98)";


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
        public void MyNpgsqlAsyncTest()
        {
            MyNpgsqlAsync? npgsql = new("", "", "", ""); 
            Assert.IsNotNull(npgsql);

            Assert.IsInstanceOfType(npgsql, typeof(MyNpgsqlBase));
        }

        [TestMethod()]
        public async Task ConnectAsyncTest()
        {
            MyNpgsqlAsync? npgsql = await ConnectAsync();
            Assert.IsNotNull(npgsql);
        }

        [TestMethod()]
        public async Task CloseAsyncTestAsync()
        {
            MyNpgsqlAsync? npgsql = await ConnectAsync();
            Assert.IsNotNull(npgsql);

            await npgsql.CloseAsync();

            Assert.IsNotNull(npgsql);

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => npgsql.CloseAsync());
        }

        [TestMethod()]
        public async Task FetchAsyncTest()
        {
            MyNpgsqlAsync? npgsql = await ConnectAsync();
            Assert.IsNotNull(npgsql);

            var ts = new CancellationTokenSource();
            CancellationToken cts = ts.Token;

            List<Teachers>? teacherList = await npgsql.FetchAsync<Teachers>();
            Assert.IsNotNull(teacherList);

            Assert.IsTrue(teacherList.Any());
        }

        [TestMethod()]
        public async Task FetchAsyncTest2()
        {
            MyNpgsqlAsync? npgsql = await ConnectAsync();
            Assert.IsNotNull(npgsql);

            var ts = new CancellationTokenSource();
            CancellationToken cts = ts.Token;

            List<Teachers>? teacherList = await npgsql.FetchAsync<Teachers>(null, cts);
            Assert.IsNotNull(teacherList);

            Assert.IsTrue(teacherList.Any());

            Teachers? teachers = await npgsql.FetchOneAsync<Teachers>("SELECT COUNT(*) num FROM teachers");

            Assert.IsNotNull(teachers);

            Assert.AreEqual(teachers.Count, teacherList.Count);
        }

        [TestMethod()]
        public async Task FetchAsyncTest1Async()
        {
            MyNpgsqlAsync? npgsql = await ConnectAsync();
            Assert.IsNotNull(npgsql);

            await npgsql.ExecuteNonQueryAsync(TempSalesTable);

            await npgsql.ExecuteNonQueryAsync(TempSalesInsert);


            var ts = new CancellationTokenSource();
            CancellationToken cts = ts.Token;

            DbParams param = new DbParams("pDate", DateTime.Parse("2023-11-10"));
            List<Sales>? salesList = await npgsql.FetchAsync<Sales>("SELECT * FROM sales_temp WHERE date=@pDate", param, cts);
            Assert.IsNotNull(salesList);

            Sales? sales = await npgsql.FetchOneAsync<Sales>("SELECT COUNT(*) num FROM sales_temp WHERE date=@pDate", param, cts);

            Assert.IsNotNull(sales);

            Assert.AreEqual(sales.Count, salesList.Count);
        }

        [TestMethod()]
        public async Task FetchEnumerableAsyncTest()
        {
            MyNpgsqlAsync? npgsql = await ConnectAsync();

            DbParams param = new DbParams("pDate", DateTime.Parse("2023-11-10"));

            await foreach (var test in npgsql.FetchEnumerableAsync<Sales>("SELECT * FROM sales", param))
            {
                Assert.IsNotNull(test);
            }
        }

        [TestMethod()]
        public async Task FetchEnumerableAsyncTest2()
        {
            MyNpgsqlAsync? npgsql = await ConnectAsync();
            Assert.IsNotNull(npgsql);

            var ts = new CancellationTokenSource();
            CancellationToken cts = ts.Token;

            DbParams param = new DbParams("pDate", DateTime.Parse("2023-11-10"));

            await foreach (var test in npgsql.FetchEnumerableAsync<Sales>("SELECT * FROM sales", param, cts))
            {
                Assert.IsNotNull(test);
            }
        }

        [TestMethod()]
        public async Task FetchOneAsyncTestAsync()
        {
            MyNpgsqlAsync? npgsql = await ConnectAsync();
            Assert.IsNotNull(npgsql);

            DbParams param = new DbParams("pDate", DateTime.Parse("2023-11-10"));

            Sales? sales = await npgsql.FetchOneAsync<Sales>("SELECT COUNT(*) num FROM sales WHERE date=@pDate", param);

            Assert.IsNotNull(sales);
        }

        [TestMethod()]
        public async Task FetchOneAsyncTest1()
        {
            MyNpgsqlAsync? npgsql = await ConnectAsync();
            Assert.IsNotNull(npgsql);

            var ts = new CancellationTokenSource();
            CancellationToken cts = ts.Token;

            DbParams param = new DbParams("pDate", DateTime.Parse("2023-11-10"));

            Sales? sales = await npgsql.FetchOneAsync<Sales>("SELECT COUNT(*) num FROM sales WHERE date=@pDate", param, cts);

            Assert.IsNotNull(sales);
        }

        [TestMethod()]
        public async Task InsertAsyncTest()
        {
            MyNpgsqlAsync? npgsql = await ConnectAsync();
            Assert.IsNotNull(npgsql);

            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            await npgsql.ExecuteNonQueryAsync(sql);

            MyTemp myTemp = new MyTemp();
            myTemp.c = 23;

            int rows = await npgsql.InsertAsync(myTemp);
            Assert.AreEqual(1, rows);

            myTemp.c = 42;

            int secondRow = await npgsql.InsertAsync(myTemp);
            Assert.AreEqual(1, secondRow);

            List<MyTemp>? myTempList = await npgsql.FetchAsync<MyTemp>();
            Assert.IsNotNull(myTempList);

            Assert.AreEqual(2, myTempList.Count);
        }

        [TestMethod()]
        public async Task InsertReturningAsyncTest()
        {
            MyNpgsqlAsync? npgsql = await ConnectAsync();
            Assert.IsNotNull(npgsql);

            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            await npgsql.ExecuteNonQueryAsync(sql);

            MyTemp myTemp = new MyTemp();
            myTemp.c = 23;

            MyTemp myTempReturning = await npgsql.InsertReturningAsync(myTemp);
            Assert.IsNotNull(myTempReturning);
            Assert.AreEqual(0, myTempReturning.num);

            Assert.AreEqual(23, myTempReturning.c);
        }

        [TestMethod()]
        public async Task InsertManyAsyncTest()
        {
            MyNpgsqlAsync? npgsql = await ConnectAsync();
            Assert.IsNotNull(npgsql);

            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            await npgsql.ExecuteNonQueryAsync(sql);

            MyTemp myTemp = new MyTemp();
            myTemp.c = 23;

            List<MyTemp> tList = new()
            {
                myTemp,
                myTemp,
                myTemp
            };

            int rows = await npgsql.InsertManyAsync(tList);
            Assert.AreEqual(3, rows);
        }

        [TestMethod()]
        public async Task InsertManyReturningAsyncTest()
        {

            MyNpgsqlAsync? npgsql = await ConnectAsync();
            Assert.IsNotNull(npgsql);

            var ts = new CancellationTokenSource();
            CancellationToken cts = ts.Token;

            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            await npgsql.ExecuteNonQueryAsync(sql, cts);

            MyTemp myTemp = new MyTemp();
            myTemp.c = 23;

            List<MyTemp> tList = new()
            {
                myTemp,
                myTemp,
                myTemp
            };

            List<MyTemp>? myTempList = await npgsql.InsertManyReturningAsync(tList);
            Assert.IsNotNull(myTempList);
            Assert.AreEqual(3, myTempList.Count);

            MyTemp? tempList = await npgsql.FetchOneAsync<MyTemp>("SELECT COUNT(*) num FROM mytemp");
            Assert.IsNotNull(tempList);

            Assert.AreEqual(tempList.num, myTempList.Count);
        }

        [TestMethod()]
        public async Task UpdateAsyncTest()
        {
            MyNpgsqlAsync? npgsql = await ConnectAsync();
            Assert.IsNotNull(npgsql);

            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            await npgsql.ExecuteNonQueryAsync(sql);


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

            int rows = await npgsql.InsertManyAsync(tList);
            Assert.AreEqual(5, rows);

            var ts = new CancellationTokenSource();
            CancellationToken cts = ts.Token;

            DbParams param = new DbParams("temp", 44);

            myTemp.c = 100;

            int num = await npgsql.UpdateAsync(myTemp, "c=@temp", param);

            Assert.AreEqual(2, num);
        }

        [TestMethod()]
        public async Task DeleteAsyncTest()
        {
            MyNpgsqlAsync? npgsql = await ConnectAsync();
            Assert.IsNotNull(npgsql);

            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            await npgsql.ExecuteNonQueryAsync(sql);


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

            int rows = await npgsql.InsertManyAsync(tList);
            Assert.AreEqual(5, rows);

            int delRows = await npgsql.DeleteAsync("MyTemp");
            Assert.AreEqual(5, delRows);
        }

        [TestMethod()]
        public async Task DeleteAsyncTest2()
        {
            MyNpgsqlAsync? npgsql = await ConnectAsync();
            Assert.IsNotNull(npgsql);

            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            await npgsql.ExecuteNonQueryAsync(sql);


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

            int rows = await npgsql.InsertManyAsync(tList);
            Assert.AreEqual(5, rows);

            int delRows = await npgsql.DeleteAsync("MyTemp");
            Assert.AreEqual(5, delRows);
        }

        [TestMethod()]
        public async Task DeleteAsyncTest3()
        {
            MyNpgsqlAsync? npgsql = await ConnectAsync();
            Assert.IsNotNull(npgsql);

            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            await npgsql.ExecuteNonQueryAsync(sql);


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

            int rows = await npgsql.InsertManyAsync(tList);
            Assert.AreEqual(5, rows);

            DbParams param = new DbParams("temp", 44);
            int delRows = await npgsql.DeleteAsync<MyTemp>("c=@temp", param);
            Assert.AreEqual(2, delRows);
        }

        [TestMethod()]
        public async Task ExecuteNonQueryAsyncTest()
        {
            MyNpgsqlAsync? npgsql = await ConnectAsync();
            Assert.IsNotNull(npgsql);

            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            await npgsql.ExecuteNonQueryAsync(sql);

            string insertSql = "INSERT INTO mytemp(c) VALUES(697)";

            int insertedRow = await npgsql.ExecuteNonQueryAsync(insertSql);

            Assert.AreEqual(1, insertedRow);
        }

        [TestMethod()]
        public async Task ExecuteNonQueryAsyncTest1()
        {
            MyNpgsqlAsync? npgsql = await ConnectAsync();
            Assert.IsNotNull(npgsql);

            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            await npgsql.ExecuteNonQueryAsync(sql);

            string insertSql = "INSERT INTO mytemp(c) VALUES(697), (2), (88)";

            var ts = new CancellationTokenSource();
            CancellationToken cts = ts.Token;

            int insertedRow = await npgsql.ExecuteNonQueryAsync(insertSql, cts);

            Assert.AreEqual(3, insertedRow);
        }

        [TestMethod()]
        public async Task ExecuteOneAsyncTestAsync()
        {
            MyNpgsqlAsync? npgsql = await ConnectAsync();
            Assert.IsNotNull(npgsql);

            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            MyTemp? myTemp = await npgsql.ExecuteOneAsync<MyTemp>(sql);
            Assert.IsNull(myTemp);
        }

        [TestMethod()]
        public async Task ExecuteOneAsyncTest2()
        {
            MyNpgsqlAsync? npgsql = await ConnectAsync();
            Assert.IsNotNull(npgsql);

            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            MyTemp? myTemp = await npgsql.ExecuteOneAsync<MyTemp>(sql);
            Assert.IsNull(myTemp);

            string insertSql = "INSERT INTO mytemp(c) VALUES(697), (2), (88)";

            var ts = new CancellationTokenSource();
            CancellationToken cts = ts.Token;

            MyTemp? myTempInsert = await npgsql.ExecuteOneAsync<MyTemp>(insertSql, null, cts);
            Assert.IsNull(myTempInsert);

            MyTemp? myTempSelect = await npgsql.ExecuteOneAsync<MyTemp>("SELECT c FROM mytemp", null, cts);
            Assert.IsNotNull(myTempSelect);

            Assert.AreEqual(typeof(MyTemp).Name, myTempSelect.GetType().Name);
        }

        [TestMethod()]
        public async Task ExecuteAsyncTest()
        {
            MyNpgsqlAsync? npgsql = await ConnectAsync();
            Assert.IsNotNull(npgsql);


            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            List<MyTemp>? myTemp = await npgsql.ExecuteAsync<MyTemp>(sql);
            Assert.IsNull(myTemp);
        }

        [TestMethod()]
        public async Task DumpAsyncTest()
        {
            MyNpgsqlAsync? npgsql = await ConnectAsync();
            Assert.IsNotNull(npgsql);

            string sql = "CREATE TEMP TABLE mytemp(c INT)";

            int num = await npgsql.ExecuteNonQueryAsync(sql);

            Assert.AreEqual(-1, num);

            string insertSql = "INSERT INTO mytemp(c) VALUES(697), (2), (88)";

            var ts = new CancellationTokenSource();
            CancellationToken cts = ts.Token;

            int insertedRow = await npgsql.ExecuteNonQueryAsync(insertSql, cts);

            Assert.AreEqual(3, insertedRow);

            string selectSql = "SELECT c FROM mytemp";

            List<Dictionary<string, object>>  dict = await npgsql.DumpAsync(selectSql, null, cts);

            Assert.IsNotNull(dict);
            Assert.AreEqual(3, dict.Count);

            List<Dictionary<string, object>> expectedResult = new()
            {
                new Dictionary<string, object>{ { "c", 697 } },
                new Dictionary<string, object>{ { "c", 2 } },
                new Dictionary<string, object>{ { "c", 88 } }
            };

            Assert.AreEqual(expectedResult[0].Values.Count, dict[0].Values.Count);
        }

        [TestMethod()]
        public async Task CreateAsyncTest()
        {
            MyNpgsqlAsync? npgsql = await ConnectAsync();
            Assert.IsNotNull(npgsql);

            await npgsql.CreateAsync<Person>(true, true);

            const string firstName = "Test";
            const string lastName = "Last Test";
            Person person = new();
            person.first_name = firstName;
            person.last_name = lastName;

            await npgsql.InsertAsync(person);


            Person? p = await npgsql.FetchOneAsync<Person>();
            
            Assert.IsNotNull(p);
            Assert.AreEqual(firstName, p.first_name);
            Assert.AreEqual(lastName, p.last_name);
        }
    }
}