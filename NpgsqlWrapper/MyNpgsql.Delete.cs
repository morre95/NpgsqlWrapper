using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NpgsqlWrapper
{
    public partial class MyNpgsql : MyNpgsqlBase
    {

        /// <summary>
        /// Deletes rows from a database table based on the specified conditions.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsql pgsql = new(host, username, password, database);
        /// pgsql.Connect();
        /// 
        /// int affectedRows = pgsql.Delete("teachers", "id=@id", new DbParams("id", 11));
        /// Console.WriteLine(affectedRows);
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="tableName">The name of the table to delete rows from.</param>
        /// <param name="where">Optional WHERE clause to specify which rows to delete.</param>
        /// <param name="whereParameters">Additional parameters to include in the SQL query.</param>
        /// <returns>The number of rows affected by the delete operation.</returns>
        /// <exception cref="ArgumentException">Throws if number of @field don't correspond to the number of parameters.</exception>
        /// <exception cref="ArgumentNullException">Thrown when connection is null.</exception>
        public int Delete(string tableName, string? where = null, Dictionary<string, object>? whereParameters = null)
        {
            if (_conn == null) throw new ArgumentNullException(nameof(_conn));
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
        /// <example>
        /// Usage:
        /// <code>
        /// <![CDATA[
        /// MyNpgsql pgsql = new(host, username, password, database);
        /// pgsql.Connect();
        /// 
        /// int affectedRows = pgsql.Delete<Teachers>("id=@id", new DbParams("id", 11));
        /// Console.WriteLine(affectedRows);
        /// ]]>
        /// </code>
        /// </example>
        /// <typeparam name="T">The type to take table name from.</typeparam>
        /// <param name="where">The sql where command.</param>
        /// <param name="whereParameters">The parameters to bind to the SQL where command.</param>
        /// <returns>The number of effected rows</returns>
        /// <exception cref="ArgumentNullException">Thrown when connection is null.</exception>
        public int Delete<T>(string? where = null, Dictionary<string, object>? whereParameters = null)
        {
            if (_conn == null) throw new ArgumentNullException(nameof(_conn));
            string tableName = GetTableName<T>();
            return Delete(tableName, where, whereParameters);
        }
    }
}
