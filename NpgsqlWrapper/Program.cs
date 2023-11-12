using Npgsql;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NpgsqlWrapper
{

    public class Person
    {
        // TODO: detta ska skapa en PK nyckel som är serial och not null
        [Field("person_id", "serial", true, true)]
        public int PersonId { get; set; }

        // TODO: detta ska skapa ett text fält med namn first_name och med default värde "FirstName"
        public string first_name { get; set; } = "FirstName";

        // TODO: detta ska skapa ett text fält med namn last_name utan default värde
        public string last_name { get; set; }


        // TODO: detta ska skapa ett varchar(25) fält med namn new_address med default värde "Banglore"
        [Field("new_address", "varchar(25)")]
        public string AddressLine { get; set; } = "Banglore";

        // TODO: detta ska skapa ett integer fält med namn my_int utan default värde och not null
        [Field("my_int", "int", true)]
        public int MyInt { get; set; }

        // TODO: detta ska skapa ett numeric(15, 2) fält med CHECK (price >= 0) och namn my_int med default värde 55.11
        [Field("my_numeric", "numeric(15, 2) CHECK (price >= 0)")]
        public double MyNumric { get; set; } = 55.11;
    }

    public class MyNpgsqlCreateAsync
    {
        private MyNpgsqlAsync _myNpgsql;

        public MyNpgsqlCreateAsync(string host, string username, string password, string database)
        {
            _myNpgsql = new(host, username, password, database);
        }

        public async void ConnectAsync()
        {
            await _myNpgsql.ConnectAsync();
        }
    }

    public class MyTemp
    {
        public int? c { get; set; }

        [UpdateIgnore]
        [InsertIgnore]
        public Int64 num { get; set; }
    }

    internal class Program
    {
        static async Task Main(string[] args)
        {

            Person p = new Person();




            // Edit en uncomment this code the first time you run this. Then remove it
            /*DatabaseConfig config = new DatabaseConfig
            {
                Server = "localhost",
                Port = 5432,
                Username = "Username",
                Password = "Password",
                Database = "Database"
            };

            string configFile = "config.json";

            DatabaseConfig.Save(configFile, config);*/


            string? host, username, password, database;

            GetDatabaseLogin(out host, out username, out password, out database);
            await TeachersAsync(host, username, password, database);
        }

        private static IEnumerable<FieldAttribute> GetAttributes<T>(T obj)
        {
            foreach (var attribute in obj.GetType().GetProperties())
            {
                if (attribute.GetCustomAttribute<FieldAttribute>(true) != null)
                {
                    FieldAttribute attr = attribute.ReadAttribute<FieldAttribute>();
                    attr.SetValue(attr.FieldName, attribute.GetValue(obj));
                    yield return attr;
                }
                else
                {
                    FieldAttribute attr = new(attribute.Name, attribute.GetType().ToString());
                    attr.SetValue(attribute.Name, attribute.GetValue(obj));
                    yield return attr;
                }
            }
        }

        private static void GetDatabaseLogin(out string? host, out string? username, out string? password, out string? database)
        {
            string configFile = "config.json";

            DatabaseConfig config = DatabaseConfig.Load(configFile);

            host = config.Server + ":" + config.Port;
            username = config.Username;
            password = config.Password;
            database = config.Database;

            if (host == null) { throw new ArgumentNullException(); }
            if (username == null) { throw new ArgumentNullException(); }
            if (password == null) { throw new ArgumentNullException(); }
            if (database == null) { throw new ArgumentNullException(); }
        }

        private static async Task TeachersAsync(string? host, string? username, string? password, string? database)
        {
            School school = new School();
            school.Connect(host, username, password, database);

            string command;
            do
            {
                Console.Write("> ");
                command = Console.ReadLine();

                if (command.ToLower().StartsWith("help"))
                {
                    Dictionary<string, string> helpList = new Dictionary<string, string>()
                    {
                        {"Command", "Description"},
                        {"help", "Print this list of commands"},
                        {"list", "List all teachers"},
                        {"add", "Add new teacher"},
                        {"delete", "Delete teacher"},
                        {"subject", "Look up teacher's by subject"},
                        {"table", "Print teachers in a table"},
                        {"exit", "exit tha app"}
                    };
                    PrintHelpList(helpList);
                }
                else if (command == "list")
                {
                    foreach (var teacher in await school.GetAll())
                    {
                        Console.WriteLine($"#{teacher.id}: {teacher.first_name} {teacher.last_name} has salary {teacher.salary} for teaching {teacher.Subjectet}");
                    }
                }
                else if (command == "add")
                {
                    Console.Write("First Name> ");
                    string firstName = Console.ReadLine();
                    Console.Write("Last Name> ");
                    string lastName = Console.ReadLine();

                    Console.Write("Subject> ");
                    string subject = Console.ReadLine();

                    string dirtySalary;
                    int salary;
                    do
                    {
                        Console.Write("Salary> ");
                        dirtySalary = Console.ReadLine();
                    } while (!int.TryParse(dirtySalary, out salary));

                    var teacherToAdd = new Teachers()
                    {
                        first_name = firstName,
                        last_name = lastName,
                        Subjectet = subject,
                        salary = salary
                    };
                    await school.Insert(teacherToAdd);
                }
                else if (command == "edit")
                {
                    string dirtyId;
                    int id;
                    do
                    {
                        Console.Write("Id> ");
                        dirtyId = Console.ReadLine();
                    } while (!int.TryParse(dirtyId, out id));
                    var teacher = await school.GetById(id);
                    Console.WriteLine($"First name: {teacher.first_name}");
                    Console.WriteLine($"Last name: {teacher.last_name}");
                    Console.WriteLine($"Subject: {teacher.Subjectet}");
                    Console.WriteLine($"Salary: {teacher.salary}");

                    Console.Write("First Name> ");
                    string firstName = Console.ReadLine();
                    if (firstName == "") firstName = teacher.first_name;
                    Console.Write("Last Name> ");
                    string lastName = Console.ReadLine();
                    if (lastName == "") lastName = teacher.last_name;
                    Console.Write("Subject> ");
                    string subject = Console.ReadLine();
                    if (subject == "") subject = teacher.Subjectet;

                    string dirtySalary;
                    int salary;
                    do
                    {
                        Console.Write("Salary> ");
                        dirtySalary = Console.ReadLine();
                        if (dirtySalary == "") dirtySalary = teacher.salary.ToString();
                    } while (!int.TryParse(dirtySalary, out salary));

                    var teacherToEdit = new Teachers()
                    {
                        first_name = firstName,
                        last_name = lastName,
                        Subjectet = subject,
                        salary = salary
                    };
                    await school.EditById(id, teacherToEdit);
                }
                else if (command == "delete")
                {
                    string dirtyId;
                    int id;
                    do
                    {
                        Console.Write("Id> ");
                        dirtyId = Console.ReadLine();
                    } while (!int.TryParse(dirtyId, out id));
                    await school.DeleteById(id);
                }
                else if (command == "subject")
                {
                    int i = 0;
                    List<string> list = new List<string>();
                    var all = await school.GetAll();
                    foreach (var subject in all.Select(x => x.Subjectet).Distinct())
                    {
                        Console.WriteLine($"#{++i} {subject}");
                        list.Add(subject);
                    }

                    string dirtyId;
                    int id;
                    do
                    {
                        Console.Write("#> ");
                        dirtyId = Console.ReadLine();
                    } while (!int.TryParse(dirtyId, out id));

                    foreach (var teacher in await school.GetBySubject(list[id - 1]))
                    {
                        Console.WriteLine($"#{teacher.id}: {teacher.first_name} {teacher.last_name} has salary {teacher.salary} for teaching {teacher.Subjectet}");
                    }
                }
                else if (command == "table")
                {
                    var all = await school.GetAll();
                    int longestName = all.Max(x => (x.first_name + " " + x.last_name).Length) + 1;
                    int longestSubject = all.Max(x => x.Subjectet.Length) + 1;
                    int longestSalary = all.Max(x => x.salary.ToString().Length);

                    string header = $"ID  |{"Name".PadRight(longestName)}|{"Subject".PadRight(longestSubject)}|Salary";
                    Console.WriteLine(header);
                    Console.WriteLine(new string('-', header.Length));

                    foreach (var teacher in await school.GetAll())
                    {
                        Console.WriteLine($"#{teacher.id.ToString().PadLeft(3, '0')}|{(teacher.first_name + " " + teacher.last_name).PadRight(longestName)}|{teacher.Subjectet.PadRight(longestSubject)}|{teacher.salary.ToString().PadLeft(longestSalary)}");
                    }
                }
                else if (command == "cancel")
                {
                    try
                    {
                        var ts = new CancellationTokenSource();
                        CancellationToken cts = ts.Token;
                        cts.Register(() => Console.WriteLine("Is the operation cancelled now?"));
                        ts.Cancel();
                        foreach (var teacher in await school.GetAll(cts))
                        {
                            Console.WriteLine($"#{teacher.id}: {teacher.first_name} {teacher.last_name} has salary {teacher.salary} for teaching {teacher.Subjectet}");
                        }
                    } 
                    catch (OperationCanceledException oce)
                    {
                        Console.WriteLine(oce.Message);
                    }
                    
                }
                else if ( command == "exit")
                {
                    school.Close();
                }

            } while (command != "exit");
        }

        private static void PrintHelpList(Dictionary<string, string> helpList)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Usage of teacher utility \n");
            Console.ResetColor();

            var longestCommand = helpList.Max(x => x.Key.Length);
            var longestRow = helpList.Max(y => $"{y.Key.PadRight(longestCommand)} | {y.Value}".Length);

            for (int i = 0; i < helpList.Count; i++)
            {
                var item = helpList.ElementAt(i);
                if (i == 0)
                {
                    Console.WriteLine($"{item.Key.PadRight(longestCommand)} | {item.Value}");
                    Console.WriteLine("".PadLeft(longestRow, '-'));
                }
                else
                {
                    Console.Write($"{item.Key.PadRight(longestCommand)} | ");
                    Console.WriteLine(item.Value);
                }
            }
        }
    }

    /*
CREATE TABLE teachers
(
    id serial NOT NULL,
    first_name character varying(25),
    last_name character varying(25),
    subject character varying(20),
    salary integer,
    PRIMARY KEY (id)
);


TODO: Skapa ett scrip kring det här i din wrapper klass som kan skapa dessa tabeller

    Använd [variable] variabelnamnet för att tala om vilken typ kolumn en ska ha

    Finns som script i PgAdmin: sales_NPGSQL_test.sql

CREATE TABLE sales
(
    sale_id serial NOT NULL,
    date date,
    customer_id integer,
    product_id integer,
    quantity integer CHECK (quantity >= 0),
    total_price numeric(15, 2) CHECK (total_price >= 0),
    PRIMARY KEY (sale_id)
);

CREATE TABLE customer
(
    customer_id serial NOT NULL,
    first_name varchar(50),
    last_name varchar(50),
    email varchar(100),
    PRIMARY KEY (customer_id)
);


CREATE TABLE product
(
    product_id serial NOT NULL,
    product_name varchar(100),
    price numeric(15, 2) CHECK (price >= 0),
    PRIMARY KEY (product_id)
);



-- Lägg till några kunder
INSERT INTO customer (first_name, last_name, email)
VALUES
    ('Anna', 'Andersson', 'anna@example.com'),
    ('Bengt', 'Bengtsson', 'bengt@example.com'),
    ('Cecilia', 'Carlsson', 'cecilia@example.com'),
	('Erik', 'Eriksson', 'erik@example.com'),
    ('David', 'Davidsson', 'david@example.com'),
    ('Frida', 'Fridsson', 'frida@example.com');

INSERT INTO product (product_name, price)
VALUES
    ('Produkt A', 10.99),
    ('Produkt B', 15.49),
    ('Produkt C', 7.99),
	('Produkt D', 12.99),
    ('Produkt E', 8.49),
    ('Produkt F', 14.99);


INSERT INTO sales (date, customer_id, product_id, quantity, total_price)
VALUES
    ('2023-11-10', 1, 1, 3, 32.97),
    ('2023-11-11', 2, 2, 2, 30.98),
	('2023-11-12', 4, 3, 4, 51.96), 
    ('2023-11-13', 5, 4, 5, 64.95), 
    ('2023-11-14', 6, 5, 2, 16.98);

*/
    public class Teachers
    {
        public int? id { get; set; }
        public string? first_name { get; set; }
        public string? last_name { get; set; }

        [Field("subject", "test")]
        public string? Subjectet { get; set; }
        public int? salary { get; set; }

        [Field("num", "int64")]
        public Int64? Count { get; set; }
    }

    public class School
    {
        MyNpgsqlAsync? pgsql = null;
        public async void Connect(string? host, string? username, string? password, string? database)
        {
            pgsql = new(host, username, password, database);
            await pgsql.ConnectAsync();
        }

        public async Task Close()
        {
            await pgsql.CloseAsync();
        }

        public async Task<List<Teachers>> GetAll(CancellationToken cancellationToken = default)
        {
            return await pgsql.FetchAsync<Teachers>(null, cancellationToken);
        }

        public async Task<int> DeleteById(int id)
        {
            DbParams p = new("id", id);
            return await pgsql.DeleteAsync<Teachers>($"id = @id", p);
        }

        public async Task<int> Insert(Teachers teacher)
        {
            return await pgsql.InsertAsync(teacher);
        }

        public async Task<Teachers> GetById(int id)
        {
            DbParams p = new("id", id);
            return await pgsql.FetchOneAsync<Teachers>("SELECT * FROM teachers WHERE id=@id", p);
        }

        public async Task<int> EditById(int id, Teachers teacher)
        {
            DbParams p = new("id", id);
            return await pgsql.UpdateAsync(teacher, "id=@id", p);
        }

        public async Task<List<Teachers>> GetBySubject(string subject)
        {
            DbParams p = new("subject", subject);
            return await pgsql.FetchAsync<Teachers>("SELECT * FROM teachers WHERE subject=@subject", p);
        }
    }
}
