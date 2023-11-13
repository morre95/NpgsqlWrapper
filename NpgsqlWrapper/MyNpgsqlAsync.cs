using Npgsql;
using System.Reflection;

namespace NpgsqlWrapper
{
    /// <summary>
    /// Wrapper for Npgsql: https://github.com/npgsql/npgsql
    /// </summary>
    public partial class MyNpgsqlAsync : MyNpgsqlBase
    {

        /// <summary>
        /// Setting up the connection string
        /// </summary>
        /// <param name="host">Host IP</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="database">Database name</param>
        public MyNpgsqlAsync(string host, string username, string password, string database) :
            base(host, username, password, database)
        { }

        /// <summary>
        /// Builds the connection to the database, asynchronously.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsqlAsync pgsql = new(host, username, password, database);
        /// await pgsql.ConnectAsync();
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="cancellationToken">
        /// An optional token to cancel the asynchronous operation. The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionString);
            var dataSource = dataSourceBuilder.Build();

            _conn = await dataSource.OpenConnectionAsync(cancellationToken);
        }

        /// <summary>
        /// Close connection to database, asynchronously.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsqlAsync pgsql = new(host, username, password, database);
        /// await pgsql.ConnectAsync();
        /// 
        /// ...
        /// Do your database work here
        /// ...
        /// 
        /// await pgsql.CloseAsync();
        /// ]]>
        /// </code>
        /// </example>
        /// <exception cref="ArgumentNullException">Throws if connections is not made</exception>
        public async Task CloseAsync()
        {
            if ( _conn == null ) throw new ArgumentNullException(nameof( _conn));
            await _conn.CloseAsync();
            _conn = null;
        }

        /// <summary>
        /// Executes a <see cref="NpgsqlDataReader"/> object that returns a list of objects of type 'T' asynchronously.
        /// </summary>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="propertyList">List of <see cref="PropertyInfo"/> made out of T</param>
        /// <param name="cmd">The <see cref="NpgsqlCommand"/> command object</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the asynchronous operation. The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>The result of the SQL query mapped to the specified type.</returns>
        private static async Task<List<T>?> ExecuteReaderMenyAsync<T>(List<PropertyInfo> propertyList, NpgsqlCommand cmd, CancellationToken cancellationToken = default)
        {
            List<T> returnList = new List<T>();
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                T item = Activator.CreateInstance<T>();
                item = SetObjectValues(propertyList, item, reader);
                returnList.Add(item);
            }
            if (returnList.Count <= 0) return default;
            return returnList;
        }

        /// <summary>
        /// Executes a SQL command and returns the number of affected rows asynchronously.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsqlAsync pgsql = new(host, username, password, database);
        /// await pgsql.ConnectAsync();
        /// 
        /// DbParams p = new DbParams
        /// {
        ///     { "firstName", "First name" },
        ///     { "LastName", "Last name" },
        ///     { "subject", "the best subject" },
        ///     { "salary", 250 }
        /// };
        /// await pgsql.ExecuteNonQueryAsync("INSERT INTO teachers(first_name, last_name, subject, salary) VALUES(@firstName, @lastName, @subject, @salary)", p);
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="sqlQuery">The SQL command to execute.</param>
        /// <param name="parameters">The parameters to bind to the SQL command.</param>
        /// <returns>The number of rows affected by the SQL command.</returns>
        /// <exception cref="ArgumentNullException">Thrown when connection is null.</exception>
        public async Task<int> ExecuteNonQueryAsync(string sqlQuery, Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            if (_conn == null) throw new ArgumentNullException(nameof(_conn));
            await using var cmd = new NpgsqlCommand(sqlQuery, _conn);
            AddParameters(sqlQuery, parameters, cmd);
            return await cmd.ExecuteNonQueryAsync(cancellationToken);

        }

        /// <summary>
        /// Executes a SQL command and returns the number of affected rows asynchronously.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsqlAsync pgsql = new(host, username, password, database);
        /// await pgsql.ConnectAsync();
        /// 
        /// await pgsql.ExecuteNonQueryAsync("CALL stored_procedure_name(argument_list)");
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="sql">The SQL command to execute.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the asynchronous operation. The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>The number of rows affected by the SQL command.</returns>
        /// <exception cref="ArgumentNullException">Thrown when connection is null.</exception>
        public async Task<int> ExecuteNonQueryAsync(string sql, CancellationToken cancellationToken = default)
        {
            return await ExecuteNonQueryAsync(sql, new DbParams(), cancellationToken);
        }

        /// <summary>
        /// Executes a SQL query that returns a single result asynchronously and maps it to a specified type.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsqlAsync pgsql = new(host, username, password, database);
        /// await pgsql.ConnectAsync();
        /// 
        /// DbParams p = new DbParams("id", 23);
        /// 
        /// var teacher = await pgsql.ExecuteOneAsync<Teacher>("SELECT * FROM teachers WHERE id=@id", p); 
        /// Console.WriteLine(teacher.first_name);
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="sqlQuery">The SQL query to execute.</param>
        /// <param name="parameters">The parameters to bind to the SQL query.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the asynchronous operation. The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>The result of the SQL query mapped to the specified type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when '_conn' is null.</exception>
        public async Task<T?> ExecuteOneAsync<T>(string sqlQuery, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
        {
            if (_conn == null) throw new ArgumentNullException(nameof(_conn));
            IEnumerable<PropertyInfo> propertyList = typeof(T).GetProperties();
            T item = Activator.CreateInstance<T>();
            await using (var cmd = new NpgsqlCommand(sqlQuery, _conn))
            {
                AddParameters(sqlQuery, parameters, cmd);
                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    return SetObjectValues(propertyList.ToList(), item, reader);
                }
            }
            return default;
        }

        /// <summary>
        /// Executes a SQL query that returns a list of objects of type 'T' asynchronously.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsqlAsync pgsql = new(host, username, password, database);
        /// await pgsql.ConnectAsync();
        /// 
        /// DbParams p = new DbParams()
        /// {
        ///     { "id", 23 }
        /// }
        /// 
        /// foreach (var item in await pgsql.ExecuteAsync<Teachers>("SELECT * FROM teachers WHERE id<@id", p))
        /// {
        ///     Console.WriteLine(item.first_name);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type of objects to retrieve from the database.</typeparam>
        /// <param name="sqlQuery">The SQL query to execute.</param>
        /// <param name="parameters">The parameters to bind to the SQL query.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the asynchronous operation. The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>A list of objects of type 'T' retrieved from the database.</returns>
        public async Task<List<T>?> ExecuteAsync<T>(string sqlQuery, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
        {
            List<PropertyInfo> propertyList = typeof(T).GetProperties().ToList();
            List<T>? returnList = new List<T>();

            await using (var cmd = new NpgsqlCommand(sqlQuery, _conn))
            {
                AddParameters(sqlQuery, parameters, cmd);

                returnList = await ExecuteReaderMenyAsync<T>(propertyList, cmd, cancellationToken);
            }
            if (returnList == null || returnList.Count <= 0) return default;
            return returnList;
        }

        /// <summary>
        /// Dumps a record set from the database into a List of Dictionarys
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsqlAsync pgsql = new(host, username, password, database);
        /// await pgsql.ConnectAsync();
        /// 
        /// foreach (var item in await pgsql.DumpAsync("SELECT * FROM teachers"))
        /// {
        ///     Console.WriteLine(item["first_name"]);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="sqlQuery">The SQL query to execute.</param>
        /// <param name="parameters">The parameters to bind to the SQL query.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the asynchronous operation. The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>A list of objects of type '<see cref="Dictionary{string, object}"/>' retrieved from the database.</returns>
        /// <exception cref="ArgumentException">Throws if number of @field don't correspond to the number of parameters</exception>
        public async Task<List<Dictionary<string, object>>> DumpAsync(string sqlQuery, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
        {
            List<Dictionary<string, object>> returnList = new();
            await using (var cmd = new NpgsqlCommand(sqlQuery, _conn))
            {
                AddParameters(sqlQuery, parameters, cmd);

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        dict.Add(reader.GetName(i), reader[i]);
                    }
                    returnList.Add(dict);
                }
            }

            return returnList;
        }


        /// <summary>
        /// Creates database table from class structure asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of object to map table structure from.</typeparam>
        /// <param name="dropIfExists">If true it adds DROP TABLE IF EXISTS statement</param>
        /// <param name="isTempTable">If true it creates a temporary table</param>
        /// <exception cref="ArgumentNullException">Thrown if name of field is set via the attribute [Field("name_of_field")] but no FieldType.</exception>
        public async Task CreateAsync<T>(bool dropIfExists = false, bool isTempTable = false)
        {
            IEnumerable<PropertyInfo?> propertyList = typeof(T).GetProperties().Where(x => x != null)!;
            IEnumerable<FieldAttribute> fieldAttributes = PrepareCreateFields<T>(propertyList);

            string queryString = PrepareCreateSql<T>(fieldAttributes);

            string tableName = GetTableName<T>();

            queryString = isTempTable ? $"CREATE TEMP TABLE {tableName}({queryString}" : $"CREATE TABLE {tableName}({queryString}";

            if (dropIfExists)
            {
                await using var batch = new NpgsqlBatch(_conn)
                {
                    BatchCommands =
                    {
                        new($"DROP TABLE IF EXISTS {tableName}"),
                        new(queryString)
                    }
                };

                await batch.ExecuteNonQueryAsync();
            }
            else
            {
                await ExecuteNonQueryAsync(queryString);
            }
        }
    }
}
