using Npgsql;
using System.Reflection;
using System.Diagnostics;

namespace NpgsqlWrapper
{
    /// <summary>
    /// Wrapper for Npgsql: https://github.com/npgsql/npgsql
    /// </summary>
    internal class MyNpgsql : MyNpgsqlBase
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
        public void Connect()
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionString);
            var dataSource = dataSourceBuilder.Build();

            _conn = dataSource.OpenConnection();
        }

        /// <summary>
        /// Closing database connection
        /// </summary>
        /// <exception cref="ArgumentNullException">Throws if connections is not made</exception>
        public void Close()
        {
            if (_conn == null) throw new ArgumentNullException(nameof(_conn));
            _conn.Close();
        }

        /// <summary>
        /// Fetching a result set from the database.
        /// </summary>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="sql">Query string. If leaved empty it will run a simple SELECT * FROM MyTableClass.</param>
        /// <returns>List of objects with data from the database</returns>
        public IEnumerable<T> Fetch<T>(string? sql = null)
        {
            if (sql == null) sql = $"SELECT * FROM {typeof(T).Name}";
            return Execute<T>(sql);
        }

        /// <summary>
        /// Fetching a result set from the database.
        /// </summary>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="sql">Query string.</param>
        /// <param name="parameters">Arguments for the query.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> mapped with data set from database</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public IEnumerable<T> Fetch<T>(string sql, Dictionary<string, object> parameters)
        {
            if (_conn == null) throw new ArgumentNullException(nameof(_conn));
            using (var tx = _conn.BeginTransaction())
            {
                List<PropertyInfo> propertyList = typeof(T).GetProperties().ToList();
                using (var cmd = _conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = sql;

                    AddParameters(sql, parameters, cmd);

                    return ExecuteRederMany<T>(propertyList, cmd);
                }

            }
        }

        /// <summary>
        /// Executes a SQL query that returns a list of objects of type 'T'.
        /// </summary>
        /// <typeparam name="T">The type of objects to retrieve from the database.</typeparam>
        /// <param name="sqlQuery">The SQL query to execute.</param>
        /// <param name="parameters">The parameters to bind to the SQL query.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> of objects of type 'T' retrieved from the database.</returns>
        public IEnumerable<T> Execute<T>(string sqlQuery, Dictionary<string, object>? parameters = null)
        {
            List<PropertyInfo> propertyList = typeof(T).GetProperties().ToList();

            using (var cmd = new NpgsqlCommand(sqlQuery, _conn))
            {
                if (parameters != null)
                {
                    foreach (KeyValuePair<string, object> kvp in parameters)
                    {
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                    }
                }

                return ExecuteRederMany<T>(propertyList, cmd);
            }
        }

        /// <summary>
        /// Executes a <see cref="NpgsqlDataReader"/> object that returns a list of objects of type <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="propertyList">List of <see cref="PropertyInfo"/> made out of 'T'.</param>
        /// <param name="cmd">The <see cref="NpgsqlCommand"/> command object</param>
        /// <returns>The <see cref="IEnumerable{T}"/> of the SQL query mapped to the specified type.</returns>
        private static IEnumerable<T> ExecuteRederMany<T>(List<PropertyInfo> propertyList, NpgsqlCommand cmd)
        {
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
        /// <param name="propertyList">List of <see cref="PropertyInfo"/> made out of 'T'.</param>
        /// <param name="cmd">The <see cref="NpgsqlCommand"/> command object</param>
        /// <returns>A object of the type 'T' mapped with data from ths sql command</returns>
        private static T? ExecuteReder<T>(List<PropertyInfo> propertyList, NpgsqlCommand cmd)
        {
            T item = Activator.CreateInstance<T>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                return SetObjectValues(propertyList, item, reader);
            }
            return default;
        }

        /// <summary>
        /// Fetches one result from the database.
        /// </summary>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="sql">Query string. If leaved empty it will run a simple SELECT * FROM MyTableClass LIMIT 1.</param>
        /// <returns>An object with data from the database.</returns>
        public T? FetchOne<T>(string? sql = null)
        {
            sql ??= $"SELECT * FROM {typeof(T).Name} FETCH FIRST 1 ROW ONLY";
            return ExecuteOne<T>(sql);
        }

        /// <summary>
        /// Fetches a list of data from the database with sql injection safety.
        /// </summary>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="sql">Query string.</param>
        /// <param name="parameters">Arguments for the query.</param>
        /// <returns>List of objects with data from the database.</returns>
        /// <exception cref="ArgumentNullException">Throws if no DB connection is made.</exception>
        /// <exception cref="ArgumentException">Throws if number of @field don't correspond to the number of arguments.</exception>
        public T? ExecuteOne<T>(string sql, Dictionary<string, object>? parameters = null)
        {
            if (_conn == null) throw new ArgumentNullException(nameof(_conn));
            List<PropertyInfo> propertyList = typeof(T).GetProperties().ToList();
            
            using (var cmd = new NpgsqlCommand(sql, _conn))
            {
                if (parameters != null)
                {
                    foreach (KeyValuePair<string, object> kvp in parameters)
                    {
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                    }
                }

                return ExecuteReder<T>(propertyList, cmd);
            }
        }

        /// <summary>
        /// Inserts. 
        /// </summary>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="objToInsert">Objects with data.</param>
        /// <returns>The number of rows affected by the insert operation.</returns>
        public int Insert<T>(T objToInsert)
        {
            PrepareInsertSql(objToInsert, out string sql, out DbParams parameters);
            return ExecuteNonQuery(sql, parameters);
        }

        /// <summary>
        /// Inserts with returning statement. 
        /// </summary>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="objToInsert">Objects with data.</param>
        /// <returns>The number of rows affected by the insert operation.</returns>
        public T? InsertReturning<T>(T objToInsert)
        {
            PrepareInsertSql(objToInsert, out string sql, out DbParams parameters);
            sql += " RETURNING *";
            Debug.WriteLine(sql);
            return ExecuteOne<T>(sql, parameters);
        }

        /// <summary>
        /// Inserts many. 
        /// </summary>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="objToInsert">Objects with data.</param>
        /// <returns>The number of rows affected by the insert operation.</returns>
        public int InsertMany<T>(List<T> objToInsert)
        {
            PrepareManyInsertSql(objToInsert, out string sql, out DbParams parameters);
            return ExecuteNonQuery(sql, parameters);
        }

        /// <summary>
        /// Executes a SQL command and returns number of effected rows.
        /// </summary>
        /// <param name="sql">SQL query</param>
        /// <param name="parameters">The parameters to bind to the SQL command.</param>
        /// <returns>The number of effected rows</returns>
        /// <exception cref="ArgumentNullException">Thrown when connection is null.</exception>
        public int ExecuteNonQuery(string sql, Dictionary<string, object> parameters)
        {
            if (_conn == null) throw new ArgumentNullException(nameof(_conn));
            using var cmd = new NpgsqlCommand(sql, _conn);
            foreach (KeyValuePair<string, object> kvp in parameters)
            {
                cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
            }
            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes a SQL command and returns number of effected rows.
        /// </summary>
        /// <param name="sql">SQL query</param>
        /// <returns>The number of effected rows</returns>
        public int ExecuteNonQuery(string sql)
        {
            return ExecuteNonQuery(sql, new DbParams());
        }

        /// <summary>
        /// Inserts many with returning statement.
        /// </summary>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="listToInsert">Objects with data.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> objects containing the inserted records.</returns>
        public IEnumerable<T> InsertManyReturning<T>(List<T> listToInsert)
        {
            PrepareManyInsertSql(listToInsert, out string sql, out DbParams parameters);
            sql += " RETURNING *";
            return Execute<T>(sql, parameters);
        }

        /// <summary>
        /// Updates rows in a database table based on the provided object's properties.
        /// </summary>
        /// <typeparam name="T">The type of object representing the table to update.</typeparam>
        /// <param name="table">The object representing the table to update.</param>
        /// <param name="where">Optional WHERE clause to specify which rows to update.</param>
        /// <param name="whereParameters">Additional parameters to include in the SQL query.</param>
        /// <returns>The number of rows affected by the update operation.</returns>
        public int Update<T>(T table, string? where = null, Dictionary<string, object>? whereParameters = null)
        {
            PrepareUpdateSql(table, where, whereParameters, out string sql, out DbParams returnParameters);
            return ExecuteNonQuery(sql, returnParameters);
        }

        /// <summary>
        /// Deletes rows from a database table based on the specified conditions.
        /// </summary>
        /// <param name="tableName">The name of the table to delete rows from.</param>
        /// <param name="where">Optional WHERE clause to specify which rows to delete.</param>
        /// <param name="whereParameters">Additional parameters to include in the SQL query.</param>
        /// <returns>The number of rows affected by the delete operation.</returns>
        /// <exception cref="ArgumentException">Throws if number of @field don't correspond to the number of parameters.</exception>
        public int Delete(string tableName, string? where = null, Dictionary<string, object>? whereParameters = null)
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

            return ExecuteNonQuery(sql, whereParameters);
        }

        /// <summary>
        /// Executes a SQL command that deletes records from the database.
        /// </summary>
        /// <typeparam name="T">The type to take table name from.</typeparam>
        /// <param name="where">The sql where command.</param>
        /// <param name="whereParameters">The parameters to bind to the SQL where command.</param>
        /// <returns>The number of effected rows</returns>
        public int Delete<T>(string? where = null, Dictionary<string, object>? whereParameters = null)
        {
            string tableName = typeof(T).GetType().Name;
            return Delete(tableName, where, whereParameters);
        }

        /// <summary>
        /// Dumps a record set from the database into a List of Dictionarys
        /// </summary>
        /// <param name="sqlQuery">The SQL query to execute.</param>
        /// <param name="parameters">The parameters to bind to the SQL query.</param>
        /// <returns>A list of objects of type '<see cref="Dictionary{string, object}"/>' retrieved from the database.</returns>
        public IEnumerable<Dictionary<string, object>> Dump(string sqlQuery, Dictionary<string, object>? parameters = null)
        {
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

        
    }
}
