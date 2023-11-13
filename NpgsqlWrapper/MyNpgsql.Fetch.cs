
using Npgsql;
using System.Reflection;

namespace NpgsqlWrapper
{
    public partial class MyNpgsql : MyNpgsqlBase
    {
        /// <summary>
        /// Fetching a result set from the database.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsql pgsql = new(host, username, password, database);
        /// pgsql.Connect();
        /// 
        /// foreach (var item in pgsql.Fetch<Teachers>())
        /// {
        ///     Console.WriteLine(item.first_name);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="sql">Query string. If leaved empty it will run a simple SELECT * FROM MyTableClass.</param>
        /// <returns>List of objects with data from the database</returns>
        public IEnumerable<T> Fetch<T>(string? sql = null)
        {
            if (_conn == null) throw new ArgumentNullException(nameof(_conn));
            if (sql == null) sql = $"SELECT * FROM {GetTableName<T>()}";
            return Execute<T>(sql);
        }

        /// <summary>
        /// Fetching a result set from the database.
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
        /// foreach (var item in pgsql.Fetch<Teachers>("SELECT * FROM teachers WHERE id<@id", p))
        /// {
        ///     Console.WriteLine(item.first_name);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="sql">Query string.</param>
        /// <param name="parameters">The parameters to bind to the SQL query.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> mapped with data set from database</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public IEnumerable<T> Fetch<T>(string sql, Dictionary<string, object> parameters)
        {
            if (_conn == null) throw new ArgumentNullException(nameof(_conn));
            using var tx = _conn.BeginTransaction();
            IEnumerable<PropertyInfo> properties = typeof(T).GetProperties();
            using var cmd = _conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;

            AddParameters(sql, parameters, cmd);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                T item = Activator.CreateInstance<T>();
                item = SetObjectValues(properties.ToList(), item, reader);
                yield return item;
            }
        }

        /// <summary>
        /// Fetches one result from the database.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsql pgsql = new(host, username, password, database);
        /// pgsql.Connect();
        /// 
        /// Teachers teacher = pgsql.FetchOne<Teachers>()
        /// Console.WriteLine(teacher.first_name);
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="sql">Query string. If leaved empty it will run a simple SELECT * FROM MyTableClass LIMIT 1.</param>
        /// <returns>An object with data from the database.</returns>
        public T? FetchOne<T>(string? sql = null)
        {
            sql ??= $"SELECT * FROM {GetTableName<T>()} FETCH FIRST 1 ROW ONLY";
            return ExecuteOne<T>(sql);
        }

        /// <summary>
        /// Fetches a list of data from the database with sql injection safety.
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
        ///     { "id", 5 }
        /// };
        /// 
        /// Teachers teacher = pgsql.ExecuteOne<Teachers>("SELECT * FROM teachers WHERE id=@id", p)
        /// Console.WriteLine(teacher.first_name);
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="sql">Query string.</param>
        /// <param name="parameters">Arguments for the query.</param>
        /// <returns>List of objects with data from the database.</returns>
        /// <exception cref="ArgumentNullException">Throws if no DB connection is made.</exception>
        /// <exception cref="ArgumentException">Throws if number of @field don't correspond to the number of arguments.</exception>
        public T? ExecuteOne<T>(string sql, Dictionary<string, object>? parameters = null)
        {
            if (_conn == null) throw new ArgumentNullException(nameof(_conn));
            IEnumerable<PropertyInfo> properties = typeof(T).GetProperties();

            using var cmd = new NpgsqlCommand(sql, _conn);
            AddParameters(sql, parameters, cmd);

            return ExecuteReader<T>(properties, cmd);
        }
    }
}
