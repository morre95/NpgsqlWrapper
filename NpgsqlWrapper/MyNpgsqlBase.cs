using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NpgsqlWrapper
{
    /// <summary>
    /// Wrapper for Npgsql: https://github.com/npgsql/npgsql
    /// </summary>
    public class MyNpgsqlBase
    {
        /// <summary>
        /// The connection string provided by the user, including the password.
        /// </summary>
        protected readonly string? _connectionString;

        /// <summary>
        /// Connection object
        /// </summary>
        protected NpgsqlConnection? _conn = null;

        /// <summary>
        /// Return the latest inserted id
        /// </summary>
        public Int64 LastInsertedID
        {
            get
            {
                if (_conn == null) throw new ArgumentNullException(nameof(_conn));
                using var cmd = new NpgsqlCommand("SELECT lastval()", _conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (reader[0].GetType() == typeof(Int64))
                        return reader.GetInt64(0);
                }
                return -1;
            }
        }

        /// <summary>
        /// Constructor, building connection string with parameter provided by the user
        /// </summary>
        /// <param name="host">Host for server</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="database">Database</param>
        public MyNpgsqlBase(string host, string username, string password, string database)
        {
            _connectionString = $"Host={host};Username={username};Password={password};Database={database}";
        }


        /// <summary>
        /// Returns the number of times @field is in a sql query.
        /// </summary>
        /// <param name="sqlQuery">Query string.</param>
        /// <returns>Number of escaped @field.</returns>
        protected static int GetSqlNumParams(string sqlQuery)
        {
            string pattern = @"[=<>,]+\s*@";
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
                PropertyInfo? propertyInfo = propertyList.Find(prop => prop.Name == fieldName || (prop.ReadAttribute<FieldAttribute>() != null && prop.ReadAttribute<FieldAttribute>().FieldName == fieldName));

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
            if (listToInsert.Any(x => x == null)) throw new ArgumentNullException(nameof(listToInsert));

            IEnumerable<PropertyInfo> propertyList = typeof(T).GetProperties();
            IEnumerable<string> fieldNames = GetFieldNames(listToInsert[0], propertyList);

            if (fieldNames.Count() <= 0)
            {
                throw new ArgumentException("There are no non-null fields to insert.");
            }

            string table = listToInsert[0]!.GetType().Name;


            sql = $"INSERT INTO {table} ({string.Join(",", fieldNames)}) VALUES";
            args = new();
            int i;
            for (i = 0; i < listToInsert.Count; i++)
            {
                sql += "(";
                foreach (PropertyInfo property in propertyList)
                {
                    object? value = property.GetValue(listToInsert[i], null);

                    IEnumerable<InsertIgnoreAttribute> ignoreAttributes = property.GetCustomAttributes<InsertIgnoreAttribute>(true);

                    if (value == null || ignoreAttributes.Any()) { continue; }
                    if (property.GetCustomAttribute<FieldAttribute>(true) != null)
                    {
                        FieldAttribute attr = property.ReadAttribute<FieldAttribute>();
                        args.Add($"{attr.FieldName}_{i}", value);
                        sql += $"@{attr.FieldName}_{i},";
                    }
                    else
                    {
                        args.Add($"{property.Name}_{i}", value);
                        sql += $"@{property.Name}_{i},";
                    }
                }

                sql = sql.Remove(sql.Length - 1, 1);
                sql += "),";

            }

            sql = sql.Remove(sql.Length - 1, 1);

            if (args.Count != fieldNames.Count() * i) throw new ArgumentException("List of arguments don't match objects to insert");
        }

        

        protected static void PrepareInsertSql<T>(T objToInsert, out string sql, out DbParams args)
        {
            if (objToInsert == null)
            {
                throw new ArgumentNullException(nameof(objToInsert));
            }

            IEnumerable<PropertyInfo> propertyList = typeof(T).GetProperties();

            IEnumerable<string> fieldNames = GetFieldNames(objToInsert, propertyList);

            if (fieldNames.Count() == 0)
            {
                throw new ArgumentException("There are no non-null fields to insert.");
            }

            string table = objToInsert.GetType().Name;
            sql = $"INSERT INTO {table} ({string.Join(",", fieldNames)}) VALUES(";
            args = new();
            foreach (var property in propertyList)
            {
                object? value = property.GetValue(objToInsert, null);

                IEnumerable<InsertIgnoreAttribute> ignoreAttributes = property.GetCustomAttributes<InsertIgnoreAttribute>(true);

                if (value == null || ignoreAttributes.Any()) { continue; }
                if (property.GetCustomAttribute<FieldAttribute>(true) != null)
                {
                    FieldAttribute attr = property.ReadAttribute<FieldAttribute>();
                    args.Add($"{attr.FieldName}", value);
                    sql += $"@{attr.FieldName},";
                }
                else
                {
                    args.Add($"{property.Name}", value);
                    sql += $"@{property.Name},";
                }
            }
            sql = sql.Remove(sql.Length - 1, 1) + ")";

            if (args.Count != fieldNames.Count()) throw new ArgumentException("The specified fields don't match the number of values to insert");
        }

        private static IEnumerable<string> GetFieldNames<T>(T fieldObject, IEnumerable<PropertyInfo> propertyList)
        {
            foreach (PropertyInfo property in propertyList)
            {
                if (property.GetValue(fieldObject, null) != null)
                {
                    string fieldName = property.Name;
                    if (property.ReadAttribute<FieldAttribute>() != null)
                    {
                        fieldName = property.ReadAttribute<FieldAttribute>().FieldName;
                    }

                    IEnumerable<InsertIgnoreAttribute> ignoreAttributes = property.GetCustomAttributes<InsertIgnoreAttribute>(true);

                    if (!ignoreAttributes.Any())
                    {
                        yield return fieldName;
                    }
                }
            }
        }

        protected static void PrepareUpdateSql<T>(T table, string? where, Dictionary<string, object>? whereArgs, out string sql, out DbParams returnArgs)
        {
            if (table == null)
            {
                throw new ArgumentNullException();
            }
            IEnumerable<PropertyInfo?> propertyList = typeof(T).GetProperties()!;

            sql = $"UPDATE {table.GetType().Name} SET ";
            returnArgs = new();
            foreach (var property in propertyList)
            {
                if (property != null)
                {
                    object value = property.GetValue(table, null)!;

                    IEnumerable<UpdateIgnoreAttribute> ignoreAttributes = property.GetCustomAttributes<UpdateIgnoreAttribute>(true);

                    if (value == null || ignoreAttributes.Any()) { continue; }
                    FieldAttribute attr = property.ReadAttribute<FieldAttribute>();
                    if (attr != null)
                    {
                        returnArgs.Add(attr.FieldName, value);
                        sql += $"{attr.FieldName} = @{attr.FieldName},";
                    }
                    else
                    {
                        returnArgs.Add(property.Name, value);
                        sql += $"{property.Name} = @{property.Name},";
                    }
                    
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
                foreach (KeyValuePair<string, object> kvp in parameters)
                {
                    cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                }
            }
        }
    }
}
