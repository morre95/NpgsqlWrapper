using Npgsql;
using NpgsqlTypes;
using System.Diagnostics;
using System.Reflection;


namespace NpgsqlWrapper
{
    /// <summary>
    /// Wrapper for Npgsql: https://github.com/npgsql/npgsql
    /// </summary>
    internal class MyNpgsqlAsync : MyNpgsqlBase
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
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionString);
            var dataSource = dataSourceBuilder.Build();

            _conn = await dataSource.OpenConnectionAsync();
        }

        public async Task CloseAsync()
        {
            if (_conn == null) throw new ArgumentNullException();

            await _conn.CloseAsync();
        }

        /// <summary>
        /// Close connection to database, asynchronously.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Throws if connections is not made</exception>
        public async Task CloseAsync()
        {
            if ( _conn == null ) throw new ArgumentNullException(nameof( _conn));
            await _conn.CloseAsync();
        }

        /// <summary>
        /// Fetches asynchronously a list of data from the database.
        /// </summary>
        /// <typeparam name="T">Class with fields to fetch from database.</typeparam>
        /// <param name="sql">Query string. If leaved empty it will run a simple SELECT * FROM MyTableClass.</param>
        /// <returns>List of objects with data from the database</returns>
        public async Task<List<T>> FetchAsync<T>(string? sql = null)
        {
            if (sql == null) sql = $"SELECT * FROM {typeof(T).Name}";
            return await ExecuteAsync<T>(sql);
        }

        /// <summary>
        /// Fetches asynchronously a list of data from the database with sql injection safety.
        /// </summary>
        /// <typeparam name="T">Class with fields to fetch from database.</typeparam>
        /// <param name="sql">Query string.</param>
        /// <param name="parameters">Arguments for the query.</param>
        /// <returns>List of objects with data from the database</returns>
        /// <exception cref="ArgumentNullException">Throws if no DB connection is made.</exception>
        /// <exception cref="ArgumentException">Throws if number of @field don't correspond to the number of arguments.</exception>
        public async Task<List<T>> FetchAsync<T>(string sql, Dictionary<string, object> parameters)
        {
            if (_conn == null) throw new ArgumentNullException(nameof(_conn));
            List<T> returnList = new List<T>();
            using (var tx = _conn.BeginTransaction())
            {
                List<PropertyInfo> propertyList = typeof(T).GetProperties().ToList();
                using (var cmd = _conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = sql;

                    if (GetSqlNumParams(sql) != parameters.Count)
                    {
                        throw new ArgumentException("List of arguments don't match the sql query");
                    }

                    foreach (KeyValuePair<string, object> kvp in parameters)
                    {
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                    }

                    returnList = await ExecuteReaderMenyAsync(returnList, propertyList, cmd);
                }

            }
            return returnList;
        }

        /// <summary>
        /// Executes a <see cref="NpgsqlDataReader"/> object that returns a list of objects of type 'T' asynchronously.
        /// </summary>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="returnList"></param>
        /// <param name="propertyList">List of <see cref="PropertyInfo"/> made out of T</param>
        /// <param name="cmd">The <see cref="NpgsqlCommand"/> command object</param>
        /// <returns>The result of the SQL query mapped to the specified type.</returns>
        private static async Task<List<T>> ExecuteReaderMenyAsync<T>(List<T> returnList, List<PropertyInfo> propertyList, NpgsqlCommand cmd)
        {
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                T item = Activator.CreateInstance<T>();
                item = SetObjectValues(propertyList, item, reader);
                returnList.Add(item);
            }
            return returnList;
        }

        /// <summary>
        /// Fetches asynchronously one result from the database.
        /// </summary>
        /// <typeparam name="T">Class with fields you want to fetch from database.</typeparam>
        /// <param name="sql">Query string. If leaved empty it will run a simple SELECT * FROM MyTableClass LIMIT 1.</param>
        /// <returns>An object with data from the database.</returns>
        public async Task<T> FetchOneAsync<T>(string? sql = null)
        {
            if (sql == null) sql = $"SELECT * FROM {typeof(T).Name} FETCH FIRST 1 ROW ONLY";
            return await ExecuteOneAsync<T>(sql);
        }

        /// <summary>
        /// Fetches asynchronously a list of data from the database with sql injection safety.
        /// </summary>
        /// <typeparam name="T">Class with fields you want to fetch from database.</typeparam>
        /// <param name="sql">Query string.</param>
        /// <param name="parameters">Arguments for the query.</param>
        /// <returns>List of objects with data from the database.</returns>
        /// <exception cref="ArgumentNullException">Throws if no DB connection is made.</exception>
        /// <exception cref="ArgumentException">Throws if number of @field don't correspond to the number of arguments.</exception>
        public async Task<T> FetchOneAsync<T>(string sql, Dictionary<string, object> parameters)
        {
            if (_conn == null) throw new ArgumentNullException(nameof(_conn));
            // TBD: kolla om det finns LIMIT 1 eller FETCH FIRST 1 ROW ONLY eller liknande och läg till det om det fattas
            await using (var tx = await _conn.BeginTransactionAsync())
            {
                List<PropertyInfo> propertyList = typeof(T).GetProperties().ToList();
                T item = Activator.CreateInstance<T>();
                using (var cmd = _conn.CreateCommand())
                {
                    cmd.Transaction = tx;

                    cmd.CommandText = sql;

                    if (GetSqlNumParams(sql) != parameters.Count)
                    {
                        throw new ArgumentException("List of arguments don't match the sql query");
                    }

                    foreach (KeyValuePair<string, object> kvp in parameters)
                    {
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                        /*kvp.Value is DateTime
                        ? cmd.Parameters.AddWithValue(kvp.Key, NpgsqlDbType.TimestampTz, kvp.Value)
                        : cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);*/
                    }

                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        return SetObjectValues(propertyList, item, reader);
                    }

                }

                return item;
            }
        }

        /// <summary>
        /// Inserts asynchronously. 
        /// </summary>
        /// <typeparam name="T">Object with data to insert into the database.</typeparam>
        /// <param name="objToInsert">Objects with data.</param>
        /// <returns>The number of rows affected by the insert operation.</returns>
        public async Task<int> InsertAsync<T>(T objToInsert)
        {
            PrepareInsertSql(objToInsert, out string sql, out DbParams parameters);
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        /// <summary>
        /// Inserts asynchronously with returning statement.
        /// </summary>
        /// <typeparam name="T">The type of objects to insert.</typeparam>
        /// <param name="objToInsert">The object with records to insert.</param>
        /// <returns>A list of objects containing the inserted records.</returns>
        public async Task<T> InsertReturningAsync<T>(T objToInsert)
        {
            PrepareInsertSql(objToInsert, out string sql, out DbParams parameters);
            sql += " RETURNING *";
            Debug.WriteLine(sql);
            return await ExecuteOneAsync<T>(sql, parameters);
        }

        /// <summary>
        /// Inserts a list of objects into a database table asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of objects to insert.</typeparam>
        /// <param name="objToInsert">The object with records to insert.</param>
        /// <returns>The number of rows affected by the insert operation.</returns>
        public async Task<int> InsertManyAsync<T>(List<T> objToInsert)
        {
            PrepareManyInsertSql(objToInsert, out string sql, out DbParams parameters);
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        /// <summary>
        /// Inserts a list of objects into a database table asynchronously and returns the inserted records.
        /// </summary>
        /// <typeparam name="T">The type of objects to insert.</typeparam>
        /// <param name="listToInsert">The list of objects to insert.</param>
        /// <returns>A list of objects containing the inserted records.</returns>
        /// <exception cref="ArgumentException">Throws if number of @field don't correspond to the number of arguments.</exception>
        public async Task<List<T>> InsertManyReturningAsync<T>(List<T> listToInsert)
        {
            PrepareManyInsertSql(listToInsert, out string sql, out DbParams parameters);
            sql += " RETURNING *";
            return await ExecuteAsync<T>(sql, parameters);
        }



        /// <summary>
        /// Updates rows in a database table asynchronously based on the provided object's properties.
        /// </summary>
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

            return await ExecuteNonQueryAsync(sql, returnParams);
        }



        /// <summary>
        /// Deletes rows from a database table asynchronously based on the specified conditions.
        /// </summary>
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

            return await ExecuteNonQueryAsync(sql, whereParameters);
        }


        /// <summary>
        /// Executes a SQL command that deletes records from the database asynchronously.
        /// </summary>
        /// <typeparam name="T">The type to take table name from.</typeparam>
        /// <param name="where">The sql where command.</param>
        /// <param name="whereParameters">The parameters to bind to the SQL where command.</param>
        /// <returns>The number of effected rows</returns>
        public async Task<int> DeleteAsync<T>(string? where = null, Dictionary<string, object>? whereParameters = null)
        {
            string tableName = typeof(T).Name;
            return await DeleteAsync(tableName, where, whereParameters);
        }

        /// <summary>
        /// Executes a SQL command that does not return a result set asynchronously.
        /// </summary>
        /// <param name="sql">The SQL command to execute.</param>
        /// <param name="parameters">The parameters to bind to the SQL command.</param>
        /// <returns>The number of rows affected by the SQL command.</returns>
        /// <exception cref="ArgumentNullException">Thrown when '_conn' is null.</exception>
        public async Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object> parameters)
        {
            if (_conn == null) throw new ArgumentNullException(nameof(_conn));
            await using (var cmd = new NpgsqlCommand(sql, _conn))
            {
                foreach (KeyValuePair<string, object> kvp in parameters)
                {
                    cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                }
                return await cmd.ExecuteNonQueryAsync();
            }

        }

        /// <summary>
        /// Executes a SQL query that returns a single result asynchronously and maps it to a specified type.
        /// </summary>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="parameters">The parameters to bind to the SQL query.</param>
        /// <returns>The result of the SQL query mapped to the specified type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when '_conn' is null.</exception>
        public async Task<T> ExecuteOneAsync<T>(string sql, Dictionary<string, object>? parameters = null)
        {
            if (_conn == null) throw new ArgumentNullException(nameof(_conn));
            List<PropertyInfo> propertyList = typeof(T).GetProperties().ToList();
            T item = Activator.CreateInstance<T>();
            await using (var cmd = new NpgsqlCommand(sql, _conn))
            {

                if (parameters != null)
                {
                    foreach (KeyValuePair<string, object> kvp in parameters)
                    {
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                    }
                }
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    return SetObjectValues(propertyList, item, reader);
                }
            }
            //return default;
            return item;
        }



        /// <summary>
        /// Executes a SQL query that returns a list of objects of type 'T' asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of objects to retrieve from the database.</typeparam>
        /// <param name="sqlQuery">The SQL query to execute.</param>
        /// <param name="parameters">The parameters to bind to the SQL query.</param>
        /// <returns>A list of objects of type 'T' retrieved from the database.</returns>
        public async Task<List<T>> ExecuteAsync<T>(string sqlQuery, Dictionary<string, object>? parameters = null)
        {
            List<PropertyInfo> propertyList = typeof(T).GetProperties().ToList();
            List<T> returnList = new List<T>();

            await using (var cmd = new NpgsqlCommand(sqlQuery, _conn))
            {
                if (parameters != null)
                {
                    foreach (KeyValuePair<string, object> kvp in parameters)
                    {
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                    }
                }

                returnList = await ExecuteReaderMenyAsync(returnList, propertyList, cmd);
            }
            //if (returnList.Count <= 0) return default;
            return returnList;
        }

        /// <summary>
        /// Dumps a record set from the database into a List of Dictionarys
        /// </summary>
        /// <param name="sqlQuery">The SQL query to execute.</param>
        /// <param name="parameters">The parameters to bind to the SQL query.</param>
        /// <returns>A list of objects of type '<see cref="Dictionary{string, object}"/>' retrieved from the database.</returns>
        /// <exception cref="ArgumentException">Throws if number of @field don't correspond to the number of parameters</exception>
        public async Task<List<Dictionary<string, object>>> DumpAsync(string sqlQuery, Dictionary<string, object>? parameters = null)
        {
            List<Dictionary<string, object>> returnList = new();
            await using (var cmd = new NpgsqlCommand(sqlQuery, _conn))
            {
                if (parameters != null)
                {
                    if (GetSqlNumParams(sqlQuery) != parameters.Count)
                    {
                        throw new ArgumentException("List of arguments don't match the sql query");
                    }

                    foreach (KeyValuePair<string, object> kvp in parameters)
                    {
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                    }
                }

                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            dict.Add(reader.GetName(i), reader[i]);
                        }
                        returnList.Add(dict);
                    }
                }
            }

            return returnList;
        }
    }
}
