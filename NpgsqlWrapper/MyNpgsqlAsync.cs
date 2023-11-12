using Npgsql;
using NpgsqlTypes;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;


namespace NpgsqlWrapper
{
    /// <summary>
    /// Wrapper for Npgsql: https://github.com/npgsql/npgsql
    /// </summary>
    public class MyNpgsqlAsync : MyNpgsqlBase
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
        /// Fetches asynchronously a result set from the database.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsqlAsync pgsql = new(host, username, password, database);
        /// await pgsql.ConnectAsync();
        /// 
        /// foreach (var item in await pgsql.FetchAsync<Teachers>())
        /// {
        ///     Console.WriteLine(item.first_name);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="sql">Query string. If leaved empty it will run a simple SELECT * FROM MyTableClass.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the asynchronous operation. The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>List of objects with data from the database</returns>
        public async Task<List<T>?> FetchAsync<T>(string? sql = null, CancellationToken cancellationToken = default)
        {
            if (sql == null) sql = $"SELECT * FROM {GetTableName<T>()}";
            return await ExecuteAsync<T>(sql, null, cancellationToken);
        }

        /// <summary>
        /// Fetches asynchronously a list of data from the database with sql injection safety.
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
        /// foreach (var item in await pgsql.FetchAsync<Teachers>("SELECT * FROM teachers WHERE id<@id", p))
        /// {
        ///     Console.WriteLine(item.first_name);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="sql">Query string.</param>
        /// <param name="parameters">The parameters to bind to the SQL query.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the asynchronous operation. The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>List of objects with data from the database.</returns>
        /// <exception cref="ArgumentNullException">Throws if no DB connection is made.</exception>
        /// <exception cref="ArgumentException">Throws if number of @field don't correspond to the number of arguments.</exception>
        public async Task<List<T>?> FetchAsync<T>(string sql, Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            if (_conn == null) throw new ArgumentNullException(nameof(_conn));
            List<T>? returnList = new List<T>();
            using (var tx = _conn.BeginTransaction())
            {
                List<PropertyInfo> propertyList = typeof(T).GetProperties().ToList();
                using var cmd = _conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = sql;

                AddParameters(sql, parameters, cmd);

                returnList = await ExecuteReaderMenyAsync<T>(propertyList, cmd, cancellationToken);

            }
            return returnList;
        }

        /*public async IAsyncEnumerable<T?> FetchEnumerableAsync<T>(string? sql = null)
        {
            throw new NotImplementedException();*/

        /*
        TODO: Detta är körbart om man returnerar Task<IEnumerable<T?>>
        sql ??= $"SELECT * FROM {typeof(T).Name}";  
        return FetchEnumerableAsync<T>(sql, new DbParams()).ToBlockingEnumerable();
        */

        // Man kör den så här då
        /*foreach (Teachers techer in await pgsql.FetchEnumerableAsync<Teachers>()) 
        {
            Console.WriteLine(techer.first_name);
        }*/
        //}

        /// <summary>
        /// Fetches asynchronously a list of data from the database with sql injection safety.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsqlAsync pgsql = new(host, username, password, database);
        /// await pgsql.ConnectAsync();
        /// 
        /// await foreach (var t in pgsql.FetchEnumerableAsync<Teachers>("SELECT * FROM teachers WHERE id<@id", new DbParams("id", 23))) 
        /// {
        ///     Console.WriteLine(t.first_name);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="sql">Query string.</param>
        /// <param name="parameters">The parameters to bind to the SQL command.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the asynchronous operation. The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns><see cref="IAsyncEnumerable{T}"/> object of the SQL query mapped to the specified type.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async IAsyncEnumerable<T?> FetchEnumerableAsync<T>(string sql, Dictionary<string, object> parameters, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (_conn == null) throw new ArgumentNullException(nameof(_conn));
            await using var tx = await _conn.BeginTransactionAsync(cancellationToken);

            await using var cmd = _conn.CreateCommand();

            cmd.Transaction = tx;
            cmd.CommandText = sql;

            AddParameters(sql, parameters, cmd);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            IEnumerable<PropertyInfo> propertyList = typeof(T).GetProperties();

            while (await reader.ReadAsync(cancellationToken))
            {
                T item = Activator.CreateInstance<T>();
                item = SetObjectValues(propertyList.ToList(), item, reader);
                yield return item;
            }
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
        /// Fetches asynchronously one result from the database.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsqlAsync pgsql = new(host, username, password, database);
        /// await pgsql.ConnectAsync();
        /// 
        /// var teacher = await pgsql.FetchOneAsync<Teachers>(); 
        /// Console.WriteLine(teacher.first_name);
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="sql">Query string. If leaved empty it will run a simple SELECT * FROM MyTableClass LIMIT 1.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the asynchronous operation. The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>An object with data from the database.</returns>
        public async Task<T?> FetchOneAsync<T>(string? sql = null, CancellationToken cancellationToken = default)
        {
            if (sql == null) sql = $"SELECT * FROM {GetTableName<T>()} FETCH FIRST 1 ROW ONLY";
            return await ExecuteOneAsync<T>(sql, null, cancellationToken);
        }

        /// <summary>
        /// Fetches asynchronously a list of data from the database with sql injection safety.
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
        /// var teacher = await pgsql.FetchOneAsync<Teacher>("SELECT * FROM teachers WHERE id=@id", p); 
        /// Console.WriteLine(teacher.first_name);
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="sql">Query string.</param>
        /// <param name="parameters">The parameters to bind to the SQL query.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the asynchronous operation. The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>List of objects with data from the database.</returns>
        /// <exception cref="ArgumentNullException">Throws if no DB connection is made.</exception>
        /// <exception cref="ArgumentException">Throws if number of @field don't correspond to the number of arguments.</exception>
        public async Task<T?> FetchOneAsync<T>(string sql, Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            if (_conn == null) throw new ArgumentNullException(nameof(_conn));
            // TBD: kolla om det finns LIMIT 1 eller FETCH FIRST 1 ROW ONLY eller liknande och läg till det om det fattas
            await using var tx = await _conn.BeginTransactionAsync();
            List<PropertyInfo> propertyList = typeof(T).GetProperties().ToList();
            T item = Activator.CreateInstance<T>();
            using (var cmd = _conn.CreateCommand())
            {
                cmd.Transaction = tx;

                cmd.CommandText = sql;

                AddParameters(sql, parameters, cmd);

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    return SetObjectValues(propertyList, item, reader);
                }

            }

            return default;
            //return item;
        }

        /// <summary>
        /// Inserts asynchronously. 
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsqlAsync pgsql = new(host, username, password, database);
        /// await pgsql.ConnectAsync();
        /// 
        /// var teacherToAdd = new Teachers()
        ///         {
        ///             first_name = firstName,
        ///             last_name = lastName,
        ///             subject = subject,
        ///             salary = salary
        ///         };
        /// await pgsql.InsertAsync(teacherToAdd);
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="objToInsert">Objects with data.</param>
        /// <returns>The number of rows affected by the insert operation.</returns>
        public async Task<int> InsertAsync<T>(T objToInsert)
        {
            PrepareInsertSql(objToInsert, out string sql, out DbParams parameters);
            return await ExecuteNonQueryAsync(sql, parameters, CancellationToken.None);
        }

        /// <summary>
        /// Inserts asynchronously with returning statement.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsqlAsync pgsql = new(host, username, password, database);
        /// await pgsql.ConnectAsync();
        /// 
        /// var teacherToAdd = new Teachers()
        ///         {
        ///             first_name = firstName,
        ///             last_name = lastName,
        ///             subject = subject,
        ///             salary = salary
        ///         };
        /// var teacher = await pgsql.InsertReturningAsync(teacherToAdd); 
        /// Console.WriteLine(teacher.first_name);
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type of objects to insert.</typeparam>
        /// <param name="objToInsert">The object with records to insert.</param>
        /// <returns>A list of objects containing the inserted records.</returns>
        public async Task<T?> InsertReturningAsync<T>(T objToInsert)
        {
            PrepareInsertSql(objToInsert, out string sql, out DbParams parameters);
            sql += " RETURNING *";
            return await ExecuteOneAsync<T>(sql, parameters, CancellationToken.None);
        }

        /// <summary>
        /// Inserts a list of objects into a database table asynchronously.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsqlAsync pgsql = new(host, username, password, database);
        /// await pgsql.ConnectAsync();
        /// 
        /// var teacherToAdd1 = new Teachers()
        ///         {
        ///             first_name = firstName1,
        ///             last_name = lastName1,
        ///             subject = subject1,
        ///             salary = salary1
        ///         };
        /// var teacherToAdd2 = new Teachers()
        ///         {
        ///             first_name = firstName2,
        ///             last_name = lastName2,
        ///             subject = subject2,
        ///             salary = salary2
        ///         };
        /// 
        /// List<Teachers> addMe = new();
        /// addMe.Add(teacherToAdd1);
        /// addMe.Add(teacherToAdd2);
        /// 
        /// int affectedRows = await pgsql.InsertManyAsync(addMe);
        /// Console.WriteLine(affectedRows);
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type of objects to insert.</typeparam>
        /// <param name="objToInsert">The object with records to insert.</param>
        /// <returns>The number of rows affected by the insert operation.</returns>
        public async Task<int> InsertManyAsync<T>(List<T> objToInsert)
        {
            PrepareManyInsertSql(objToInsert, out string sql, out DbParams parameters);
            return await ExecuteNonQueryAsync(sql, parameters, CancellationToken.None);
        }

        /// <summary>
        /// Inserts a list of objects into a database table asynchronously and returns the inserted records.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsqlAsync pgsql = new(host, username, password, database);
        /// await pgsql.ConnectAsync();
        /// 
        /// var teacherToAdd1 = new Teachers()
        ///         {
        ///             first_name = firstName1,
        ///             last_name = lastName1,
        ///             subject = subject1,
        ///             salary = salary1
        ///         };
        /// var teacherToAdd2 = new Teachers()
        ///         {
        ///             first_name = firstName2,
        ///             last_name = lastName2,
        ///             subject = subject2,
        ///             salary = salary2
        ///         };
        /// 
        /// List<Teachers> addMe = new();
        /// addMe.Add(teacherToAdd1);
        /// addMe.Add(teacherToAdd2);
        /// 
        /// var teachers = await pgsql.InsertReturningAsync(addMe); 
        /// 
        /// foreach (var teacher in teachers)
        ///     Console.WriteLine(teacher.first_name);
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type of objects to insert.</typeparam>
        /// <param name="listToInsert">The list of objects to insert.</param>
        /// <returns>A list of objects containing the inserted records.</returns>
        /// <exception cref="ArgumentException">Throws if number of @field don't correspond to the number of arguments.</exception>
        public async Task<List<T>?> InsertManyReturningAsync<T>(List<T> listToInsert)
        {
            PrepareManyInsertSql(listToInsert, out string sql, out DbParams parameters);
            sql += " RETURNING *";
            return await ExecuteAsync<T>(sql, parameters, CancellationToken.None);
        }



        /// <summary>
        /// Updates rows in a database table asynchronously based on the provided object's properties.
        /// </summary>
        /// <example>
        /// Usage:
        /// Clarification:
        /// It is not necessary to use WHERE in the 'where' parameter. The funktion will work the same with it and without it.
        /// <code>
        /// <![CDATA[
        /// MyNpgsqlAsync pgsql = new(host, username, password, database);
        /// await pgsql.ConnectAsync();
        /// 
        /// var editMe = new Teachers()
        ///  {
        ///      first_name = firstName,
        ///      last_name = lastName,
        ///      subject = subject,
        ///      salary = salary
        ///  };
        ///  
        /// DbParams p = new DbParams("id", 11);
        /// 
        /// await pgsql.UpdateAsync(editMe, "id=@id", p);
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type of object representing the table to update.</typeparam>
        /// <param name="table">The object representing the table to update.</param>
        /// <param name="where">Optional WHERE clause to specify which rows to update.</param>
        /// <param name="whereParameters">Additional parameters to include in the SQL query.</param>
        /// <returns>The number of rows affected by the update operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the 'table' parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when a duplicate parameter name is detected in 'whereParameters'.</exception>
        public async Task<int> UpdateAsync<T>(T table, string? where = null, Dictionary<string, object>? whereParameters = null)
        {
            PrepareUpdateSql(table, where, whereParameters, out string sql, out DbParams returnParams);

            return await ExecuteNonQueryAsync(sql, returnParams, CancellationToken.None);
        }



        /// <summary>
        /// Deletes all rows from a database table asynchronously.
        /// </summary>
        /// <param name="tableName">The name of the table to delete rows from.</param>
        /// <returns></returns>
        public async Task<int> DeleteAsync(string tableName)
        {
            string sql = PrepareDeleteSql(tableName, null);
            return await ExecuteNonQueryAsync(sql, new DbParams(), CancellationToken.None);
        }


        /// <summary>
        /// Deletes rows from a database table asynchronously based on the specified conditions.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsqlAsync pgsql = new(host, username, password, database);
        /// await pgsql.ConnectAsync();
        /// 
        /// DbParams p = new DbParams("id", 11);
        /// 
        /// await pgsql.DeleteAsync("teachers", "id=@id", p);
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="tableName">The name of the table to delete rows from.</param>
        /// <param name="where">Optional WHERE clause to specify which rows to delete.</param>
        /// <param name="whereParameters">Additional parameters to include in the SQL query.</param>
        /// <returns>The number of rows affected by the delete operation.</returns>
        /// <exception cref="ArgumentException">Throws if number of @field don't correspond to the number of parameters.</exception>
        public async Task<int> DeleteAsync(string tableName, string? where = null, Dictionary<string, object>? whereParameters = null)
        {
            string sql = PrepareDeleteSql(tableName, where);

            if (whereParameters == null)
            {
                whereParameters = new DbParams();
            }
            else
            {
                if (GetSqlNumParams(sql) != whereParameters.Count)
                {
                    throw new ArgumentException("List of arguments don't match the sql query");
                }
            }

            return await ExecuteNonQueryAsync(sql, whereParameters, CancellationToken.None);
        }


        /// <summary>
        /// Executes a SQL command that deletes records from the database asynchronously.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsqlAsync pgsql = new(host, username, password, database);
        /// await pgsql.ConnectAsync();
        /// 
        /// DbParams p = new DbParams("id", 11);
        /// 
        /// await pgsql.DeleteAsync<Teachers>("id=@id", p);
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type to take table name from.</typeparam>
        /// <param name="where">The sql where command.</param>
        /// <param name="whereParameters">The parameters to bind to the SQL where command.</param>
        /// <returns>The number of effected rows</returns>
        public async Task<int> DeleteAsync<T>(string where, Dictionary<string, object> whereParameters)
        {
            string tableName = GetTableName<T>();
            return await DeleteAsync(tableName, where, whereParameters);
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
    }
}
