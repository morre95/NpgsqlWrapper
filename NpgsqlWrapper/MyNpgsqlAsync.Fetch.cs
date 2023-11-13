using System.Reflection;
using System.Runtime.CompilerServices;

namespace NpgsqlWrapper
{
    public partial class MyNpgsqlAsync : MyNpgsqlBase
    {
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
    }
}
