
namespace NpgsqlWrapper
{
    public partial class MyNpgsqlAsync : MyNpgsqlBase
    {
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
    }
}
