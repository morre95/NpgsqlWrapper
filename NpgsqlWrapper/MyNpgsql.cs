using Npgsql;
using System.Reflection;
using System.Diagnostics;


namespace NpgsqlWrapper
{
    /// <summary>
    /// Wrapper for Npgsql: https://github.com/npgsql/npgsql
    /// </summary>
    public partial class MyNpgsql : MyNpgsqlBase
    {
        /// <summary>
        /// Constructor, building connection string with parameter provided by the user
        /// </summary>
        /// <param name="host">Host for server</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="database">Database</param>
        public MyNpgsql(string host, string username, string password, string database) :
            base(host, username, password, database)
        { }

        /// <summary>
        /// Builds the connection to the database
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsql pgsql = new(host, username, password, database);
        /// pgsql.Connect();
        /// ]]>
        /// </code>
        /// </example>
        public void Connect()
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionString);
            var dataSource = dataSourceBuilder.Build();

            _conn = dataSource.OpenConnection();
        }

        /// <summary>
        /// Closing database connection
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsql pgsql = new(host, username, password, database);
        /// pgsql.Connect();
        /// 
        /// ...
        /// Do your database work here
        /// ...
        /// 
        /// pgsql.Close();
        /// ]]>
        /// </code>
        /// </example>
        /// <exception cref="ArgumentNullException">Throws if connections is not made</exception>
        public void Close()
        {
            if (_conn == null) throw new ArgumentNullException(nameof(_conn));
            _conn.Close();
            _conn = null;
        }

        

        /// <summary>
        /// Executes a SQL query that returns a list of objects of type 'T'.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsql pgsql = new(host, username, password, database);
        /// pgsql.Connect();
        /// 
        /// DbParams p = new DbParams()
        /// {
        ///     { "id", 23 }
        /// }
        /// 
        /// foreach (var item in pgsql.Execute<Teachers>("SELECT * FROM teachers WHERE id<@id", p))
        /// {
        ///     Console.WriteLine(item.first_name);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type of objects to retrieve from the database.</typeparam>
        /// <param name="sqlQuery">The SQL query to execute.</param>
        /// <param name="parameters">The parameters to bind to the SQL query.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> of objects of type 'T' retrieved from the database.</returns>
        public IEnumerable<T> Execute<T>(string sqlQuery, Dictionary<string, object>? parameters = null)
        {
            IEnumerable<PropertyInfo> properties = typeof(T).GetProperties();

            using var cmd = new NpgsqlCommand(sqlQuery, _conn);
            AddParameters(sqlQuery, parameters, cmd);

            List<PropertyInfo> propertyList = properties.ToList();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                T item = Activator.CreateInstance<T>();
                item = SetObjectValues(propertyList, item, reader);
                yield return item;
            }
        }

        /// <summary>
        /// Executes a <see cref="NpgsqlDataReader"/> object that returns a list of objects of type 'T'.
        /// </summary>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="properties">List of <see cref="PropertyInfo"/> made out of 'T'.</param>
        /// <param name="cmd">The <see cref="NpgsqlCommand"/> command object</param>
        /// <returns>A object of the type 'T' mapped with data from ths sql command</returns>
        private static T? ExecuteReader<T>(IEnumerable<PropertyInfo> properties, NpgsqlCommand cmd)
        {
            T item = Activator.CreateInstance<T>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                return SetObjectValues(properties.ToList(), item, reader);
            }
            return default;
        }

        

        

        /// <summary>
        /// Executes a SQL command and returns number of effected rows.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsql pgsql = new(host, username, password, database);
        /// pgsql.Connect();
        /// 
        /// DbParams p = new DbParams
        /// {
        ///     { "firstName", "First name" },
        ///     { "LastName", "Last name" },
        ///     { "subject", "the best subject" },
        ///     { "salary", 250 }
        /// };
        /// pgsql.ExecuteNonQuery("INSERT INTO teachers(first_name, last_name, subject, salary) VALUES(@firstName, @lastName, @subject, @salary)", p);
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="sql">SQL query</param>
        /// <param name="parameters">The parameters to bind to the SQL command.</param>
        /// <returns>The number of effected rows</returns>
        /// <exception cref="ArgumentNullException">Thrown when connection is null.</exception>
        public int ExecuteNonQuery(string sql, Dictionary<string, object> parameters)
        {
            if (_conn == null) throw new ArgumentNullException(nameof(_conn));
            using var cmd = new NpgsqlCommand(sql, _conn);
            AddParameters(sql, parameters, cmd);
            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes a SQL command and returns number of effected rows.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsql pgsql = new(host, username, password, database);
        /// pgsql.Connect();
        /// 
        /// int affectedRows = pgsql.ExecuteNonQuery("CALL stored_procedure_name(argument_list)");
        /// Console.WriteLine(affectedRows);
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="sql">SQL query</param>
        /// <returns>The number of effected rows</returns>
        /// <exception cref="ArgumentNullException">Thrown when connection is null.</exception>
        public int ExecuteNonQuery(string sql)
        {
            if (_conn == null) throw new ArgumentNullException(nameof(_conn));
            return ExecuteNonQuery(sql, new DbParams());
        }

        /// <summary>
        /// Dumps a record set from the database into a List of Dictionarys
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsql pgsql = new(host, username, password, database);
        /// pgsql.Connect();
        /// 
        /// foreach (var item in pgsql.Dump("SELECT * FROM teachers"))
        /// {
        ///     Console.WriteLine(item["first_name"]);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="sqlQuery">The SQL query to execute.</param>
        /// <param name="parameters">The parameters to bind to the SQL query.</param>
        /// <returns>A list of objects of type '<see cref="Dictionary{string, object}"/>' retrieved from the database.</returns>
        /// <exception cref="ArgumentNullException">Thrown when connection is null.</exception>
        public IEnumerable<Dictionary<string, object>> Dump(string sqlQuery, Dictionary<string, object>? parameters = null)
        {
            if (_conn == null) throw new ArgumentNullException(nameof(_conn));
            using var cmd = new NpgsqlCommand(sqlQuery, _conn);

            AddParameters(sqlQuery, parameters, cmd);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    dict.Add(reader.GetName(i), reader[i]);
                }
                yield return dict;
            }
        }


        /// <summary>
        /// Creates database table from class structure.
        /// </summary>
        /// <typeparam name="T">The type of object to map table structure from.</typeparam>
        /// <param name="dropIfExists">If true it adds DROP TABLE IF EXISTS statement.</param>
        /// <param name="isTempTable">If true it creates a temporary table</param>
        /// <exception cref="ArgumentNullException">Thrown if name of field is set via the attribute [Field("name_of_field")] but no FieldType.</exception>
        public void Create<T>(bool dropIfExists = false, bool isTempTable = false)
        {
            IEnumerable<PropertyInfo?> propertyList = typeof(T).GetProperties().Where(x => x != null)!;
            IEnumerable<FieldAttribute> fieldAttributes = PrepareCreateFields<T>(propertyList);

            string queryString = PrepareCreateSql<T>(fieldAttributes);

            string tableName = GetTableName<T>();
            queryString = isTempTable ? $"CREATE TEMP TABLE {tableName}({queryString}" : $"CREATE TABLE {tableName}({queryString}";

            if (dropIfExists)
            {
                using var batch = new NpgsqlBatch(_conn)
                {
                    BatchCommands =
                    {
                        new($"DROP TABLE IF EXISTS {tableName}"),
                        new(queryString)
                    }
                };

                batch.ExecuteNonQuery();
            }
            else
            {
                ExecuteNonQuery(queryString);
            }
        }
    }
}
