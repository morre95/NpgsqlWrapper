using Npgsql;
using System.Reflection;
using System.Diagnostics;

namespace NpgsqlWrapper
{
    internal class MyNpgsql : MyNpgsqlBase
    {
        public MyNpgsql(string host, string username, string password, string database) :
            base(host, username, password, database)
        { }

        public void Connect()
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionString);
            var dataSource = dataSourceBuilder.Build();

            _conn = dataSource.OpenConnection();
        }

        public void Close()
        {
            if (_conn == null) throw new ArgumentNullException();

            _conn.Close();
        }

        public IEnumerable<T> Fetch<T>(string? sql = null)
        {
            if (sql == null) sql = $"SELECT * FROM {typeof(T).Name}";
            return Execute<T>(sql);
        }

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

                    if (GetSqlNumParams(sql) != parameters.Count)
                    {
                        throw new ArgumentException("List of arguments don't match the sql query");
                    }

                    foreach (KeyValuePair<string, object> kvp in parameters)
                    {
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                    }
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        T item = Activator.CreateInstance<T>();
                        item = SetObjectValues(propertyList, item, reader);
                        yield return item;
                    }
                }

            }
        }

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

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    T item = Activator.CreateInstance<T>();
                    item = SetObjectValues(propertyList, item, reader);
                    yield return item;
                }
            }
        }

        public T? FetchOne<T>(string? sql = null)
        {
            sql ??= $"SELECT * FROM {typeof(T).Name} FETCH FIRST 1 ROW ONLY";
            return ExecuteOne<T>(sql);
        }

        public T? ExecuteOne<T>(string sql, Dictionary<string, object>? parameters = null)
        {
            if (_conn == null) throw new ArgumentNullException(nameof(_conn));
            List<PropertyInfo> propertyList = typeof(T).GetProperties().ToList();
            T item = Activator.CreateInstance<T>();
            using (var cmd = new NpgsqlCommand(sql, _conn))
            {
                if (parameters != null)
                {
                    foreach (KeyValuePair<string, object> kvp in parameters)
                    {
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                    }
                }

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    return SetObjectValues(propertyList, item, reader);
                }
            }
            return default;
        }

        public int Insert<T>(T objToInsert)
        {
            PrepareInsertSql(objToInsert, out string sql, out DbParams parameters);
            return ExecuteNonQuery(sql, parameters);
        }

        public T? InsertReturning<T>(T objToInsert)
        {
            PrepareInsertSql(objToInsert, out string sql, out DbParams parameters);
            sql += " RETURNING *";
            Debug.WriteLine(sql);
            return ExecuteOne<T>(sql, parameters);
        }

        public int InsertMany<T>(List<T> objToInsert)
        {
            PrepareManyInsertSql(objToInsert, out string sql, out DbParams parameters);
            return ExecuteNonQuery(sql, parameters);
        }

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

        public IEnumerable<T> InsertManyReturning<T>(List<T> listToInsert)
        {
            PrepareManyInsertSql(listToInsert, out string sql, out DbParams parameters);
            sql += " RETURNING *";
            return Execute<T>(sql, parameters);
        }

        public int Update<T>(T table, string? where = null, Dictionary<string, object>? whereParameters = null)
        {
            PrepareUpdateSql(table, where, whereParameters, out string sql, out DbParams returnParameters);
            return ExecuteNonQuery(sql, returnParameters);
        }

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

        public int Delete<T>(string? where = null, Dictionary<string, object>? whereParameters = null)
        {
            string tableName = typeof(T).GetType().Name;
            return Delete(tableName, where, whereParameters);
        }

        public IEnumerable<Dictionary<string, object>> Dump(string sqlQuery, Dictionary<string, object>? parameters = null)
        {
            using var cmd = new NpgsqlCommand(sqlQuery, _conn);
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
