using Npgsql;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NpgsqlWrapper
{
    internal class MyNpgsqlBase
    {

        protected readonly string? _connectionString;
        protected NpgsqlConnection? _conn = null;

        public MyNpgsqlBase(string host, string username, string password, string database)
        {
            _connectionString = $"Host={host};Username={username};Password={password};Database={database}";
        }

        public Int64 LastInsertedID
        {
            get
            {
                if (_conn == null) throw new ArgumentNullException(nameof(_conn));
                using var cmd = new NpgsqlCommand("SELECT lastval() AS id", _conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (reader[0].GetType() == typeof(Int64))
                        return reader.GetInt64(0);
                }
                return -1;
            }
        }

        protected static int GetSqlNumParams(string sqlQuery)
        {
            string pattern = @"[=<>]+\s*@";
            MatchCollection matches = Regex.Matches(sqlQuery, pattern);
            return matches.Count;

        }

        /// <summary>
        /// Maps the values from a database reader to the properties of an object of type 'T'.
        /// </summary>
        /// <typeparam name="T">The type of object to map.</typeparam>
        /// <param name="propertyList">The list of properties of type 'T'.</param>
        /// <param name="item">The object to which the values will be mapped.</param>
        /// <param name="reader">The database reader containing the values to map.</param>
        /// <returns>The object 'item' with values mapped from the database reader.</returns>
        protected static T SetObjectValues<T>(List<PropertyInfo> propertyList, T item, NpgsqlDataReader reader)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string fieldName = reader.GetName(i);
                PropertyInfo? propertyInfo = propertyList.Find(prop => prop.Name == fieldName);

                if (propertyInfo != null)
                {
                    object value = reader[i];
                    if (value is DBNull)
                    {
                        propertyInfo.SetValue(item, null);
                    }
                    else
                    {
                        propertyInfo.SetValue(item, value);
                    }
                }
            }
            return item;
        }

        protected static void PrepareManyInsertSql<T>(List<T> listToInsert, out string sql, out DbParams args)
        {
            List<PropertyInfo> propertyList = typeof(T).GetProperties().ToList();

            List<string> fieldNames = GetFieldNames(listToInsert[0], propertyList);

            if (fieldNames.Count == 0)
            {
                throw new ArgumentException("There are no non-null fields to insert.");
            }

            if (listToInsert.Any(x => x == null)) throw new ArgumentException();

            string table = listToInsert[0]!.GetType().Name;


            sql = $"INSERT INTO {table} ({string.Join(",", fieldNames)}) VALUES";
            args = new();
            int i;
            for (i = 0; i < listToInsert.Count; i++)
            {
                sql += "(";
                foreach (PropertyInfo property in propertyList)
                {
                    if (property.GetValue(listToInsert[i], null) == null) { continue; }
                    args.Add($"{property.Name}_{i}", property.GetValue(listToInsert[i], null)!);
                    sql += $"@{property.Name}_{i},";
                }

                sql = sql.Remove(sql.Length - 1, 1);
                sql += "),";

            }

            sql = sql.Remove(sql.Length - 1, 1);

            if (args.Count != fieldNames.Count * i) throw new ArgumentException("List of arguments don't match objects to insert");
        }

        protected static void PrepareInsertSql<T>(T objToInsert, out string sql, out DbParams args)
        {
            if (objToInsert == null)
            {
                throw new ArgumentNullException(nameof(objToInsert));
            }
            List<PropertyInfo> propertyList = typeof(T).GetProperties().ToList();

            List<string> fieldNames = GetFieldNames(objToInsert, propertyList);

            if (fieldNames.Count == 0)
            {
                throw new ArgumentException("There are no non-null fields to insert.");
            }

            string table = objToInsert.GetType().Name;
            sql = $"INSERT INTO {table} ({string.Join(",", fieldNames)}) VALUES(";
            args = new();
            foreach (var property in propertyList)
            {
                if (property.GetValue(objToInsert, null) == null) { continue; }
                args.Add(property.Name, property.GetValue(objToInsert, null)!);
                sql += $"@{property.Name},";
            }
            sql = sql.Remove(sql.Length - 1, 1) + ")";

            if (args.Count != fieldNames.Count) throw new ArgumentException("The specified fields don't match the number of values to insert");
        }

        private static List<string> GetFieldNames<T>(T fieldObject, List<PropertyInfo> propertyList)
        {
            return propertyList.Where(x => x.GetValue(fieldObject, null) != null).Select(x => x.Name).ToList();
        }

        protected static void PrepareUpdateSql<T>(T table, string? where, Dictionary<string, object>? whereArgs, out string sql, out DbParams returnArgs)
        {
            if (table == null)
            {
                throw new ArgumentNullException();
            }
            List<PropertyInfo?> propertyList = typeof(T).GetProperties().ToList()!;

            sql = $"UPDATE {table.GetType().Name} SET ";
            returnArgs = new();
            foreach (var property in propertyList)
            {
                if (property != null)
                {
                    if (property.GetValue(table, null) == null) { continue; }

                    returnArgs.Add(property.Name, property.GetValue(table, null)!);
                    sql += $"{property.Name} = @{property.Name},";
                }
            }

            sql = sql.Remove(sql.Length - 1, 1);

            sql = AddWhereToQuery(where, sql);

            if (whereArgs != null)
            {
                foreach (var pair in whereArgs)
                {
                    if (!returnArgs.ContainsKey(pair.Key)) returnArgs.Add(pair.Key, pair.Value);
                    else throw new ArgumentException($"{pair.Key} is not unique");
                }
            }
        }

        protected static string AddWhereToQuery(string? where, string sql)
        {
            if (where != null)
            {
                sql += " WHERE ";
                where = Regex.Replace(where, "where ", "", RegexOptions.IgnoreCase);
                sql += where;
            }
            return sql;
        }

        protected static string PrepareDeleteSql(string tableName, string? where) 
            => AddWhereToQuery(where, $"DELETE FROM {tableName}");

        protected static void AddParameters(string sqlQuery, Dictionary<string, object>? parameters, NpgsqlCommand cmd)
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
        }
    }
}
